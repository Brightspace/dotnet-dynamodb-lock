using System;

namespace DotnetDynamoDBLock.Locks {

	internal sealed class AcquireArgs {

		public AcquireArgs(
				string key,
				TimeSpan duration,
				byte[] token,
				string label
			) {

			Key = key;
			Duration = duration;
			Token = token;
			Label = label;
		}

		public string Key { get; }
		public TimeSpan Duration { get; }
		public byte[] Token { get; }
		public string Label { get; }
	}
}
