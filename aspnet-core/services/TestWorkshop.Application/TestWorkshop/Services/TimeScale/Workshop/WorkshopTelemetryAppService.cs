using Microsoft.AspNetCore.Http;
using System.IO;
using TestWorkshop.TimeScale;
using Volo.Abp.BlobStoring;
using Volo.Abp.Domain.Entities;

namespace TestWorkshop;

/// <summary>
/// 遥测服务应用
/// </summary>
public class WorkshopTelemetryAppService : TestWorkshopAppService, IWorkshopTelemetryAppService
{
    private readonly IBlobContainer _blobContainer;
    private readonly IWorkshopTelemetryTaskRepository _telemetryTaskRepository;
    private readonly ICurrentTenant _currentTenant;

    public WorkshopTelemetryAppService(
        IBlobContainer blobContainer,
        IWorkshopTelemetryTaskRepository telemetryTaskRepository,
        ICurrentTenant currentTenant)
    {
        _blobContainer = blobContainer;
        _telemetryTaskRepository = telemetryTaskRepository;
        _currentTenant = currentTenant;
    }

    /// <summary>
    /// 上传遥测文件
    /// </summary>
    public async Task<WorkshopTelemetryTaskDto> UploadAsync(IFormFile file)
    {
        if (file == null || file.Length == 0)
            throw new UserFriendlyException("Please select a file to upload");

        var fileId = GuidGenerator.Create();
        var blobName = fileId.ToString("N") + Path.GetExtension(file.FileName);

        try
        {
            // 1️⃣ 保存文件到 Blob 存储
            await using (var stream = file.OpenReadStream())
            {
                await _blobContainer.SaveAsync(blobName, stream, true);
            }

            // 2️⃣ 创建任务记录
            var task = new WorkshopTelemetryTask
            {
                FileId = fileId,
                BlobName = blobName,
                FileName = file.FileName,
                FileSize = file.Length,
                Status = 0, // Pending
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                IsDeleted = false,
                TenantId = _currentTenant.Id,
                RetryCount = 0
            };

            await _telemetryTaskRepository.InsertAsync(task);

            // ✅ 使用 ObjectMapper 进行转换
            return ObjectMapper.Map<WorkshopTelemetryTask, WorkshopTelemetryTaskDto>(task);
        }
        catch (Exception ex)
        {
            // 如果数据库操作失败，删除已上传的文件
            try
            {
                await _blobContainer.DeleteAsync(blobName);
            }
            catch { /* 忽略清理异常 */ }

            throw;
        }
    }

    /// <summary>
    /// 获取任务详情
    /// </summary>
    public async Task<WorkshopTelemetryTaskDto> GetAsync(long id)
    {
        var task = await _telemetryTaskRepository.GetAsync(id);
        // ✅ 使用 ObjectMapper 进行转换
        return ObjectMapper.Map<WorkshopTelemetryTask, WorkshopTelemetryTaskDto>(task);
    }

    /// <summary>
    /// 根据文件名搜索
    /// </summary>
    public async Task<List<WorkshopTelemetryTaskDto>> SearchByFileNameAsync(string fileName)
    {
        var tasks = await _telemetryTaskRepository.SearchByFileNameAsync(fileName);
        // ✅ 使用 ObjectMapper 进行转换
        return ObjectMapper.Map<List<WorkshopTelemetryTask>, List<WorkshopTelemetryTaskDto>>(tasks);
    }

    /// <summary>
    /// 分页查询
    /// </summary>
    public async Task<PagedResultDto<WorkshopTelemetryTaskDto>> GetListAsync(TelemetryTaskListInput input)
    {
        var result = await _telemetryTaskRepository.GetListAsync(
            input.FileName,
            input.Status,
            input.SkipCount,
            input.MaxResultCount);

        // ✅ 使用 ObjectMapper 进行转换
        var dtos = ObjectMapper.Map<List<WorkshopTelemetryTask>, List<WorkshopTelemetryTaskDto>>(result.Items.ToList());

        return new PagedResultDto<WorkshopTelemetryTaskDto>(result.TotalCount, dtos);
    }

    /// <summary>
    /// 获取统计信息
    /// </summary>
    public async Task<WorkshopTelemetryStatisticsDto> GetStatisticsAsync()
    {
        var (totalFiles, totalSize, pendingCount, processingCount, successCount, failedCount, totalRecords)
            = await _telemetryTaskRepository.GetStatisticsDataAsync();

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
    /// 删除任务
    /// </summary>
    public async Task DeleteAsync(long id)
    {
        var task = await _telemetryTaskRepository.GetAsync(id);

        if (task != null)
        {
            // ✅ 禁止删除正在处理的任务
            if (task.Status == 1)
                throw new UserFriendlyException("Cannot delete a task that is currently being processed");

            // 删除 Blob 存储中的文件
            if (!string.IsNullOrEmpty(task.BlobName))
            {
                try
                {
                    await _blobContainer.DeleteAsync(task.BlobName);
                }
                catch { /* Blob 文件可能已删除 */ }
            }

            // 物理删除数据库记录
            await _telemetryTaskRepository.DeleteAsync(task);
        }
    }

    /// <summary>
    /// 重新处理失败的任务
    /// </summary>
    public async Task RetryAsync(long id)
    {
        var task = await _telemetryTaskRepository.GetAsync(id);

        if (task == null)
            throw new EntityNotFoundException($"Telemetry task {id} not found");

        if (task.Status != 3)
            throw new UserFriendlyException("Only failed tasks can be retried");

        // 重置任务状态
        task.Status = 0; // Pending
        task.RetryCount = 0;
        task.Error = null;
        task.NextRetryTime = null;

        await _telemetryTaskRepository.UpdateAsync(task);
    }
}