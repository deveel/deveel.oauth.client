using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Deveel.Authentication {
	class TestHttpHandler : HttpMessageHandler {
		private Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> callback;

		public TestHttpHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> callback) {
			this.callback = callback;
		}

		protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) {
			return callback(request, cancellationToken);
		}
	}
}
