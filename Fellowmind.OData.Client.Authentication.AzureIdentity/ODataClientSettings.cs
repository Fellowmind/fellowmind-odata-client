using Fellowmind.OData.Client.Core;

namespace Fellowmind.OData.Client.Authentication.AzureIdentity
{
    public class ODataClientSettings : ODataClientBaseSettings
    {
        /// <summary>
        /// Initializes a new instance of <see cref="ODataClientSettings"/> class.
        /// </summary>
        /// <param name="resourceApiUri">The resource uri ending with api and version.</param>
        public ODataClientSettings(string resourceApiUri)
            : base(resourceApiUri)
        {

        }

        /// <summary>
        /// Initializes a new instance of <see cref="ODataClientSettings"/> class.
        /// </summary>
        /// <param name="resourceApiUri">The resource uri ending with api and version.</param>
        /// <param name="managedIdentityId">ClientId (guid) of the user-assigned managed identity object in Azure. If null then assumes system-assigned managed identity.</param>
        public ODataClientSettings(string resourceApiUri, string managedIdentityId)
            : base(resourceApiUri)
        {
            ManagedIdentityId = managedIdentityId;
        }

        public string ManagedIdentityId { get; }
    }
}
