using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Training.Api.Requests;
using Training.Application.UseCases.Sessions;

namespace Training.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Authorize]
[Route("api/v{version:apiVersion}/sessions")]
public sealed class SessionsController : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Start(
        [FromBody] StartSessionRequest request,
        [FromServices] StartSessionUseCase useCase,
        CancellationToken cancellationToken)
    {
        var response = await useCase.ExecuteAsync(request.RoutineId, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = response.Id, version = "1.0" }, response);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(
        Guid id,
        [FromServices] GetSessionByIdUseCase useCase,
        CancellationToken cancellationToken)
    {
        var response = await useCase.ExecuteAsync(id, cancellationToken);
        if (response is null)
        {
            return NotFound();
        }

        return Ok(response);
    }

    [HttpGet("{id:guid}/detail")]
    public async Task<IActionResult> GetDetail(
        Guid id,
        [FromServices] GetSessionDetailUseCase useCase,
        CancellationToken cancellationToken)
    {
        var response = await useCase.ExecuteAsync(id, cancellationToken);
        if (response is null)
        {
            return NotFound();
        }

        return Ok(response);
    }

    [HttpPatch("{id:guid}/note")]
    public async Task<IActionResult> UpdateNote(
        Guid id,
        [FromBody] UpdateSessionNoteRequest request,
        [FromServices] UpdateSessionNoteUseCase useCase,
        CancellationToken cancellationToken)
    {
        var updated = await useCase.ExecuteAsync(id, request.Note, cancellationToken);
        if (!updated)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpGet]
    public async Task<IActionResult> List(
        [FromServices] ListSessionsUseCase useCase,
        CancellationToken cancellationToken)
    {
        var response = await useCase.ExecuteAsync(cancellationToken);
        return Ok(response);
    }

    [HttpPost("{id:guid}/end")]
    public async Task<IActionResult> End(
        Guid id,
        [FromBody] EndSessionRequest request,
        [FromServices] EndSessionUseCase useCase,
        CancellationToken cancellationToken)
    {
        var ended = await useCase.ExecuteAsync(id, request.EndedAtUtc, cancellationToken);
        if (!ended)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpPost("{id:guid}/exercises/{sessionExerciseId:guid}/logged-sets")]
    public async Task<IActionResult> AddLoggedSet(
        Guid id,
        Guid sessionExerciseId,
        [FromBody] AddSessionLoggedSetRequest request,
        [FromServices] AddSessionLoggedSetUseCase useCase,
        CancellationToken cancellationToken)
    {
        var added = await useCase.ExecuteAsync(
            id,
            sessionExerciseId,
            request.Repetitions,
            request.WeightKg,
            request.DurationSeconds,
            request.DistanceMeters,
            request.Rpe,
            request.HeartRateBpm,
            request.Note,
            cancellationToken);

        if (!added)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpDelete("{id:guid}/exercises/{sessionExerciseId:guid}/logged-sets")]
    public async Task<IActionResult> RemoveLoggedSet(
        Guid id,
        Guid sessionExerciseId,
        [FromBody] RemoveSessionLoggedSetRequest request,
        [FromServices] RemoveSessionLoggedSetUseCase useCase,
        CancellationToken cancellationToken)
    {
        var removed = await useCase.ExecuteAsync(id, sessionExerciseId, request.SetOrder, cancellationToken);
        if (!removed)
        {
            return NotFound();
        }

        return NoContent();
    }
}
