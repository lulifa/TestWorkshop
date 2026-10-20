namespace TestWorkshop;

/// <summary>
/// 车间测试订单管理
/// </summary>
[Route("api/workshop/orders")]
public class WorkshopOrderController : TestWorkshopController
{
    private readonly IWorkshopOrderAppService _service;

    public WorkshopOrderController(IWorkshopOrderAppService service)
    {
        _service = service;
    }

    [HttpGet]
    public virtual async Task<PagedResultDto<WorkshopOrderDto>> GetListAsync(WorkshopOrderListInput input)
    {
        return await _service.GetListAsync(input);
    }

    [HttpGet("{id}")]
    public virtual async Task<WorkshopOrderDto> GetAsync(Guid id)
    {
        return await _service.GetAsync(id);
    }

    [HttpPost]
    public virtual async Task<WorkshopOrderDto> CreateAsync(WorkshopOrderCreateDto input)
    {
        return await _service.CreateAsync(input);
    }

    [HttpPut("{id}")]
    public virtual async Task<WorkshopOrderDto> UpdateAsync(Guid id, WorkshopOrderUpdateDto input)
    {
        return await _service.UpdateAsync(id, input);
    }

    [HttpDelete("{id}")]
    public virtual async Task DeleteAsync(Guid id)
    {
        await _service.DeleteAsync(id);
    }

    [HttpPost("{id}/start")]
    public virtual async Task<WorkshopOrderDto> StartTestAsync(Guid id)
    {
        return await _service.StartTestAsync(id);
    }

    [HttpPost("{id}/end")]
    public virtual async Task<WorkshopOrderDto> EndTestAsync(Guid id)
    {
        return await _service.EndTestAsync(id);
    }

    [HttpPost("{id}/cancel")]
    public virtual async Task<WorkshopOrderDto> CancelAsync(Guid id)
    {
        return await _service.CancelAsync(id);
    }

    [HttpPost("{id}/audits")]
    public virtual async Task<WorkshopOrderDto> AuditAsync(
        Guid id,
        WorkshopOrderAuditInput input)
    {
        return await _service.AuditAsync(id, input);
    }

    [HttpGet("{id}/audits")]
    public virtual async Task<List<WorkshopOrderAuditRecordDto>> GetAuditRecordsAsync(Guid id)
    {
        return await _service.GetAuditRecordsAsync(id);
    }

    [HttpGet("{id}/test-report/download")]
    public virtual async Task<IRemoteStreamContent> DownloadTestReportAsync(Guid id)
    {
        return await _service.DownloadTestReportAsync(id);
    }

    [HttpGet("{id}/test-report/preview")]
    public virtual async Task<IRemoteStreamContent> PreviewTestReportAsync(Guid id)
    {
        return await _service.PreviewTestReportAsync(id);
    }

    [HttpGet("{id}/test-report-files")]
    public virtual async Task<List<WorkshopOrderReportFileDto>> GetTestReportFilesAsync(Guid id)
    {
        return await _service.GetTestReportFilesAsync(id);
    }

    [HttpGet("{id}/test-report-files/{fileKey}/preview")]
    public virtual async Task<IRemoteStreamContent> PreviewTestReportFileAsync(Guid id, string fileKey)
    {
        return await _service.PreviewTestReportFileAsync(id, fileKey);
    }

    [HttpGet("project-supervisors")]
    public virtual async Task<List<WorkshopOrderProjectSupervisorDto>> GetProjectSupervisorOptionsAsync()
    {
        return await _service.GetProjectSupervisorOptionsAsync();
    }

    [HttpGet("{id}/telemetry-tasks")]
    public virtual async Task<PagedResultDto<WorkshopTelemetryTaskDto>> GetTelemetryTasksAsync(
        Guid id,
        WorkshopTelemetryTaskListInput input)
    {
        return await _service.GetTelemetryTasksAsync(id, input);
    }

    [HttpGet("{id}/telemetry-graphs")]
    public virtual async Task<WorkshopOrderTelemetryGraphDto> GetTelemetryGraphAsync(Guid id)
    {
        return await _service.GetTelemetryGraphAsync(id);
    }
}
