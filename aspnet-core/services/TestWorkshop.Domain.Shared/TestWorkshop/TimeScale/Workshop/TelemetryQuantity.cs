namespace TestWorkshop.TimeScale;

/// <summary>
/// 实时数据物理量编码（固定集合）。
/// 这是平台内部标识：时序宽表列、阈值规则、报警判定统一使用它。
/// 下位机上报的字符串在数据入口按这里归一化（与下位机约定直接上报 Code，忽略大小写）。
/// </summary>
public static class TelemetryQuantity
{
    public const string Pressure = "Pressure";
    public const string Flow = "Flow";
    public const string Temperature = "Temperature";
    public const string Torque = "Torque";
    public const string Vibration = "Vibration";

    private static readonly TelemetryQuantityDefinition[] Definitions =
    {
        new(Pressure, "压力", "MPa"),
        new(Flow, "流量", "L/min"),
        new(Temperature, "温度", "℃"),
        new(Torque, "扭矩", "N·m"),
        new(Vibration, "振动", "mm/s")
    };

    /// <summary>
    /// 全部物理量定义（顺序固定，界面按此顺序展示）
    /// </summary>
    public static IReadOnlyList<TelemetryQuantityDefinition> All => Definitions;

    /// <summary>
    /// 按编码查找定义（忽略大小写、自动去空格），找不到返回 null
    /// </summary>
    public static TelemetryQuantityDefinition Find(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return null;
        }

        var normalized = code.Trim();
        return Definitions.FirstOrDefault(
            x => string.Equals(x.Code, normalized, StringComparison.OrdinalIgnoreCase));
    }

    public static bool IsDefined(string code)
    {
        return Find(code) != null;
    }

    /// <summary>
    /// 把下位机上报的字符串归一化成规范编码；认不出返回 null
    /// </summary>
    public static string Normalize(string raw)
    {
        return Find(raw)?.Code;
    }
}

/// <summary>
/// 物理量定义：编码 + 中文名 + 单位
/// </summary>
public sealed class TelemetryQuantityDefinition
{
    public TelemetryQuantityDefinition(string code, string name, string unit)
    {
        Code = code;
        Name = name;
        Unit = unit;
    }

    public string Code { get; }

    public string Name { get; }

    public string Unit { get; }
}
