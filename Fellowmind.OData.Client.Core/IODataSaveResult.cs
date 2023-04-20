using Microsoft.OData.Client;
using System;
using System.Collections.Generic;

namespace Fellowmind.OData.Client.Core
{
    /// <summary>
    /// Defines OData save result.
    /// </summary>
    public interface IODataSaveResult
    {
        /// <summary>
        /// Gets the <see cref="DataServiceResponse"/> from OData client.
        /// </summary>
        DataServiceResponse Response { get; }

        /// <summary>
        /// Gets a value indicating whether the operation was successful or not.
        /// </summary>
        bool Success { get; }

        /// <summary>
        /// Save operation entity results.
        /// </summary>
        IDictionary<BaseEntityType, Uri> EntityResults { get; }
    }
}
