using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

namespace SalesApp
{
    public class Startup
    {
        public static string ClientId;
        public static string ClientSecret;
        public static string Authority;
        public static string OrderServiceResourceId;
        public static string InventoryServiceResourceId;
        public static string GraphResourceId;
        public static string Tenant;

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

            if (env.IsDevelopment())
            {
                // For more details on using the user secret store see http://go.microsoft.com/fwlink/?LinkID=532709
                builder.AddUserSecrets();
            }
            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddMvc();

            services.AddAuthentication(
                SharedOptions => SharedOptions.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme);

            services.Configure<MvcOptions>(options =>
            {
                options.Filters.Add(new RequireHttpsAttribute());
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            Authority = String.Format(Configuration["Authentication:AzureAd:AadInstance"], Configuration["Authentication:AzureAd:Tenant"]);
            ClientId = Configuration["Authentication:AzureAd:ClientId"];
            ClientSecret = Configuration["Authentication:AzureAd:ClientSecret"];
            OrderServiceResourceId = Configuration["Authentication:AzureAd:OrderServiceResourceId"];
            InventoryServiceResourceId = Configuration["Authentication:AzureAd:InventoryServiceResourceId"];
            GraphResourceId = Configuration["Authentication:AzureAd:GraphResourceId"];
            Tenant = Configuration["Authentication:AzureAd:Tenant"];

            app.UseCookieAuthentication();

            var options = new OpenIdConnectOptions
            {
                ClientId = ClientId,
                Authority = Authority,
                CallbackPath = Configuration["Authentication:AzureAd:CallbackPath"],
                ResponseType = OpenIdConnectResponseType.CodeIdToken,
                Events = new OpenIdConnectEvents
                {
                    OnAuthorizationCodeReceived = OnAuthorizationCodeReceived,
                },
                TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                {
                     NameClaimType = "name"
                }
            };

            app.UseOpenIdConnectAuthentication(options);

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }

        private async Task OnAuthorizationCodeReceived(AuthorizationCodeReceivedContext context)
        {
            ClientCredential clientCred = new ClientCredential(ClientId, ClientSecret);
            AuthenticationContext authContext = new AuthenticationContext(Authority);
            AuthenticationResult authResult = await authContext.AcquireTokenByAuthorizationCodeAsync(
                context.ProtocolMessage.Code, new Uri(context.Properties.Items[OpenIdConnectDefaults.RedirectUriForCodePropertiesKey]), clientCred, GraphResourceId);

            // Notify the OIDC middleware that we already took care of code redemption.
            context.HandleCodeRedemption();
        }
    }
}
