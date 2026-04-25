using Training.Domain.Common;
using Training.Domain.Exercises;
using Training.Domain.Sessions;

namespace Training.Api.Tests;

public sealed class SessionAggregateTests
{
    [Fact]
    public void Start_WithValidInput_CreatesSession()
    {
        var session = Session.Start(Guid.NewGuid());
        session.AddNote(SessionNote.Create("Push day"));

        Assert.NotEqual(Guid.Empty, session.Id);
        Assert.False(session.IsEnded);
        Assert.Equal(TimeSpan.Zero, session.StartedAtUtc.Offset);
        Assert.Equal(SessionNote.Create("Push day"), session.Note);
    }

    [Fact]
    public void AddExercise_WithDuplicateExerciseId_ThrowsInvalidOperationException()
    {
        var session = Session.Start(Guid.NewGuid());
        var exerciseId = Guid.NewGuid();
        session.AddExercise(exerciseId, ExerciseName.Create("Bench Press"), [CreateLoggedSet(1)]);

        var ex = Assert.Throws<InvalidOperationException>(() =>
            session.AddExercise(exerciseId, ExerciseName.Create("Bench Press Variant"), [CreateLoggedSet(1)]));

        Assert.Equal("The session already contains this exercise.", ex.Message);
    }

    [Fact]
    public void AddLoggedSet_WithNonSequentialOrder_ThrowsInvalidOperationException()
    {
        var session = CreateSessionWithSingleExercise();
        var sessionExerciseId = session.Exercises[0].Id;

        var ex = Assert.Throws<InvalidOperationException>(() =>
            session.AddLoggedSet(sessionExerciseId, CreateLoggedSet(3)));

        Assert.Equal("Logged set order must be sequential. Expected order 2.", ex.Message);
    }

    [Fact]
    public void RemoveLoggedSet_WithUnknownOrderForExercise_ThrowsInvalidOperationException()
    {
        var session = CreateSessionWithSingleExercise();
        var sessionExerciseId = session.Exercises[0].Id;

        var ex = Assert.Throws<InvalidOperationException>(() =>
            session.RemoveLoggedSet(sessionExerciseId, Order.Create(2)));

        Assert.Equal("The logged set was not found for the session exercise.", ex.Message);
    }

    [Fact]
    public void RemoveLoggedSet_NormalizesRemainingSetOrder()
    {
        var session = CreateSessionWithSingleExercise();
        var sessionExerciseId = session.Exercises[0].Id;

        session.AddLoggedSet(sessionExerciseId, CreateLoggedSet(2));
        session.AddLoggedSet(sessionExerciseId, CreateLoggedSet(3));
        session.AddLoggedSet(sessionExerciseId, CreateLoggedSet(4));

        session.RemoveLoggedSet(sessionExerciseId, Order.Create(3));

        var orders = session.Exercises[0].LoggedSets.Select(x => x.Order.Value).ToArray();
        Assert.Equal([1, 2, 3], orders);
    }

    [Fact]
    public void Complete_WithNonUtcTimestamp_ThrowsArgumentException()
    {
        var session = Session.Start(Guid.NewGuid());
        session.AddExercise(Guid.NewGuid(), ExerciseName.Create("Bench Press"), [CreateLoggedSet(1)]);
        var nonUtc = new DateTimeOffset(2026, 1, 1, 9, 0, 0, TimeSpan.FromHours(2));

        var ex = Assert.Throws<ArgumentException>(() => session.EndSession(nonUtc));

        Assert.Equal("endedAtUtc", ex.ParamName);
    }

    [Fact]
    public void Complete_WithTimestampOutsideAllowedWindow_ThrowsInvalidOperationException()
    {
        var session = Session.Start(Guid.NewGuid());
        session.AddExercise(Guid.NewGuid(), ExerciseName.Create("Bench Press"), [CreateLoggedSet(1)]);

        var ex = Assert.Throws<InvalidOperationException>(() =>
            session.EndSession(DateTimeOffset.UtcNow.AddMinutes(6)));

        Assert.Equal("Session end time is outside the allowed window (from 1 minute in the past to 5 minutes in the future).", ex.Message);
    }

    [Fact]
    public void Complete_WhenEarlierThanStart_ThrowsInvalidOperationException()
    {
        var session = Session.Start(Guid.NewGuid());
        session.AddExercise(Guid.NewGuid(), ExerciseName.Create("Bench Press"), [CreateLoggedSet(1)]);

        var ex = Assert.Throws<InvalidOperationException>(() =>
            session.EndSession(session.StartedAtUtc.AddSeconds(-1)));

        Assert.Equal("Session end time cannot be earlier than session start time.", ex.Message);
    }

    [Fact]
    public void CompletedSession_BlocksFurtherMutations()
    {
        var session = Session.Start(Guid.NewGuid());
        session.AddExercise(Guid.NewGuid(), ExerciseName.Create("Bench Press"), [CreateLoggedSet(1)]);
        session.EndSession(DateTimeOffset.UtcNow);

        var ex = Assert.Throws<InvalidOperationException>(() =>
            session.UpdateNote(SessionNote.Create("post completion update")));

        Assert.Equal("Cannot modify a completed session.", ex.Message);
    }

    private static Session CreateSessionWithSingleExercise()
    {
        var session = Session.Start(Guid.NewGuid());
        session.AddExercise(Guid.NewGuid(), ExerciseName.Create("Bench Press"), [CreateLoggedSet(1)]);
        return session;
    }

    private static LoggedSet CreateLoggedSet(int order)
    {
        return LoggedSet.Create(Order.Create(order), RpeScale.Create(8), repetitions: 10, weight: Weight.Create(60));
    }
}
