using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Training.Api.Requests;
using Training.Application.UseCases.Exercises;
using Training.Domain.Exercises;

namespace Training.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Authorize]
[Route("api/v{version:apiVersion}/exercises")]
public sealed class ExercisesController : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateExerciseRequest request,
        [FromServices] CreateExerciseUseCase useCase,
        CancellationToken cancellationToken)
    {
        var muscleGroup = MuscleGroup.All.FirstOrDefault(x => x.Name.Value.Equals(request.PrimaryMuscleGroup, StringComparison.OrdinalIgnoreCase));
        if (muscleGroup is null)
        {
            return BadRequest(new { error = "Unknown primary muscle group." });
        }

        var response = await useCase.ExecuteAsync(
            request.Name,
            request.Description,
            request.DifficultyLevel,
            request.Category,
            request.MovementPattern,
            muscleGroup,
            request.RequiredEquipment,
            cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = response.Id, version = "1.0" }, response);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(
        Guid id,
        [FromServices] GetExerciseByIdUseCase useCase,
        CancellationToken cancellationToken)
    {
        var response = await useCase.ExecuteAsync(id, cancellationToken);
        if (response is null)
        {
            return NotFound();
        }

        return Ok(response);
    }

    [HttpPatch("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateExerciseRequest request,
        [FromServices] UpdateExerciseUseCase useCase,
        CancellationToken cancellationToken)
    {
        var muscleGroup = MuscleGroup.All.FirstOrDefault(x => x.Name.Value.Equals(request.PrimaryMuscleGroup, StringComparison.OrdinalIgnoreCase));
        if (muscleGroup is null)
        {
            return BadRequest(new { error = "Unknown primary muscle group." });
        }

        var updated = await useCase.ExecuteAsync(
            id,
            request.Name,
            request.Description,
            request.DifficultyLevel,
            request.Category,
            request.MovementPattern,
            muscleGroup,
            request.RequiredEquipment,
            cancellationToken);

        if (updated is null)
        {
            return NotFound();
        }

        return Ok(updated);
    }

    [HttpGet]
    public async Task<IActionResult> List(
        [FromServices] ListExercisesUseCase useCase,
        CancellationToken cancellationToken)
    {
        var response = await useCase.ExecuteAsync(cancellationToken);
        return Ok(response);
    }
}
