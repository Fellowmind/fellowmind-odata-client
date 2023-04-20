using System.Threading.Tasks;

namespace Fellowmind.OData.Client.Core
{
    /// <summary>
    /// An authenticator that only holds an access token.
    /// </summary>
    public class AccessTokenAuthenticator : IODataClientAuthenticator
    {
        private string _accessToken;

        /// <summary>
        /// Initializes a new instance of the <see cref="AccessTokenAuthenticator"/> class.
        /// </summary>
        public AccessTokenAuthenticator()
        { 
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AccessTokenAuthenticator"/> class.
        /// </summary>
        /// <param name="accessToken">The access token to store.</param>
        public AccessTokenAuthenticator(string accessToken)
        {
            _accessToken = accessToken;
        }

        /// <summary>
        /// Sets the access token.
        /// </summary>
        /// <param name="accessToken"></param>
        public void SetAccessToken(string accessToken)
        {
            _accessToken = accessToken;
        }

        /// <summary>
        /// Gets the access token.
        /// </summary>
        /// <returns></returns>
        public string GetAccessToken()
        {
            return _accessToken;
        }

        /// <summary>
        /// Gets the access token.
        /// </summary>
        /// <returns></returns>
        public Task<string> GetAccessTokenAsync()
        {
            return Task.FromResult(_accessToken);
        }
    }
}
