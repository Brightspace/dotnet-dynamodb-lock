using System;
using System.Collections.Generic;
using System.Globalization;
using Amazon.DynamoDBv2.Model;

namespace DotnetDynamoDBLock.Locks {

	internal static class AttributeValueExtensions {

		public static AttributeValue ToAttributeValue( this DateTimeOffset value ) {
			return new AttributeValue {
				N = value.ToUnixTimeSeconds().ToString( CultureInfo.InvariantCulture )
			};
		}

		public static DateTimeOffset GetDateTimeOffsetAttribute(
				this Dictionary<string, AttributeValue> attributes,
				string attributeName
			) {

			if( !attributes.TryGetValue( attributeName, out AttributeValue? attr ) ) {
				throw new AttributeException( $"Item missing '{ attributeName }' attribute." );
			}

			if( attr.N == null ) {
				throw new AttributeException( $"Attribute '{ attributeName }' is not numeric." );
			}

			if( !long.TryParse( attr.N, out long seconds ) ) {
				throw new AttributeException( $"Attribute '{ attributeName }' contains invalid numeric: { attr.N }" );
			}

			return DateTimeOffset.FromUnixTimeSeconds( seconds );
		}

		public static string GetStringAttribute(
				this Dictionary<string, AttributeValue> attributes,
				string attributeName
			) {

			if( !attributes.TryGetValue( attributeName, out AttributeValue? attr ) ) {
				throw new AttributeException( $"Item missing '{ attributeName }' attribute." );
			}

			if( attr.S == null ) {
				throw new AttributeException( $"Attribute '{ attributeName }' is not a string." );
			}

			return attr.S;
		}


		private sealed class AttributeException : Exception {

			public AttributeException( string message )
				: base( message ) {
			}
		}
	}
}
