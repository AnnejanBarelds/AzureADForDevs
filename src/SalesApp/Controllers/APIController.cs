using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using System.IO;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Authentication.Cookies;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace SalesApp.Controllers
{
    public class APIController : Controller
    {
        // GET: /<controller>/
        public async Task<ActionResult> PlaceOrder()
        {
            var url = GetOrderUrl() + "/api/order/placeorder";

            return await ExecuteCall(url, Startup.OrderServiceResourceId);
        }

        public async Task<ActionResult> ReadInventory()
        {
            var url = GetInventoryUrl() + "/api/inventory/read";

            return await ExecuteCall(url, Startup.InventoryServiceResourceId);
        }

        public async Task<ActionResult> WriteInventory()
        {
            var url = GetInventoryUrl() + "/api/inventory/write";

            return await ExecuteCall(url, Startup.InventoryServiceResourceId);
        }

        public async Task<ActionResult> PlaceOrderV2()
        {
            var url = GetOrderUrl() + "/api/order/placeorderv2";

            return await ExecuteCall(url, Startup.OrderServiceResourceId);
        }

        private async Task<ActionResult> ExecuteCall(string url, string resource)
        {
            HttpClient client = new HttpClient();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);

            var token = await GetToken(resource);
            
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            HttpResponseMessage response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var responseToken = await response.Content.ReadAsStringAsync();
                ViewBag.StatusCode = response.StatusCode;
                ViewBag.Token = TokenPrettify(responseToken);
                return View("Claims");
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                var responseToken = await response.Content.ReadAsStringAsync();
                ViewBag.StatusCode = response.StatusCode;
                ViewBag.Token = TokenPrettify(responseToken);
                return View("Claims");
            }
            else
            {
                return View("Error");
            }
        }

        private async Task<string> GetToken(string resource)
        {
            string userObjectID = (User.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier"))?.Value;
            AuthenticationContext authContext = new AuthenticationContext(Startup.Authority);
            ClientCredential credential = new ClientCredential(Startup.ClientId, Startup.ClientSecret);
            AuthenticationResult result = await authContext.AcquireTokenSilentAsync(resource, credential, new UserIdentifier(userObjectID, UserIdentifierType.UniqueId));
            return result.AccessToken;
        }

        private string GetInventoryUrl()
        {
            string url = "https://localhost:44321";
            return url;
        }

        private string GetOrderUrl()
        {
            string url = "https://localhost:44346";
            return url;
        }

        private static string TokenPrettify(string token)
        {
            var i = token.IndexOf("}.{");
            var headerString = token.Substring(0, i + 1);
            var valueString = token.Substring(i + 2);

            var header = JsonPrettify(headerString);
            var value = JsonPrettify(valueString);

            return header + Environment.NewLine + "." + Environment.NewLine + value;
        }

        private static string JsonPrettify(string json)
        {

            dynamic parsedJson = JsonConvert.DeserializeObject(json);
            return JsonConvert.SerializeObject(parsedJson, Formatting.Indented);
        }
    }
}
