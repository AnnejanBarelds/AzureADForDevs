using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Http.Features.Authentication;
using Microsoft.AspNetCore.Http.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace InventoryService.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class InventoryController : Controller
    {
        private const string ScopeClaimType = "http://schemas.microsoft.com/identity/claims/scope";

        // GET api/values
        [HttpGet("read")]
        public async Task<IActionResult> Read()
        {
            var token = await GetToken();
            
            return new ObjectResult(token.ToString());
        }

        // GET api/values
        [HttpGet("write")]
        public async Task<IActionResult> Write()
        {
            var token = await GetToken();
            var result = new ObjectResult(token.ToString());
            result.StatusCode = 403;

            // Check for correct scope
            var scopeClaim = User.FindFirst(ScopeClaimType);
            if (scopeClaim != null)
            {
                var scopes = scopeClaim.Value.Split(' ');
                if (scopes.Contains("InventoryService.Write"))
                {
                    result.StatusCode = 200;
                }
            }

            return result;
        }

        [HttpGet("update")]
        public async Task<IActionResult> Update()
        {
            var token = await GetToken();
            var result = new ObjectResult(token.ToString());
            result.StatusCode = 403;

            // Check for role
            if (User.IsInRole("InventoryService.Update"))
            {
                result.StatusCode = null;
            }

            return result;
        }

        private async Task<string> GetToken()
        {
            AuthenticateInfo info = await HttpContext.Authentication.GetAuthenticateInfoAsync(JwtBearerDefaults.AuthenticationScheme);
            var tokenHandler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
            return tokenHandler.ReadToken(info.Properties.Items[".Token.access_token"]).ToString();
        }
    }
}
