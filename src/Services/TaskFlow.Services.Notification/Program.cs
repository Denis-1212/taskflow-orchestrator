using Microsoft.EntityFrameworkCore;

using RabbitMQ.Module.Deduplication;
using RabbitMQ.Module.Extensions;

using TaskFlow.Services.Notification.Application.Services;
using TaskFlow.Services.Notification.Handlers;
using TaskFlow.Services.Notification.Infrastructure;
using TaskFlow.Services.Notification.Services;
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

// RabbitMQ connection
IConfigurationSection rabbitMqConfig = builder.Configuration.GetSection("RabbitMQ");
string rabbitMqConnectionString =
    $"amqp://{rabbitMqConfig["Username"]}:{rabbitMqConfig["Password"]}@{rabbitMqConfig["Host"]}:{rabbitMqConfig["Port"]}{rabbitMqConfig["VirtualHost"]}";

// Add RabbitMQ module
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
    });

// Add background service to start consumers
builder.Services.AddHostedService<RabbitMQConsumerHostedService>();
builder.Services.AddControllers();

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
