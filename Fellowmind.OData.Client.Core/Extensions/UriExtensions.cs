using System;
using System.Linq;
using System.Web;

namespace Fellowmind.OData.Client.Core.Extensions
{
    /// <summary>
    /// Extensions for Uri.
    /// </summary>
    public static class UriExtensions
    {
        /// <summary>
        /// Add or update a parameter in Uri.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="paramName"></param>
        /// <param name="paramValue"></param>
        /// <returns></returns>
        public static Uri AddOrUpdateParameter(this Uri url, string paramName, string paramValue)
        {
            var uriBuilder = new UriBuilder(url);
            var query = HttpUtility.ParseQueryString(uriBuilder.Query);

            if (query.AllKeys.Contains(paramName))
            {
                query[paramName] = paramValue;
            }
            else
            {
                query.Add(paramName, paramValue);
            }

            uriBuilder.Query = query.ToString();

            return uriBuilder.Uri;
        }
    }
}
