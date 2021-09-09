using System;

namespace Deveel.Authentication {
	public class OAuthClientOptions {
		public string ClientId { get; set; }

		public string ClientSecret { get; set; }

		public string Audience { get; set; }

		public string[] Scopes { get; set; }

		public string TokenUrl { get; set; }

		public string AuthorizeUrl { get; set; }

		// 1 day default expiration
		public int DefaultTokenExpiration { get; set; } = 60 * 60 * 24;
	}
}
