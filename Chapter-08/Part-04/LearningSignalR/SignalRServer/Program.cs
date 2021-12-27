using MessagePack;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.IdentityModel.Tokens;
using SignalRServer;
using SignalRServer.Hubs;
using System.IdentityModel.Tokens.Jwt;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAnyGet",
        builder => builder.AllowAnyOrigin()
            .WithMethods("GET")
            .AllowAnyHeader());

    options.AddPolicy("AllowExampleDomain",
        builder => builder.WithOrigins("https://example.com")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials());
});

JwtSecurityTokenHandler.DefaultMapInboundClaims = false;

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = "oidc";
})
.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme)
.AddOpenIdConnect("oidc", options =>
{
    options.Authority = "https://localhost:5001";
    options.ClientId = "webAppClient";
    options.ClientSecret = "webAppClientSecret";
    options.ResponseType = "code";
    options.CallbackPath = "/signin-oidc";
    options.SaveTokens = true;
    options.RequireHttpsMetadata = false;
    options.GetClaimsFromUserInfoEndpoint = true;
})
.AddJwtBearer(options =>
{
    options.Authority = "https://localhost:5001";
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateAudience = false
    };
    options.RequireHttpsMetadata = false;
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var path = context.HttpContext.Request.Path;
            if (path.StartsWithSegments("/learningHub"))
            {
                // Attempt to get a token from a query sting used by WebSocket
                var accessToken = context.Request.Query["access_token"];

                // If not present, extract the token from Authorization header
                if (string.IsNullOrWhiteSpace(accessToken))
                {
                    accessToken = context.Request.Headers["Authorization"]
                        .ToString()
                        .Replace("Bearer ", "");
                }

                context.Token = accessToken;
            }

            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("BasicAuth", policy =>
    {
        policy.RequireAuthenticatedUser();
    });

    options.AddPolicy("AdminClaim", policy =>
    {
        policy.RequireClaim("admin");
    });

    options.AddPolicy("AdminOnly", policy =>
    {
        policy.Requirements.Add(new RoleRequirement("admin"));
    });
});

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
})
.AddMessagePackProtocol(options =>
{
    options.SerializerOptions = MessagePackSerializerOptions.Standard
        .WithSecurity(MessagePackSecurity.UntrustedData)
        .WithCompression(MessagePackCompression.Lz4Block)
        .WithAllowAssemblyVersionMismatch(true)
        .WithOldSpec()
        .WithOmitAssemblyVersion(true);
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

app.UseCors("AllowAnyGet")
   .UseCors("AllowExampleDomain");

app.UseAuthentication();
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
