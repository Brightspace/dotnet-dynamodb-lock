using System;
using System.Text.Json.Serialization;

namespace DotnetDynamoDBLock.Config {

	internal sealed class LockConfig {

		[JsonPropertyName( "awsRegion" )]
		public string? AwsRegion { get; set; }

		[JsonPropertyName( "tableName" )]
		public string TableName { get; set; } = string.Empty;

		[JsonPropertyName( "lockKey" )]
		public string LockKey { get; set; } = string.Empty;

		[JsonPropertyName( "roleArn" )]
		public string? RoleArn { get; set; }

		[JsonPropertyName( "lockDuration" )]
		public int LockDurationSeconds { get; set; } = 60;

		[JsonPropertyName( "retryAfter" )]
		public int RetryAfterSeconds { get; set; } = 5;

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
