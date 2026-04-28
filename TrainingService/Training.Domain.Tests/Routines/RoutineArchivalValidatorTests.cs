using Training.Domain.Routines;

namespace Training.Domain.Tests.Routines;

public class RoutineArchivalValidatorTests
{
    private sealed class FakeActiveSessionChecker : IActiveSessionChecker
    {
        private readonly bool _hasActiveSessions;
        public FakeActiveSessionChecker(bool hasActiveSessions) => _hasActiveSessions = hasActiveSessions;
        public bool HasActiveSessions(Guid routineId) => _hasActiveSessions;
    }

    [Fact]
    public void EnsureRoutineCanBeArchived_NoActiveSessions_DoesNotThrow()
    {
        var checker = new FakeActiveSessionChecker(false);
        var validator = new RoutineArchivalValidator(checker);
        validator.EnsureRoutineCanBeArchived(Guid.NewGuid());
    }

    [Fact]
    public void EnsureRoutineCanBeArchived_ActiveSessions_Throws()
    {
        var checker = new FakeActiveSessionChecker(true);
        var validator = new RoutineArchivalValidator(checker);
        Assert.Throws<InvalidOperationException>(() => validator.EnsureRoutineCanBeArchived(Guid.NewGuid()));
    }
}
