using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using System.Diagnostics;

using OpenIdConnectSSO.Client.ViewModels;

namespace OpenIdConnectSSO.Client.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index() => View();


        [Authorize]
        public IActionResult Privacy() => View();


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
