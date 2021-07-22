using System;

namespace Deveel.Authentication {
	public sealed class OAuthRefreshTokenRequest : OAuthGrantRequest {
		public OAuthRefreshTokenRequest(string refreshToken) {
			if (string.IsNullOrWhiteSpace(refreshToken))
				throw new ArgumentException($"'{nameof(refreshToken)}' cannot be null or whitespace.", nameof(refreshToken));

			RefreshToken = refreshToken;
		}

		public override OAuthGrantType GrantType => OAuthGrantType.RefreshToken;

		public string RefreshToken { get; }
	}
}
