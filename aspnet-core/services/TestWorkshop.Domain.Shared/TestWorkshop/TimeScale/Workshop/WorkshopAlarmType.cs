namespace TestWorkshop.TimeScale;

/// <summary>
/// 报警类型（4 大类）
/// </summary>
public enum WorkshopAlarmType
{
    /// <summary>
    /// 阈值报警：实时值超出阈值上下限
    /// </summary>
    Threshold = 1,

    /// <summary>
    /// 关联报警：跨参数公式换算出的正常范围校验（公式待提供，暂未实现）
    /// </summary>
    Relation = 2,

    /// <summary>
    /// 设备报警：传感器数值异常、测试台急停等
    /// </summary>
    Device = 3,

    /// <summary>
    /// 通信报警：心跳间隔超时
    /// </summary>
    Communication = 4
}
