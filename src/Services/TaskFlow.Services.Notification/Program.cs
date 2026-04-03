using Microsoft.EntityFrameworkCore;

using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;

using RabbitMQ.Module;

using TaskFlow.Services.Notification.Application.Services;
using TaskFlow.Services.Notification.Clients;
using TaskFlow.Services.Notification.Extensions;
using TaskFlow.Services.Notification.Infrastructure;
using TaskFlow.Services.Notification.Middleware;
using TaskFlow.Services.Notification.Services;
using TaskFlow.Services.Notification.Settings;
using TaskFlow.Services.Task.Clients;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
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
app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (Exception ex)
    {
        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
        logger.LogError(
            ex,
            "Unhandled exception occurred processing {Method} {Path}",
            context.Request.Method,
            context.Request.Path);

        throw;
    }
});

app.UseMigrations();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

var module = app.Services.GetRequiredService<MessagingModule>();
await module.StartConsumersAsync();

app.MapPrometheusScrapingEndpoint();
app.Use(async (context, next) =>
{
    var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
    logger.LogInformation("Request: {Method} {Path}", context.Request.Method, context.Request.Path);
    await next();
    logger.LogInformation("Response: {StatusCode}", context.Response.StatusCode);
});

app.MapControllers();
app.MapHealthChecks("/health/live");
app.MapHealthChecks("/health/ready");

app.Run();
