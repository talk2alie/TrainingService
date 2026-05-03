using Training.Domain.Exercises;

namespace Training.Api.Requests;

public sealed record UpdateRoutineRequest(string Name, string Description, DifficultyLevel DifficultyLevel);
