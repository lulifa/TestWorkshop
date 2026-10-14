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

    public async Task<List<WorkshopAlarm>> GetActiveListAsync()
    {
        var dbSet = await GetDbSetAsync();
        return await dbSet
            .Where(x => x.Status == WorkshopAlarmStatus.Active)
            .ToListAsync();
    }

    public async Task<bool> ClaimLevelAsync(Guid alarmId, int level, DateTime notifiedAt)
    {
        var dbSet = await GetDbSetAsync();
        var rows = await dbSet
            .Where(x => x.Id == alarmId && x.Level < level)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(x => x.Level, level)
                .SetProperty(x => x.LastNotifiedAt, (DateTime?)notifiedAt));

        return rows > 0;
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
