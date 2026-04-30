using Microsoft.AspNetCore.Http;

namespace TestWorkshop;

[Route("api/telemetry")]
public class TelemetryController : TestWorkshopController, ITelemetryAppService
{
    private readonly ITelemetryAppService _service;
    public TelemetryController(ITelemetryAppService service)
    {
        _service = service;
    }

    public async Task Upload(IFormFile file)
    {
        await _service.Upload(file);
    }
}
