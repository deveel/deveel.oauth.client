using System;

namespace Deveel.Authentication {
	public class OAuthAuthorizationCodeRequest : OAuthGrantRequest {
		public OAuthAuthorizationCodeRequest(string clientId, string clientSecret, string code) {
			if (string.IsNullOrWhiteSpace(clientId)) 
				throw new ArgumentException($"'{nameof(clientId)}' cannot be null or whitespace.", nameof(clientId));
			if (string.IsNullOrWhiteSpace(clientSecret)) 
				throw new ArgumentException($"'{nameof(clientSecret)}' cannot be null or whitespace.", nameof(clientSecret));
			if (string.IsNullOrWhiteSpace(code))
				throw new ArgumentException($"'{nameof(code)}' cannot be null or whitespace.", nameof(code));

			ClientId = clientId;
			ClientSecret = clientSecret;
			Code = code;
		}

		public override OAuthGrantType GrantType => OAuthGrantType.AuthorizationCode;

		public string Code { get; }

		public Uri RedirectUri { get; set; }

		public string ClientId { get; }

		public string ClientSecret { get; }

		public string Audience { get; set; }
	}
}
