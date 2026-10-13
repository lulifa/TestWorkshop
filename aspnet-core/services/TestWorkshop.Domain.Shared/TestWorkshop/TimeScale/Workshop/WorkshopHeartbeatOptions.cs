namespace TestWorkshop.TimeScale;

/// <summary>
/// 设备心跳监控与通信报警分级配置。
/// 断线时长达到某级阈值时通知对应范围的人员，只向上逐级升级通知。
/// </summary>
public class WorkshopHeartbeatOptions
{
    /// <summary>
    /// 心跳监控扫描间隔（分钟）。
    /// </summary>
    public int CheckIntervalMinutes { get; set; } = 1;

    /// <summary>
    /// 一级阈值（分钟）：超过后通知车间测试工程师。
    /// </summary>
    public int Level1Minutes { get; set; } = 5;

    /// <summary>
    /// 二级阈值（分钟）：超过后通知测试工程师 + 车间主管。
    /// </summary>
    public int Level2Minutes { get; set; } = 10;

    /// <summary>
    /// 三级阈值（分钟）：超过后通知测试工程师 + 车间主管 + 系统管理员。
    /// </summary>
    public int Level3Minutes { get; set; } = 20;

    public void Validate()
    {
        if (CheckIntervalMinutes <= 0)
        {
            throw new ArgumentException("WorkshopHeartbeat:CheckIntervalMinutes 必须大于 0");
        }

        if (Level1Minutes <= 0)
        {
            throw new ArgumentException("WorkshopHeartbeat:Level1Minutes 必须大于 0");
        }

        if (Level2Minutes <= Level1Minutes)
        {
            throw new ArgumentException("WorkshopHeartbeat:Level2Minutes 必须大于 Level1Minutes");
        }

        if (Level3Minutes <= Level2Minutes)
        {
            throw new ArgumentException("WorkshopHeartbeat:Level3Minutes 必须大于 Level2Minutes");
        }
    }
}
