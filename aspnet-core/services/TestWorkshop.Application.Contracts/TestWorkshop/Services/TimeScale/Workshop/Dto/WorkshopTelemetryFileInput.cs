namespace TestWorkshop;

/// <summary>
/// 上传遥测文件请求参数
/// </summary>
public class WorkshopTelemetryFileInput
{
    /// <summary>
    /// 通道类型（如 PilotDriveFb）
    /// </summary>
    [Required]
    [DynamicStringLength(typeof(TestWorkshopConsts), nameof(TestWorkshopConsts.MaxLength128))]
    public string ChannelType { get; set; }

    /// <summary>
    /// 当前上传的文件名
    /// </summary>
    public string CurrentFile { get; set; }

    /// <summary>
    /// 设备编码
    /// </summary>
    [Required]
    [DynamicStringLength(typeof(TestWorkshopConsts), nameof(TestWorkshopConsts.MaxLength64))]
    public string DeviceCode { get; set; }


    /// <summary>
    /// 文件生成时间（格式：yyyy-MM-dd HH:mm:ss）
    /// </summary>
    [Required]
    public string FileTime { get; set; }

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
    [Required]
    public TestInfoInput TestInfo { get; set; }
}

/// <summary>
/// 测试信息
/// </summary>
public class TestInfoInput
{
    /// <summary>
    /// 先导部件序列号（如 "Yk23D875"，Pilot = 先导，非“引航员”）
    /// </summary>
    [DynamicStringLength(typeof(TestWorkshopConsts), nameof(TestWorkshopConsts.MaxLength64))]
    public string PilotSN { get; set; }

    /// <summary>
    /// 船名
    /// </summary>
    [DynamicStringLength(typeof(TestWorkshopConsts), nameof(TestWorkshopConsts.MaxLength128))]
    public string ShipName { get; set; }

    /// <summary>
    /// 测试开始时间1（格式：yyyy-MM-dd HH:mm:ss，可为空）
    /// </summary>
    public string StartTime { get; set; }

    /// <summary>
    /// 测试结束时间1（格式：yyyy-MM-dd HH:mm:ss，可为空）
    /// </summary>
    public string EndTime { get; set; }

    /// <summary>
    /// 测试开始时间2（格式：yyyy-MM-dd HH:mm:ss，可为空）
    /// </summary>
    public string StartTime2 { get; set; }

    /// <summary>
    /// 测试结束时间2（格式：yyyy-MM-dd HH:mm:ss，可为空）
    /// </summary>
    public string EndTime2 { get; set; }

    /// <summary>
    /// 测试人
    /// </summary>
    [DynamicStringLength(typeof(TestWorkshopConsts), nameof(TestWorkshopConsts.MaxLength64))]
    public string TestBy { get; set; }

    /// <summary>
    /// 测试日期（格式：yyyy-MM-dd，可为空）
    /// </summary>
    public string TestDate { get; set; }

    /// <summary>
    /// 被测设备编码
    /// </summary>
    [DynamicStringLength(typeof(TestWorkshopConsts), nameof(TestWorkshopConsts.MaxLength64))]
    public string TestedDeviceCode { get; set; }

    /// <summary>
    /// 被测设备名称
    /// </summary>
    [DynamicStringLength(typeof(TestWorkshopConsts), nameof(TestWorkshopConsts.MaxLength128))]
    public string TestedDeviceName { get; set; }
}
