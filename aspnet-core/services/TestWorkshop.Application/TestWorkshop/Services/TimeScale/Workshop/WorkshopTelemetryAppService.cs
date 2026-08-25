using Microsoft.AspNetCore.Http;
using System.IO;
using TestWorkshop.TimeScale;

namespace TestWorkshop;

/// <summary>
/// 遥测服务应用
/// </summary>
[Authorize]
public class WorkshopTelemetryAppService : TestWorkshopAppService, IWorkshopTelemetryAppService
{
    private readonly IWorkshopTelemetryTaskManager _taskManager;
    private readonly IWorkshopTelemetryTaskRepository _taskRepository;
    private readonly IFileObjectRepository _fileObjectRepository;
    private readonly ICurrentTenant _currentTenant;

    public WorkshopTelemetryAppService(
        IWorkshopTelemetryTaskManager taskManager,
        IWorkshopTelemetryTaskRepository taskRepository,
        IFileObjectRepository fileObjectRepository,
        ICurrentTenant currentTenant)
    {
        _taskManager = taskManager;
        _taskRepository = taskRepository;
        _fileObjectRepository = fileObjectRepository;
        _currentTenant = currentTenant;
    }

    /// <summary>
    /// 上传遥测文件
    /// </summary>
    [AllowAnonymous]
    public async Task<WorkshopTelemetryTaskDto> UploadAsync(IFormFile file)
    {
        if (file == null || file.Length == 0)
            throw new UserFriendlyException("请选择有效的文件");

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (extension != ".csv")
            throw new UserFriendlyException("仅支持 .csv 文件");

        var task = await _taskManager.CreateTaskFromFileAsync(
            stream: file.OpenReadStream(),
            fileName: file.FileName,
            contentType: file.ContentType
        );

        var fileObject = await _fileObjectRepository.GetAsync(task.FileObjectId);

        return new WorkshopTelemetryTaskDto
        {
            Id = task.Id,
            FileObjectId = task.FileObjectId,
            FileName = fileObject.FileName,
            FileSize = fileObject.FileSize,
            Status = task.Status,
            RetryCount = task.RetryCount,
            Error = task.Error,
            RecordCount = task.RecordCount,
            CreatedAt = task.CreatedAt,
            ProcessedAt = task.ProcessedAt,
            ExpiresAt = task.ExpiresAt
        };
    }

    /// <summary>
    /// 获取任务详情
    /// </summary>
    public async Task<WorkshopTelemetryTaskDto> GetAsync(long id)
    {
        var (task, fileObject) = await _taskManager.GetTaskWithFileAsync(id);

        return new WorkshopTelemetryTaskDto
        {
            Id = task.Id,
            FileObjectId = task.FileObjectId,
            FileName = fileObject.FileName,
            FileSize = fileObject.FileSize,
            Status = task.Status,
            RetryCount = task.RetryCount,
            Error = task.Error,
            RecordCount = task.RecordCount,
            CreatedAt = task.CreatedAt,
            ProcessedAt = task.ProcessedAt,
            ExpiresAt = task.ExpiresAt
        };
    }

    /// <summary>
    /// 根据文件名搜索
    /// </summary>
    public async Task<List<WorkshopTelemetryTaskDto>> SearchByFileNameAsync(string fileName)
    {
        // ✅ 用 Repository 已有的方法
        var tasks = await _taskRepository.SearchByFileNameAsync(fileName);
        var dtos = new List<WorkshopTelemetryTaskDto>();

        foreach (var task in tasks)
        {
            var fileObject = await _fileObjectRepository.GetAsync(task.FileObjectId);
            dtos.Add(new WorkshopTelemetryTaskDto
            {
                Id = task.Id,
                FileObjectId = task.FileObjectId,
                FileName = fileObject.FileName,
                FileSize = fileObject.FileSize,
                Status = task.Status,
                RetryCount = task.RetryCount,
                Error = task.Error,
                RecordCount = task.RecordCount,
                CreatedAt = task.CreatedAt,
                ProcessedAt = task.ProcessedAt,
                ExpiresAt = task.ExpiresAt
            });
        }

        return dtos;
    }

    /// <summary>
    /// 分页查询
    /// </summary>
    public async Task<PagedResultDto<WorkshopTelemetryTaskDto>> GetListAsync(WorkshopTelemetryTaskListInput input)
    {
        // ✅ 用 Repository 已有的方法
        var result = await _taskRepository.GetPagedListAsync(
            input.FileName,
            input.Status,
            input.SkipCount,
            input.MaxResultCount);

        var dtos = new List<WorkshopTelemetryTaskDto>();
        foreach (var task in result.Items)
        {
            var fileObject = await _fileObjectRepository.GetAsync(task.FileObjectId);
            dtos.Add(new WorkshopTelemetryTaskDto
            {
                Id = task.Id,
                FileObjectId = task.FileObjectId,
                FileName = fileObject.FileName,
                FileSize = fileObject.FileSize,
                Status = task.Status,
                RetryCount = task.RetryCount,
                Error = task.Error,
                RecordCount = task.RecordCount,
                CreatedAt = task.CreatedAt,
                ProcessedAt = task.ProcessedAt,
                ExpiresAt = task.ExpiresAt
            });
        }

        return new PagedResultDto<WorkshopTelemetryTaskDto>(result.TotalCount, dtos);
    }

    /// <summary>
    /// 获取统计信息
    /// </summary>
    public async Task<WorkshopTelemetryStatisticsDto> GetStatisticsAsync()
    {
        var (totalFiles, totalSize, pendingCount, processingCount, successCount, failedCount, totalRecords)
            = await _taskRepository.GetStatisticsDataAsync();

        return new WorkshopTelemetryStatisticsDto
        {
            TotalFiles = totalFiles,
            TotalSize = totalSize,
            PendingCount = pendingCount,
            ProcessingCount = processingCount,
            SuccessCount = successCount,
            FailedCount = failedCount,
            TotalRecords = totalRecords
        };
    }

    /// <summary>
    /// 删除任务（级联删除 FileObject 和物理文件）
    /// </summary>
    public async Task DeleteAsync(long id)
    {
        await _taskManager.DeleteTaskAsync(id);
    }

    /// <summary>
    /// 重新处理失败的任务
    /// </summary>
    public async Task RetryAsync(long id)
    {
        await _taskManager.RetryTaskAsync(id);
    }
}