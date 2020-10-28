using System;

namespace DotnetDynamoDBLock.Locks {

	internal sealed class LockInfo {

		public LockInfo(
				string key,
				string label,
				DateTimeOffset acquired,
				DateTimeOffset expires
			) {

			Key = key;
			Label = label;
			Acquired = acquired;
			Expires = expires;
		}

		public string Key { get; }
		public string Label { get; }
		public DateTimeOffset Acquired { get; }
		public DateTimeOffset Expires { get; }
	}
}
