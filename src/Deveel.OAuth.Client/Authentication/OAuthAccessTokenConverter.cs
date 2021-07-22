using System;
using System.Linq;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Deveel.Authentication {
	public sealed class OAuthAccessTokenConverter : JsonConverter<OAuthAccessToken> {
		public override OAuthAccessToken ReadJson(JsonReader reader, Type objectType, OAuthAccessToken existingValue, bool hasExistingValue, JsonSerializer serializer) {
			var obj = JToken.ReadFrom(reader);

			var tokenType = obj.Value<string>("token_type");
			var accessToken = obj.Value<string>("access_token");
			var expiresIn = obj.Value<int?>("expires_in");
			var expiresAt = obj.Value<DateTime?>("expires_at");

			TimeSpan? expiration = null;

			if (expiresIn != null)
				expiration = TimeSpan.FromSeconds(expiresIn.Value);

			var scope = obj.Value<string>("scope");
			var refreshToken = obj.Value<string>("refresh_token");

			return new OAuthAccessToken(tokenType, accessToken, expiresIn) {
				ExpiresAt = expiresAt,
				Expiration = expiration,
				RefreshToken = refreshToken,
				Scope = scope
			};
		}

		public override void WriteJson(JsonWriter writer, OAuthAccessToken value, JsonSerializer serializer) {
			if (value != null) {
				var json = JObject.FromObject(value);
				json.WriteTo(writer, serializer.Converters?.ToArray());
			}
		}
	}
}
