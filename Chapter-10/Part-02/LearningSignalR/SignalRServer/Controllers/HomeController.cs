using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using SignalRHubs;
using SignalRServer.Models;
using System.Diagnostics;

namespace SignalRServer.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly IHubContext<Hub> hubContext;

        public HomeController(IHubContext<Hub> hubContext)
        {
            this.hubContext = hubContext;
        }

        public async Task<IActionResult> Index()
        {
            var accessToken = await HttpContext.GetTokenAsync("access_token");
            Console.WriteLine($"Access token: {accessToken}");
            await hubContext.Clients.All.SendAsync("ReceiveMessage", "Index page has been opened by a client.");

            return View();
        }

        public IActionResult WebAssemblyClient()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult LogOut()
        {
            return new SignOutResult(new[] {CookieAuthenticationDefaults.AuthenticationScheme, "oidc" });
        }
    }
}