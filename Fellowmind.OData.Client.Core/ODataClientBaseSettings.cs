using System;

namespace Fellowmind.OData.Client.Core
{
    /// <summary>
    /// The base class for ODataClient settings.
    /// </summary>
    public class ODataClientBaseSettings : IODataClientBaseSettings
    {
        /// <summary>
        /// Initializes a new instance of <see cref="ODataClientBaseSettings"/>
        /// </summary>
        /// <param name="resourceApiUri"></param>
        public ODataClientBaseSettings(string resourceApiUri)
        {
            if (string.IsNullOrEmpty(resourceApiUri))
            {
                throw new ArgumentNullException(nameof(resourceApiUri));
            }

            // Make sure resource API uri doesn't contain '/' in the end. This
            // will fail authentication with FO.
            ResourceApiUri = resourceApiUri.TrimEnd('/');
            ResourceUri = new Uri(resourceApiUri).GetLeftPart(UriPartial.Authority).TrimEnd('/');
        }

        ///<inheritdoc/>
        public string ResourceApiUri { get; }

        ///<inheritdoc/>
        public string ResourceUri { get; protected set; }
    }
}
