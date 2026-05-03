using Training.Domain.Sessions;

namespace Training.Api.Requests;

public sealed record AddSessionLoggedSetRequest(
    int? Repetitions,
    decimal? WeightKg,
    int? DurationSeconds,
    decimal? DistanceMeters,
    RpeScale Rpe,
    int? HeartRateBpm,
    string? Note);
