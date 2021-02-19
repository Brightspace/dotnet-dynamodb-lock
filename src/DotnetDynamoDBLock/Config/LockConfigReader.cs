using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace DotnetDynamoDBLock.Config {

	internal static class LockConfigReader {

		public static async Task<LockConfig> ReadAsync(
				string path,
				CancellationToken cancellationToken
			) {

			using FileStream fs = new FileStream(
					path,
					FileMode.Open,
					FileAccess.Read,
					FileShare.Read
				);

			LockConfig? config = await JsonSerializer
				.DeserializeAsync<LockConfig>(
					fs,
					new JsonSerializerOptions {
						AllowTrailingCommas = false,
						ReadCommentHandling = JsonCommentHandling.Skip
					},
					cancellationToken
				)
				.ConfigureAwait( continueOnCapturedContext: false );

			if( config == null ) {
				throw new ConfigException( "Config is required." );
			}

			if( string.IsNullOrEmpty( config.TableName ) ) {
				throw new ConfigException( "'tableName' is a required config." );
			}

			if( string.IsNullOrEmpty( config.LockKey ) ) {
				throw new ConfigException( "'lockKey' is a required config." );
			}

			if( config.LockDuration <= TimeSpan.Zero ) {
				throw new ConfigException( "'lockDuration' must be greater than zero." );
			}

			return config;
		}
	}
}
