using Microsoft.OData.Client;
using System;

namespace Fellowmind.OData.Client.Core
{
    /// <summary>
    /// Event arguments for ContextCreated event.
    /// </summary>
    /// <typeparam name="TClient">DataServiceContext-based OData client context type.</typeparam>
    public class ContextCreatedEventArgs<TClient> : EventArgs where TClient : DataServiceContext
    {
        /// <summary>
        /// Creates an instance of a <see cref="ContextCreatedEventArgs{TClient}"/>.
        /// </summary>
        /// <param name="client">The created OData client context.</param>
        public ContextCreatedEventArgs(TClient client)
        {
            DataContext = client;
        }

        /// <summary>
        /// Gets the created <see cref="DataServiceContext"/>.
        /// </summary>
        public TClient DataContext { get; }
    }
}
