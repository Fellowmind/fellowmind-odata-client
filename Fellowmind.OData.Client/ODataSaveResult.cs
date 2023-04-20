using Fellowmind.OData.Client.Core;
using Microsoft.OData.Client;
using System;
using System.Collections.Generic;

namespace Fellowmind.OData.Client
{
    /// <summary>
    /// Implements <see cref="IODataSaveResult"/>.
    /// </summary>
    public class ODataSaveResult : IODataSaveResult
    {
        /// <summary>
        /// Creates an instance of <see cref="ODataSaveResult"/>.
        /// </summary>
        /// <param name="success">A value indicating if the operation was successful.</param>
        /// <param name="response">A <see cref="DataServiceResponse"/> from OData client.</param>
        public ODataSaveResult(bool success, DataServiceResponse response)
        {
            Success = success;
            Response = response;
        }

        /// <summary>
        /// Creates an instance of <see cref="ODataSaveResult"/>.
        /// </summary>
        /// <param name="success">A value indicating if the operation was successful.</param>
        /// <param name="response">A <see cref="DataServiceResponse"/> from OData client.</param>
        /// <param name="entityResults">Save operation entity results.</param>
        public ODataSaveResult(bool success, DataServiceResponse response, IDictionary<BaseEntityType, Uri> entityResults)
            : this(success, response)
        {
            EntityResults = entityResults;
        }

        /// <inheritdoc />
        public DataServiceResponse Response { get; }

        /// <inheritdoc />
        public bool Success { get; }

        /// <inheritdoc />
        public IDictionary<BaseEntityType, Uri> EntityResults { get; }
    }
}
