namespace Training.Domain.Common;

public sealed class AggregateNotPersistableException : Exception
{
    public AggregateNotPersistableException(string message) : base(message) { }
}
