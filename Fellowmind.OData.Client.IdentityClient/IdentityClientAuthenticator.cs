using Microsoft.Identity.Client;
using System;
using System.Threading.Tasks;

namespace Fellowmind.OData.Client.Authentication.IdentityClient
{
    public class IdentityClientAuthenticator : IIdentityClientAuthenticator
    {
        private readonly IConfidentialClientApplication _app;
        private readonly string[] _scopes;
        private AuthenticationResult _authenticationResult;

        public IdentityClientAuthenticator(ODataClientSettings settings)
        {
            var settings1 = settings ?? throw new ArgumentNullException(nameof(settings));

            ConfidentialClientApplicationOptions applicationOptions = new ConfidentialClientApplicationOptions
            {
                ClientId = settings1.ClientId,
                ClientSecret = settings1.ClientSecret,
                ClientName = "Fellowmind OData Client",
                ClientVersion = "1.0.0",
                AzureCloudInstance = AzureCloudInstance.AzurePublic,
                TenantId = settings1.TenantId,
                RedirectUri = "https://login.microsoftonline.com/common/oauth2/nativeclient"
            };

            _app = ConfidentialClientApplicationBuilder.CreateWithApplicationOptions(applicationOptions).Build();
            _scopes = new[] { settings1.ResourceUri + "/.default" };
        }

        public async Task<string> GetAccessTokenAsync()
        {
            try
            {
                _authenticationResult = await _app.AcquireTokenForClient(_scopes).ExecuteAsync().ConfigureAwait(false);
            }
            catch (Exception e)
            {
                throw new UnauthorizedAccessException("Failed to authenticate with AAD by application.", e);
            }

            return _authenticationResult.AccessToken;
        }
    }
}
