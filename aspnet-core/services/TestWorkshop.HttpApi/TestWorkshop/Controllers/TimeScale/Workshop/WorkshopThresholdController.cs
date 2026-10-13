using TestWorkshop.TimeScale;

namespace TestWorkshop;

/// <summary>
/// 阈值管理（按设备类型维护各数据类型的上下限阈值）
/// </summary>
[Route("api/workshop/threshold")]
public class WorkshopThresholdController : TestWorkshopController
{
    private readonly IWorkshopThresholdAppService Service;

    public WorkshopThresholdController(IWorkshopThresholdAppService thresholdAppService)
    {
        Service = thresholdAppService;
    }

    /// <summary>
    /// 获取某个设备类型下的阈值配置（固定返回全部数据类型）
    /// </summary>
    [HttpGet("{deviceType:int}")]
    public virtual async Task<ListResultDto<WorkshopThresholdRuleDto>> GetByDeviceTypeAsync(DeviceTypeEnum deviceType)
    {
        return await Service.GetByDeviceTypeAsync(deviceType);
    }

    /// <summary>
    /// 按设备类型保存阈值配置
    /// </summary>
    [HttpPost("save")]
    public virtual async Task SaveAsync(WorkshopThresholdSaveInput input)
    {
        await Service.SaveAsync(input);
    }
}
