using System.Text;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

using OpenTelemetry.Metrics;

using Serilog;

using StackExchange.Redis;

using TaskFlow.Services.Auth.Application.Services;
using TaskFlow.Services.Auth.Extensions;
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

int restPort = builder.Configuration.GetValue("Ports:Rest", 5001);
int grpcPort = builder.Configuration.GetValue("Ports:Grpc", 5007);
builder.WebHost.ConfigureKestrel(options =>
{
    // REST API
    options.ListenAnyIP(
        restPort,
        listenOptions =>
        {
            listenOptions.Protocols = HttpProtocols.Http1;
        });

    // gRPC
    options.ListenAnyIP(
        grpcPort,
        listenOptions =>
        {
            listenOptions.Protocols = HttpProtocols.Http2;
        });
});

builder.Services.AddGrpc();
builder.Host.UseSerilog();
builder.Services.AddControllers();
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
builder.Services.AddRabbitMQModule(builder.Configuration);
builder.Services.AddScoped<IAuthService, AuthService>();

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

builder.Services.AddOpenTelemetry()
    .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation()
        .AddPrometheusExporter(options =>
        {
            options.ScrapeResponseCacheDurationMilliseconds = 0;
        }));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();

WebApplication app = builder.Build();

app.UseGlobalExceptionHandler();

app.UseMigrations();

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapPrometheusScrapingEndpoint();
app.MapControllers();
app.MapGrpcService<AuthGrpcService>();
app.MapHealthChecks("/health/live");
app.UseRequestLogging();

app.Run();
