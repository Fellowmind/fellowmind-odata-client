using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using Xunit;
using Fellowmind.OData.Test.Models;
using Default;
using System.Linq;

namespace Fellowmind.OData.Tests
{
    /// <summary>
    /// Test working with entities from multiple contexts.
    /// </summary>
    public class MultiClientTests : TestBase
    {
        [Fact]
        public async Task EntityFromContext1_CanBeModified_ThroughContext2()
        {
            TestEntity entity;
            using (TestODataClient client = ServiceProvider.GetService<TestODataClient>())
            {
                entity = await client.GetEntityAsync<TestEntity>(Guid.Parse("00000000-0000-0000-0000-100000000000"));
            }

            using(TestODataClient client = ServiceProvider.GetService<TestODataClient>())
            {
                client.AttachEntityAndTrack(entity);

                Container context = client.GetDataContext();
                Assert.Single(context.Entities);
                Assert.True(context.Entities.All(x => x.State == Microsoft.OData.Client.EntityStates.Unchanged));

                entity.Name = "Test";
                Assert.True(context.Entities.All(x => x.State == Microsoft.OData.Client.EntityStates.Modified));
            }
        }
    }
}
