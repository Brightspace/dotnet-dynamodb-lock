using System;

namespace DotnetDynamoDBLock {

	internal sealed class CommandArgumentException : Exception {

		public CommandArgumentException( string message )
			: base( message ) {
		}
	}
}
