using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;

namespace DotnetDynamoDBLock.Locks {

	internal sealed class LocksProvider : IDisposable {
		
		private const string KeyAttribute = "key";
		private const string AcquiredAttribute = "acquired";
		private const string ExpiresAttribute = "expires";
		private const string TokenAttribute = "token";
		private const string LabelAttribute = "label";

		private readonly IAmazonDynamoDB m_db;
		private readonly Func<DateTimeOffset> m_nowProvider;
		private readonly string m_tableName;

		public LocksProvider(
				IAmazonDynamoDB db,
				Func<DateTimeOffset> nowProvider,
				string tableName
			) {

			m_db = db;
			m_nowProvider = nowProvider;
			m_tableName = tableName;
		}

		public void Dispose() {
			m_db.Dispose();
		}

		public async Task<LockInfo?> TryAcquire(
				AcquireArgs args,
				CancellationToken cancellationToken
			) {

			DateTimeOffset now = m_nowProvider();
			DateTimeOffset expires = now.Add( args.Duration );

			Dictionary<string, AttributeValue> item = new Dictionary<string, AttributeValue> {
				{ KeyAttribute, new AttributeValue { S = args.Key } },
				{ AcquiredAttribute, now.ToAttributeValue() },
				{ ExpiresAttribute, expires.ToAttributeValue() },
				{ TokenAttribute, new AttributeValue { B = new MemoryStream( args.Token ) } },
				{ LabelAttribute, new AttributeValue { S = args.Label } }
			};

			PutItemRequest request = new PutItemRequest {
				TableName = m_tableName,
				Item = item,
				ConditionExpression = "attribute_not_exists( #key ) OR #expires <= :now",
				ExpressionAttributeNames = new Dictionary<string, string> {
					{ "#key", KeyAttribute },
					{ "#expires", ExpiresAttribute }
				},
				ExpressionAttributeValues = new Dictionary<string, AttributeValue> {
					{ ":now", now.ToAttributeValue() }
				}
			};

			try {
				await m_db
					.PutItemAsync( request, cancellationToken )
					.ConfigureAwait( continueOnCapturedContext: false );

				return new LockInfo(
						label: args.Label,
						acquired: now,
						expires: expires
					);

			} catch( ConditionalCheckFailedException ) {
				return null;
			}
		}

		public async Task<LockInfo?> TryGet(
				string key,
				CancellationToken cancellationToken 
			) {

			GetItemRequest request = new GetItemRequest {
				TableName = m_tableName,
				Key = new Dictionary<string, AttributeValue> {
					{ KeyAttribute, new AttributeValue{ S = key } }
				},
				ConsistentRead = true,
				ProjectionExpression = "#label, #acquired, #expires",
				ExpressionAttributeNames = new Dictionary<string, string> {
					{ "#label", LabelAttribute },
					{ "#acquired", AcquiredAttribute },
					{ "#expires", ExpiresAttribute }
				}
			};

			try {
				GetItemResponse response = await m_db
					.GetItemAsync( request, cancellationToken )
					.ConfigureAwait( continueOnCapturedContext: false );

				Dictionary<string, AttributeValue> item = response.Item;
				if( item.Count == 0 ) {
					return null;
				}

				string label = item.GetStringAttribute( LabelAttribute );
				DateTimeOffset acquired = item.GetDateTimeOffsetAttribute( AcquiredAttribute );
				DateTimeOffset expires = item.GetDateTimeOffsetAttribute( ExpiresAttribute );

				return new LockInfo(
						label: label,
						acquired: acquired,
						expires: expires
					);

			} catch( ResourceNotFoundException ) {
				return null;
			}
		}

		public async Task<bool> TryReleaseAsync(
				string key,
				byte[] token,
				CancellationToken cancellationToken
			) {

			DateTimeOffset now = m_nowProvider();

			DeleteItemRequest request = new DeleteItemRequest {
				TableName = m_tableName,
				Key = new Dictionary<string, AttributeValue> {
					{ KeyAttribute, new AttributeValue{ S = key } }
				},
				ConditionExpression = "#token = :token AND #expires >= :now",
				ExpressionAttributeNames = new Dictionary<string, string> {
					{ "#expires", ExpiresAttribute },
					{ "#token", TokenAttribute }
				},
				ExpressionAttributeValues = new Dictionary<string, AttributeValue> {
					{ ":now", now.ToAttributeValue() },
					{ ":token", new AttributeValue { B = new MemoryStream( token ) } }
				},
				ReturnValues = ReturnValue.NONE
			};

			try {
				await m_db
					.DeleteItemAsync( request, cancellationToken )
					.ConfigureAwait( continueOnCapturedContext: false );

				return true;

			} catch( ConditionalCheckFailedException ) {
				return false;
			}
		}

	}
}
