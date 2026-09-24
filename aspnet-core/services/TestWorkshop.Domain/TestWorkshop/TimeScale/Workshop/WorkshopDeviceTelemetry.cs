namespace TestWorkshop.TimeScale;

/// <summary>
/// 设备遥测数据实体 - 用于存储下位机上传的实时采集数据
/// 超级表
/// </summary>
public class WorkshopDeviceTelemetry
{
    /// <summary>
    /// 车间设备ID（关联设备表，可通过此ID获取 DevType 等设备基础信息）
    /// </summary>
    public Guid DeviceId { get; set; }

    /// <summary>
    /// 测试任务ID（关联测试任务表）
    /// </summary>
    public long TaskId { get; set; }

    /// <summary>
    /// 数据采集时间（对应上传数据里的 StartTime）
    /// 用于：按时间查询、排序、在界面上展示“该段波形的采集时间”
    /// </summary>
    public DateTime Timestamp { get; set; }


    // ===== 遥测数据本身 =====

    /// <summary>
    /// 通道类型
    /// 可选值：
    /// "PilotDriveFb" = "先导驱动反馈"
    /// "FivaFbCurrent" = "FIVA 阀反馈电流"
    /// "PlungerFb"     = "柱塞反馈"
    /// "FopPressure"   = "出口工作压力"
    /// "BackPressure"  = "背压"
    /// "Flow"          = "流量"
    /// "Cmd"           = "控制给定指令"
    /// "PilotPosFb"    = "先导位置反馈"
    /// </summary>
    public string ChannelType { get; set; }

    /// <summary>
    /// 采集的浮点数数组（波形数据）
    /// 图谱绘制时，X轴 = 数组索引（第几个点），Y轴 = 数组元素值
    /// </summary>
    public double[] Value { get; set; }


    /// <summary>
    /// 被测设备编码（如 "SN12345"）
    /// </summary>
    public string TestedDeviceCode { get; set; }

    /// <summary>
    /// 被测设备名称（如 "一号液压泵"）
    /// </summary>
    public string TestedDeviceName { get; set; }

    /// <summary>
    /// 先导部件序列号（如 "Yk23D875"，Pilot = 先导，非“引航员”）
    /// </summary>
    public string PilotSN { get; set; }

    /// <summary>
    /// 被测设备最终装配的目标船名（如 "CAP SAN JUAN"）
    /// </summary>
    public string ShipName { get; set; }

    /// <summary>
    /// 测试人（如 "Wang.Y.T"）
    /// </summary>
    public string TestBy { get; set; }
}
