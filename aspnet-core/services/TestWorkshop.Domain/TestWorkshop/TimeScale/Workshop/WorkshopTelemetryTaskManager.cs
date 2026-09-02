namespace TestWorkshop.TimeScale;

public class WorkshopTelemetryTaskManager : DomainService, IWorkshopTelemetryTaskManager
{
    private readonly IFileObjectManager _fileObjectManager;
    private readonly IWorkshopTelemetryTaskRepository _taskRepository;
    private readonly IFileObjectRepository _fileObjectRepository;
    private readonly IUnitOfWorkManager _unitOfWorkManager;
    private readonly ILogger<WorkshopTelemetryTaskManager> _logger;

    public WorkshopTelemetryTaskManager(
        IFileObjectManager fileObjectManager,
        IWorkshopTelemetryTaskRepository taskRepository,
        IFileObjectRepository fileObjectRepository,
        IUnitOfWorkManager unitOfWorkManager,
        ILogger<WorkshopTelemetryTaskManager> logger)
    {
        _fileObjectManager = fileObjectManager;
        _taskRepository = taskRepository;
        _fileObjectRepository = fileObjectRepository;
        _unitOfWorkManager = unitOfWorkManager;
        _logger = logger;
    }

    /// <summary>
    /// 上传遥测文件并创建任务
    /// </summary>
    [UnitOfWork]
    public virtual async Task<WorkshopTelemetryTask> CreateTaskFromFileAsync(
        Stream stream,
        string fileName,
        string contentType)
    {
        // 1. 先创建任务并保存，拿到 Task.Id
        var fileObjectId = Guid.NewGuid();
        var task = new WorkshopTelemetryTask(
            fileObjectId: fileObjectId,
            expiresAt: DateTime.UtcNow.AddDays(7),
            tenantId: CurrentTenant.Id
        );
        await _taskRepository.InsertAsync(task);
        await _unitOfWorkManager.Current.SaveChangesAsync();

        // 2. 通过 FileObjectManager 追加上传文件（不删除历史文件），OwnerId 使用 Task.Id
        var fileObject = await _fileObjectManager.AppendAsync(
            stream: stream,
            fileName: fileName,
            ownerType: nameof(WorkshopTelemetryTask),
            ownerId: task.Id.ToString(),
            contentType: contentType,
            fileId: fileObjectId
        );

        _logger.LogInformation("创建遥测任务成功: TaskId={TaskId}, FileObjectId={FileObjectId}", task.Id, fileObject.Id);
        return task;
    }

    /// <summary>
    /// 获取任务及其关联的 FileObject
    /// </summary>
    public virtual async Task<(WorkshopTelemetryTask Task, FileObject FileObject)> GetTaskWithFileAsync(long taskId)
    {
        var task = await _taskRepository.GetAsync(taskId);
        if (task == null)
            throw new UserFriendlyException($"任务不存在: {taskId}");

        var fileObject = await _fileObjectRepository.FindAsync(task.FileObjectId);

        return (task, fileObject);
    }

    /// <summary>
    /// 删除任务（级联删除 FileObject 和物理文件）- 真删除
    /// </summary>
    [UnitOfWork]
    public virtual async Task DeleteTaskAsync(long taskId)
    {
        var task = await _taskRepository.GetAsync(taskId);
        if (task == null) return;

        if (task.Status == 1)
            throw new BusinessException("不能删除正在处理的任务");

        // 1. 如果 FileObject 还存在，先删除文件元数据和物理文件
        var fileObject = await _fileObjectRepository.FindAsync(task.FileObjectId);
        if (fileObject != null)
        {
            await _fileObjectManager.DeleteFileAsync(task.FileObjectId);
        }

        // 2. 真删除任务（物理删除）
        await _taskRepository.DeleteAsync(task);

        _logger.LogInformation("删除任务成功: TaskId={TaskId}", taskId);
    }

    /// <summary>
    /// 重试失败的任务
    /// </summary>
    [UnitOfWork]
    public virtual async Task RetryTaskAsync(long taskId)
    {
        var task = await _taskRepository.GetAsync(taskId);
        if (task == null)
            throw new UserFriendlyException($"任务不存在: {taskId}");

        if (task.Status != 3)
            throw new UserFriendlyException("只有失败的任务才能重试");

        task.ResetForRetry();
        await _taskRepository.UpdateAsync(task);

        _logger.LogInformation("任务已重置为重试: TaskId={TaskId}", taskId);
    }

    /// <summary>
    /// 清理已过期且已完成的任务 - 真删除
    /// </summary>
    [UnitOfWork]
    public virtual async Task<int> CleanupExpiredTasksAsync(int batchSize = 100)
    {
        // 1. 获取已过期且可清理的任务（包含关联的 FileObject）
        var expiredItems = await _taskRepository.GetExpiredCompletedTasksAsync(batchSize);
        if (expiredItems.Count == 0)
        {
            return 0;
        }

        var successCount = 0;
        long freedSize = 0;

        foreach (var (task, fileObject) in expiredItems)
        {
            try
            {
                // 2. 如果 FileObject 还存在，删除物理文件 + FileObject 记录
                if (fileObject != null)
                {
                    await _fileObjectManager.DeleteFileAsync(fileObject.Id);
                    freedSize += fileObject.FileSize;
                }

                // 3. 真删除任务
                await _taskRepository.DeleteAsync(task);

                successCount++;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "清理任务失败: TaskId={TaskId}, FileObjectId={FileObjectId}",
                    task.Id, fileObject.Id);
                // 继续处理下一个，不中断整个批次
            }
        }

        _logger.LogInformation("清理完成：成功处理 {Count} 个任务，释放空间约 {Size} 字节",
            successCount, freedSize);

        return successCount;
    }
}
