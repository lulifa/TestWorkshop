using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace TestWorkshop;

[Route("api/telemetry")]
[ApiController]
public class TelemetryController : TestWorkshopController
{
    private readonly ITelemetryAppService _telemetryAppService;

    public TelemetryController(ITelemetryAppService telemetryAppService)
    {
        _telemetryAppService = telemetryAppService;
    }

    /// <summary>
    /// 上传遥测文件
    /// </summary>
    [HttpPost("upload")]
    public async Task<TelemetryTaskDto> UploadAsync(IFormFile file)
    {
        return await _telemetryAppService.UploadAsync(file);
    }

    /// <summary>
    /// 获取任务详情
    /// </summary>
    [HttpGet("{id}")]
    public async Task<TelemetryTaskDto> GetAsync(long id)
    {
        return await _telemetryAppService.GetAsync(id);
    }

    /// <summary>
    /// 搜索任务（按文件名）
    /// </summary>
    [HttpGet("search")]
    public async Task<List<TelemetryTaskDto>> SearchAsync(string fileName)
    {
        return await _telemetryAppService.SearchByFileNameAsync(fileName);
    }

    /// <summary>
    /// 分页查询
    /// </summary>
    [HttpPost("list")]
    public async Task<PagedResultDto<TelemetryTaskDto>> GetListAsync(TelemetryTaskListInput input)
    {
        return await _telemetryAppService.GetListAsync(input);
    }

    /// <summary>
    /// 获取统计信息
    /// </summary>
    [HttpGet("statistics")]
    public async Task<TelemetryStatisticsDto> GetStatisticsAsync()
    {
        return await _telemetryAppService.GetStatisticsAsync();
    }

    /// <summary>
    /// 删除任务
    /// </summary>
    [HttpDelete("{id}")]
    public async Task DeleteAsync(long id)
    {
        await _telemetryAppService.DeleteAsync(id);
    }

    /// <summary>
    /// 重新处理任务
    /// </summary>
    [HttpPost("{id}/retry")]
    public async Task RetryAsync(long id)
    {
        await _telemetryAppService.RetryAsync(id);
    }
}