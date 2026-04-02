using System.Text;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

using OpenTelemetry.Metrics;

using Serilog;

using StackExchange.Redis;

using TaskFlow.Services.Auth.Application.Services;
using TaskFlow.Services.Auth.Infrastructure;
using TaskFlow.Services.Auth.Services;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .Enrich.WithProperty("Service", "AuthService")
    .WriteTo.Console()
    .WriteTo.Seq("http://seq:5341")
    .CreateLogger();

AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    // REST API
    options.ListenLocalhost(
        5001,
        listenOptions =>
        {
            listenOptions.Protocols = HttpProtocols.Http1;
        });

    // gRPC
    options.ListenLocalhost(
        5007,
        listenOptions =>
        {
            listenOptions.Protocols = HttpProtocols.Http2;
        });
});

// Add services
builder.Services.AddGrpc();
builder.Host.UseSerilog();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();

builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddDbContext<AuthDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IJwtService, JwtService>();

// in-memory реализация (вместо Redis)
// builder.Services.AddSingleton<IRefreshTokenService, InMemoryRefreshTokenService>();

string redisConnectionString = builder.Configuration.GetConnectionString("Redis") ?? throw new InvalidOperationException();
builder.Services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(redisConnectionString));
builder.Services.AddScoped<IRefreshTokenService, RefreshTokenService>();

builder.Services.AddScoped<IAuthService, AuthService>();

// Add authentication
string? jwtSecret = builder.Configuration["Jwt:Secret"];

if (string.IsNullOrEmpty(jwtSecret))
{
    throw new InvalidOperationException("JWT Secret is not configured");
}

byte[] key = Encoding.UTF8.GetBytes(jwtSecret);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(key)
        };
    });

builder.Services.AddAuthorization();

// OpenTelemetry metrics
builder.Services.AddOpenTelemetry()
    .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation()
        .AddPrometheusExporter(options =>
        {
            options.ScrapeResponseCacheDurationMilliseconds = 0;
        }));

WebApplication app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler(errorApp =>
    {
        errorApp.Run(async context =>
        {
            context.Response.StatusCode = 500;
            context.Response.ContentType = "application/problem+json";

            var exceptionHandler = context.Features.Get<IExceptionHandlerFeature>();
            Exception? error = exceptionHandler?.Error;

            var response = new
            {
                Title = "An error occurred",
                Status = 500,
                Detail = app.Environment.IsDevelopment() ? error?.Message : "Internal server error"
            };

            await context.Response.WriteAsJsonAsync(response);
        });
    });
}

if (app.Environment.IsDevelopment())
{
    // Автоматическое применение миграций
    using IServiceScope scope = app.Services.CreateScope();

    var dbContext = scope.ServiceProvider.GetRequiredService<AuthDbContext>();

    try
    {
        app.Logger.LogInformation("Applying database migrations...");
        await dbContext.Database.MigrateAsync();
        app.Logger.LogInformation("Migrations applied successfully");
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "Failed to apply migrations");
        throw;
    }
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();
app.MapGrpcService<AuthGrpcService>();
app.MapHealthChecks("/health/live");
app.MapHealthChecks("/health/ready");
app.MapPrometheusScrapingEndpoint();

app.Run();
