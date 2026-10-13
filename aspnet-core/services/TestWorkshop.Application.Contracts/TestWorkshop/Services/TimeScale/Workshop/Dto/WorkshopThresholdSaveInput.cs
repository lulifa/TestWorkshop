namespace TestWorkshop;

/// <summary>
/// 按设备类型保存阈值规则（一次提交该类型下的全部数据类型）
/// </summary>
public class WorkshopThresholdSaveInput
{
    /// <summary>
    /// 设备类型
    /// </summary>
    [Required]
    public DeviceTypeEnum DeviceType { get; set; }

    /// <summary>
    /// 各数据类型的阈值配置
    /// </summary>
    [Required]
    public List<WorkshopThresholdItemInput> Items { get; set; } = new();
}

/// <summary>
/// 单个数据类型的阈值配置
/// </summary>
public class WorkshopThresholdItemInput
{
    /// <summary>
    /// 数据类型（物理量编码）
    /// </summary>
    [Required]
    [StringLength(64)]
    public string DataItemType { get; set; }

    /// <summary>
    /// 上限阈值
    /// </summary>
    public decimal? UpperLimit { get; set; }

    /// <summary>
    /// 下限阈值
    /// </summary>
    public decimal? LowerLimit { get; set; }

    /// <summary>
    /// 报警内容
    /// </summary>
    [StringLength(256)]
    public string AlarmContent { get; set; }

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool Enabled { get; set; } = true;
}
