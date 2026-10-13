namespace TestWorkshop;

public interface IWorkshopThresholdAppService : IApplicationService
{
    /// <summary>
    /// 获取某个设备类型下、全部数据类型的阈值配置（未配置的数据类型也会返回，便于界面按固定行展示）
    /// </summary>
    Task<ListResultDto<WorkshopThresholdRuleDto>> GetByDeviceTypeAsync(DeviceTypeEnum deviceType);

    /// <summary>
    /// 按设备类型保存阈值配置
    /// </summary>
    Task SaveAsync(WorkshopThresholdSaveInput input);
}
