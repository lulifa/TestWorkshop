namespace TestWorkshop.TimeScale;

/// <summary>
/// 报警升级配置。所有报警类型共用一套。
/// 条件持续满足 5 / 10 / 20 分钟，依次升级通知范围。
/// </summary>
public class WorkshopAlarmOptions
{
    /// <summary>
    /// 报警扫描间隔（分钟）
    /// </summary>
    public int CheckIntervalMinutes { get; set; } = 1;

    /// <summary>
    /// 一级阈值（分钟）：通知车间测试工程师
    /// </summary>
    public int Level1Minutes { get; set; } = 5;

    /// <summary>
    /// 二级阈值（分钟）：通知测试工程师 + 车间主管
    /// </summary>
    public int Level2Minutes { get; set; } = 10;

    /// <summary>
    /// 三级阈值（分钟）：通知测试工程师 + 车间主管 + 系统管理员
    /// </summary>
    public int Level3Minutes { get; set; } = 20;

    public void Validate()
    {
        if (CheckIntervalMinutes <= 0)
        {
            throw new ArgumentException("WorkshopAlarm:CheckIntervalMinutes 必须大于 0");
        }

        if (Level1Minutes <= 0)
        {
            throw new ArgumentException("WorkshopAlarm:Level1Minutes 必须大于 0");
        }

        if (Level2Minutes <= Level1Minutes)
        {
            throw new ArgumentException("WorkshopAlarm:Level2Minutes 必须大于 Level1Minutes");
        }

        if (Level3Minutes <= Level2Minutes)
        {
            throw new ArgumentException("WorkshopAlarm:Level3Minutes 必须大于 Level2Minutes");
        }
    }
}
