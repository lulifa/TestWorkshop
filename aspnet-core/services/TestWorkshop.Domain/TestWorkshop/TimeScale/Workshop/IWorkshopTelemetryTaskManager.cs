namespace TestWorkshop.TimeScale;

public interface IWorkshopTelemetryTaskManager : IDomainService
{

    /// <summary>
    /// 上传遥测文件并创建任务
    /// </summary>
    Task<WorkshopTelemetryTask> CreateTaskFromFileAsync(
        Stream stream,
        string fileName,
        string contentType);

    /// <summary>
    /// 获取任务及其关联的 FileObject
    /// </summary>
    Task<(WorkshopTelemetryTask Task, FileObject FileObject)> GetTaskWithFileAsync(long taskId);

    /// <summary>
    /// 删除任务（级联删除 FileObject 和物理文件）
    /// </summary>
    Task DeleteTaskAsync(long taskId);

    /// <summary>
    /// 重试失败的任务
    /// </summary>
    Task RetryTaskAsync(long taskId);

    /// <summary>
    /// 清理已过期且已完成的任务（级联删除 FileObject 和物理文件）
    /// </summary>
    Task<int> CleanupExpiredTasksAsync(int batchSize = 50);


}
