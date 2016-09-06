using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Http.Features.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Net.Http;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System.Net.Http.Headers;

namespace OrderService.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class OrderController : Controller
    {
        // GET api/values
        [HttpGet("placeorder")]
        public async Task<IActionResult> PlaceOrder()
        {
            return new ObjectResult(await GetToken());
        }

        [HttpGet("placeorderv2")]
        public async Task<IActionResult> PlaceOrderV2()
        {
            HttpClient client = new HttpClient();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, GetInventoryUrl() + "/api/inventory/write");

            var token = await GetToken(Startup.InventoryServiceResourceId);

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            HttpResponseMessage response = await client.SendAsync(request);

            var secondToken = await response.Content.ReadAsStringAsync();
            return new ObjectResult(secondToken);
        }

        private string GetInventoryUrl()
        {
            var url = "https://gouvieuxdemoinventoryservice.azurewebsites.net";
#if DEBUG
            url = "https://localhost:44321";
#endif
            return url;
        }

        private async Task<string> GetToken(string resource)
        {
            AuthenticationContext authContext = new AuthenticationContext(Startup.Authority);
            ClientCredential credential = new ClientCredential(Startup.ClientId, Startup.ClientSecret);

            var token = await GetEncodedToken();
            var username = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var userAssertion = new UserAssertion(token, "urn:ietf:params:oauth:grant-type:jwt-bearer", username);

            AuthenticationResult result = await authContext.AcquireTokenAsync(resource, credential, userAssertion);

            return result.AccessToken;
        }

        private async Task<string> GetEncodedToken()
        {
            AuthenticateInfo info = await HttpContext.Authentication.GetAuthenticateInfoAsync(JwtBearerDefaults.AuthenticationScheme);
            return info.Properties.Items[".Token.access_token"];
        }

        private async Task<string> GetToken()
        {
            var tokenString = await GetEncodedToken();
            var tokenHandler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
            var token = tokenHandler.ReadToken(tokenString);

            return token.ToString();
        }
    }
}
