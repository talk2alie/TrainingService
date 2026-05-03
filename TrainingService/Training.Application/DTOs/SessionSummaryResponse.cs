namespace Training.Application.DTOs;

public sealed record SessionSummaryResponse(
    Guid Id,
    Guid? RoutineId,
    DateTimeOffset StartedAtUtc,
    DateTimeOffset? EndedAtUtc,
    bool IsEnded,
    IReadOnlyList<SessionExerciseResponse> Exercises);
