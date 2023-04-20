using System;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using Xunit;
using Fellowmind.OData.Test.Models;

namespace Fellowmind.OData.Tests
{
    public class FOCompanyFilterTests : TestBase
    {
        [Fact]
        public async Task RequestNotModifiedWhenFeatureIsOff()
        {
            using (TestODataClient client = ServiceProvider.GetService<TestODataClient>())
            {
                client.SetDynamicsOperationsTargetCompany(null);

                string requestUri = string.Empty;
                client.GetDataContext().SendingRequest2 += (sender, args) => { requestUri = args.RequestMessage.Url.AbsoluteUri; };
                Uri uri = new Uri(BaseUrl);
                await client.FakeExecuteAsync(uri);

                Assert.Equal(BaseUrl, requestUri);
            }
        }

        [Fact]
        public async Task RequestModifiedCorrectlyWhenNoPreviousParameters()
        {
            using (TestODataClient client = ServiceProvider.GetService<TestODataClient>())
            {
                client.SetDynamicsOperationsTargetCompany("area");

                string requestUri = string.Empty;
                client.GetDataContext().SendingRequest2 += (sender, args) => { requestUri = args.RequestMessage.Url.AbsoluteUri; };
                Uri uri = new Uri(BaseUrl);
                await client.FakeExecuteAsync(uri);

                Assert.Equal(BaseUrl + "?$filter=dataAreaId%20eq%20'area'&cross-company=true", requestUri);
            }
        }

        [Fact]
        public async Task RequestModifiedCorrectlyWhenPreviousFilterExists()
        {
            using (TestODataClient client = ServiceProvider.GetService<TestODataClient>())
            {
                client.SetDynamicsOperationsTargetCompany("area");

                string requestUri = string.Empty;
                client.GetDataContext().SendingRequest2 += (sender, args) => { requestUri = args.RequestMessage.Url.AbsoluteUri; };
                Uri uri = new Uri(BaseUrl + "?$filter=myField eq '1' and myField eq 'jee'");
                await client.FakeExecuteAsync(uri);

                Assert.Equal(BaseUrl + "?$filter=myField%20eq%20'1'%20and%20myField%20eq%20'jee'%20and%20dataAreaId%20eq%20'area'&cross-company=true", requestUri);
            }
        }

        [Fact]
        public async Task RequestModifiedCorrectlyForAllCompanies()
        {
            using (TestODataClient client = ServiceProvider.GetService<TestODataClient>())
            {
                client.SetDynamicsOperationsTargetCompany(string.Empty);

                string requestUri = string.Empty;
                client.GetDataContext().SendingRequest2 += (sender, args) => { requestUri = args.RequestMessage.Url.AbsoluteUri; };
                Uri uri = new Uri(BaseUrl + "TestEntity" + "?$filter=amount eq '1' and name eq 'jee'");
                await client.FakeExecuteAsync(uri);

                Assert.Equal(BaseUrl + "TestEntity?$filter=amount%20eq%20'1'%20and%20name%20eq%20'jee'&cross-company=true", requestUri);
            }
        }

        [Fact]
        public async Task RequestModifiedCorrectlyForAllCompaniesWhenNoExistingParameters()
        {
            using (TestODataClient client = ServiceProvider.GetService<TestODataClient>())
            {
                client.SetDynamicsOperationsTargetCompany(string.Empty);

                string requestUri = string.Empty;
                client.GetDataContext().SendingRequest2 += (sender, args) => { requestUri = args.RequestMessage.Url.AbsoluteUri; };
                Uri uri = new Uri(BaseUrl);
                await client.FakeExecuteAsync(uri);

                Assert.Equal(BaseUrl + "?&cross-company=true", requestUri);
            }
        }

        [Fact]
        public async Task RequestModifiedCorrectlyForDelete()
        {
            using (TestODataClient client = ServiceProvider.GetService<TestODataClient>())
            {
                client.SetDynamicsOperationsTargetCompany(string.Empty);

                string requestUri = string.Empty;
                client.GetDataContext().SendingRequest2 += (sender, args) => { requestUri = args.RequestMessage.Url.AbsoluteUri; };
                Uri uri = new Uri(BaseUrl);

                TestEntity entity = await client.GetEntityAndTrackAsync<TestEntity>(Guid.Parse("00000000-0000-0000-0000-100000000000"));
                client.DeleteEntity(entity);
                await client.SaveChangesAsync(Client.Core.SaveOptions.PostOnlySetProperties);

                Assert.Equal(BaseUrl + "TestEntities(00000000-0000-0000-0000-100000000000)?&cross-company=true", requestUri);
            }
        }
    }
}
