namespace TestWorkshop.TimeScale;

/// <summary>
/// 遥测任务实体 - 管理下位机采集数据的处理状态
/// 与 FileObject 通过 FileObjectId 关联
/// </summary>
public class WorkshopTelemetryTask : Entity<long>, IMultiTenant
{
    /// <summary>
    /// 关联的 FileObject ID（指向 FileObject 表）
    /// </summary>
    public Guid FileObjectId { get; private set; }

    /// <summary>
    /// 处理状态 (0 = Pending, 1 = Processing, 2 = Success, 3 = Failed)
    /// </summary>
    public int Status { get; private set; }

    /// <summary>
    /// 重试次数
    /// </summary>
    public int RetryCount { get; private set; }

    /// <summary>
    /// 下次重试时间
    /// </summary>
    public DateTime? NextRetryTime { get; private set; }

    /// <summary>
    /// 错误信息
    /// </summary>
    public string Error { get; private set; }

    /// <summary>
    /// 解析的记录数
    /// </summary>
    public int? RecordCount { get; private set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// 处理完成时间
    /// </summary>
    public DateTime? ProcessedAt { get; private set; }

    /// <summary>
    /// 任务开始处理的时间（用于判断卡死）
    /// </summary>
    public DateTime? ProcessingStartedAt { get; private set; }

    /// <summary>
    /// 过期时间
    /// </summary>
    public DateTime ExpiresAt { get; private set; }

    /// <summary>
    /// 租户ID
    /// </summary>
    public Guid? TenantId { get; private set; }


    protected WorkshopTelemetryTask() { }

    public WorkshopTelemetryTask(
        Guid fileObjectId,
        DateTime expiresAt,
        Guid? tenantId = null)
    {
        FileObjectId = fileObjectId;
        Status = 0;
        RetryCount = 0;
        CreatedAt = DateTime.UtcNow;
        ExpiresAt = expiresAt;
        TenantId = tenantId;
    }

    /// <summary>
    /// 标记为处理中
    /// </summary>
    public void MarkAsProcessing()
    {
        Status = 1;
        ProcessingStartedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// 标记为成功
    /// </summary>
    public void MarkAsSuccess(int recordCount)
    {
        Status = 2;
        RecordCount = recordCount;
        ProcessedAt = DateTime.UtcNow;
        ProcessingStartedAt = null;
    }

    /// <summary>
    /// 标记为失败
    /// </summary>
    public void MarkAsFailed(string error)
    {
        Status = 3;
        Error = error;
        RetryCount++;
        // 指数退避：2^RetryCount 分钟，最大 60 分钟
        var delayMinutes = Math.Min(60, Math.Pow(2, RetryCount));
        NextRetryTime = DateTime.UtcNow.AddMinutes(delayMinutes);
        ProcessingStartedAt = null;
    }

    /// <summary>
    /// 重置为重试
    /// </summary>
    public void ResetForRetry()
    {
        if (Status != 3)
            throw new InvalidOperationException("只有失败的任务才能重试");

        Status = 0;
        Error = null;
        NextRetryTime = null;
        ProcessingStartedAt = null;
    }
}