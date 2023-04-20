using System;
using System.Linq;
using System.Threading.Tasks;
using Fellowmind.OData.Client.Core;
using Fellowmind.OData.Test.Models;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Fellowmind.OData.Tests
{
    public class SaveResultTests : TestBase
    {
        [Fact]
        public async Task NormalSaveOperation_IsSuccessful()
        {
            TestODataClient client = ServiceProvider.GetService<TestODataClient>();
            TestEntity testEntity = await client.GetEntityAndTrackAsync<TestEntity>(Guid.Parse("00000000-0000-0000-0000-100000000000"));
            testEntity.Name = "Moi";

            // Batch save will crash with the test server.
            IODataSaveResult result = await client.SaveChangesAsync(SaveOptions.PostOnlySetProperties);
            Assert.True(result.Success);
            Assert.NotNull(result.Response);
            Assert.Null(result.EntityResults);
            Assert.True(client.GetDataContext().Entities.All(x => x.State == Microsoft.OData.Client.EntityStates.Unchanged));

            testEntity = await client.GetEntityAsync<TestEntity>(Guid.Parse("00000000-0000-0000-0000-100000000000"));
            Assert.Equal("Moi", testEntity.Name);
        }

        [Fact]
        public async Task SaveOperation_WithEntityResults_IsSuccessful()
        {
            TestODataClient client = ServiceProvider.GetService<TestODataClient>();
            TestEntity testEntity = await client.GetEntityAndTrackAsync<TestEntity>(Guid.Parse("00000000-0000-0000-0000-100000000000"));
            testEntity.Name = "Moi";

            // Batch save will crash with the test server.
            IODataSaveResult result = await client.SaveChangesWithEntityResultsAsync(SaveOptions.PostOnlySetProperties);
            Assert.True(result.Success);
            Assert.NotNull(result.Response);
            Assert.NotNull(result.EntityResults);
            Assert.True(client.GetDataContext().Entities.All(x => x.State == Microsoft.OData.Client.EntityStates.Unchanged));

            Assert.Equal("Moi", (result.EntityResults.Single().Key as TestEntity).Name);
            Assert.Contains("00000000-0000-0000-0000-100000000000", result.EntityResults.Single().Value.AbsoluteUri);

            testEntity = await client.GetEntityAsync<TestEntity>(Guid.Parse("00000000-0000-0000-0000-100000000000"));
            Assert.Equal("Moi", testEntity.Name);
        }
    }
}
