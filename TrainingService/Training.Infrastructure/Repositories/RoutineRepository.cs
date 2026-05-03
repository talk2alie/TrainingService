using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Training.Domain.Common;
using Training.Domain.Exercises;
using Training.Domain.Routines;
using Training.Domain.Sessions;
using Training.Infrastructure.Persistence;

namespace Training.Infrastructure.Repositories;

public sealed class RoutineRepository : IRoutineRepository
{
    private readonly TrainingDbContext _dbContext;

    public RoutineRepository(TrainingDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Routine?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var entity = await _dbContext.Routines.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null)
        {
            return null;
        }

        var exerciseDtos = JsonSerializer.Deserialize<List<RoutineExerciseJson>>(entity.ExercisesJson) ?? [];

        var routineExercises = exerciseDtos.Select(ex =>
        {
            var plannedSets = ex.PlannedSets.Select(ps => PlannedSet.Create(
                Order.Create(ps.Order),
                ps.Repetitions,
                ps.WeightKg.HasValue ? Weight.Create(ps.WeightKg.Value) : null,
                ps.DurationSeconds.HasValue ? Duration.Create(TimeSpan.FromSeconds(ps.DurationSeconds.Value)) : null,
                ps.DistanceMeters.HasValue ? Distance.Create(ps.DistanceMeters.Value) : null,
                ps.TargetRpe.HasValue ? (RpeScale)ps.TargetRpe.Value : null)).ToArray();

            return RoutineExercise.Rehydrate(
                ex.Id,
                ex.ExerciseId,
                ExerciseName.Create(ex.ExerciseName),
                Order.Create(ex.Order),
                plannedSets);
        }).ToArray();

        return Routine.Rehydrate(
            entity.Id,
            entity.OwnerUserId,
            entity.CreatedAtUtc,
            RoutineName.Create(entity.Name),
            RoutineDescription.Create(entity.Description),
            (DifficultyLevel)entity.DifficultyLevel,
            routineExercises,
            entity.ArchivedAtUtc);
    }

    public async Task<IReadOnlyList<Routine>> ListByOwnerAsync(Guid ownerUserId, CancellationToken cancellationToken)
    {
        var entities = await _dbContext.Routines.AsNoTracking()
            .Where(x => x.OwnerUserId == ownerUserId)
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        var result = new List<Routine>(entities.Count);
        foreach (var entity in entities)
        {
            var routine = await GetByIdAsync(entity.Id, cancellationToken);
            if (routine is not null)
            {
                result.Add(routine);
            }
        }

        return result;
    }

    public async Task AddAsync(Routine routine, CancellationToken cancellationToken)
    {
        await _dbContext.Routines.AddAsync(Map(routine), cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Routine routine, CancellationToken cancellationToken)
    {
        var entity = await _dbContext.Routines.FirstOrDefaultAsync(x => x.Id == routine.Id, cancellationToken);
        if (entity is null)
        {
            return;
        }

        var mapped = Map(routine);
        entity.Name = mapped.Name;
        entity.Description = mapped.Description;
        entity.DifficultyLevel = mapped.DifficultyLevel;
        entity.ArchivedAtUtc = mapped.ArchivedAtUtc;
        entity.ExercisesJson = mapped.ExercisesJson;

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken)
    {
        return _dbContext.Routines.AnyAsync(x => x.Id == id, cancellationToken);
    }

    private static RoutineEntity Map(Routine routine)
    {
        var exercises = routine.Exercises.Select(x => new RoutineExerciseJson(
            x.Id,
            x.ExerciseId,
            x.ExerciseName.Value,
            x.Order.Value,
            x.PlannedSets.Select(ps => new PlannedSetJson(
                ps.Order.Value,
                ps.Repetitions,
                ps.Weight?.Kilograms,
                ps.Duration is null ? null : (int)ps.Duration.Value.TotalSeconds,
                ps.Distance?.Meters,
                ps.TargetRpe is null ? null : (int)ps.TargetRpe.Value)).ToArray())).ToArray();

        return new RoutineEntity
        {
            Id = routine.Id,
            OwnerUserId = routine.OwnerUserId,
            CreatedAtUtc = routine.CreatedAtUtc,
            Name = routine.Name.Value,
            Description = routine.Description.Value,
            DifficultyLevel = (int)routine.DifficultyLevel,
            ArchivedAtUtc = routine.ArchivedAtUtc,
            ExercisesJson = JsonSerializer.Serialize(exercises)
        };
    }
}
