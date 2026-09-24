namespace TestWorkshop;

public interface IWorkshopTelemetryAppService : IApplicationService
{
    /// <summary>
    /// 上传遥测文件
    /// </summary>
    Task<WorkshopTelemetryTaskDto> UploadAsync(IFormFile file, WorkshopTelemetryFileInput input);

    /// <summary>
    /// 获取任务详情
    /// </summary>
    Task<WorkshopTelemetryTaskDto> GetAsync(long id);

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
    /// 批量删除任务（级联删除 FileObject 和物理文件）
    /// </summary>
    Task DeleteManyAsync(WorkshopTelemetryBatchDeleteInput input);

    /// <summary>
    /// 重新处理失败的任务
    /// </summary>
    Task RetryAsync(long id);
}
