using Microsoft.EntityFrameworkCore;

using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;

using Serilog;

using TaskFlow.Services.Project.Application.Services;
using TaskFlow.Services.Project.Infrastructure;
using TaskFlow.Services.Project.Middleware;
using TaskFlow.Services.Project.Services;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .Enrich.WithProperty("Service", "ProjectService")
    .WriteTo.Console()
    .WriteTo.Seq("http://seq:5341")
    .CreateLogger();

try
{
    WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

    // Add services
    builder.Host.UseSerilog();
    builder.Services.AddControllers();

    builder.Services.AddOpenTelemetry()
        .WithMetrics(metrics => metrics
            .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("ProjectService"))
            .AddAspNetCoreInstrumentation()
            .AddPrometheusExporter());

    string? connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

    if (string.IsNullOrEmpty(connectionString))
    {
        throw new InvalidOperationException("Database connection string is not configured");
    }

    builder.Services.AddDbContext<ProjectDbContext>(options =>
        options.UseNpgsql(connectionString));

    builder.Services.AddGrpc();
    builder.Services.AddScoped<IProjectService, ProjectService>();

    builder.Services.AddCors(options =>
    {
        options.AddPolicy(
            "Development",
            policy =>
            {
                policy.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader();
            });
    });

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

    app.MapGrpcService<ProjectGrpcService>();

    app.UseMigrations();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
        app.UseCors("Development");
    }

    app.UseUserIdExtraction();
    app.MapPrometheusScrapingEndpoint();
    app.MapControllers();

    app.MapHealthChecks("/health/live");
    app.MapHealthChecks("/health/ready");
    app.MapGrpcService<ProjectGrpcService>();

    app.Use(async (context, next) =>
    {
        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
        logger.LogInformation("Request: {Method} {Path}", context.Request.Method, context.Request.Path);
        await next();
        logger.LogInformation("Response: {StatusCode}", context.Response.StatusCode);
    });

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application start-up failed");
    throw;
}
finally
{
    Log.CloseAndFlush();
}
