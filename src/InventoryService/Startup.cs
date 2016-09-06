using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace InventoryService
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddMvc();

            services.AddAuthentication();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            // Configure the app to use Jwt Bearer Authentication
            app.UseJwtBearerAuthentication(new JwtBearerOptions
            {
                //Events = new JwtBearerEvents
                //{

                //    OnTokenValidated = x => x.Ticket.Properties
                //}
                //AutomaticAuthenticate = true,
                //AutomaticChallenge = true,

                Authority = String.Format(Configuration["Authentication:AzureAd:AadInstance"], Configuration["Authentication:AzureAD:Tenant"]),
                Audience = Configuration["Authentication:AzureAd:Audience"],
                SaveToken = true
            });

            app.UseMvc();
        }
    }
}
