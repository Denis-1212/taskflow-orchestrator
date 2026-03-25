using Microsoft.EntityFrameworkCore;

using RabbitMQ.Module.Deduplication;
using RabbitMQ.Module.Extensions;

using TaskFlow.Services.Audit.Application.Services;
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

// RabbitMQ configuration
IConfigurationSection rabbitMqConfig = builder.Configuration.GetSection("RabbitMQ");
string rabbitMqConnectionString =
    $"amqp://{rabbitMqConfig["Username"]}:{rabbitMqConfig["Password"]}@{rabbitMqConfig["Host"]}:{rabbitMqConfig["Port"]}{rabbitMqConfig["VirtualHost"]}";

builder.Services.AddRabbitMQModule(
    options =>
    {
        options.ConnectionString = rabbitMqConnectionString;
        options.DeliveryControl.MaxRetryAttempts = 3;
        options.DeliveryControl.RetryBaseDelayMs = 1000;
        options.DeliveryControl.EnableDeadLetter = true;
        options.Deduplication.StoreType = DeduplicationStoreType.InMemory;
    },
    module =>
    {
        // Task events
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

        // User events
        module.AddConsumer<UserRegisteredEvent, UserRegisteredHandler>(c =>
        {
            c.QueueName = "audit.user-registered";
            c.ExchangeName = "taskflow.events";
            c.RoutingKey = "user.registered";
            c.PrefetchCount = 10;
        });

        // Project events
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
    });

// Add background service to start consumers
builder.Services.AddHostedService<RabbitMQConsumerHostedService>();

WebApplication app = builder.Build();

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
