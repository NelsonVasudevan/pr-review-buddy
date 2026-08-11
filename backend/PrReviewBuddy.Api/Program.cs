var builder = WebApplication.CreateBuilder(args);

// Allow our frontend (running on localhost:5173) to call this backend.
// Without this, the browser blocks the request for security reasons (this is called CORS).
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseCors("AllowFrontend");

// This is our very first API endpoint. It just proves the backend is alive.
app.MapGet("/api/hello", () =>
{
    return Results.Ok(new
    {
        message = "PR Review Buddy backend is alive.",
        timestampUtc = DateTime.UtcNow
    });
});

app.Run();
