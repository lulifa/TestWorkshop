namespace TestWorkshop;

/// <summary>
/// IP归属地解析工具类
/// </summary>
public static class IpLocationHelper
{
    /// <summary>
    /// 获取带归属地的IP地址
    /// </summary>
    /// <param name="ip">原始IP地址</param>
    /// <returns>格式：IP(省份城市) 或 原始IP(未知地区)</returns>
    public static string GetIpWithLocation(string ip)
    {
        if (string.IsNullOrEmpty(ip))
            return ip;

        // 处理本地地址
        if (ip == "127.0.0.1" || ip == "::1" || ip == "0:0:0:0:0:0:0:1" || ip.Equals("localhost", StringComparison.OrdinalIgnoreCase))
            return "127.0.0.1(本地)";

        try
        {
            var info = IpTool.Search(ip);
            return $"{info.IpAddress}({info.Province}{info.City})";
        }
        catch
        {
            return $"{ip}(未知地区)";
        }
    }

    /// <summary>
    /// 解析浏览器信息
    /// </summary>
    /// <param name="userAgent">User-Agent 字符串</param>
    /// <returns>格式：浏览器名称 版本</returns>
    public static string GetBrowserInfo(string userAgent)
    {
        if (string.IsNullOrWhiteSpace(userAgent))
            return "Unknown";

        try
        {
            var parser = Parser.GetDefault();
            var info = parser.Parse(userAgent);
            return $"{info.UA.Family} {info.UA.Major}.{info.UA.Minor}";
        }
        catch
        {
            return "Other";
        }
    }
}
