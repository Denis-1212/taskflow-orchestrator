using Serilog;

using TaskFlow.ApiGateway.Extensions;

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

app.UseGlobalExceptionHandler();
app.SetStaticFilesDirectory();
app.UseHttpsRedirection();

app.UseAuthentication();
app.UnauthorizedRequestBlocking();
app.UseAuthorization();

app.UseUserIdPropagation();
app.UseRateLimiter();

app.MapHealthChecks("/health/live");
app.MapReverseProxy();
app.MapFallbackToFile("index.html");

app.Run();
