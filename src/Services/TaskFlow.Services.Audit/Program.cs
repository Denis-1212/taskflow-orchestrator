using Microsoft.EntityFrameworkCore;

using TaskFlow.Services.Audit.Application.Services;
using TaskFlow.Services.Audit.Extentions;
using TaskFlow.Services.Audit.Handlers;
using TaskFlow.Services.Audit.Infrastructure;
using TaskFlow.Services.Audit.Services;
using TaskFlow.Shared.Messaging.Events;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();

builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddDbContext<AuditDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("AuditDatabase")));

builder.Services.AddRabbitMQModuleWithHandlers(
    builder.Configuration,
    module =>
    {
        module.AddConsumer<TaskCreatedEvent, TaskCreatedHandler>(c =>
        {
            c.QueueName = "audit.task-created";
            c.ExchangeName = "taskflow.events";
            c.RoutingKey = "task.created";
            c.PrefetchCount = 10;
        });

        module.AddConsumer<TaskAssignedEvent, TaskAssignedHandler>(c =>
        {
            c.QueueName = "audit.task-assigned";
            c.ExchangeName = "taskflow.events";
            c.RoutingKey = "task.assigned";
            c.PrefetchCount = 10;
        });

        module.AddConsumer<TaskStatusChangedEvent, TaskStatusChangedHandler>(c =>
        {
            c.QueueName = "audit.task-status-changed";
            c.ExchangeName = "taskflow.events";
            c.RoutingKey = "task.status.changed";
            c.PrefetchCount = 10;
        });

        module.AddConsumer<UserRegisteredEvent, UserRegisteredHandler>(c =>
        {
            c.QueueName = "audit.user-registered";
            c.ExchangeName = "taskflow.events";
            c.RoutingKey = "user.registered";
            c.PrefetchCount = 10;
        });

        module.AddConsumer<ProjectCreatedEvent, ProjectCreatedHandler>(c =>
        {
            c.QueueName = "audit.project-created";
            c.ExchangeName = "taskflow.events";
            c.RoutingKey = "project.created";
            c.PrefetchCount = 10;
        });

        module.AddConsumer<UserAddedToProjectEvent, UserAddedToProjectHandler>(c =>
        {
            c.QueueName = "audit.user-added-to-project";
            c.ExchangeName = "taskflow.events";
            c.RoutingKey = "project.user.added";
            c.PrefetchCount = 10;
        });

        _ = module.StartConsumersAsync();
    });

builder.Services.AddHostedService<RabbitMQConsumerHostedService>();

WebApplication app = builder.Build();

// Автоматическое применение миграций
using (IServiceScope scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AuditDbContext>();

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
