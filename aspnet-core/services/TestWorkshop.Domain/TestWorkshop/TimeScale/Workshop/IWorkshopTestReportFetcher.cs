namespace TestWorkshop.TimeScale;

/// <summary>
/// 外部测试报告接口适配器。接口协议确定后实现该接口。
/// </summary>
public interface IWorkshopTestReportFetcher
{
    Task<WorkshopTestReportFetchResult> FetchAsync(
        WorkshopTestReportFetchRequest request,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// 获取外部测试报告所需的最小订单信息。
/// </summary>
public class WorkshopTestReportFetchRequest
{
    public Guid OrderId { get; set; }

    public string OrderNo { get; set; }

    public string DeviceCode { get; set; }

    public string DeviceName { get; set; }

    public DeviceTypeEnum DeviceType { get; set; }

    public DateTime? TestStartedAt { get; set; }

    public DateTime? TestEndedAt { get; set; }

    public DateTime RequiredCompletionTime { get; set; }
}

public enum WorkshopTestReportFetchResultStatus
{
    NotReady = 0,
    Ready = 1,
    Failed = 2
}

public class WorkshopTestReportFetchResult
{
    public WorkshopTestReportFetchResultStatus Status { get; set; }

    public List<WorkshopTestReportFileContent> Files { get; set; } = [];

    public string ExternalReportId { get; set; }

    public string ErrorCode { get; set; }

    public string ErrorMessage { get; set; }

    public int? HttpStatusCode { get; set; }

    /// <summary>
    /// 是否适合自动重试。鉴权、参数错误等应返回 false。
    /// </summary>
    public bool IsTransient { get; set; } = true;
}

public class WorkshopTestReportFileContent
{
    public string FileName { get; set; }

    public string ContentType { get; set; }

    public Stream Content { get; set; }
}
