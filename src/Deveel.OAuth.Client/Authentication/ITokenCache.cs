using System;
using System.Threading;
using System.Threading.Tasks;

namespace Deveel.Authentication {
	public interface ITokenCache {
		Task<OAuthAccessToken> GetTokenAsync(string tokenName, CancellationToken cancellationToken);

		Task SetTokenAsync(string tokenName, OAuthAccessToken accessToken, TimeSpan expiration, CancellationToken cancellationToken);

		Task<bool> RemoveTokenAsync(string tokenName, CancellationToken cancellationToken);
	}
}