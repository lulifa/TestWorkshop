namespace TestWorkshop;

/// <summary>
/// 下位机心跳（连通性探测）返回内容。
/// 下位机请求该接口成功即代表与平台通信正常；
/// 同时返回服务器时间，供下位机校时——订单匹配与遥测归属都依赖时间窗，
/// 下位机时钟漂移会直接导致数据挂错订单或变成未匹配数据。
/// </summary>
public class TelemetryOnlineResultDto
{
    /// <summary>
    /// 是否在线（能返回就说明通）
    /// </summary>
    public bool Online { get; set; } = true;

    /// <summary>
    /// 服务器本地时间（与订单、遥测数据使用的时间口径一致）
    /// </summary>
    public DateTime ServerTime { get; set; }

    /// <summary>
    /// 服务器时间的 Unix 毫秒时间戳（避免下位机处理时区字符串出错）
    /// </summary>
    public long Timestamp { get; set; }
}
