using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Default;
using Fellowmind.OData.Test.Models;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Fellowmind.OData.Tests
{
    public class SelectQueryTests : TestBase
    {
        [Fact]
        public async Task Entities_CanBeModified_AfterSelectQuery()
        {
            TestODataClient client = ServiceProvider.GetService<TestODataClient>();
            IEnumerable<TestEntity> entities = await client.GetTestEntitiesFiltered();

            Container context = client.GetDataContext();
            Assert.Equal(entities.Count(), context.Entities.Count);

            foreach(TestEntity entity in entities)
            {
                client.TrackEntity(entity);
                entity.Name = "Jeejee";
            }

            Assert.True(context.Entities.All(x => x.State == Microsoft.OData.Client.EntityStates.Modified));
            Assert.Equal(context.Entities.Count, entities.Count());
        }
    }
}
