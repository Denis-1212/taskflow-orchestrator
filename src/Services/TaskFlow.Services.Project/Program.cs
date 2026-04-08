using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;

using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;

using Serilog;

using TaskFlow.Services.Project.Application.Services;
using TaskFlow.Services.Project.Clients;
using TaskFlow.Services.Project.Extensions;
using TaskFlow.Services.Project.Infrastructure;
using TaskFlow.Services.Project.Services;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .Enrich.WithProperty("Service", "ProjectService")
    .WriteTo.Console()
    .WriteTo.Seq("http://seq:5341")
    .CreateLogger();

try
{
    AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
    WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
    int restPort = builder.Configuration.GetValue("Ports:Rest", 5002);
    int grpcPort = builder.Configuration.GetValue("Ports:Grpc", 5006);
    builder.WebHost.ConfigureKestrel(options =>
    {
        // REST API на HTTP/1.1
        options.ListenAnyIP(
            restPort,
            listenOptions =>
            {
                listenOptions.Protocols = HttpProtocols.Http1;
            });

        // gRPC на HTTP/2
        options.ListenAnyIP(
            grpcPort,
            listenOptions =>
            {
                listenOptions.Protocols = HttpProtocols.Http2;
            });
    });

    builder.Host.UseSerilog();
    builder.Services.AddControllers();

    builder.Services.AddOpenTelemetry()
        .WithMetrics(metrics => metrics
            .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("ProjectService"))
            .AddAspNetCoreInstrumentation()
            .AddPrometheusExporter());

    builder.Services.AddScoped<IAuthGrpcClient, AuthGrpcClient>();

    string? connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

    if (string.IsNullOrEmpty(connectionString))
    {
        throw new InvalidOperationException("Database connection string is not configured");
    }

    builder.Services.AddDbContext<ProjectDbContext>(options =>
        options.UseNpgsql(connectionString)
            .EnableSensitiveDataLogging()
            .LogTo(Console.WriteLine, LogLevel.Information));

    builder.Services.AddRabbitMQModule(builder.Configuration);
    builder.Services.AddGrpc();
    builder.Services.AddScoped<IProjectService, ProjectService>();

    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();
    builder.Services.AddHealthChecks();

    WebApplication app = builder.Build();
    app.UseGlobalExceptionHandler();

    app.MapGrpcService<ProjectGrpcService>();
    app.Logger.LogInformation("gRPC service registered: ProjectGrpcService");
    app.UseMigrations();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseUserIdExtraction();
    app.MapPrometheusScrapingEndpoint();
    app.MapControllers();

    app.MapHealthChecks("/health/live");

    app.UseRequestLogging();

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
