using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

namespace Training.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/training")]
public sealed class TrainingController : ControllerBase
{
    [HttpGet("status")]
    public IActionResult GetStatus()
    {
        return Ok(new
        {
            service = "Training.Api",
            status = "ok"
        });
    }
}
