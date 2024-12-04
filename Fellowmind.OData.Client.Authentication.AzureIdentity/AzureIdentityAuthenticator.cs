using System;
using System.Threading;
using System.Threading.Tasks;
using Azure.Core;
using Azure.Identity;

namespace Fellowmind.OData.Client.Authentication.AzureIdentity
{
    /// <summary>
    /// Authenticator that uses Azure.Identity library and managed identities.
    /// https://docs.microsoft.com/en-us/dotnet/api/overview/azure/identity-readme
    /// </summary>
    public class AzureIdentityAuthenticator : IAzureIdentityAuthenticator
    {
        private readonly TokenCredential _tokenCredential;
        private readonly TokenRequestContext _tokenRequestContext;

        /// <summary>
        /// Initializes a new instance of <see cref="AzureIdentityAuthenticator"/>.
        /// By default this implementation uses <see cref="ChainedTokenCredential"/> that consists of <see cref="ManagedIdentityCredential"/>, <see cref="EnvironmentCredential"/> and <see cref="DefaultAzureCredential"/>.
        /// </summary>
        /// <param name="settings">The ODataClient settings object.</param>
        public AzureIdentityAuthenticator(ODataClientSettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            _tokenCredential = new ChainedTokenCredential(
                new EnvironmentCredential(),
                new ManagedIdentityCredential(string.IsNullOrEmpty(settings.ManagedIdentityId) ? null : settings.ManagedIdentityId),
                new DefaultAzureCredential()
                );
            
            string tokenRequestScope = settings.ResourceUri + "/.default";
            _tokenRequestContext = new TokenRequestContext(scopes: new[] { tokenRequestScope }) { };
        }

        /// <summary>
        /// Initializes a new instance of <see cref="AzureIdentityAuthenticator"/>.
        /// </summary>
        /// <param name="credential">The <see cref="TokenCredential"/> to use.</param>
        /// <param name="settings">The ODataClient settings object.</param>
        public AzureIdentityAuthenticator(ODataClientSettings settings, TokenCredential credential)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            _tokenCredential = credential ?? throw new ArgumentNullException(nameof(credential));

            string tokenRequestScope = settings.ResourceUri + "/.default";
            _tokenRequestContext = new TokenRequestContext(scopes: new[] { tokenRequestScope }) { };
        }

        public async Task<string> GetAccessTokenAsync()
        {
            var accessToken = await _tokenCredential.GetTokenAsync(_tokenRequestContext, CancellationToken.None).ConfigureAwait(false);

            return accessToken.Token;
        }
    }
}
