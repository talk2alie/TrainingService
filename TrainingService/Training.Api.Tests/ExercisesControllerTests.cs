using Training.Api.Controllers;
using Training.Api.Requests;
using Training.Application.DTOs;
using Training.Application.UseCases.Exercises;
using Training.Domain.Exercises;
using Microsoft.AspNetCore.Mvc;

namespace Training.Api.Tests;

public sealed class ExercisesControllerTests
{
    [Fact]
    public async Task GetById_WhenNotFound_ReturnsNotFound()
    {
        var controller = new ExercisesController();
        var useCase = new GetExerciseByIdUseCase(new FakeExerciseRepository(null));

        var result = await controller.GetById(Guid.NewGuid(), useCase, CancellationToken.None);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Create_WithKnownMuscleGroup_ReturnsCreated()
    {
        var controller = new ExercisesController();
        var request = new CreateExerciseRequest(
            "Bench Press",
            "Press",
            DifficultyLevel.Beginner,
            ExerciseCategory.Strength,
            MovementPattern.Push,
            "Chest",
            [EquipmentType.Barbell]);

        var useCase = new CreateExerciseUseCase(new FakeExerciseRepository(null));

        var result = await controller.Create(request, useCase, CancellationToken.None);

        Assert.IsType<CreatedAtActionResult>(result);
    }

    [Fact]
    public async Task List_ReturnsOk()
    {
        var controller = new ExercisesController();
        var useCase = new ListExercisesUseCase(new FakeExerciseRepository(null));

        var result = await controller.List(useCase, CancellationToken.None);

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task Update_WhenNotFound_ReturnsNotFound()
    {
        var controller = new ExercisesController();
        var request = new UpdateExerciseRequest(
            "Bench Press",
            "Press",
            DifficultyLevel.Beginner,
            ExerciseCategory.Strength,
            MovementPattern.Push,
            "Chest",
            [EquipmentType.Barbell]);
        var useCase = new UpdateExerciseUseCase(new FakeExerciseRepository(null));

        var result = await controller.Update(Guid.NewGuid(), request, useCase, CancellationToken.None);

        Assert.IsType<NotFoundResult>(result);
    }

    private sealed class FakeExerciseRepository : IExerciseRepository
    {
        private readonly Exercise? _exercise;

        public FakeExerciseRepository(Exercise? exercise)
        {
            _exercise = exercise;
        }

        public Task<Exercise?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            return Task.FromResult(_exercise);
        }

        public Task<IReadOnlyList<Exercise>> ListAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult<IReadOnlyList<Exercise>>(_exercise is null ? [] : [_exercise]);
        }

        public Task AddAsync(Exercise exercise, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task UpdateAsync(Exercise exercise, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken)
        {
            return Task.FromResult(false);
        }
    }
}
