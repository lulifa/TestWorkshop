using TestWorkshop.TimeScale;

namespace TestWorkshop;

/// <summary>
/// 业务归属类型分类
/// 默认所有类型均为"状态型"（走 business 路径）
/// 只有明确注册为"日志型"的才走 rawdata 路径
/// </summary>
public static class FileOwnerTypeCategories
{
    /// <summary>
    /// 日志型数据（不可变，只追加）
    /// 存储路径: rawdata/{OwnerType}/{yyyy}/{MM}/{dd}/
    /// 只有这些类型会走日志型存储
    /// </summary>
    private static readonly HashSet<string> _logTypes = new()
    {
        nameof(WorkshopDeviceTelemetry), // 设备遥测数据
    };


    /// <summary>
    /// 判断是否为日志型数据
    /// </summary>
    public static bool IsLogData(string ownerType)
    {
        return ownerType != null && _logTypes.Contains(ownerType);
    }

    /// <summary>
    /// 注册日志型类型（供其他模块扩展）
    /// </summary>
    public static void RegisterLogType(string type)
    {
        if (!_logTypes.Contains(type))
        {
            _logTypes.Add(type);
        }
    }

}
