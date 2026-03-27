using Microsoft.EntityFrameworkCore;

using TaskFlow.Services.Project.Application.Services;
using TaskFlow.Services.Project.Infrastructure;
using TaskFlow.Services.Project.Services;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();

builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddDbContext<ProjectDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add gRPC
builder.Services.AddGrpc();

WebApplication app = builder.Build();

using (IServiceScope scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ProjectDbContext>();

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
app.MapGrpcService<ProjectGrpcService>();

app.Run();
