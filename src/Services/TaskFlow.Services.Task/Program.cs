using TaskFlow.Services.Task.Clients;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();

// Register gRPC clients
builder.Services.AddScoped<IAuthGrpcClient, AuthGrpcClient>();
builder.Services.AddScoped<IProjectGrpcClient, ProjectGrpcClient>();

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
