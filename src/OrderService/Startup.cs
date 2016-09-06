using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace OrderService
{
    public class Startup
    {
        public static string ClientId;
        public static string ClientSecret;
        public static string Authority;
        public static string InventoryServiceResourceId;

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

            Authority = String.Format(Configuration["Authentication:AzureAd:AadInstance"], Configuration["Authentication:AzureAd:Tenant"]);
            ClientId = Configuration["Authentication:AzureAd:ClientId"];
            ClientSecret = Configuration["Authentication:AzureAd:ClientSecret"];
            InventoryServiceResourceId = Configuration["Authentication:AzureAd:InventoryServiceResourceId"];

            // Configure the app to use Jwt Bearer Authentication
            app.UseJwtBearerAuthentication(new JwtBearerOptions
            {
                //Events = new JwtBearerEvents
                //{
                    
                //    OnTokenValidated = x => x.Ticket.Properties
                //}
                //AutomaticAuthenticate = true,
                //AutomaticChallenge = true,
                
                Authority = Authority,
                Audience = Configuration["Authentication:AzureAd:Audience"],
                SaveToken = true
            });

            app.UseMvc();
        }
    }
}
