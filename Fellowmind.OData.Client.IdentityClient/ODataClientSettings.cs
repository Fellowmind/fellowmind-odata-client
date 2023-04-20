using System;
using Fellowmind.OData.Client.Core;

namespace Fellowmind.OData.Client.Authentication.IdentityClient
{
    public class ODataClientSettings : ODataClientBaseSettings
    {
        public ODataClientSettings(
            string tenantId,
            string clientId,
            string clientSecret,
            string resourceUri,
            string resourceApiUri)
            : base(resourceApiUri)
        {
            if (string.IsNullOrEmpty(tenantId))
            {
                throw new ArgumentNullException(nameof(tenantId));
            }

            if (string.IsNullOrEmpty(clientId))
            {
                throw new ArgumentNullException(nameof(clientId));
            }

            if (string.IsNullOrEmpty(clientSecret))
            {
                throw new ArgumentNullException(nameof(clientSecret));
            }

            if (string.IsNullOrEmpty(resourceUri))
            {
                throw new ArgumentNullException(nameof(resourceUri));
            }

            TenantId = tenantId;
            ClientId = clientId;
            ClientSecret = clientSecret;

            // Make sure resource uri doesn't contain '/' in the end. This will fail queries with FO.
            ResourceUri = resourceUri.TrimEnd('/');
        }

        public string ClientId { get; }
        public string TenantId { get; }
        public string ClientSecret { get;  }
    }
}
