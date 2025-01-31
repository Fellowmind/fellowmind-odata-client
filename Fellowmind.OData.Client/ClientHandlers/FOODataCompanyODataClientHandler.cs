using System.Collections.Generic;
using Fellowmind.OData.Client.Core.Extensions;
using Microsoft.OData.Client;
using Microsoft.OData.Extensions.Client;

namespace Fellowmind.OData.Client.ClientHandlers
{
    /// <summary>
    /// Register this handler to add cross-company parameter to the query.
    /// </summary>
    public class FOODataCompanyODataClientHandler : IODataClientHandler
    {
        // operations that might need cross-company paramter. POST is on the list because actions are executed that way
        private static readonly HashSet<string> CrossCompanyMethods = new HashSet<string> { "POST", "PUT", "PATCH", "GET", "MERGE", "DELETE" };

        public void OnClientCreated(ClientCreatedArgs args)
        {
            var context = args.ODataClient;

            context.BuildingRequest += OnContextOnBuildingRequest;
        }

        private void OnContextOnBuildingRequest(object sender, BuildingRequestEventArgs e)
        {
            if (CrossCompanyMethods.Contains(e.Method))
            {
                e.RequestUri = e.RequestUri.AddOrUpdateParameter("cross-company", "true");
            }
        }
    }
}