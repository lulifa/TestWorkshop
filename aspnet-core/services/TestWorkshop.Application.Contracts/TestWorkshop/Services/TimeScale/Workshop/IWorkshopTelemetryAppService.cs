namespace TestWorkshop;

public interface IWorkshopTelemetryAppService : IApplicationService
{
    /// <summary>
    /// 上传遥测文件
    /// </summary>
    Task<WorkshopTelemetryTaskDto> UploadAsync(IFormFile file);

    /// <summary>
    /// 获取任务详情
    /// </summary>
    Task<WorkshopTelemetryTaskDto> GetAsync(long id);

    /// <summary>
    /// 根据文件名搜索
    /// </summary>
    Task<List<WorkshopTelemetryTaskDto>> SearchByFileNameAsync(string fileName);

    /// <summary>
    /// 分页查询任务
    /// </summary>
    Task<PagedResultDto<WorkshopTelemetryTaskDto>> GetListAsync(WorkshopTelemetryTaskListInput input);

    /// <summary>
    /// 获取统计信息
    /// </summary>
    Task<WorkshopTelemetryStatisticsDto> GetStatisticsAsync();

    /// <summary>
    /// 删除任务（物理删除）
    /// </summary>
    Task DeleteAsync(long id);

    /// <summary>
    /// 重新处理失败的任务
    /// </summary>
    Task RetryAsync(long id);
}