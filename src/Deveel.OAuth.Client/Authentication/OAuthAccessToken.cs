using System;
using System.Diagnostics;

using Newtonsoft.Json;

namespace Deveel.Authentication {
	[DebuggerDisplay("{TokenType} {AccessToken}")]
	public sealed class OAuthAccessToken {
		public OAuthAccessToken(string tokenType, string accessToken, int? expiresIn = null) {
			if (string.IsNullOrWhiteSpace(tokenType))
				throw new ArgumentException($"'{nameof(tokenType)}' cannot be null or whitespace.", nameof(tokenType));
			if (string.IsNullOrWhiteSpace(accessToken))
				throw new ArgumentException($"'{nameof(accessToken)}' cannot be null or whitespace.", nameof(accessToken));

			TokenType = tokenType;
			AccessToken = accessToken;
			ExpiresIn = expiresIn;

			if (expiresIn != null) {
				Expiration = TimeSpan.FromSeconds(expiresIn.Value);
				ExpiresAt = DateTime.Now.AddSeconds(expiresIn.Value);
			}
		}

		[JsonProperty("expires_in")]
		public int? ExpiresIn { get; }

		[JsonIgnore]
		public TimeSpan? Expiration { get; internal set; }

		[JsonProperty("expires_at")]
		public DateTime? ExpiresAt { get; internal set; }

		[JsonIgnore]
		public bool HasExpiration => Expiration != null;

		[JsonProperty("scope")]
		public string Scope { get; set; }

		[JsonProperty("token_type", Required = Required.Always)]
		public string TokenType { get; }

		[JsonProperty("access_token", Required = Required.Always)]
		public string AccessToken { get; }

		[JsonProperty("refresh_token")]
		public string RefreshToken { get; set; }

		public bool? CacheAllowed { get; set; }

		private string GetDebuggerDisplay() {
			return ToString();
		}
	}
}
