using System;
using System.CodeDom.Compiler;
using System.Text.Json.Serialization;

namespace DotnetDynamoDBLock.Config {

	internal sealed class LockConfig {

		public static void WriteUsage( IndentedTextWriter writer ) {

			writer.WriteLine( "Config:" );
			writer.Indent++;
			writer.WriteLine();
			{
				writer.WriteLine( "{" );
				writer.Indent++;
				{
					writer.WriteLine( "\"tableName\": \"locks\"," );
					writer.WriteLine( "\"lockKey\": \"build\"," );
					writer.WriteLine();

					writer.WriteLine( "\"awsRegion\": \"us-east-1\"," );
					writer.WriteLine( "\"roleArn\": \"arn:aws:iam::111111111111:role/build\"" );
					writer.WriteLine();

					writer.WriteLine( "\"acquireTimeout\":300," );
					writer.WriteLine( "\"lockDuration\":1800," );
					writer.WriteLine( "\"retryAfter\":5," );

				}
				writer.Indent--;
				writer.WriteLine( "}" );
			}

			writer.WriteLine();
			writer.WriteLine( "tableName:         The DynamoDB table name                                   [required]" );
			writer.WriteLine( "lockKey:           The unique key for the lock                               [required]" );
			writer.WriteLine();

			writer.WriteLine( "awsRegion:         The aws region of the table                               [optional]" );
			writer.WriteLine( "roleArn:           The arn of the role to assume                             [optional]" );
			writer.WriteLine();

			writer.WriteLine( "acquireTimeout:    How long to wait to acquire the lock before timing out    [optional]" );
			writer.WriteLine( "lockDuration:      How long the lock is valid for                            [optional]" );
			writer.WriteLine( "retryAfter:        How long to wait inbetween retries to acquire the lock    [optional]" );
			writer.WriteLine();

			writer.Indent--;
			writer.WriteLine();
		}

		[JsonPropertyName( "tableName" )]
		public string TableName { get; set; } = string.Empty;

		[JsonPropertyName( "lockKey" )]
		public string LockKey { get; set; } = string.Empty;

		[JsonPropertyName( "awsRegion" )]
		public string? AwsRegion { get; set; }

		[JsonPropertyName( "roleArn" )]
		public string? RoleArn { get; set; }

		[JsonPropertyName( "acquireTimeout" )]
		public int AcquireTimeoutSeconds { get; set; } = 300;

		[JsonPropertyName( "lockDuration" )]
		public int LockDurationSeconds { get; set; } = 1800;

		[JsonPropertyName( "retryAfter" )]
		public int RetryAfterSeconds { get; set; } = 5;

		[JsonIgnore]
		public TimeSpan AcquireTimeout {
			get { return TimeSpan.FromSeconds( AcquireTimeoutSeconds ); }
		}

		[JsonIgnore]
		public TimeSpan LockDuration {
			get { return TimeSpan.FromSeconds( LockDurationSeconds ); }
		}

		[JsonIgnore]
		public TimeSpan RetryAfter {
			get { return TimeSpan.FromSeconds( RetryAfterSeconds ); }
		}
	}
}
