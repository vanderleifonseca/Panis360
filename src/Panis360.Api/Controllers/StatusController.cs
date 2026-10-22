using Microsoft.AspNetCore.Mvc;

namespace Panis360.Api.Controllers;

[ApiController]
[Route("api/v1/status")]
public class StatusController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new
        {
            sistema = "Panis360",
            status = "API em funcionamento",
            versao = "0.1.0",
            dataHoraUtc = DateTimeOffset.UtcNow
        });
    }
}