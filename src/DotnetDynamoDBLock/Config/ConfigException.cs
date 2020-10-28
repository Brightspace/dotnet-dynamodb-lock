using System;
using System.Runtime.Serialization;

namespace DotnetDynamoDBLock.Config {

	internal sealed class ConfigException : Exception {

		public ConfigException( string message )
			: base( message ) {
		}
	}
}