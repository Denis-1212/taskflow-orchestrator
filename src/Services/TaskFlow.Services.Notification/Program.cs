using Microsoft.EntityFrameworkCore;

using TaskFlow.Services.Notification.Application.Services;
using TaskFlow.Services.Notification.Extentions;
using TaskFlow.Services.Notification.Handlers;
using TaskFlow.Services.Notification.Infrastructure;
using TaskFlow.Shared.Messaging.Events;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
string? connectionString = builder.Configuration.GetConnectionString("NotificationDatabase");
// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();

builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddDbContext<NotificationDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddRabbitMQModuleWithHandlers(
    builder.Configuration,
    module =>
    {
        module.AddConsumer<TaskCreatedEvent, TaskCreatedHandler>(c =>
        {
            c.QueueName = "notification.task-created";
            c.ExchangeName = "taskflow.events";
            c.RoutingKey = "task.created";
            c.PrefetchCount = 10;
        });

        module.AddConsumer<TaskAssignedEvent, TaskAssignedHandler>(c =>
        {
            c.QueueName = "notification.task-assigned";
            c.ExchangeName = "taskflow.events";
            c.RoutingKey = "task.assigned";
            c.PrefetchCount = 10;
        });

        module.AddConsumer<TaskStatusChangedEvent, TaskStatusChangedHandler>(c =>
        {
            c.QueueName = "notification.task-status-changed";
            c.ExchangeName = "taskflow.events";
            c.RoutingKey = "task.status.changed";
            c.PrefetchCount = 10;
        });

        _ = module.StartConsumersAsync();
    });

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

// Автоматическое применение миграций
using (IServiceScope scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();

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
