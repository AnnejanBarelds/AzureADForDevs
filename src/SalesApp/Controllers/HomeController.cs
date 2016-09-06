using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.Azure.ActiveDirectory.GraphClient;
using Microsoft.Azure.ActiveDirectory.GraphClient.Extensions;

namespace SalesApp.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public async Task<ActionResult> ListEmployees()
        {
            var client = GetActiveDirectoryClient();

            var groups = new List<Group>();
            var result = await client.Me.MemberOf.ExecuteAsync();
            await Merge(groups, result);

            if (groups.Any(group => group.DisplayName == "Sales Managers"))
            {
                var users = new List<User>();
                var result2 = await client.Me.DirectReports.ExecuteAsync();
                await Merge(users, result2);
                ViewBag.Employees = string.Join(Environment.NewLine, users.Select(user => user.DisplayName).ToArray());
                return View("Employees");
            }

            return View("AccessDenied");
        }

        public IActionResult SetColor()
        {
            if (User.IsInRole("Admin"))
            {
                ViewBag.IsColorSet = true;
                return View("Index");
            }

            return View("AccessDenied");
        }

        public IActionResult Error()
        {
            return View();
        }

        private ActiveDirectoryClient GetActiveDirectoryClient()
        {
            Uri baseServiceUri = new Uri(Startup.GraphResourceId);
            ActiveDirectoryClient activeDirectoryClient =
                new ActiveDirectoryClient(new Uri(baseServiceUri, Startup.Tenant),
                    async () => await GetToken());
            return activeDirectoryClient;
        }

        private async Task<string> GetToken()
        {
            string userObjectID = (User.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier"))?.Value;
            AuthenticationContext authContext = new AuthenticationContext(Startup.Authority);
            ClientCredential credential = new ClientCredential(Startup.ClientId, Startup.ClientSecret);
            AuthenticationResult result = await authContext.AcquireTokenSilentAsync(Startup.GraphResourceId, credential, new UserIdentifier(userObjectID, UserIdentifierType.UniqueId));
            return result.AccessToken;
        }

        private async Task Merge<T>(List<T> items, IPagedCollection<IDirectoryObject> items2)
        {
            items.AddRange(items2.CurrentPage.Where(item => item is T).Cast<T>());
            if (items2.MorePagesAvailable)
            {
                var nextPage = await items2.GetNextPageAsync();
                await Merge(items, nextPage);
            }
        }
    }
}
