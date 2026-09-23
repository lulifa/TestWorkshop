namespace TestWorkshop;

/// <summary>
/// 上传遥测文件请求参数
/// </summary>
public class WorkshopTelemetryFileInput
{
    /// <summary>
    /// 通道类型（如 PilotDriveFb）
    /// </summary>
    public string ChannelType { get; set; }

    /// <summary>
    /// 当前上传的文件名
    /// </summary>
    public string CurrentFile { get; set; }

    /// <summary>
    /// 设备编码
    /// </summary>
    public string DeviceCode { get; set; }

    /// <summary>
    /// 本次任务包含的文件总数
    /// </summary>
    public int FileCount { get; set; }

    /// <summary>
    /// 分组标识（同一批次文件的归属组，如 FIVA_@2025-6-13-15-40-44）
    /// </summary>
    public string Group { get; set; }

    /// <summary>
    /// 关联的 .if 源文件名（如 @2025-6-13-15-40-44.if）
    /// </summary>
    public string IfSource { get; set; }

    /// <summary>
    /// 数据源标识（如 @2025-6-13-15-45-34）
    /// </summary>
    public string Source { get; set; }

    /// <summary>
    /// 测试信息
    /// </summary>
    public TestInfoInput TestInfo { get; set; }
}

/// <summary>
/// 测试信息
/// </summary>
public class TestInfoInput
{
    /// <summary>
    /// 设备类型（如 NAB-II-60）
    /// </summary>
    public string DevType { get; set; }

    /// <summary>
    /// 引航员序列号
    /// </summary>
    public string PilotSN { get; set; }

    /// <summary>
    /// 船名
    /// </summary>
    public string ShipName { get; set; }

    /// <summary>
    /// 测试开始时间（格式：yyyy-MM-dd HH:mm:ss）
    /// </summary>
    public string StartTime { get; set; }

    /// <summary>
    /// 测试人
    /// </summary>
    public string TestBy { get; set; }

    /// <summary>
    /// 测试日期（格式：yyyy-MM-dd）
    /// </summary>
    public string TestDate { get; set; }

    /// <summary>
    /// 被测设备编码
    /// </summary>
    public string TestedDeviceCode { get; set; }

    /// <summary>
    /// 被测设备名称
    /// </summary>
    public string TestedDeviceName { get; set; }
}
