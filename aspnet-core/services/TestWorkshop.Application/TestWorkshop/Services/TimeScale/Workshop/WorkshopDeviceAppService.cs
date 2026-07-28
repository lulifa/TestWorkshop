using TestWorkshop.TimeScale;

namespace TestWorkshop;

public class WorkshopDeviceAppService : TestWorkshopAppService, IWorkshopDeviceAppService
{
    protected IWorkshopDeviceRepository WorkshopDeviceRepository { get; }
    public WorkshopDeviceAppService(IWorkshopDeviceRepository workshopDeviceRepository)
    {
        WorkshopDeviceRepository = workshopDeviceRepository;
    }




}
