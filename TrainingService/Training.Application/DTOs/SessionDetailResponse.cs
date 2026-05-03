namespace Training.Application.DTOs;

public sealed record SessionDetailResponse(
    Guid Id,
    Guid? RoutineId,
    DateTimeOffset StartedAtUtc,
    DateTimeOffset? EndedAtUtc,
    bool IsEnded,
    string? Note,
    IReadOnlyList<SessionExerciseDetailResponse> Exercises);
