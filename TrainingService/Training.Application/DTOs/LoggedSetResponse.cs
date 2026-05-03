namespace Training.Application.DTOs;

public sealed record LoggedSetResponse(
    int Order,
    string Rpe,
    int? Repetitions,
    decimal? WeightKg,
    int? DurationSeconds,
    decimal? DistanceMeters,
    int? HeartRateBpm,
    string? Note);
