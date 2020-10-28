using System;
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.Runtime;
using DotnetDynamoDBLock.Config;

namespace DotnetDynamoDBLock.Locks {

	internal static class LocksProviderFactory {

		public static LocksProvider Create( LockConfig config ) {

			AWSCredentials sourceCredentials = FallbackCredentialsFactory
				.GetCredentials( fallbackToAnonymous: false );

			AWSCredentials credentials;
			if( !string.IsNullOrEmpty( config.RoleArn ) ) {

				credentials = new AssumeRoleAWSCredentials(
						sourceCredentials,
						roleArn: config.RoleArn,
						roleSessionName: $"dotnet-dynamodb-lock"
					);

			} else {
				credentials = sourceCredentials;
			}

			AmazonDynamoDBConfig dbConfig = new AmazonDynamoDBConfig {
				MaxErrorRetry = 10,
				RetryMode = RequestRetryMode.Standard,
				Timeout = TimeSpan.FromSeconds( 10 )
			};

			if( !string.IsNullOrEmpty( config.AwsRegion ) ) {
				dbConfig.RegionEndpoint = RegionEndpoint.GetBySystemName( config.AwsRegion );
			}

			IAmazonDynamoDB db = new AmazonDynamoDBClient( credentials, dbConfig );

			return new LocksProvider(
					db,
					() => DateTimeOffset.UtcNow,
					tableName: config.TableName
				);
		}
	}
}
