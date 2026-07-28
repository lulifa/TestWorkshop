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
        auditLogInfo.ClientIpAddress = IpLocationHelper.GetIpWithLocation(auditLogInfo.ClientIpAddress);
        auditLogInfo.BrowserInfo = IpLocationHelper.GetBrowserInfo(auditLogInfo.BrowserInfo);
        return base.ConvertAsync(auditLogInfo);
    }
}