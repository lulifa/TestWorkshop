namespace TestWorkshop.TimeScale;

public interface IWorkshopAlarmRepository : IBasicRepository<WorkshopAlarm, Guid>
{
    /// <summary>
    /// 查找一条进行中的报警，不存在返回 null
    /// </summary>
    Task<WorkshopAlarm> FindActiveAsync(WorkshopAlarmType alarmType, Guid deviceId, string dataItemType);

    /// <summary>
    /// 获取可查询对象（列表页筛选用）
    /// </summary>
    Task<IQueryable<WorkshopAlarm>> GetQueryableAsync();

    /// <summary>
    /// 原子地标记恢复：只有进行中的报警才能被标记，返回是否成功。
    /// </summary>
    Task<bool> ClaimClearAsync(Guid alarmId, DateTime clearedAt);

}
