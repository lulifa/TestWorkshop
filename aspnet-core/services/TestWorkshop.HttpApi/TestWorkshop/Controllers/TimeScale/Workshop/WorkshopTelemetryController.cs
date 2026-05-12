using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TestWorkshop;

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
    [AllowAnonymous]
    public async Task<WorkshopTelemetryTaskDto> UploadAsync([Required] IFormFile file)
    {
        return await Service.UploadAsync(file);
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
    /// 搜索任务（按文件名）
    /// </summary>
    [HttpGet("search")]
    public async Task<List<WorkshopTelemetryTaskDto>> SearchAsync(string fileName)
    {
        return await Service.SearchByFileNameAsync(fileName);
    }

    /// <summary>
    /// 分页查询
    /// </summary>
    [HttpPost("list")]
    public async Task<PagedResultDto<WorkshopTelemetryTaskDto>> GetListAsync(TelemetryTaskListInput input)
    {
        return await Service.GetListAsync(input);
    }

    /// <summary>
    /// 获取统计信息
    /// </summary>
    [HttpGet("statistics")]
    public async Task<WorkshopTelemetryStatisticsDto> GetStatisticsAsync()
    {
        return await Service.GetStatisticsAsync();
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
    /// 重新处理任务
    /// </summary>
    [HttpPost("{id}/retry")]
    public async Task RetryAsync(long id)
    {
        await Service.RetryAsync(id);
    }
}