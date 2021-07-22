using System;

namespace Deveel.Authentication {
	public abstract class OAuthGrantRequest {
		public abstract OAuthGrantType GrantType { get; }

		public string TokenName { get; set; }
	}
}
