using Microsoft.EntityFrameworkCore;

using TaskFlow.Services.Task.Application.Services;
using TaskFlow.Services.Task.Clients;
using TaskFlow.Services.Task.Extensions;
using TaskFlow.Services.Task.Infrastructure;
using TaskFlow.Services.Task.Services;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();

// Register gRPC clients
builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddScoped<IAuthGrpcClient, AuthGrpcClient>();
builder.Services.AddScoped<IProjectGrpcClient, ProjectGrpcClient>();
builder.Services.AddDbContext<TaskDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHostedService<OutboxProcessorService>();

builder.Services.AddRabbitMQModuleWithHandlers(
    builder.Configuration,
    module =>
    {
        // Task Service не потребляет сообщения, только публикует
        // Поэтому здесь нет AddConsumer
        // logger.LogInformation("RabbitMQ module configured for Task Service");
    });

WebApplication app = builder.Build();

// Автоматическое применение миграций
using (IServiceScope scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<TaskDbContext>();

    try
    {
        app.Logger.LogInformation("Applying database migrations...");
        dbContext.Database.Migrate();
        app.Logger.LogInformation("Migrations applied successfully");
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "Failed to apply migrations");
        throw; // хотим чтобы контейнер падал при ошибке
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health/live");
app.MapHealthChecks("/health/ready");

app.Run();
