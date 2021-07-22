using System;
using System.Collections.Generic;

namespace Deveel.Authentication {
	public sealed class OAuthAuthenticationCodeInfo {
		public OAuthAuthenticationCodeInfo(string clientId, Uri redirectUri) {
			if (string.IsNullOrWhiteSpace(clientId))
				throw new ArgumentException($"'{nameof(clientId)}' cannot be null or whitespace.", nameof(clientId));

			ClientId = clientId;
			RedirectUri = redirectUri ?? throw new ArgumentNullException(nameof(redirectUri));
		}

		public Uri RedirectUri { get; }

		public IList<string> Scopes { get; set; }

		public string State { get; set; }

		public string ClientId { get; }
	}
}
