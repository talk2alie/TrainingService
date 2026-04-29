namespace Training.Domain.Routines;

public sealed class InvalidRoutineOperationException : Exception
{
    public InvalidRoutineOperationException(string message) : base(message) { }
}

public sealed class RoutineInvariantViolationException : Exception
{
    public RoutineInvariantViolationException(string message, string? paramName = null)
        : base(paramName is null ? message : $"{message} (Parameter: {paramName})") { }
}
