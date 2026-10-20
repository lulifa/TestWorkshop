namespace TestWorkshop.EntityFrameworkCore;

public class WorkshopAlarmRepository
    : EfCoreRepository<TestWorkshopDbContext, WorkshopAlarm, Guid>,
      IWorkshopAlarmRepository
{
    public WorkshopAlarmRepository(IDbContextProvider<TestWorkshopDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }

    public async Task<WorkshopAlarm> FindActiveAsync(
        WorkshopAlarmType alarmType,
        Guid deviceId,
        string dataItemType)
    {
        var dbSet = await GetDbSetAsync();
        return await dbSet.FirstOrDefaultAsync(
            x => x.AlarmType == alarmType
                 && x.DeviceId == deviceId
                 && x.DataItemType == dataItemType
                 && x.Status == WorkshopAlarmStatus.Active);
    }

    public async Task<bool> ClaimClearAsync(Guid alarmId, DateTime clearedAt)
    {
        var dbSet = await GetDbSetAsync();
        var rows = await dbSet
            .Where(x => x.Id == alarmId && x.Status == WorkshopAlarmStatus.Active)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(x => x.Status, WorkshopAlarmStatus.Cleared)
                .SetProperty(x => x.ClearedAt, (DateTime?)clearedAt));

        return rows > 0;
    }

}
