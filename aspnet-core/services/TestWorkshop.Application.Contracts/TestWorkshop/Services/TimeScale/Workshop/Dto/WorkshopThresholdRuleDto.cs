namespace TestWorkshop;

/// <summary>
/// 阈值规则 DTO（按设备类型 + 数据类型一行）
/// </summary>
public class WorkshopThresholdRuleDto
{
    /// <summary>
    /// 规则ID；未配置时为空
    /// </summary>
    public Guid? Id { get; set; }

    /// <summary>
    /// 设备类型
    /// </summary>
    public DeviceTypeEnum DeviceType { get; set; }

    /// <summary>
    /// 数据类型（物理量编码）
    /// </summary>
    public string DataItemType { get; set; }

    /// <summary>
    /// 数据类型名称（压力/流量/温度/扭矩/振动）
    /// </summary>
    public string DataItemName { get; set; }

    /// <summary>
    /// 单位
    /// </summary>
    public string Unit { get; set; }

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
    public string AlarmContent { get; set; }

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool Enabled { get; set; }
}
