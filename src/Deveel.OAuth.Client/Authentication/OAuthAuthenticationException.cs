using System;

namespace Deveel.Authentication {
	public class OAuthAuthenticationException : Exception {
		public OAuthAuthenticationException() {
		}

		public OAuthAuthenticationException(string message) : base(message) {
		}

		public OAuthAuthenticationException(string message, Exception innerException) : base(message, innerException) {
		}
	}
}
