using System;

namespace DotnetDynamoDBLock.Locks {

	internal sealed class LockInfo {

		public LockInfo(
				string label,
				DateTimeOffset acquired,
				DateTimeOffset expires
			) {

			Label = label;
			Acquired = acquired;
			Expires = expires;
		}

		public string Label { get; }
		public DateTimeOffset Acquired { get; }
		public DateTimeOffset Expires { get; }
	}
}
