using Fellowmind.OData.Test.Models;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Fellowmind.OData.Tests
{
    public class ODataClientTests : TestBase
    {
        [Fact]
        public async Task GetEntities_ReturnsEntities()
        {
            TestODataClient client = ServiceProvider.GetService<TestODataClient>();
            IEnumerable<TestEntity> entities = await client.GetTestEntities();
            Assert.NotNull(entities);
        }

        [Fact]
        public async Task GetEntity_WhenNotFound_ReturnsNull()
        {
            TestODataClient client = ServiceProvider.GetService<TestODataClient>();

            TestEntity entity = await client.GetEntityAsync<TestEntity>(Guid.Parse("10000000-0000-0000-0000-100000000000"));
            Assert.Null(entity);
        }

        [Fact]
        public async Task DeleteEntity_DeletesEntity()
        {
            TestODataClient client = ServiceProvider.GetService<TestODataClient>();

            TestEntity entity = await client.GetEntityAsync<TestEntity>(Guid.Parse("00000000-0000-0000-0000-100000000000"));
            client.DeleteEntity(entity);

            await client.SaveChangesAsync(Client.Core.SaveOptions.None);

            entity = await client.GetEntityAsync<TestEntity>(Guid.Parse("00000000-0000-0000-0000-100000000000"));
            Assert.Null(entity);
        }

        [Fact]
        public async Task Saving_WithoutChanges_Succeeds()
        {
            TestODataClient client = ServiceProvider.GetService<TestODataClient>();

            // This loads one entity to context.
            TestEntity entity = await client.GetEntityAsync<TestEntity>(Guid.Parse("00000000-0000-0000-0000-100000000000"));

            await client.SaveChangesAsync();
        }
    }
}
