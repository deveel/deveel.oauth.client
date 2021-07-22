using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

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

		public Uri GetAuthorizationUrl(OAuthAuthenticationCodeInfo authenticationInfo) {
			var uriBuilder = new UriBuilder(Options.AuthorizeUrl);

			var queryString = new StringBuilder();
			queryString.Append($"client_id={authenticationInfo.ClientId}");
			queryString.Append("&response_type=code");
			queryString.Append($"&redirect_uri={HttpUtility.UrlEncode(authenticationInfo.RedirectUri.ToString())}");

			if (!String.IsNullOrWhiteSpace(authenticationInfo.State))
				queryString.Append($"&state={authenticationInfo.State}");

			if (authenticationInfo.Scopes != null && authenticationInfo.Scopes.Count > 0)
				queryString.Append($"&scope={String.Join(" ", authenticationInfo.Scopes)}");

			uriBuilder.Query = queryString.ToString();

			return uriBuilder.Uri;
		}

		public async Task<OAuthAccessToken> RequestAccessTokenAsync(OAuthGrantRequest request, CancellationToken cancellationToken = default) {
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

		private Task<OAuthAccessToken> RequestTokenAsync(OAuthGrantRequest request, CancellationToken cancellationToken) {
			try {
				switch (request.GrantType) {
					case OAuthGrantType.ClientCredentials: {
							if (!(request is OAuthClientCredentialsRequest clientCredentials))
								throw new ArgumentException("The request is invalid: must be client_credentials");

							return RequestWithClientCredentials(clientCredentials, cancellationToken);
						}
					case OAuthGrantType.RefreshToken: {
							throw new NotImplementedException();
						}
					case OAuthGrantType.AuthorizationCode:
						if (!(request is OAuthAuthorizationCodeRequest authorizationCode))
							throw new ArgumentException("The request is invalid: must be an authorization_code");

						return RequestWithAuthorizationCode(authorizationCode, cancellationToken);

					default:
						throw new NotSupportedException("Grant type not supported");
				}
			} catch (OAuthAuthenticationException) {
				throw;
			} catch(Exception ex) {
				throw new OAuthAuthenticationException("Could not authenticate because of an unknown error", ex);
			}
		}

		private async Task<OAuthAccessToken> GetAccessTokenAsync(object requestBody, CancellationToken cancellationToken) {
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
			if (response.StatusCode != System.Net.HttpStatusCode.OK)
				throw new OAuthAuthenticationException($"The server responded with an error: {response.StatusCode}: {response.ReasonPhrase}");

			var responseContent = await response.Content.ReadAsStringAsync();
			var authResponse = JObject.Parse(responseContent);

			if (!authResponse.TryGetValue("access_token", out var accessToken))
				throw new OAuthAuthenticationException("The provider returned no 'access_token' in the response");

			if (!authResponse.TryGetValue("token_type", out var tokenType))
				throw new OAuthAuthenticationException("The provider returned no 'token_type' in the response");

			bool? cacheAllowed = null;

			if (response.Headers.TryGetValues("Cache-Control", out var cacheControl)) cacheAllowed = !string.Equals(cacheControl.ToString(), "no-store");

			return new OAuthAccessToken(tokenType.ToString(), accessToken.ToString(), authResponse.Value<int?>("expires_in")) {
				RefreshToken = authResponse.Value<string>("refresh_token"),
				CacheAllowed = cacheAllowed
			};

		}

		private Task<OAuthAccessToken> RequestWithAuthorizationCode(OAuthAuthorizationCodeRequest authorizationCode, CancellationToken cancellationToken) {
			var requestBody = new {
				grant_type = "authorization_code",
				code = authorizationCode.Code,
				redirect_uri = authorizationCode.RedirectUri.ToString(),
				client_id = authorizationCode.ClientId,
				client_secret = authorizationCode.ClientSecret
			};

			return GetAccessTokenAsync(requestBody, cancellationToken);
		}

		private Task<OAuthAccessToken> RequestWithClientCredentials(OAuthClientCredentialsRequest clientCredentials, CancellationToken cancellationToken) {
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

			return GetAccessTokenAsync(requestBody, cancellationToken);
		}

		public void Dispose() {
			if (disposeClient)
				httpClient?.Dispose();
		}
	}
}
