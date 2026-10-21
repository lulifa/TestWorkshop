namespace TestWorkshop.EntityFrameworkCore;

using System.Text;
using Microsoft.Extensions.Options;

/// <summary>
/// 外部测试报告接口未配置时的占位适配器。
/// UseMock=true 时返回一份文本报告，用于联调页面和状态流转。
/// </summary>
public class PlaceholderWorkshopTestReportFetcher : IWorkshopTestReportFetcher
{
    private readonly WorkshopTestReportOptions _options;

    public PlaceholderWorkshopTestReportFetcher(
        IOptions<WorkshopTestReportOptions> options)
    {
        _options = options.Value;
    }

    public Task<WorkshopTestReportFetchResult> FetchAsync(
        WorkshopTestReportFetchRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!_options.UseMock)
        {
            return Task.FromResult(new WorkshopTestReportFetchResult
            {
                Status = WorkshopTestReportFetchResultStatus.Failed,
                ErrorCode = "NOT_CONFIGURED",
                ErrorMessage = "外部测试报告接口尚未配置",
                IsTransient = false
            });
        }

        var content = $"Mock test report for order {request.OrderNo}";
        var bytes = Encoding.UTF8.GetBytes(content);
        return Task.FromResult(new WorkshopTestReportFetchResult
        {
            Status = WorkshopTestReportFetchResultStatus.Ready,
            ExternalReportId = $"MOCK-{request.OrderNo}",
            IsTransient = false,
            Files =
            [
                new WorkshopTestReportFileContent
                {
                    FileName = $"{request.OrderNo}-mock-report.txt",
                    ContentType = "text/plain",
                    Content = new MemoryStream(bytes, writable: false)
                }
            ]
        });
    }
}
