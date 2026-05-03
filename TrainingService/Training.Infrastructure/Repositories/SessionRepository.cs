using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Training.Domain.Common;
using Training.Domain.Exercises;
using Training.Domain.Sessions;
using Training.Infrastructure.Persistence;

namespace Training.Infrastructure.Repositories;

public sealed class SessionRepository : ISessionRepository
{
    private readonly TrainingDbContext _dbContext;

    public SessionRepository(TrainingDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Session?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var entity = await _dbContext.Sessions.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null)
        {
            return null;
        }

        var exerciseDtos = JsonSerializer.Deserialize<List<SessionExerciseJson>>(entity.ExercisesJson) ?? [];
        var sessionExercises = exerciseDtos.Select(exercise =>
        {
            var loggedSets = exercise.LoggedSets.Select(ls => LoggedSet.Create(
                Order.Create(ls.Order),
                (RpeScale)ls.Rpe,
                ls.Repetitions,
                ls.WeightKg.HasValue ? Weight.Create(ls.WeightKg.Value) : null,
                ls.DurationSeconds.HasValue ? Duration.Create(TimeSpan.FromSeconds(ls.DurationSeconds.Value)) : null,
                ls.DistanceMeters.HasValue ? Distance.Create(ls.DistanceMeters.Value) : null,
                ls.HeartRateBpm.HasValue ? HeartRate.Create(ls.HeartRateBpm.Value) : null,
                ls.Note is null ? null : LogNote.Create(ls.Note))).ToArray();

            return SessionExercise.Rehydrate(
                exercise.Id,
                exercise.ExerciseId,
                ExerciseName.Create(exercise.ExerciseName),
                Order.Create(exercise.Order),
                loggedSets);
        }).ToArray();

        return Session.Rehydrate(
            entity.Id,
            entity.OwnerUserId,
            entity.RoutineId,
            entity.StartedAtUtc,
            sessionExercises,
            null,
            entity.EndedAtUtc,
            entity.Note is null ? null : SessionNote.Create(entity.Note));
    }

    public async Task<IReadOnlyList<Session>> ListByOwnerAsync(Guid ownerUserId, CancellationToken cancellationToken)
    {
        var entities = await _dbContext.Sessions.AsNoTracking()
            .Where(x => x.OwnerUserId == ownerUserId)
            .OrderByDescending(x => x.StartedAtUtc)
            .ToListAsync(cancellationToken);

        var result = new List<Session>(entities.Count);
        foreach (var entity in entities)
        {
            var session = await GetByIdAsync(entity.Id, cancellationToken);
            if (session is not null)
            {
                result.Add(session);
            }
        }

        return result;
    }

    public async Task AddAsync(Session session, CancellationToken cancellationToken)
    {
        await _dbContext.Sessions.AddAsync(Map(session), cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Session session, CancellationToken cancellationToken)
    {
        var entity = await _dbContext.Sessions.FirstOrDefaultAsync(x => x.Id == session.Id, cancellationToken);
        if (entity is null)
        {
            return;
        }

        var mapped = Map(session);
        entity.RoutineId = mapped.RoutineId;
        entity.EndedAtUtc = mapped.EndedAtUtc;
        entity.Note = mapped.Note;
        entity.ExercisesJson = mapped.ExercisesJson;

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken)
    {
        return _dbContext.Sessions.AnyAsync(x => x.Id == id, cancellationToken);
    }

    private static SessionEntity Map(Session session)
    {
        var exercises = session.Exercises.Select(x => new SessionExerciseJson(
            x.Id,
            x.ExerciseId,
            x.ExerciseNameSnapshot.Value,
            x.Order.Value,
            x.LoggedSets.Select(ls => new LoggedSetJson(
                ls.Order.Value,
                (int)ls.Rpe,
                ls.Repetitions,
                ls.Weight?.Kilograms,
                ls.Duration is null ? null : (int)ls.Duration.Value.TotalSeconds,
                ls.Distance?.Meters,
                ls.HeartRate?.BeatsPerMinute,
                ls.Note?.Value)).ToArray())).ToArray();

        return new SessionEntity
        {
            Id = session.Id,
            OwnerUserId = session.OwnerUserId,
            RoutineId = session.RoutineId,
            StartedAtUtc = session.StartedAtUtc,
            EndedAtUtc = session.EndedAtUtc,
            Note = session.Note?.Value,
            ExercisesJson = JsonSerializer.Serialize(exercises)
        };
    }
}
