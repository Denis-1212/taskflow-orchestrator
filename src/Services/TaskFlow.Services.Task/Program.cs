using Microsoft.EntityFrameworkCore;

using TaskFlow.Services.Task.Application.Services;
using TaskFlow.Services.Task.Clients;
using TaskFlow.Services.Task.Infrastructure;

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
