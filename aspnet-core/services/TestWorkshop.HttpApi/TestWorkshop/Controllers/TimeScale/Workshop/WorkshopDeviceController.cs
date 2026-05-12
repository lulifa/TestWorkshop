namespace TestWorkshop;

[Route("api/workshop/device")]
public class WorkshopDeviceController : TestWorkshopController
{
    private readonly IWorkshopDeviceAppService Service;
    public WorkshopDeviceController(IWorkshopDeviceAppService workshopDeviceAppService)
    {
        Service = workshopDeviceAppService;
    }
}
