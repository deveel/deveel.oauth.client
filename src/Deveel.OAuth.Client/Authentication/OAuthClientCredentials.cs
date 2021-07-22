using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Rest;

namespace Deveel.Authentication {
	public class OAuthClientCredentials : ServiceClientCredentials {
		private readonly OAuthClientOptions options;
		private readonly ITokenCache tokenCache;
		private readonly string tokenName;
		private readonly string clientId;
		private readonly string clientSecret;
		private readonly string[] scopes;
		private readonly string audience;

		public OAuthClientCredentials(OAuthClientOptions options, ITokenCache tokenCache, string tokenName, string clientId, string clientSecret, params string[] scopes)
			: this(options, tokenCache, tokenName, clientId, clientSecret, null, scopes) {
		}

		public OAuthClientCredentials(OAuthClientOptions options, string clientId, string clientSecret, string audience, params string[] scopes)
			: this(options, null, null, clientId, clientSecret, audience, scopes) {
		}

		public OAuthClientCredentials(OAuthClientOptions options, string clientId, string clientSecret, params string[] scopes)
			: this(options, clientId, clientSecret, null, scopes) {
		}

		public OAuthClientCredentials(OAuthClientOptions options, ITokenCache tokenCache, string tokenName, string clientId, string clientSecret, string audience, params string[] scopes) {
			if (string.IsNullOrWhiteSpace(clientId))
				throw new ArgumentException($"'{nameof(clientId)}' cannot be null or whitespace.", nameof(clientId));

			if (string.IsNullOrWhiteSpace(clientSecret))
				throw new ArgumentException($"'{nameof(clientSecret)}' cannot be null or whitespace.", nameof(clientSecret));

			this.options = options ?? throw new ArgumentNullException(nameof(options));
			this.tokenCache = tokenCache;
			this.tokenName = tokenName;
			this.clientId = clientId;
			this.clientSecret = clientSecret;
			this.audience = audience;
			this.scopes = scopes;
		}

		public override async Task ProcessHttpRequestAsync(HttpRequestMessage request, CancellationToken cancellationToken) {
			var client = new OAuthAuthenticationClient(options, tokenCache);
			var authRequest = new OAuthClientCredentialsRequest(clientId, clientSecret) {
				Scopes = scopes != null ? new List<string>(scopes) : null,
				Audience = audience,
				TokenName = tokenName
			};

			var authToken = await client.RequestAccessTokenAsync(authRequest, cancellationToken);

			if (authToken != null)
				request.Headers.Authorization = new AuthenticationHeaderValue(authToken.TokenType, authToken.AccessToken);
		}
	}
}
