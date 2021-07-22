using System;

namespace Deveel.Authentication {
	public class OAuthAuthorizationCodeRequest : OAuthGrantRequest {
		public OAuthAuthorizationCodeRequest(string code, Uri redirectUri) {
			if (string.IsNullOrWhiteSpace(code))
				throw new ArgumentException($"'{nameof(code)}' cannot be null or whitespace.", nameof(code));

			Code = code;
			RedirectUri = redirectUri ?? throw new ArgumentNullException(nameof(redirectUri));
		}

		public override OAuthGrantType GrantType => OAuthGrantType.AuthorizationCode;

		public string Code { get; }

		public Uri RedirectUri { get; }

		public string ClientId { get; set; }

		public string ClientSecret { get; set; }
	}
}
