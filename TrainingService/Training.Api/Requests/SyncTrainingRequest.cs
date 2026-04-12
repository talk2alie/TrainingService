namespace Training.Api.Requests;

public sealed record SyncTrainingRequest(string? DeviceId, DateTimeOffset? SyncedAtUtc);
