namespace Training.Api.Middleware;

public sealed record ErrorEnvelope(
    string Code,
    string Message,
    string CorrelationId,
    IDictionary<string, string[]>? Details = null);
