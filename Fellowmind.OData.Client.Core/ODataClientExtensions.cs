using Microsoft.OData.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Fellowmind.OData.Client.Core
{
    /// <summary>
    /// Contains useful extension methods for ODataClient.
    /// </summary>
    public static class ODataClientExtensions
    {
        public static DataServiceQuery<TEntity> ToODataDTS<TEntity>(this IQueryable<TEntity> query) where TEntity : class
        {
            return (DataServiceQuery<TEntity>)query;
        }

        public static async Task<IEnumerable<TEntity>> ExecuteODataQueryAsync<TEntity>(this IQueryable<TEntity> query) where TEntity : class
        {
            return (await query.ToODataDTS().ExecuteAsync().ConfigureAwait(false)).ToList();
        }

        public static async Task<IEnumerable<TEntity>> ExecuteODataQueryAsync<TEntity>(this IQueryable<TEntity> query, CancellationToken cancellationToken) where TEntity : class
        {
            return (await query.ToODataDTS().ExecuteAsync(cancellationToken).ConfigureAwait(false)).ToList();
        }

        public static DataServiceQuery<TEntity> WhereContains<TEntity>(this DataServiceQuery<TEntity> query, string fieldName, IEnumerable<string> values) where TEntity : class
        {
            // Contains is not supported by Mircosoft.ODataClient so we need to build it ourselves
            var filter = string.Join(" or ", values.Select(x => $"({fieldName} eq '{x}')"));
            return query.AddQueryOption("$filter", filter);
        }

        /// <summary>
        /// Sets target company for Dynamics Finance &amp; Operations GET requests.
        /// </summary>
        /// <remarks>By default, Dynamics F&amp;O OData requests return data only for the company set to the integration user.</remarks>
        /// <param name="query">The query.</param>
        /// <param name="companyId">Empty or null string will return data for all companies. Giving a company (DataAreaId) will target queries to data of that company.</param>
        public static DataServiceQuery<TEntity> WithTargetCompany<TEntity>(this DataServiceQuery<TEntity> query, string companyId) where TEntity : class
        {
            if (!string.IsNullOrEmpty(companyId))
            {
                query = query.AddQueryOption("dataAreaId", companyId);
            }

            return query;
        }

        /// <summary>
        /// Sets cross company for Dynamics Finance &amp; Operations requests.
        /// </summary>
        /// <remarks>By default, Dynamics F&amp;O OData requests return data only for the company set to the integration user.</remarks>
        /// <returns>An entity received using 'cross-company=true' cannot be updated or deleted in F&amp;O. Get the entity to update using 'WithTargetCompany'.</returns>
        /// <param name="query">The query.</param>
        public static DataServiceQuery<TEntity> WithCrossCompany<TEntity>(this DataServiceQuery<TEntity> query) where TEntity : class
        {
            query = query.AddQueryOption("cross-company", "true");
            return query;
        }

        /// <summary>
        /// Parses the guid from an identity uri.
        /// </summary>
        /// <param name="identityUri">The identity uri of an entity.</param>
        /// <returns></returns>
        public static Guid ParseGuidFromIdentity(Uri identityUri)
        {
            return new Guid(identityUri.AbsolutePath.Substring(identityUri.AbsolutePath.Length - 36 - 1, 36));
        }
    }
}
