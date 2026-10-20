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
    public int CommunicationTimeoutSeconds { get; set; } = 30;

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

    }
}
