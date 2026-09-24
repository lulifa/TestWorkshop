using Microsoft.AspNetCore.Mvc;

using System.Reflection;

namespace TestWorkshop;

/// <summary>
/// 遥测服务应用
/// </summary>
[Authorize(Roles = RoleConstants.admin)]
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
    public async Task<WorkshopTelemetryTaskDto> UploadAsync(IFormFile file, WorkshopTelemetryFileInput input)
    {
        if (file == null || file.Length == 0)
            throw new UserFriendlyException("请选择有效的文件");
        if (input == null)
            throw new UserFriendlyException("请提供遥测文件参数");
        if (input.DeviceCode.IsNullOrWhiteSpace())
            throw new UserFriendlyException("设备编码不能为空");
        if (input.ChannelType.IsNullOrWhiteSpace())
            throw new UserFriendlyException("通道类型不能为空");
        if (input.TestInfo == null || input.TestInfo.StartTime.IsNullOrWhiteSpace())
            throw new UserFriendlyException("测试开始时间不能为空");

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (extension != ".csv")
            throw new UserFriendlyException("仅支持 .csv 文件");

        var extraProperties = BuildTelemetryExtraProperties(input);
        var task = await _taskManager.CreateTaskFromFileAsync(
            stream: file.OpenReadStream(),
            fileName: file.FileName,
            contentType: file.ContentType,
            extraProperties: extraProperties
        );

        await CurrentUnitOfWork.SaveChangesAsync();

        var fileObject = await _fileObjectRepository.GetAsync(task.FileObjectId);

        return new WorkshopTelemetryTaskDto
        {
            Id = task.Id,
            FileObjectId = task.FileObjectId,
            FileName = fileObject?.FileName,
            FileSize = fileObject?.FileSize ?? 0,
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
            FileName = fileObject?.FileName,
            FileSize = fileObject?.FileSize ?? 0,
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
    /// 分页查询
    /// </summary>
    public async Task<PagedResultDto<WorkshopTelemetryTaskDto>> GetListAsync(WorkshopTelemetryTaskListInput input)
    {
        if (!input.IsPaged)
        {
            input.SkipCount = 0;
            input.MaxResultCount = int.MaxValue;
        }

        // ✅ 用 Repository 已有的方法
        var result = await _taskRepository.GetPagedListAsync(
            input.FileName,
            input.Status,
            input.StartTime,
            input.EndTime,
            input.SkipCount,
            input.MaxResultCount);

        var dtos = new List<WorkshopTelemetryTaskDto>();
        foreach (var task in result.Items)
        {
            var fileObject = await _fileObjectRepository.FindAsync(task.FileObjectId);
            dtos.Add(new WorkshopTelemetryTaskDto
            {
                Id = task.Id,
                FileObjectId = task.FileObjectId,
                FileName = fileObject?.FileName,
                FileSize = fileObject?.FileSize ?? 0,
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
    /// 批量删除任务（级联删除 FileObject 和物理文件）
    /// </summary>
    public async Task DeleteManyAsync(WorkshopTelemetryBatchDeleteInput input)
    {
        if (input?.Ids == null || input.Ids.Count == 0)
        {
            return;
        }

        await _taskManager.DeleteTasksAsync(input.Ids);
    }

    /// <summary>
    /// 重新处理失败的任务
    /// </summary>
    public async Task RetryAsync(long id)
    {
        await _taskManager.RetryTaskAsync(id);
    }

    /// <summary>
    /// 把上传参数扁平化写入 FileObject.ExtraProperties，后续新增简单字段无需调整持久化代码。
    /// </summary>
    private static ExtraPropertyDictionary BuildTelemetryExtraProperties(object input)
    {
        var result = new ExtraPropertyDictionary();
        AddTelemetryExtraProperties(result, input, TestWorkshopConsts.TelemetryInputExtraPropertiesPrefix);
        return result;
    }

    private static void AddTelemetryExtraProperties(
        ExtraPropertyDictionary target,
        object source,
        string prefix)
    {
        if (source == null)
        {
            return;
        }

        foreach (var property in source.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public))
        {
            if (property.GetIndexParameters().Length > 0)
            {
                continue;
            }

            var value = property.GetValue(source);
            if (value == null)
            {
                continue;
            }

            var key = prefix + property.Name;
            var propertyType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
            if (IsSimpleValue(propertyType))
            {
                target[key] = value;
            }
            else
            {
                AddTelemetryExtraProperties(target, value, key + ".");
            }
        }
    }

    private static bool IsSimpleValue(Type type)
    {
        return type == typeof(string)
               || type == typeof(decimal)
               || type == typeof(DateTime)
               || type == typeof(DateTimeOffset)
               || type == typeof(TimeSpan)
               || type == typeof(Guid)
               || type.IsEnum
               || type.IsPrimitive;
    }
}
