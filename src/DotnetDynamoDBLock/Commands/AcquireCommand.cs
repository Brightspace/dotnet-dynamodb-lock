using System;
using System.CodeDom.Compiler;
using System.Collections.Immutable;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using DotnetDynamoDBLock.Config;
using DotnetDynamoDBLock.Locks;

namespace DotnetDynamoDBLock.Commands {

	internal static class AcquireCommand {

		public static void WriteUsage( IndentedTextWriter writer ) {
			writer.WriteLine( "dotnet dynamodb-lock acquire --config <configFile> --label <label> --output <tokenFile>" );
		}

		private static void ParseArguments(
				ReadOnlySpan<string> arguments,
				out string config,
				out string label,
				out string output
			) {

			ImmutableDictionary<string, string> args = ArgumentsParser.Parse(
					arguments,
					required: ImmutableHashSet.Create(
						StringComparer.Ordinal,
						"--config",
						"--label",
						"--output"
					)
				);

			config = args[ "--config" ];
			label = args[ "--label" ];
			output = args[ "--output" ];
		}

		public static async Task<int> RunAsync(
				ReadOnlyMemory<string> arguments,
				CancellationToken cancellationToken
			) {

			ParseArguments(
					arguments.Span,
					config: out string configPath,
					label: out string label,
					output: out string outputPath
				);

			LockConfig config = await LockConfigReader
				.ReadAsync( configPath, cancellationToken )
				.ConfigureAwait( continueOnCapturedContext: false );

			LocksProvider provider = LocksProviderFactory.Create( config );

			byte[] token = Guid.NewGuid().ToByteArray();

			AcquireArgs acquireArgs = new AcquireArgs(
					key: config.LockKey,
					duration: config.LockDuration,
					token: token,
					label: label
				);

			using( CancellationTokenSource timeout = new CancellationTokenSource( config.AcquireTimeout ) ) {
				try {

					using( CancellationTokenSource cancelAndTimeout = CancellationTokenSource.CreateLinkedTokenSource(
							cancellationToken,
							timeout.Token
						) ) {

						try {
							LockInfo @lock = await AcquireAsync(
									provider,
									acquireArgs,
									config,
									cancelAndTimeout.Token
								)
								.ConfigureAwait( continueOnCapturedContext: false );

							WriteLockInfo( @lock );

							await WriteTokenFileAsync( outputPath, token )
								.ConfigureAwait( continueOnCapturedContext: false );

							return 0;

						} catch( OperationCanceledException err ) when( err.CancellationToken == cancelAndTimeout.Token ) {

							cancellationToken.ThrowIfCancellationRequested();
							timeout.Token.ThrowIfCancellationRequested();

							string msg = $"Either the { nameof( cancellationToken ) } or { nameof( timeout ) } tokens should have thrown.";
							throw new InvalidOperationException( msg );
						}
					}

				} catch( OperationCanceledException err ) when( err.CancellationToken == timeout.Token ) {
					timeout.Token.ThrowIfCancellationRequested();
					return 121;
				}
			}
		}

		private static async Task<LockInfo> AcquireAsync(
				LocksProvider provider,
				AcquireArgs acquireArgs,
				LockConfig config,
				CancellationToken cancellationToken
			) {

			// First attempt
			{
				LockInfo? @lock = await provider
					.TryAcquire( acquireArgs, cancellationToken )
					.ConfigureAwait( continueOnCapturedContext: false );

				if( @lock != null ) {
					return @lock;
				}
			}

			// Retry attempts
			for(; ; ) {

				LockInfo? current = await provider
					.TryGet( acquireArgs.Key, cancellationToken )
					.ConfigureAwait( continueOnCapturedContext: false );

				if( current != null ) {

					Console.WriteLine(
							"Locked by '{0}' at {1}. Expires at {2}.",
							current.Label,
							current.Acquired.ToString( "T" ),
							current.Expires.ToString( "T" )
						);
				}

				if( cancellationToken.WaitHandle.WaitOne( config.RetryAfter ) ) {
					cancellationToken.ThrowIfCancellationRequested();
				}

				LockInfo? @lock = await provider
					.TryAcquire( acquireArgs, cancellationToken )
					.ConfigureAwait( continueOnCapturedContext: false );

				if( @lock != null ) {
					return @lock;
				}
			}
		}

		private static void WriteLockInfo( LockInfo @lock ) {

			Console.WriteLine(
					"Acquired lock at {0}. Expires at {1}.",
					@lock.Acquired.ToString( "T" ),
					@lock.Expires.ToString( "T" )
				);
		}

		private static async Task WriteTokenFileAsync(
				string path,
				byte[] token
			) {

			string tokenStr = Convert.ToBase64String( token );
			await File
				.WriteAllTextAsync( path, tokenStr )
				.ConfigureAwait( continueOnCapturedContext: false );
		}
	}
}
