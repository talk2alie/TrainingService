namespace Training.Api.Middleware;

public sealed class GlobalExceptionMiddleware(RequestDelegate next)
{
    private readonly RequestDelegate _next = next;

    public Task InvokeAsync(HttpContext context)
    {
        return _next(context);
    }
}
