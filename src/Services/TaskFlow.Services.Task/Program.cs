using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;

using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;

using Serilog;

using TaskFlow.Services.Task.Application.Services;
using TaskFlow.Services.Task.Clients;
using TaskFlow.Services.Task.Domain;
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

int restPort = builder.Configuration.GetValue("Ports:Rest", 5003);
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(
        restPort,
        listenOptions =>
        {
            listenOptions.Protocols = HttpProtocols.Http1;
        });
});

builder.Host.UseSerilog();
builder.Services.AddControllers();
builder.Services.AddOpenTelemetry()
    .WithMetrics(metrics => metrics
        .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("TaskService"))
        .AddAspNetCoreInstrumentation()
        .AddPrometheusExporter());

builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddScoped<IAuthGrpcClient, AuthGrpcClient>();
builder.Services.AddScoped<IProjectGrpcClient, ProjectGrpcClient>();
builder.Services.AddScoped<DomainEventInterceptor>();

string? connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("Database connection string is not configured");
}

builder.Services.AddDbContext<TaskDbContext>((sp, options) =>
{
    options.UseNpgsql(connectionString)
        .EnableSensitiveDataLogging()
        .LogTo(Console.WriteLine, LogLevel.Information)
        .AddInterceptors(new DomainEventInterceptor());
});

builder.Services.AddHostedService<OutboxProcessorService>();

builder.Services.AddRabbitMQModule(builder.Configuration);

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

app.MapPrometheusScrapingEndpoint();
app.MapControllers();
app.MapHealthChecks("/health/live");

app.UseRequestLogging();

app.Run();
