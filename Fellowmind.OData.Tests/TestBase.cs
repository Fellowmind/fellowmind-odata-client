using Default;
using Fellowmind.OData.Client.Core;
using Fellowmind.OData.Tests.Server;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.Extensions.Client;
using System;
using System.Net.Http;

namespace Fellowmind.OData.Tests
{
    public class TestBase
    {
        protected IServiceProvider ServiceProvider { get; }
        protected string BaseUrl { get; }

        public TestBase()
        {
            MyWebApplicationFactory webApplication = new MyWebApplicationFactory();
            HttpClient httpClient = webApplication.CreateDefaultClient();
            BaseUrl = httpClient.BaseAddress.AbsoluteUri;

            IServiceCollection services = new ServiceCollection();
            services.AddSingleton<IODataClientBaseSettings>((sp) => new ODataClientBaseSettings(httpClient.BaseAddress.ToString()));
            services.AddSingleton<IODataClientAuthenticator>(sp => new AccessTokenAuthenticator(string.Empty));
            services.AddODataClient().AddHttpClient(httpClient);
            services.AddTransient<TestODataClient>();

            ServiceProvider = services.BuildServiceProvider();
        }
    }
}
