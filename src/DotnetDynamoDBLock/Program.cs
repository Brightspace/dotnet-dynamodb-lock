using System;
using System.CodeDom.Compiler;
using System.Threading;
using System.Threading.Tasks;
using DotnetDynamoDBLock.Commands;

namespace D2L.Lms.FeatureFlags.Checker {

	internal sealed class Program {

		private const string UsageIndent = " ";

		private static void WriteUsage( IndentedTextWriter writer ) {

			writer.WriteLine( "Usage:" );
			writer.Indent++;
			{
				AcquireCommand.WriteUsage( writer );
				ReleaseCommand.WriteUsage( writer );
			}
			writer.Indent--;
			writer.WriteLine();

			writer.WriteLine( "Config:" );
			writer.Indent++;
			{
				writer.WriteLine( "{" );
				writer.Indent++;
				{
					writer.WriteLine( "\"awsRegion\":\"us-east-1\"," );
					writer.WriteLine( "\"tableTable\":\"locks\"," );
					writer.WriteLine( "\"lockKey\":\"build\"," );
					writer.WriteLine( "\"lockDuration\":300," );
					writer.WriteLine( "\"roleArn\":\"arn:aws:iam::111111111111:role/build\"" );
				}
				writer.Indent--;
				writer.WriteLine( "  }" );
			}
			writer.Indent--;
		}

		internal static async Task<int> Main( string[] args ) {

			if( args.Length < 0 ) {

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
					switch( operation ) {

						case "acquire":
							return await AcquireCommand
								.RunAsync( args, cts.Token )
								.ConfigureAwait( continueOnCapturedContext: false );

						case "release":
							return await ReleaseCommand
								.RunAsync( args, cts.Token )
								.ConfigureAwait( continueOnCapturedContext: false );

						default:
							Console.Error.WriteLine( $"Invalid operation: { operation }" );
							Console.Error.WriteLine();
							WriteUsage( new IndentedTextWriter( Console.Out, UsageIndent ) );
							return 127;
					}

				} catch( OperationCanceledException err ) when( err.CancellationToken == cts.Token ) {
					return 130;

				} catch( Exception err ) {

					Console.WriteLine( err.ToString() );
					return 1;
				}
			}
		}
	}
}
