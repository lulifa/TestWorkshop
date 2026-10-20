namespace TestWorkshop.TimeScale;

/// <summary>
/// 报警检测与恢复配置。
/// </summary>
public class WorkshopAlarmOptions
{
    /// <summary>
    /// 报警扫描间隔（秒）
    /// </summary>
    public int CheckIntervalSeconds { get; set; } = 10;

    /// <summary>
    /// 通信中断判定（秒）：设备超过这么久没有心跳即产生通信报警
    /// </summary>
    public int CommunicationTimeoutSeconds { get; set; } = 10;

    /// <summary>
    /// 运行时长累计容差（秒）：相邻心跳间隔不超过该值时才计入设备运行时长。
    /// </summary>
    public int RuntimeGapToleranceSeconds { get; set; } = 6;

    public void Validate()
    {
        if (CheckIntervalSeconds <= 0)
        {
            throw new ArgumentException("WorkshopAlarm:CheckIntervalSeconds 必须大于 0");
        }

        if (CommunicationTimeoutSeconds <= 0)
        {
            throw new ArgumentException("WorkshopAlarm:CommunicationTimeoutSeconds 必须大于 0");
        }

        if (RuntimeGapToleranceSeconds <= 0)
        {
            throw new ArgumentException("WorkshopAlarm:RuntimeGapToleranceSeconds 必须大于 0");
        }

    }
}
