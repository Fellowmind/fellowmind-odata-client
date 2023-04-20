using System.Threading.Tasks;

namespace Fellowmind.OData.Client.Core
{
    /// <summary>
    /// Defines an interface for authentication libraries.
    /// </summary>
    public interface IODataClientAuthenticator
    {
        /// <summary>
        /// Gets the access token. Requests for a new token if neccessary.
        /// </summary>
        /// <returns></returns>
        Task<string> GetAccessTokenAsync();
    }
}
