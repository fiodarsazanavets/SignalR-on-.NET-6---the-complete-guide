using Microsoft.AspNetCore.Http.Connections;
using SignalRServer.Hubs;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSignalR(hubOptions => {
    hubOptions.KeepAliveInterval = TimeSpan.FromSeconds(10);
    hubOptions.MaximumReceiveMessageSize = 65_536;
    hubOptions.HandshakeTimeout = TimeSpan.FromSeconds(15);
    hubOptions.MaximumParallelInvocationsPerClient = 2;
    hubOptions.EnableDetailedErrors = true;
    hubOptions.StreamBufferCapacity = 15;

    if (hubOptions?.SupportedProtocols is not null)
    {
        foreach (var protocol in hubOptions.SupportedProtocols)
            Console.WriteLine($"SignalR supports {protocol} protocol.");
    }
    
})
.AddJsonProtocol(options => {
    options.PayloadSerializerOptions.PropertyNamingPolicy = null;
    options.PayloadSerializerOptions.Encoder = null;
    options.PayloadSerializerOptions.IncludeFields = false;
    options.PayloadSerializerOptions.IgnoreReadOnlyFields = false;
    options.PayloadSerializerOptions.IgnoreReadOnlyProperties = false;
    options.PayloadSerializerOptions.MaxDepth = 0;
    options.PayloadSerializerOptions.NumberHandling = JsonNumberHandling.Strict;
    options.PayloadSerializerOptions.DictionaryKeyPolicy = null;
    options.PayloadSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.Never;
    options.PayloadSerializerOptions.PropertyNameCaseInsensitive = false;
    options.PayloadSerializerOptions.DefaultBufferSize = 32_768;
    options.PayloadSerializerOptions.ReadCommentHandling = System.Text.Json.JsonCommentHandling.Skip;
    options.PayloadSerializerOptions.ReferenceHandler = null;
    options.PayloadSerializerOptions.UnknownTypeHandling = JsonUnknownTypeHandling.JsonElement;
    options.PayloadSerializerOptions.WriteIndented = true;

    Console.WriteLine($"Number of default JSON converters: {options.PayloadSerializerOptions.Converters.Count}");
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapHub<LearningHub>("/learningHub", options =>
{
    options.Transports =
                HttpTransportType.WebSockets | 
                HttpTransportType.LongPolling;
    options.CloseOnAuthenticationExpiration = true;
    options.ApplicationMaxBufferSize = 65_536;
    options.TransportMaxBufferSize = 65_536;
    options.MinimumProtocolVersion = 0;
    options.TransportSendTimeout = TimeSpan.FromSeconds(10);
    options.WebSockets.CloseTimeout = TimeSpan.FromSeconds(3);
    options.LongPolling.PollTimeout = TimeSpan.FromSeconds(10);

    Console.WriteLine($"Authorization data items: {options.AuthorizationData.Count}");
});
app.UseBlazorFrameworkFiles();

app.Run();
