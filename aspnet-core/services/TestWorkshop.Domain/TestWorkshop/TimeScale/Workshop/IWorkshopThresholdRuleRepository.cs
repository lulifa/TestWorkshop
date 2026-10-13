namespace TestWorkshop.TimeScale;

public interface IWorkshopThresholdRuleRepository : IBasicRepository<WorkshopThresholdRule, Guid>
{
    /// <summary>
    /// 获取某个设备类型下的全部阈值规则
    /// </summary>
    Task<List<WorkshopThresholdRule>> GetListByDeviceTypeAsync(DeviceTypeEnum deviceType);

    /// <summary>
    /// 按设备类型 + 数据类型查找阈值规则，不存在返回 null
    /// </summary>
    Task<WorkshopThresholdRule> FindAsync(DeviceTypeEnum deviceType, string dataItemType);
}
