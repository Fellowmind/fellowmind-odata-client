using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Fellowmind.OData.Client.Core
{
    /// <summary>
    /// A <see cref="DelegatingHandler"/> for <see cref="IODataClientAuthenticator"/> that sets the authorization token to requests.
    /// </summary>
    public class AuthenticationHandler : DelegatingHandler
    {
        private readonly IODataClientAuthenticator _odataClientAuthenticator;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthenticationHandler"/> class.
        /// </summary>
        /// <param name="odataClientAuthenticator">An instance of a <see cref="IODataClientAuthenticator"/>.</param>
        public AuthenticationHandler(IODataClientAuthenticator odataClientAuthenticator)
        {
            _odataClientAuthenticator = odataClientAuthenticator ?? throw new ArgumentNullException(nameof(odataClientAuthenticator), "The authenticator was null!");
        }

        /// <inheritdoc />
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
        {
            string bearerToken = await _odataClientAuthenticator.GetAccessTokenAsync().ConfigureAwait(false);

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
