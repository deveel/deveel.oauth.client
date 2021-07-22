using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Options;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Deveel.Authentication {
	public sealed class OAuthAuthenticationClient : IDisposable {
		private readonly ITokenCache tokenCache;
		private readonly HttpClient httpClient;
		private readonly bool disposeClient;

		public OAuthAuthenticationClient(IOptions<OAuthClientOptions> options, ITokenCache tokenCache)
			:this(new HttpClient(), options, tokenCache) {
			disposeClient = true;
		}


		public OAuthAuthenticationClient(HttpClient httpClient, IOptions<OAuthClientOptions> options, ITokenCache tokenCache)
			: this(httpClient, options?.Value, tokenCache) {
		}

		public OAuthAuthenticationClient(IOptions<OAuthClientOptions> options)
			: this(new HttpClient(), options) {
			disposeClient = true;
		}


		public OAuthAuthenticationClient(HttpClient httpClient, IOptions<OAuthClientOptions> options)
			: this(httpClient, options, null) {
		}

		public OAuthAuthenticationClient(HttpClient httpClient, OAuthClientOptions options)
			: this(httpClient, options, null) {
		}

		public OAuthAuthenticationClient(OAuthClientOptions options, ITokenCache tokenCache)
			: this(new HttpClient(), options, tokenCache) {
			disposeClient = true;
		}

		public OAuthAuthenticationClient(HttpClient httpClient, OAuthClientOptions options, ITokenCache tokenCache) {
			Options = options ?? throw new ArgumentNullException(nameof(options));
			this.httpClient = httpClient;
			this.tokenCache = tokenCache;
		}

		public OAuthClientOptions Options { get; }

		public async Task<OAuthAccessToken> RequestAsync(OAuthAuthenticationRequest request, CancellationToken cancellationToken = default) {
			if (request is null)
				throw new ArgumentNullException(nameof(request));

			var result = await GetFromCacheAsync(request.TokenName, cancellationToken);

			if (result == null) {
				result = await RequestTokenAsync(request, cancellationToken);

				if (result != null)
					await SetInCacheAsync(request.TokenName, result, cancellationToken);
			}

			return result;
		}

		private Task SetInCacheAsync(string tokenName, OAuthAccessToken result, CancellationToken cancellationToken) {
			if (result == null || tokenCache == null || string.IsNullOrWhiteSpace(tokenName))
				return Task.CompletedTask;

			if (result.CacheAllowed != null && !result.CacheAllowed.Value)
				return Task.CompletedTask;

			var expiration = TimeSpan.FromSeconds(Options.DefaultTokenExpiration);

			if (result.HasExpiration) expiration = result.Expiration.Value;

			return tokenCache.SetTokenAsync(tokenName, result, expiration, cancellationToken);
		}

		private Task<OAuthAccessToken> GetFromCacheAsync(string tokenName, CancellationToken cancellationToken) {
			if (string.IsNullOrEmpty(tokenName) || tokenCache == null)
				return Task.FromResult<OAuthAccessToken>(null);

			return tokenCache.GetTokenAsync(tokenName, cancellationToken);
		}

		private Task<OAuthAccessToken> RequestTokenAsync(OAuthAuthenticationRequest request, CancellationToken cancellationToken) {
			switch (request.GrantType) {
				case OAuthGrantType.ClientCredentials: {
						if (!(request is OAuthClientCredentialsRequest clientCredentials))
							throw new ArgumentException("The request is invalid: must be client_credentials");

						return RequestClientCredentials(clientCredentials, cancellationToken);
					}
				case OAuthGrantType.RefreshToken: {
						throw new NotImplementedException();
					}
				case OAuthGrantType.AuthorizationCode:
					throw new NotImplementedException();

				default:
					throw new NotSupportedException("Grant type not supported");
			}
		}

		private async Task<OAuthAccessToken> RequestClientCredentials(OAuthClientCredentialsRequest clientCredentials, CancellationToken cancellationToken) {
				var scope = clientCredentials.Scopes != null && clientCredentials.Scopes.Count > 0
						? string.Join(" ", clientCredentials.Scopes)
						: null;

				var requestBody = new {
					client_id = clientCredentials.ClientId,
					client_secret = clientCredentials.ClientSecret,
					scope,
					audience = clientCredentials.Audience,
					grant_type = "client_credentials",
				};

				var content = new StringBuilder();
				using (var writer = new StringWriter(content)) {
					using (var jsonWriter = new JsonTextWriter(writer)) {
						JsonSerializer.CreateDefault().Serialize(jsonWriter, requestBody);
						jsonWriter.Flush();
					}
				}

				var httpContent = new StringContent(content.ToString(), Encoding.UTF8, "application/json");
				var response = await httpClient.PostAsync(Options.TokenUrl, httpContent, cancellationToken);

				// TODO: emit a specialized exception here
				response.EnsureSuccessStatusCode();

				var responseContent = await response.Content.ReadAsStringAsync();
				var authResponse = JObject.Parse(responseContent);

				if (!authResponse.TryGetValue("access_token", out var accessToken))
					throw new InvalidOperationException("The provider returned no 'access_token' in the response");

				if (!authResponse.TryGetValue("token_type", out var tokenType))
					throw new InvalidOperationException("The provider returned no 'token_type' in the response");

				bool? cacheAllowed = null;

				if (response.Headers.TryGetValues("Cache-Control", out var cacheControl)) cacheAllowed = !string.Equals(cacheControl.ToString(), "no-store");

				return new OAuthAccessToken(tokenType.ToString(), accessToken.ToString(), authResponse.Value<int?>("expires_in")) {
					RefreshToken = authResponse.Value<string>("refresh_token"),
					CacheAllowed = cacheAllowed
				};
		}

		public void Dispose() {
			if (disposeClient)
				httpClient?.Dispose();
		}
	}
}
