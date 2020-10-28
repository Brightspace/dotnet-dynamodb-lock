using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace DotnetDynamoDBLock {

	internal static class ArgumentsParser {

		public static ImmutableDictionary<string, string> Parse(
				ReadOnlySpan<string> args,
				ImmutableHashSet<string> required
			) {

			var parsed = ImmutableDictionary.CreateBuilder<string, string>( StringComparer.Ordinal );
			List<string> extras = new List<string>();

			for( int index = 0; index < args.Length; index++ ) {

				string arg = args[ index ];

				if( parsed.ContainsKey( arg ) ) {
					throw new CommandArgumentException( $"Argument '{ arg }' specified multiple times." );
				}

				if( required.Contains( arg ) ) {

					if( index + 1 == args.Length ) {
						break;
					}

					string argValue = args[ ++index ];
					parsed.Add( arg, argValue );

				} else {
					extras.Add( arg );
				}
			}

			if( extras.Count > 0 ) {
				string msg = "Unknown arguments: " + string.Join( " ", extras );
				throw new CommandArgumentException( msg );
			}

			foreach( string arg in required ) {

				if( !parsed.ContainsKey( arg ) ) {
					throw new CommandArgumentException( $"Missing required argument '{ arg }'." );
				}
			}

			return parsed.ToImmutable();
		}
	}
}
