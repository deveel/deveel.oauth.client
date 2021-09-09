using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Options;
using Microsoft.Rest;

namespace Deveel.Authentication {
	public class OAuthClientCredentials : ServiceClientCredentials {
		private readonly OAuthClientOptions options;
		private readonly ITokenCache tokenCache;
		private readonly string tokenName;

		public OAuthClientCredentials(OAuthClientOptions options)
			: this(options, null, null) {
		}

		public OAuthClientCredentials(OAuthClientOptions options, ITokenCache tokenCache, string tokenName) {
			this.options = options ?? throw new ArgumentNullException(nameof(options));

			if (tokenCache != null &&
				String.IsNullOrWhiteSpace(tokenName))
				throw new ArgumentNullException(nameof(tokenName), "The token name must be provided if the cache is set");

			this.tokenCache = tokenCache;
			this.tokenName = tokenName;
		}

		public OAuthClientCredentials(IOptions<OAuthClientOptions> options)
			: this(options, null, null) {
		}

		public OAuthClientCredentials(IOptions<OAuthClientOptions> options, ITokenCache tokenCache, string tokenName)
			: this(options?.Value, tokenCache, tokenName) {
		}

		public override async Task ProcessHttpRequestAsync(HttpRequestMessage request, CancellationToken cancellationToken) {
			var client = new OAuthAuthenticationClient(options, tokenCache);
			var authRequest = new OAuthClientCredentialsRequest(options.ClientId, options.ClientSecret) {
				Scopes = options.Scopes != null ? new List<string>(options.Scopes) : null,
				Audience = options.Audience,
				TokenName = tokenName
			};

			var authToken = await client.RequestAccessTokenAsync(authRequest, cancellationToken);

			if (authToken != null)
				request.Headers.Authorization = new AuthenticationHeaderValue(authToken.TokenType, authToken.AccessToken);
		}
	}
}
