using Default;
using Fellowmind.OData.Client;
using Fellowmind.OData.Client.Core;
using Fellowmind.OData.Test.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fellowmind.OData.Tests
{
    public class TestODataClient : ODataClient<Container>
    {
        public TestODataClient(Microsoft.OData.Extensions.Client.IODataClientFactory clientFactory, IODataClientBaseSettings settings)
            : base(clientFactory, settings)
        {
           
        }

        public async Task<IEnumerable<TestEntity>> GetTestEntities()
        {
            return await DataContext.TestEntities.ExecuteODataQueryAsync();
        }

        public async Task<IEnumerable<TestEntity>> GetTestEntitiesFiltered()
        {
            return await DataContext.TestEntities.Select(x => new TestEntity()
            {
                Id = x.Id,
                Name = x.Name,
            }).ExecuteODataQueryAsync();
        }

        public Container GetDataContext()
        {
            return DataContext;
        }

        /// <summary>
        /// This method will generate a proper request for url validation, but doesn't successfully execute.
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public async Task FakeExecuteAsync(Uri uri)
        {
            try
            {
                await DataContext.ExecuteAsync<TestEntity>(uri);
            }
            catch { }
        }
    }
}
