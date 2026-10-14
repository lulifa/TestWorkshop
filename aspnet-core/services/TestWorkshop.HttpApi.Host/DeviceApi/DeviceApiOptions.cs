namespace TestWorkshop;

/// <summary>
/// 下位机接口（匿名接口）的共享密钥配置。
/// 下位机在请求头 X-Device-Key 里带上 ApiKey；启用了校验但没配密钥时按拒绝处理。
/// </summary>
public class DeviceApiOptions
{
    public const string SectionName = "DeviceApi";

    /// <summary>
    /// 是否启用密钥校验。仅在上线过渡期（下位机还没带上请求头）临时关闭。
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// 下位机请求头 X-Device-Key 必须匹配的共享密钥。生产环境建议用环境变量 DeviceApi__ApiKey 覆盖。
    /// </summary>
    public string ApiKey { get; set; }
}