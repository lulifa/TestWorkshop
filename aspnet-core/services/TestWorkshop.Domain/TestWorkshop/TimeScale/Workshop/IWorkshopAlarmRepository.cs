namespace TestWorkshop.TimeScale;

public interface IWorkshopAlarmRepository : IBasicRepository<WorkshopAlarm, Guid>
{
    /// <summary>
    /// 查找一条进行中的报警，不存在返回 null
    /// </summary>
    Task<WorkshopAlarm> FindActiveAsync(WorkshopAlarmType alarmType, Guid deviceId, string dataItemType);

    /// <summary>
    /// 获取全部进行中的报警
    /// </summary>
    Task<List<WorkshopAlarm>> GetActiveListAsync();

    /// <summary>
    /// 获取可查询对象（列表页筛选用）
    /// </summary>
    Task<IQueryable<WorkshopAlarm>> GetQueryableAsync();

    /// <summary>
    /// 原子地占用某个通知级别：只有当前级别小于目标级别时才会更新成功并返回 true。
    /// 用它保证"每级只发一次"，同时避免多个 worker 之间的乐观并发冲突。
    /// </summary>
    Task<bool> ClaimLevelAsync(Guid alarmId, int level, DateTime notifiedAt);

    /// <summary>
    /// 原子地标记恢复：只有进行中的报警才能被标记，返回是否成功。
    /// </summary>
    Task<bool> ClaimClearAsync(Guid alarmId, DateTime clearedAt);
}
