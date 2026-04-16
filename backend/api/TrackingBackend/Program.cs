using TimeTracking;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var trackingService = new TrackingService();
trackingService.Start();

app.MapGet("/api/status", () =>
{
    return Results.Json(new
    {
        success = true,
        message = "C# Backend läuft",
        timestamp = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss")
    });
});

app.MapPost("/api/start-tracking", () =>
{
    trackingService.Start();
    return Results.Json(new
    {
        success = true,
        message = "Tracking gestartet"
    });
});

app.MapPost("/api/stop-tracking", () =>
{
    trackingService.Stop();
    return Results.Json(new
    {
        success = true,
        message = "Tracking gestoppt"
    });
});

app.MapGet("/api/stats", () =>
{
    return Results.Json(new
    {
        success = true,
        stats = trackingService.GetStats()
    });
});

app.MapGet("/api/group-stats", () =>
{
    return Results.Json(new
    {
        success = true,
        stats = trackingService.GetGroupStats()
    });
});

app.Run("http://localhost:5000");