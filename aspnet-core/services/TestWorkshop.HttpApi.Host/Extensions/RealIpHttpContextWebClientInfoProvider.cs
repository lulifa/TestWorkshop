using Microsoft.Extensions.Logging;
using MyCSharp.HttpUserAgentParser.Providers;
using System.Net;
using Volo.Abp.AspNetCore.WebClientInfo;

namespace TestWorkshop;

/// <summary>
/// 支持代理场景的真实 IP 提供程序
/// </summary>
public class RealIpHttpContextWebClientInfoProvider : HttpContextWebClientInfoProvider
{
    private const string XForwardedForHeader = "X-Forwarded-For";

    public RealIpHttpContextWebClientInfoProvider(
        ILogger<HttpContextWebClientInfoProvider> logger,
        IHttpContextAccessor httpContextAccessor,
        IHttpUserAgentParserProvider httpUserAgentParser)
        : base(logger, httpContextAccessor, httpUserAgentParser)
    {
    }

    protected override string GetClientIpAddress()
    {
        try
        {
            var httpContext = HttpContextAccessor.HttpContext;
            if (httpContext == null)
                return null;

            string realIp = null;

            // 1. 优先从 X-Forwarded-For 获取真实 IP
            if (httpContext.Request.Headers.TryGetValue(XForwardedForHeader, out var forwardedIps))
            {
                realIp = forwardedIps.FirstOrDefault()?.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .FirstOrDefault()?.Trim();
            }

            // 2. 取不到则使用 RemoteIpAddress
            if (string.IsNullOrEmpty(realIp))
            {
                realIp = httpContext.Connection.RemoteIpAddress?.ToString();
            }

            if (string.IsNullOrEmpty(realIp))
                return null;

            // 3. 处理 IPv4 映射到 IPv6 的格式（如 ::ffff:192.168.1.1）
            if (IPAddress.TryParse(realIp, out var ipAddress))
            {
                if (ipAddress.IsIPv4MappedToIPv6)
                {
                    realIp = ipAddress.MapToIPv4().ToString();
                }
            }

            return realIp;
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "获取客户端IP地址时发生异常");
            return null;
        }
    }
}
