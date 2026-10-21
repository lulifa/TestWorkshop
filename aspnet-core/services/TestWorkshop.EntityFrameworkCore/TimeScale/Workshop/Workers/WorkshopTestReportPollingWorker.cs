namespace TestWorkshop.EntityFrameworkCore;

using Microsoft.Extensions.Options;
using Volo.Abp.Guids;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Timing;
using Volo.Abp.Uow;

/// <summary>
/// 测试报告获取 Worker。
/// 自动调度和手动请求统一通过请求记录表排队，再由同一个流程调用外部接口。
/// </summary>
public class WorkshopTestReportPollingWorker : AsyncPeriodicBackgroundWorkerBase
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<WorkshopTestReportPollingWorker> _logger;
    private readonly WorkshopTestReportOptions _options;

    public WorkshopTestReportPollingWorker(
        AbpAsyncTimer timer,
        IServiceScopeFactory scopeFactory,
        ILogger<WorkshopTestReportPollingWorker> logger,
        IOptions<WorkshopTestReportOptions> options)
        : base(timer, scopeFactory)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _options = options.Value;
        _options.Validate();
        Timer.Period = (int)TimeSpan
            .FromSeconds(_options.PollingIntervalSeconds)
            .TotalMilliseconds;
    }

    public override async Task StartAsync(CancellationToken cancellationToken = default)
    {
        if (_options.Enabled)
        {
            await RecoverStaleRecordsAsync(cancellationToken);
        }

        await base.StartAsync(cancellationToken);
    }

    protected override async Task DoWorkAsync(PeriodicBackgroundWorkerContext workerContext)
    {
        if (!_options.Enabled)
        {
            return;
        }

        var ct = workerContext.CancellationToken;
        try
        {
            await ScheduleAutomaticFetchesAsync(ct);
            await ProcessPendingRecordsAsync(ct);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "测试报告获取 Worker 执行异常，Worker 将继续运行");
        }
    }

    private async Task ScheduleAutomaticFetchesAsync(CancellationToken ct)
    {
        List<WorkshopOrder> candidates;
        DateTime now;
        using (var scope = _scopeFactory.CreateScope())
        {
            var clock = scope.ServiceProvider.GetRequiredService<IClock>();
            var orderRepository =
                scope.ServiceProvider.GetRequiredService<IWorkshopOrderRepository>();
            now = NormalizeTime(clock.Now);
            candidates = await orderRepository.GetTestReportFetchCandidatesAsync(
                now.AddDays(-_options.MaxFetchDaysAfterTest),
                _options.BatchSize,
                ct);
        }

        foreach (var candidate in candidates)
        {
            await ScheduleAutomaticFetchAsync(candidate.Id, candidate.TenantId, now, ct);
        }
    }

    private async Task ScheduleAutomaticFetchAsync(
        Guid orderId,
        Guid? tenantId,
        DateTime now,
        CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var currentTenant = scope.ServiceProvider.GetRequiredService<ICurrentTenant>();
        using (currentTenant.Change(tenantId))
        {
            var unitOfWorkManager =
                scope.ServiceProvider.GetRequiredService<IUnitOfWorkManager>();
            using var uow = unitOfWorkManager.Begin(requiresNew: true, isTransactional: true);

            var orderRepository =
                scope.ServiceProvider.GetRequiredService<IWorkshopOrderRepository>();
            var recordRepository =
                scope.ServiceProvider.GetRequiredService<IWorkshopOrderReportFetchRecordRepository>();
            var guidGenerator = scope.ServiceProvider.GetRequiredService<IGuidGenerator>();

            var order = await orderRepository.FindAsync(orderId, cancellationToken: ct);
            if (order == null
                || order.Status != WorkshopOrderStatus.PendingReview
                || order.TestReportStatus == WorkshopTestReportStatus.Ready
                || await recordRepository.HasActiveRecordAsync(order.Id, ct))
            {
                return;
            }

            var latestRecord = (await recordRepository.GetListByOrderIdAsync(order.Id, ct))
                .FirstOrDefault();
            if (latestRecord is
                {
                    Status: WorkshopTestReportFetchRecordStatus.Failed,
                    IsRetryable: false
                })
            {
                return;
            }

            var attemptCount = await recordRepository.GetAttemptCountAsync(order.Id, ct);
            if (attemptCount >= _options.MaxAttempts)
            {
                order.MarkTestReportFetchFailed();
                await orderRepository.UpdateAsync(order, autoSave: true, cancellationToken: ct);
                await uow.CompleteAsync(ct);
                return;
            }

            var record = new WorkshopOrderReportFetchRecord(
                guidGenerator.Create(),
                order.Id,
                WorkshopTestReportFetchTriggerType.Automatic,
                null,
                attemptCount + 1,
                now,
                tenantId);
            await recordRepository.InsertAsync(record, autoSave: true, cancellationToken: ct);
            await uow.CompleteAsync(ct);
        }
    }

    private async Task ProcessPendingRecordsAsync(CancellationToken ct)
    {
        List<WorkshopOrderReportFetchRecord> records;
        using (var scope = _scopeFactory.CreateScope())
        {
            var clock = scope.ServiceProvider.GetRequiredService<IClock>();
            var recordRepository =
                scope.ServiceProvider.GetRequiredService<IWorkshopOrderReportFetchRecordRepository>();
            records = await recordRepository.ClaimPendingRecordsAsync(
                _options.BatchSize,
                NormalizeTime(clock.Now),
                ct);
        }

        foreach (var record in records)
        {
            await ProcessRecordAsync(record.Id, record.TenantId, ct);
        }
    }

    private async Task ProcessRecordAsync(
        Guid recordId,
        Guid? tenantId,
        CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var currentTenant = scope.ServiceProvider.GetRequiredService<ICurrentTenant>();
        using (currentTenant.Change(tenantId))
        {
            var unitOfWorkManager =
                scope.ServiceProvider.GetRequiredService<IUnitOfWorkManager>();
            using var uow = unitOfWorkManager.Begin(requiresNew: true, isTransactional: true);

            var recordRepository =
                scope.ServiceProvider.GetRequiredService<IWorkshopOrderReportFetchRecordRepository>();
            var orderRepository =
                scope.ServiceProvider.GetRequiredService<IWorkshopOrderRepository>();
            var deviceRepository =
                scope.ServiceProvider.GetRequiredService<IWorkshopDeviceRepository>();
            var fetcher =
                scope.ServiceProvider.GetRequiredService<IWorkshopTestReportFetcher>();
            var fileManager =
                scope.ServiceProvider.GetRequiredService<IFileObjectManager>();
            var fileObjectRepository =
                scope.ServiceProvider.GetRequiredService<IFileObjectRepository>();
            var guidGenerator = scope.ServiceProvider.GetRequiredService<IGuidGenerator>();
            var clock = scope.ServiceProvider.GetRequiredService<IClock>();

            var record = await recordRepository.GetAsync(recordId, cancellationToken: ct);
            if (record.Status != WorkshopTestReportFetchRecordStatus.Processing)
            {
                return;
            }

            var order = await orderRepository.GetAsync(record.OrderId, cancellationToken: ct);
            var now = NormalizeTime(clock.Now);
            if (order.Status != WorkshopOrderStatus.PendingReview)
            {
                record.MarkFailed(
                    now,
                    "ORDER_STATUS_INVALID",
                    "只有测试完成未审核的订单才能获取测试报告");
                await recordRepository.UpdateAsync(record, autoSave: true, cancellationToken: ct);
                await uow.CompleteAsync(ct);
                return;
            }

            if (order.TestReportStatus == WorkshopTestReportStatus.Ready)
            {
                record.MarkFailed(
                    now,
                    "REPORT_ALREADY_READY",
                    "测试报告已经获取");
                await recordRepository.UpdateAsync(record, autoSave: true, cancellationToken: ct);
                await uow.CompleteAsync(ct);
                return;
            }

            order.MarkTestReportFetching();
            await orderRepository.UpdateAsync(order, autoSave: true, cancellationToken: ct);

            var device = await deviceRepository.FindAsync(order.DeviceId, cancellationToken: ct);
            var request = new WorkshopTestReportFetchRequest
            {
                OrderId = order.Id,
                OrderNo = order.OrderNo,
                DeviceCode = device?.Code,
                DeviceName = device?.Name,
                DeviceType = device?.Type ?? default,
                TestStartedAt = order.TestStartedAt,
                TestEndedAt = order.TestEndedAt,
                RequiredCompletionTime = order.RequiredCompletionTime
            };

            var scheduleRetry = false;
            DateTime? retryAt = null;
            var nextAttemptNo = 0;
            try
            {
                var result = await fetcher.FetchAsync(request, ct);
                now = NormalizeTime(clock.Now);

                if (result.Status == WorkshopTestReportFetchResultStatus.Ready)
                {
                    if (result.Files.Count == 0)
                    {
                        throw new UserFriendlyException("外部接口返回报告已就绪，但没有文件");
                    }

                    var fileCount = await SaveReportFilesAsync(
                        order.Id,
                        result.Files,
                        fileManager,
                        guidGenerator,
                        ct);
                    record.MarkSucceeded(
                        now,
                        fileCount,
                        result.ExternalReportId);
                    order.SetTestReportReady(now);
                }
                else if (result.Status == WorkshopTestReportFetchResultStatus.NotReady)
                {
                    record.MarkNoReport(now, result.ExternalReportId);
                    order.MarkTestReportNotReady();
                    scheduleRetry = result.IsTransient;
                }
                else
                {
                    record.MarkFailed(
                        now,
                        result.ErrorCode,
                        result.ErrorMessage,
                        result.HttpStatusCode,
                        result.IsTransient);
                    order.MarkTestReportFetchFailed();
                    scheduleRetry = result.IsTransient;
                }

                if (scheduleRetry && record.AttemptNo < _options.MaxAttempts)
                {
                    nextAttemptNo = record.AttemptNo + 1;
                    retryAt = now.Add(_options.GetRetryDelay(record.AttemptNo));
                }

                await recordRepository.UpdateAsync(record, autoSave: true, cancellationToken: ct);
                await orderRepository.UpdateAsync(order, autoSave: true, cancellationToken: ct);
                await uow.CompleteAsync(ct);

                if (retryAt.HasValue)
                {
                    await CreateRetryRecordAsync(
                        order.Id,
                        nextAttemptNo,
                        retryAt.Value,
                        tenantId,
                        ct);
                }
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取测试报告失败: RecordId={RecordId}, OrderId={OrderId}",
                    record.Id, order.Id);

                now = NormalizeTime(clock.Now);
                record.MarkFailed(
                    now,
                    "FETCH_EXCEPTION",
                    ex.Message,
                    isRetryable: true);
                order.MarkTestReportFetchFailed();
                if (record.AttemptNo < _options.MaxAttempts)
                {
                    nextAttemptNo = record.AttemptNo + 1;
                    retryAt = now.Add(_options.GetRetryDelay(record.AttemptNo));
                }

                await recordRepository.UpdateAsync(record, autoSave: true, cancellationToken: ct);
                await orderRepository.UpdateAsync(order, autoSave: true, cancellationToken: ct);
                await uow.CompleteAsync(ct);

                if (retryAt.HasValue)
                {
                    await CreateRetryRecordAsync(
                        order.Id,
                        nextAttemptNo,
                        retryAt.Value,
                        tenantId,
                        ct);
                }
            }
        }
    }

    private async Task CreateRetryRecordAsync(
        Guid orderId,
        int attemptNo,
        DateTime scheduledAt,
        Guid? tenantId,
        CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var currentTenant = scope.ServiceProvider.GetRequiredService<ICurrentTenant>();
        using (currentTenant.Change(tenantId))
        {
            var unitOfWorkManager =
                scope.ServiceProvider.GetRequiredService<IUnitOfWorkManager>();
            using var uow = unitOfWorkManager.Begin(requiresNew: true, isTransactional: true);
            var recordRepository =
                scope.ServiceProvider.GetRequiredService<IWorkshopOrderReportFetchRecordRepository>();
            var guidGenerator = scope.ServiceProvider.GetRequiredService<IGuidGenerator>();

            if (await recordRepository.HasActiveRecordAsync(orderId, ct))
            {
                return;
            }

            var retryRecord = new WorkshopOrderReportFetchRecord(
                guidGenerator.Create(),
                orderId,
                WorkshopTestReportFetchTriggerType.Automatic,
                null,
                attemptNo,
                scheduledAt,
                tenantId);
            await recordRepository.InsertAsync(
                retryRecord,
                autoSave: true,
                cancellationToken: ct);
            await uow.CompleteAsync(ct);
        }
    }

    private async Task<int> SaveReportFilesAsync(
        Guid orderId,
        List<WorkshopTestReportFileContent> files,
        IFileObjectManager fileManager,
        IGuidGenerator guidGenerator,
        CancellationToken ct)
    {
        var ownerId = orderId.ToString();
        await fileManager.DeleteFilesAsync(SystemFileTypes.WorkshopTestReport, ownerId);

        var savedCount = 0;
        foreach (var file in files)
        {
            if (file.Content == null || file.FileName.IsNullOrWhiteSpace())
            {
                continue;
            }

            var rawFileId = guidGenerator.Create();
            await using (var normalizedContent = await NormalizeContentStreamAsync(
                             file.Content,
                             _options.MaxFileSizeBytes,
                             ct))
            {
                await fileManager.AppendAsync(
                    normalizedContent,
                    file.FileName,
                    SystemFileTypes.WorkshopTestReport,
                    ownerId,
                    file.ContentType,
                    rawFileId);
            }

            savedCount++;
        }

        return savedCount;
    }

    private static async Task<Stream> NormalizeContentStreamAsync(
        Stream stream,
        long maxFileSizeBytes,
        CancellationToken ct)
    {
        if (stream.CanSeek)
        {
            if (stream.Length > maxFileSizeBytes)
            {
                throw new UserFriendlyException(
                    $"报告文件超过大小限制 {maxFileSizeBytes} 字节");
            }

            stream.Position = 0;
            return stream;
        }

        var memory = new MemoryStream();
        var buffer = new byte[81920];
        var total = 0L;
        try
        {
            while (true)
            {
                var bytesRead = await stream.ReadAsync(buffer.AsMemory(), ct);
                if (bytesRead == 0)
                {
                    break;
                }

                total += bytesRead;
                if (total > maxFileSizeBytes)
                {
                    throw new UserFriendlyException(
                        $"报告文件超过大小限制 {maxFileSizeBytes} 字节");
                }

                await memory.WriteAsync(buffer.AsMemory(0, bytesRead), ct);
            }
        }
        catch
        {
            await memory.DisposeAsync();
            throw;
        }
        finally
        {
            await stream.DisposeAsync();
        }

        memory.Position = 0;
        return memory;
    }

    private async Task RecoverStaleRecordsAsync(CancellationToken ct)
    {
        List<WorkshopOrderReportFetchRecord> staleRecords;
        DateTime now;
        using (var scope = _scopeFactory.CreateScope())
        {
            var clock = scope.ServiceProvider.GetRequiredService<IClock>();
            var repository =
                scope.ServiceProvider.GetRequiredService<IWorkshopOrderReportFetchRecordRepository>();
            now = NormalizeTime(clock.Now);
            staleRecords = await repository.GetStaleProcessingRecordsAsync(
                now.AddMinutes(-_options.StaleProcessingMinutes),
                _options.BatchSize,
                ct);
        }

        foreach (var staleRecord in staleRecords)
        {
            await RecoverStaleRecordAsync(staleRecord.Id, staleRecord.TenantId, now, ct);
        }
    }

    private async Task RecoverStaleRecordAsync(
        Guid recordId,
        Guid? tenantId,
        DateTime now,
        CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var currentTenant = scope.ServiceProvider.GetRequiredService<ICurrentTenant>();
        using (currentTenant.Change(tenantId))
        {
            var unitOfWorkManager =
                scope.ServiceProvider.GetRequiredService<IUnitOfWorkManager>();
            using var uow = unitOfWorkManager.Begin(requiresNew: true, isTransactional: true);

            var recordRepository =
                scope.ServiceProvider.GetRequiredService<IWorkshopOrderReportFetchRecordRepository>();
            var orderRepository =
                scope.ServiceProvider.GetRequiredService<IWorkshopOrderRepository>();
            var record = await recordRepository.FindAsync(recordId, cancellationToken: ct);
            if (record == null
                || record.Status != WorkshopTestReportFetchRecordStatus.Processing)
            {
                return;
            }

            record.MarkFailed(
                now,
                "STALE_PROCESSING",
                "报告获取处理超时，已自动恢复",
                isRetryable: true);
            await recordRepository.UpdateAsync(record, autoSave: true, cancellationToken: ct);

            var order = await orderRepository.FindAsync(record.OrderId, cancellationToken: ct);
            if (order != null && order.Status == WorkshopOrderStatus.PendingReview)
            {
                order.MarkTestReportFetchFailed();
                await orderRepository.UpdateAsync(order, autoSave: true, cancellationToken: ct);
            }

            await uow.CompleteAsync(ct);
        }
    }

    private static DateTime NormalizeTime(DateTime value)
    {
        return DateTime.SpecifyKind(value, DateTimeKind.Unspecified);
    }
}
