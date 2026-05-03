using Training.Domain.Exercises;

namespace Training.Api.Requests;

public sealed record CreateRoutineRequest(
    string Name,
    string Description,
    DifficultyLevel DifficultyLevel);
