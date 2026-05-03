using Microsoft.EntityFrameworkCore;
using Training.Domain.Exercises;
using Training.Domain.Routines;
using Training.Domain.Sessions;
using Training.Domain.Common;
using System.Text.Json;

namespace Training.Infrastructure.Persistence;

public sealed class TrainingDbContext : DbContext
{
    public TrainingDbContext(DbContextOptions<TrainingDbContext> options) : base(options)
    {
    }

    public DbSet<ExerciseEntity> Exercises => Set<ExerciseEntity>();
    public DbSet<RoutineEntity> Routines => Set<RoutineEntity>();
    public DbSet<SessionEntity> Sessions => Set<SessionEntity>();
    public DbSet<OutboxMessageEntity> OutboxMessages => Set<OutboxMessageEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ExerciseEntity>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(120).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(2000).IsRequired();
            entity.Property(x => x.PrimaryMuscleGroup).HasMaxLength(120).IsRequired();
            entity.Property(x => x.RequiredEquipmentCsv).HasMaxLength(512).IsRequired();
            entity.Property(x => x.SecondaryMuscleGroupsCsv).HasMaxLength(1024).IsRequired();
            entity.Property(x => x.RowVersion).IsRowVersion().IsConcurrencyToken();
        });

        modelBuilder.Entity<RoutineEntity>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(120).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(2000).IsRequired();
            entity.Property(x => x.ExercisesJson).HasColumnType("nvarchar(max)").IsRequired();
            entity.Property(x => x.RowVersion).IsRowVersion().IsConcurrencyToken();
            entity.HasIndex(x => new { x.OwnerUserId, x.CreatedAtUtc });
        });

        modelBuilder.Entity<SessionEntity>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.ExercisesJson).HasColumnType("nvarchar(max)").IsRequired();
            entity.Property(x => x.RowVersion).IsRowVersion().IsConcurrencyToken();
            entity.HasIndex(x => new { x.OwnerUserId, x.StartedAtUtc });
        });

        modelBuilder.Entity<OutboxMessageEntity>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Type).HasMaxLength(512).IsRequired();
            entity.Property(x => x.PayloadJson).HasColumnType("nvarchar(max)").IsRequired();
            entity.HasIndex(x => x.ProcessedAtUtc);
            entity.HasIndex(x => x.OccurredAtUtc);
        });
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var outboxMessages = new List<OutboxMessageEntity>();

        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.Entity is IHasDomainEvents aggregate)
            {
                foreach (var domainEvent in aggregate.DomainEvents)
                {
                    outboxMessages.Add(new OutboxMessageEntity
                    {
                        Id = Guid.NewGuid(),
                        OccurredAtUtc = domainEvent.OccurredAtUtc,
                        Type = domainEvent.GetType().FullName ?? domainEvent.GetType().Name,
                        PayloadJson = JsonSerializer.Serialize(domainEvent)
                    });
                }

                aggregate.ClearDomainEvents();
            }
        }

        if (outboxMessages.Count > 0)
        {
            OutboxMessages.AddRange(outboxMessages);
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}

public sealed class ExerciseEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int DifficultyLevel { get; set; }
    public int Category { get; set; }
    public int MovementPattern { get; set; }
    public string PrimaryMuscleGroup { get; set; } = string.Empty;
    public string SecondaryMuscleGroupsCsv { get; set; } = string.Empty;
    public string RequiredEquipmentCsv { get; set; } = string.Empty;
    public byte[] RowVersion { get; set; } = [];
}

public sealed class RoutineEntity
{
    public Guid Id { get; set; }
    public Guid OwnerUserId { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int DifficultyLevel { get; set; }
    public DateTimeOffset? ArchivedAtUtc { get; set; }
    public string ExercisesJson { get; set; } = "[]";
    public byte[] RowVersion { get; set; } = [];
}

public sealed class SessionEntity
{
    public Guid Id { get; set; }
    public Guid OwnerUserId { get; set; }
    public Guid? RoutineId { get; set; }
    public DateTimeOffset StartedAtUtc { get; set; }
    public DateTimeOffset? EndedAtUtc { get; set; }
    public string? Note { get; set; }
    public string ExercisesJson { get; set; } = "[]";
    public byte[] RowVersion { get; set; } = [];
}
