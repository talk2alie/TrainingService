namespace Training.Api.Middleware;

using Microsoft.EntityFrameworkCore;

public sealed class GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
{
    private readonly RequestDelegate _next = next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger = logger;

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            var statusCode = ex switch
            {
                ArgumentException => StatusCodes.Status400BadRequest,
                InvalidOperationException => StatusCodes.Status409Conflict,
                DbUpdateConcurrencyException => StatusCodes.Status409Conflict,
                UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
                _ => StatusCodes.Status500InternalServerError
            };

            _logger.LogError(ex,
                "Unhandled exception for {Method} {Path}. CorrelationId: {CorrelationId}",
                context.Request.Method,
                context.Request.Path,
                context.TraceIdentifier);

            if (context.Response.HasStarted)
            {
                throw;
            }

            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/json";

            var error = new ErrorEnvelope(
                Code: statusCode == StatusCodes.Status500InternalServerError ? "internal_server_error" : "domain_error",
                Message: statusCode == StatusCodes.Status500InternalServerError ? "An unexpected error occurred." : ex.Message,
                CorrelationId: context.TraceIdentifier);

            await context.Response.WriteAsJsonAsync(error, cancellationToken: context.RequestAborted);
        }
    }
}
