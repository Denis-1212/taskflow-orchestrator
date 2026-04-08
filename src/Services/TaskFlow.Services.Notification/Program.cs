using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;

using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;

using RabbitMQ.Module;

using Serilog;

using TaskFlow.Services.Notification.Application.Services;
using TaskFlow.Services.Notification.Clients;
using TaskFlow.Services.Notification.Extensions;
using TaskFlow.Services.Notification.Infrastructure;
using TaskFlow.Services.Notification.Services;
using TaskFlow.Services.Notification.Settings;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .Enrich.WithProperty("Service", "NotificationService")
    .WriteTo.Console()
    .WriteTo.Seq("http://seq:5341")
    .CreateLogger();

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

int restPort = builder.Configuration.GetValue("Ports:Rest", 5004);
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(
        restPort,
        listenOptions =>
        {
            listenOptions.Protocols = HttpProtocols.Http1;
        });
});

string? connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("Database connection string is not configured");
}

// Add services
builder.Services.AddControllers();

builder.Services.AddOpenTelemetry()
    .WithMetrics(metrics => metrics
        .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("NotificationService"))
        .AddAspNetCoreInstrumentation()
        .AddPrometheusExporter());

builder.Services.AddScoped<IAuthGrpcClient, AuthGrpcClient>();

builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("SmtpSettings"));
builder.Services.AddScoped<IEmailService, EmailService>();

builder.Services.AddRabbitMQModuleWithHandlers(builder.Configuration);
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddDbContext<NotificationDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();

WebApplication app = builder.Build();
app.UseGlobalExceptionHandler();

app.UseMigrations();
app.UseUserIdExtraction();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

var module = app.Services.GetRequiredService<MessagingModule>();
await module.StartConsumersAsync();

app.MapPrometheusScrapingEndpoint();
app.MapControllers();
app.MapHealthChecks("/health/live");
app.UseRequestLogging();

app.Run();
