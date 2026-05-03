using Microsoft.EntityFrameworkCore;
using Training.Domain.Exercises;
using Training.Infrastructure.Persistence;

namespace Training.Infrastructure.Repositories;

public sealed class ExerciseRepository : IExerciseRepository
{
    private readonly TrainingDbContext _dbContext;

    public ExerciseRepository(TrainingDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Exercise?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var entity = await _dbContext.Exercises.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null)
        {
            return null;
        }

        var secondary = string.IsNullOrWhiteSpace(entity.SecondaryMuscleGroupsCsv)
            ? []
            : entity.SecondaryMuscleGroupsCsv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(MapMuscleGroup)
                .ToArray();

        var requiredEquipment = entity.RequiredEquipmentCsv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(x => Enum.Parse<EquipmentType>(x))
            .ToArray();

        return Exercise.Rehydrate(
            entity.Id,
            ExerciseName.Create(entity.Name),
            ExerciseDescription.Create(entity.Description),
            (DifficultyLevel)entity.DifficultyLevel,
            (ExerciseCategory)entity.Category,
            (MovementPattern)entity.MovementPattern,
            MapMuscleGroup(entity.PrimaryMuscleGroup),
            Hyperlink.Create("https://example.com/instruction"),
            ImageData.Create([1], ImageFormat.Png),
            requiredEquipment,
            secondaryMuscleGroups: secondary);
    }

    public async Task<IReadOnlyList<Exercise>> ListAsync(CancellationToken cancellationToken)
    {
        var entities = await _dbContext.Exercises.AsNoTracking().OrderBy(x => x.Name).ToListAsync(cancellationToken);
        var list = new List<Exercise>(entities.Count);

        foreach (var entity in entities)
        {
            var secondary = string.IsNullOrWhiteSpace(entity.SecondaryMuscleGroupsCsv)
                ? []
                : entity.SecondaryMuscleGroupsCsv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .Select(MapMuscleGroup)
                    .ToArray();

            var requiredEquipment = entity.RequiredEquipmentCsv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(x => Enum.Parse<EquipmentType>(x))
                .ToArray();

            list.Add(Exercise.Rehydrate(
                entity.Id,
                ExerciseName.Create(entity.Name),
                ExerciseDescription.Create(entity.Description),
                (DifficultyLevel)entity.DifficultyLevel,
                (ExerciseCategory)entity.Category,
                (MovementPattern)entity.MovementPattern,
                MapMuscleGroup(entity.PrimaryMuscleGroup),
                Hyperlink.Create("https://example.com/instruction"),
                ImageData.Create([1], ImageFormat.Png),
                requiredEquipment,
                secondaryMuscleGroups: secondary));
        }

        return list;
    }

    public async Task AddAsync(Exercise exercise, CancellationToken cancellationToken)
    {
        await _dbContext.Exercises.AddAsync(new ExerciseEntity
        {
            Id = exercise.Id,
            Name = exercise.Name.Value,
            Description = exercise.Description.Value,
            DifficultyLevel = (int)exercise.DifficultyLevel,
            Category = (int)exercise.Category,
            MovementPattern = (int)exercise.MovementPattern,
            PrimaryMuscleGroup = exercise.PrimaryMuscleGroup.Name.Value,
            SecondaryMuscleGroupsCsv = string.Join(',', exercise.SecondaryMuscleGroups.Select(x => x.Name.Value)),
            RequiredEquipmentCsv = string.Join(',', exercise.RequiredEquipment.Select(x => x.ToString()))
        }, cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Exercise exercise, CancellationToken cancellationToken)
    {
        var entity = await _dbContext.Exercises.FirstOrDefaultAsync(x => x.Id == exercise.Id, cancellationToken);
        if (entity is null)
        {
            return;
        }

        entity.Name = exercise.Name.Value;
        entity.Description = exercise.Description.Value;
        entity.DifficultyLevel = (int)exercise.DifficultyLevel;
        entity.Category = (int)exercise.Category;
        entity.MovementPattern = (int)exercise.MovementPattern;
        entity.PrimaryMuscleGroup = exercise.PrimaryMuscleGroup.Name.Value;
        entity.SecondaryMuscleGroupsCsv = string.Join(',', exercise.SecondaryMuscleGroups.Select(x => x.Name.Value));
        entity.RequiredEquipmentCsv = string.Join(',', exercise.RequiredEquipment.Select(x => x.ToString()));

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken)
    {
        return _dbContext.Exercises.AnyAsync(x => x.Id == id, cancellationToken);
    }

    private static MuscleGroup MapMuscleGroup(string name)
    {
        return MuscleGroup.All.FirstOrDefault(x => x.Name.Value.Equals(name, StringComparison.OrdinalIgnoreCase))
            ?? throw new InvalidOperationException($"Unknown muscle group '{name}'.");
    }
}
