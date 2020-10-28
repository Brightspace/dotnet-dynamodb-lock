using System;

namespace DotnetDynamoDBLock {

	internal sealed class ArgumentException : Exception {

		public ArgumentException( string message )
			: base( message ) {
		}
	}
}
