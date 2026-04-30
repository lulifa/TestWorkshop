using Microsoft.AspNetCore.Http;

namespace TestWorkshop;

public interface ITelemetryAppService
{

    Task Upload(IFormFile file);

}
