namespace Training.Infrastructure.Persistence;

public sealed class OutboxMessageEntity
{
    public Guid Id { get; set; }
    public DateTimeOffset OccurredAtUtc { get; set; }
    public string Type { get; set; } = string.Empty;
    public string PayloadJson { get; set; } = string.Empty;
    public DateTimeOffset? ProcessedAtUtc { get; set; }
}
