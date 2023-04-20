using System;
using System.Linq;
using Fellowmind.OData.Test.Models;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Routing.Conventions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Fellowmind.OData.TestServer
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<Context>(optionsAction => optionsAction.UseInMemoryDatabase(Guid.NewGuid().ToString()), ServiceLifetime.Singleton);
            services.AddOData();
            services.AddMvc(options => 
            {
                options.EnableEndpointRouting = false;
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            var builder = new ODataConventionModelBuilder(app.ApplicationServices);
            builder.EntitySet<TestEntity>("TestEntities");

            app.UseMvc(routeBuilder =>
            {
                // Enable full OData queries, you might want to consider which would be actually enabled in production scenaries
                routeBuilder.Count().Filter().OrderBy().Expand().Select().MaxTop(null);

                // Create the default collection of built-in conventions.
                var conventions = ODataRoutingConventions.CreateDefault();

                // Insert the custom convention at the start of the collection.
                //conventions.Insert(0, new NavigationIndexRoutingConvention());

                routeBuilder.MapODataServiceRoute("odata", null, builder.GetEdmModel());
            });
        }
    }
}
