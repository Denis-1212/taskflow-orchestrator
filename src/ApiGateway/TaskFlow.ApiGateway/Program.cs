using System.Text;
using System.Threading.RateLimiting;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;

using Serilog;

using TaskFlow.ApiGateway.Extensions;
using TaskFlow.ApiGateway.Gateway.Middleware;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .Enrich.WithProperty("Service", "ApiGateway")
    .WriteTo.Console()
    .WriteTo.Seq("http://seq:5341")
    .CreateLogger();

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog();

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

builder.Services.AddCustomAuthentication(builder.Configuration)
    .AddAuthorization()
    .AddCustomRateLimiting(builder.Configuration)
    .AddEndpointsApiExplorer()
    .AddHealthChecks();

WebApplication app = builder.Build();

string wwwrootPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "wwwroot");

app.UseStaticFiles(
    new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(wwwrootPath)
    });

app.UseGlobalExceptionHandler();

app.UseGlobalExceptionHandler();
app.SetStaticFilesDirectory();
app.UseHttpsRedirection();

app.UseAuthentication();
app.UnauthorizedRequestBlocking();
app.UseAuthorization();

app.UseUserIdPropagation();
app.UseRateLimiter();

app.MapHealthChecks("/health/live");
app.MapFallbackToFile("index.html");
app.Run();
