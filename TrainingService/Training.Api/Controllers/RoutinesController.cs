using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Training.Api.Requests;
using Training.Application.UseCases.Routines;

namespace Training.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Authorize]
[Route("api/v{version:apiVersion}/routines")]
public sealed class RoutinesController : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateRoutineRequest request,
        [FromServices] CreateRoutineUseCase useCase,
        CancellationToken cancellationToken)
    {
        var response = await useCase.ExecuteAsync(request.Name, request.Description, request.DifficultyLevel, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = response.Id, version = "1.0" }, response);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(
        Guid id,
        [FromServices] GetRoutineByIdUseCase useCase,
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
        [FromBody] UpdateRoutineRequest request,
        [FromServices] UpdateRoutineUseCase useCase,
        CancellationToken cancellationToken)
    {
        var updated = await useCase.ExecuteAsync(id, request.Name, request.Description, request.DifficultyLevel, cancellationToken);
        if (updated is null)
        {
            return NotFound();
        }

        return Ok(updated);
    }

    [HttpGet]
    public async Task<IActionResult> List(
        [FromServices] ListRoutinesUseCase useCase,
        CancellationToken cancellationToken)
    {
        var response = await useCase.ExecuteAsync(cancellationToken);
        return Ok(response);
    }

    [HttpPost("{id:guid}/archive")]
    public async Task<IActionResult> Archive(
        Guid id,
        [FromServices] ArchiveRoutineUseCase useCase,
        CancellationToken cancellationToken)
    {
        var archived = await useCase.ExecuteAsync(id, cancellationToken);
        if (!archived)
        {
            return NotFound();
        }

        return NoContent();
    }
}
