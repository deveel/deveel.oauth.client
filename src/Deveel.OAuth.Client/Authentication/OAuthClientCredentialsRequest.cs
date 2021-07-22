using System;
using System.Collections.Generic;

namespace Deveel.Authentication {
	public class OAuthClientCredentialsRequest : OAuthGrantRequest {
		public OAuthClientCredentialsRequest(string clientId, string clientSecret) {
			if (string.IsNullOrWhiteSpace(clientId))
				throw new ArgumentException($"'{nameof(clientId)}' cannot be null or whitespace.", nameof(clientId));
			if (string.IsNullOrWhiteSpace(clientSecret))
				throw new ArgumentException($"'{nameof(clientSecret)}' cannot be null or whitespace.", nameof(clientSecret));

			ClientId = clientId;
			ClientSecret = clientSecret;
		}

		public override OAuthGrantType GrantType => OAuthGrantType.ClientCredentials;

		public string ClientId { get; }

		public string ClientSecret { get; }

		public string Audience { get; set; }

		public IList<string> Scopes { get; set; } = new List<string>();
	}
}
