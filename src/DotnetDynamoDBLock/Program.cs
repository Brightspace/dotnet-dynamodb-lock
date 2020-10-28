using System;
using System.CodeDom.Compiler;
using System.Threading;
using System.Threading.Tasks;
using DotnetDynamoDBLock.Commands;
using DotnetDynamoDBLock.Config;

namespace DotnetDynamoDBLock {

	internal sealed class Program {

		private const string UsageIndent = "   ";

		internal static async Task<int> Main( string[] args ) {

			if( args.Length <= 0 ) {

				WriteUsage( new IndentedTextWriter( Console.Out, UsageIndent ) );
				return 2;
			}

			using( CancellationTokenSource cts = new CancellationTokenSource() ) {

				Console.CancelKeyPress += ( object sender, ConsoleCancelEventArgs e ) => {
					Console.WriteLine( "Stopping..." );
					e.Cancel = true;
					cts.Cancel();
				};

				try {
					string operation = args[ 0 ];

					ReadOnlyMemory<string> commandArgs = new ReadOnlyMemory<string>(
							args,
							start: 1,
							length: args.Length - 1
						);

					switch( operation ) {

						case "acquire":
							return await AcquireCommand
								.RunAsync( commandArgs, cts.Token )
								.ConfigureAwait( continueOnCapturedContext: false );

						case "release":
							return await ReleaseCommand
								.RunAsync( commandArgs, cts.Token )
								.ConfigureAwait( continueOnCapturedContext: false );

						default:
							Console.Error.WriteLine( $"Invalid operation: { operation }" );
							Console.Error.WriteLine();
							WriteUsage( new IndentedTextWriter( Console.Out, UsageIndent ) );
							return 127;
					}

				} catch( OperationCanceledException err ) when( err.CancellationToken == cts.Token ) {
					return 130;

				} catch( CommandArgumentException err ) {
					Console.WriteLine( err.Message );
					return 1;

				} catch( ConfigException err ) {
					Console.WriteLine( err.Message );
					return 1;

				} catch( Exception err ) {

					Console.WriteLine( err.ToString() );
					return 1;
				}
			}
		}

		private static void WriteUsage( IndentedTextWriter writer ) {

			writer.WriteLine( "Usage:" );
			writer.WriteLine();
			writer.Indent++;
			{
				AcquireCommand.WriteUsage( writer );
				ReleaseCommand.WriteUsage( writer );
			}
			writer.Indent--;
			writer.WriteLine();

			LockConfig.WriteUsage( writer );
		}

	}
}
