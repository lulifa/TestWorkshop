using IPTools.Core;
using UAParser;
using Volo.Abp.AspNetCore.ExceptionHandling;
using Volo.Abp.Auditing;
using Volo.Abp.AuditLogging;
using Volo.Abp.Guids;
using Volo.Abp.Json;

namespace TestWorkshop;

[Dependency(ReplaceServices = true)]
[ExposeServices(typeof(IAuditLogInfoToAuditLogConverter))]
public class CustomAuditLogInfoToAuditLogConverter : AuditLogInfoToAuditLogConverter
{
    public CustomAuditLogInfoToAuditLogConverter(
        IGuidGenerator guidGenerator,
        IExceptionToErrorInfoConverter exceptionToErrorInfoConverter,
        IJsonSerializer jsonSerializer,
        IOptions<AbpExceptionHandlingOptions> exceptionHandlingOptions,
        AuditLogEntityTypeFullNameConverter auditLogEntityTypeFullNameConverter)
        : base(guidGenerator, exceptionToErrorInfoConverter, jsonSerializer,
               exceptionHandlingOptions, auditLogEntityTypeFullNameConverter)
    {
    }

    public override Task<AuditLog> ConvertAsync(AuditLogInfo auditLogInfo)
    {
        auditLogInfo.ClientIpAddress = GetIpWithLocation(auditLogInfo.ClientIpAddress);
        auditLogInfo.BrowserInfo = GetBrowserInfo(auditLogInfo.BrowserInfo);
        return base.ConvertAsync(auditLogInfo);
    }

    private string GetIpWithLocation(string ip)
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
            // 降级：返回 IP(未知地区)，明确告知解析失败
            return $"{ip}(未知地区)";
        }
    }

    private string GetBrowserInfo(string userAgent)
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