using Microsoft.EntityFrameworkCore;

using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;

using Serilog;

using TaskFlow.Services.Task.Application.Services;
using TaskFlow.Services.Task.Clients;
using TaskFlow.Services.Task.Extensions;
using TaskFlow.Services.Task.Infrastructure;
using TaskFlow.Services.Task.Services;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .Enrich.WithProperty("Service", "TaskService")
    .WriteTo.Console()
    .WriteTo.Seq("http://seq:5341")
    .CreateLogger();

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services
builder.Host.UseSerilog();
builder.Services.AddControllers();
builder.Services.AddOpenTelemetry()
    .WithMetrics(metrics => metrics
        .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("ProjectService"))
        .AddAspNetCoreInstrumentation()
        .AddPrometheusExporter());

// Register gRPC clients
builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddScoped<IAuthGrpcClient, AuthGrpcClient>();
builder.Services.AddScoped<IProjectGrpcClient, ProjectGrpcClient>();
builder.Services.AddDbContext<TaskDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHostedService<OutboxProcessorService>();

builder.Services.AddRabbitMQModuleWithHandlers(builder.Configuration);

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
app.UseUserIdExtraction();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseCors("Development");
}

app.MapPrometheusScrapingEndpoint();
app.MapControllers();
app.MapHealthChecks("/health/live");
app.MapHealthChecks("/health/ready");

app.Use(async (context, next) =>
{
    var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
    logger.LogInformation("Request: {Method} {Path}", context.Request.Method, context.Request.Path);
    await next();
    logger.LogInformation("Response: {StatusCode}", context.Response.StatusCode);
});

app.Run();
