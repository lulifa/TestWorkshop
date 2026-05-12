namespace TestWorkshop;

[Route("api/workshop")]
public class WorkshopController : TestWorkshopController
{
    private readonly IWorkshopAppService Service;
    public WorkshopController(IWorkshopAppService workshopAppService)
    {
        Service = workshopAppService;
    }
}
