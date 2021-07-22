using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Moq.Protected;
using Moq;

using Newtonsoft.Json.Linq;

using Xunit;

namespace Deveel.Authentication {
	public class ClientCredentialsTests : IDisposable {
		private HttpClient GetClient(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> callback) {
			var handler = new Mock<HttpMessageHandler>();
			handler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
					ItExpr.IsAny<HttpRequestMessage>(),
					ItExpr.IsAny<CancellationToken>())
			.Returns((HttpRequestMessage request, CancellationToken token) => callback(request, token));

			return new HttpClient(handler.Object);
		}


		[Fact]
		public async Task SuccessfulAuthentication() {
			var httpClient = GetClient(async (request, token) => {
				return new HttpResponseMessage(System.Net.HttpStatusCode.OK) {
					Content = new StringContent(JObject.FromObject(new {
						access_token = "<test token content>",
						token_type = "Bearer",
						expires_in = 200300
					}).ToString(), Encoding.UTF8, "application/json")
				};
			});

			var authClient = new OAuthAuthenticationClient(httpClient, new OAuthClientOptions { TokenUrl = "https://secure.example.com/token" });
			var authResponse = await authClient.RequestAccessTokenAsync(new OAuthClientCredentialsRequest(Guid.NewGuid().ToString(), Guid.NewGuid().ToString("N")) {
				Scopes = new[] { "read:user" }
			});

			Assert.NotNull(authResponse);
			Assert.Equal("<test token content>", authResponse.AccessToken);
			Assert.Equal("Bearer", authResponse.TokenType);
		}

		public void Dispose() {
		}
	}
}
