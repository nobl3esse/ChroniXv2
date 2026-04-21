using System.Net.WebSockets;
using ChroniXApi.Services;

var whitelistService = new WhitelistService();
whitelistService.LoadWhitelist();

//Erstellen eines Singleton damit ForegroundService() nur einmal erstellt wird
var foregroundService = new ForegroundService(whitelistService);

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

app.MapGet("/foreground", () =>
{
    return foregroundService.GetForegroundWindowProcessName();
});

app.MapGet("/start", () =>
{
    foregroundService.StartTracking();
});

app.MapGet("/stop", () =>
{
    foregroundService.StopTracking();
});

app.MapGet("/times", () =>
{
    return foregroundService.GetProcessTimes();
});

app.MapGet("/whitelist", () =>
{
    return whitelistService.GetAll();
});

app.MapPost("/whitelist", (WhitelistInput input) =>
{
    whitelistService.Add(input.ProcessName);
});

app.MapDelete("/whitelist", (string name) =>
{
    whitelistService.Remove(name);
});

app.Run("http://localhost:5000");

record WhitelistInput(string ProcessName);