using System;
using System.CodeDom.Compiler;
using System.Collections.Immutable;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using DotnetDynamoDBLock.Config;
using DotnetDynamoDBLock.Locks;

namespace DotnetDynamoDBLock.Commands {

	internal static class ReleaseCommand {

		public static void WriteUsage( IndentedTextWriter writer ) {
			writer.WriteLine( "dotnet dynamodb-lock release --config <configFile> --token <tokenFile>" );
		}

		private static void ParseArguments(
				ReadOnlySpan<string> arguments,
				out string config,
				out string token
			) {

			ImmutableDictionary<string, string> args = ArgumentsParser.Parse(
					arguments,
					required: ImmutableHashSet.Create(
						StringComparer.Ordinal,
						"--config",
						"--token"
					)
				);

			config = args[ "--config" ];
			token = args[ "--token" ];
		}

		public static async Task<int> RunAsync(
				ReadOnlyMemory<string> arguments,
				CancellationToken cancellationToken
			) {

			ParseArguments(
					arguments.Span,
					config: out string configPath,
					token: out string tokenPath
				);

			LockConfig config = await LockConfigReader
				.ReadAsync( configPath, cancellationToken )
				.ConfigureAwait( continueOnCapturedContext: false );

			LocksProvider provider = LocksProviderFactory.Create( config );

			string tokenRaw = await File
				.ReadAllTextAsync( tokenPath, cancellationToken )
				.ConfigureAwait( continueOnCapturedContext: false );

			string key = config.LockKey;
			byte[] token = Convert.FromBase64String( tokenRaw );

			bool released = await provider
				.TryReleaseAsync(
					key,
					token,
					cancellationToken
				)
				.ConfigureAwait( continueOnCapturedContext: false );

			if( released ) {
				return 0;
			}

			LockInfo? current = await provider
				.TryGet( key, cancellationToken )
				.ConfigureAwait( continueOnCapturedContext: false );

			if( current == null ) {
				Console.WriteLine( "Lock already expired." );
				return 0;
			}

			Console.WriteLine( $"Locked already expired and reacquired by '{ current.Label }' at { current.Acquired }." );
			return 0;
		}
	}
}
