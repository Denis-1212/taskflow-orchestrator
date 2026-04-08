using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;

using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;

using RabbitMQ.Module;

using Serilog;

using TaskFlow.Services.Audit.Application.Services;
using TaskFlow.Services.Audit.Extensions;
using TaskFlow.Services.Audit.Infrastructure;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .Enrich.WithProperty("Service", "AuthService")
    .WriteTo.Console()
    .WriteTo.Seq("http://seq:5341")
    .CreateLogger();

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

int restPort = builder.Configuration.GetValue("Ports:Rest", 5005);
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
        .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("AuditService"))
        .AddAspNetCoreInstrumentation()
        .AddPrometheusExporter());

builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddDbContext<AuditDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddRabbitMQModuleWithHandlers(builder.Configuration);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();

WebApplication app = builder.Build();
app.UseGlobalExceptionHandler();

app.UseMigrations();

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
