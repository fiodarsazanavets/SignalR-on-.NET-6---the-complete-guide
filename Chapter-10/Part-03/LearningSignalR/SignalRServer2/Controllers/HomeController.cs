using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SignalRServer2.Models;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text;

namespace SignalRServer2.Controllers
{
    public class HomeController : Controller
    {
        private readonly string signalRHubUrl;

        public HomeController()
        {
            signalRHubUrl = "https://<instance-name>.service.signalr.net/api/v1/hubs/learningHub";
        }

        public async Task<IActionResult> Index()
        {
            using var client = new HttpClient();

            var payloadMessage = new
            {
                Target = "ReceiveMessage",
                Arguments = new[]
                {
                    "Client connected to a secondary web application"
                }
            };

            var request = new HttpRequestMessage(HttpMethod.Post, new UriBuilder(signalRHubUrl).Uri);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Headers.Add("Authorization", "Bearer <your JWT>");
            request.Content = new StringContent(JsonConvert.SerializeObject(payloadMessage), Encoding.UTF8, "application/json");

            var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

            if (!response.IsSuccessStatusCode)
                throw new Exception("Failure sending SignalR message.");

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
    }
}