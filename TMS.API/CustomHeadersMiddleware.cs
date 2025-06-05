namespace TMS.API.Middleware;

public class CustomHeadersMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<CustomHeadersMiddleware> _logger;

    public CustomHeadersMiddleware(RequestDelegate next, ILogger<CustomHeadersMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Logowanie nagłówków przychodzących
        _logger.LogInformation("Incoming request headers:");
        foreach (var header in context.Request.Headers)
        {
            _logger.LogInformation($"{header.Key}: {header.Value}");
        }

        // Dodawanie własnych nagłówków do odpowiedzi
        context.Response.OnStarting(() =>
        {
            context.Response.Headers.Add("X-TMS-Version", "1.0.0");
            context.Response.Headers.Add("X-TMS-Server-Time", DateTime.UtcNow.ToString("o"));
            return Task.CompletedTask;
        });

        await _next(context);
    }
}