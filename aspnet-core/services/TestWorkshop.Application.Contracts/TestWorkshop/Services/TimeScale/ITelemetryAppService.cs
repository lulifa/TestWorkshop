namespace TestWorkshop;

public interface ITelemetryAppService : IApplicationService
{
    /// <summary>
    /// 上传遥测文件
    /// </summary>
    Task<TelemetryTaskDto> UploadAsync(IFormFile file);

    /// <summary>
    /// 获取任务详情
    /// </summary>
    Task<TelemetryTaskDto> GetAsync(long id);

    /// <summary>
    /// 根据文件名搜索
    /// </summary>
    Task<List<TelemetryTaskDto>> SearchByFileNameAsync(string fileName);

    /// <summary>
    /// 分页查询任务
    /// </summary>
    Task<PagedResultDto<TelemetryTaskDto>> GetListAsync(TelemetryTaskListInput input);

    /// <summary>
    /// 获取统计信息
    /// </summary>
    Task<TelemetryStatisticsDto> GetStatisticsAsync();

    /// <summary>
    /// 删除任务（物理删除）
    /// </summary>
    Task DeleteAsync(long id);

    /// <summary>
    /// 重新处理失败的任务
    /// </summary>
    Task RetryAsync(long id);
}

public class TelemetryTaskListInput : PagedAndSortedResultRequestDto
{
    /// <summary>
    /// 文件名过滤
    /// </summary>
    public string FileName { get; set; }

    /// <summary>
    /// 状态过滤 (0/1/2/3)
    /// </summary>
    public int? Status { get; set; }
}