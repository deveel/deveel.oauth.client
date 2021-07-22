using System;

namespace Deveel.Authentication {
	public abstract class OAuthAuthenticationRequest {
		public abstract OAuthGrantType GrantType { get; }

		public string TokenName { get; set; }
	}
}
