namespace TestWorkshop;

using Volo.Abp.Auditing;

/// <summary>
/// 下位机遥测数据及任务管理
/// </summary>
[Route("api/workshop/telemetry")]
public class WorkshopTelemetryController : TestWorkshopController
{
    private readonly IWorkshopTelemetryAppService Service;

    public WorkshopTelemetryController(IWorkshopTelemetryAppService telemetryAppService)
    {
        Service = telemetryAppService;
    }

    /// <summary>
    /// 上传遥测文件
    /// </summary>
    [HttpPost("upload")]
    public async Task<WorkshopTelemetryTaskDto> UploadAsync([Required] IFormFile file, [FromForm] WorkshopTelemetryFileInput input)
    {
        return await Service.UploadAsync(file, input);
    }

    /// <summary>
    /// 下位机心跳：按设备编码登记在线状态；设备编码未知则请求失败
    /// </summary>
    [HttpGet("online")]
    [DisableAuditing]
    public async Task<TelemetryOnlineResultDto> OnlineAsync([Required] string deviceCode)
    {
        // 心跳是匿名 GET，禁止任何代理/浏览器缓存，避免出现“假在线”
        Response.Headers.CacheControl = "no-store";
        return await Service.OnlineAsync(deviceCode);
    }

    /// <summary>
    /// 获取任务详情
    /// </summary>
    [HttpGet("{id}")]
    public async Task<WorkshopTelemetryTaskDto> GetAsync(long id)
    {
        return await Service.GetAsync(id);
    }

    /// <summary>
    /// 分页查询
    /// </summary>
    [HttpGet]
    public async Task<PagedResultDto<WorkshopTelemetryTaskDto>> GetListAsync(WorkshopTelemetryTaskListInput input)
    {
        return await Service.GetListAsync(input);
    }

    /// <summary>
    /// 获取统计信息
    /// </summary>
    [HttpGet("statistics")]
    public async Task<WorkshopTelemetryStatisticsDto> GetStatisticsAsync(WorkshopTelemetryTaskListInput input)
    {
        return await Service.GetStatisticsAsync(input);
    }

    /// <summary>
    /// 删除任务
    /// </summary>
    [HttpDelete("{id}")]
    public async Task DeleteAsync(long id)
    {
        await Service.DeleteAsync(id);
    }

    /// <summary>
    /// 批量删除任务
    /// </summary>
    [HttpPost("batch-delete")]
    public async Task DeleteManyAsync(WorkshopTelemetryBatchDeleteInput input)
    {
        await Service.DeleteManyAsync(input);
    }

    /// <summary>
    /// 重新处理任务
    /// </summary>
    [HttpPost("{id}/retry")]
    public async Task RetryAsync(long id)
    {
        await Service.RetryAsync(id);
    }
}
