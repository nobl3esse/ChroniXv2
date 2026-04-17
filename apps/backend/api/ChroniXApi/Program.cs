using ChroniXApi.Services;

var builder = WebApplication.CreateBuilder(args);
// CORS erlauben damit Electron zugreifen darf
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost", "file://")
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();
app.UseCors();

app.MapGet("/processes", () =>
{
    var service = new ProcessService();
    return service.GetRunningProcesses();
});

app.MapGet("/status", () =>
{
    return new { success = true, message = "running " };
});

app.Run("http://localhost:5000");