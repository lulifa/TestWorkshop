using Volo.Abp.BlobStoring;

namespace TestWorkshop;

public class FileObjectManager : DomainService, IFileObjectManager
{
    private readonly IBlobContainer _blobContainer;
    private readonly IFileObjectRepository _fileObjectRepository;
    private readonly IGuidGenerator _guidGenerator;
    private readonly ILogger<FileObjectManager> _logger;

    public FileObjectManager(
        IBlobContainer blobContainer,
        IFileObjectRepository fileObjectRepository,
        IGuidGenerator guidGenerator,
        ILogger<FileObjectManager> logger)
    {
        _blobContainer = blobContainer;
        _fileObjectRepository = fileObjectRepository;
        _guidGenerator = guidGenerator;
        _logger = logger;
    }

    /// <summary>
    /// 上传单个文件（覆盖模式：上传新文件，自动删除同 ownerType + ownerId 的所有旧文件）
    /// </summary>
    [UnitOfWork]
    public virtual async Task<FileObject> UploadAsync(Stream stream, string fileName, string ownerType, string ownerId = null, string contentType = null)
    {
        return await UploadCoreAsync(stream, fileName, ownerType, ownerId, contentType, deleteOld: true);
    }

    /// <summary>
    /// 批量上传文件（覆盖模式：上传所有新文件，再统一删除同 ownerType + ownerId 的所有旧文件）
    /// </summary>
    [UnitOfWork]
    public virtual async Task<List<FileObject>> BatchUploadAsync(List<(Stream Stream, string FileName, string ContentType)> files, string ownerType, string ownerId)
    {
        Check.NotNullOrWhiteSpace(ownerType, nameof(ownerType));
        Check.NotNullOrWhiteSpace(ownerId, nameof(ownerId));

        if (files == null || !files.Any())
        {
            _logger.LogWarning("批量上传文件列表为空，跳过");
            return new List<FileObject>();
        }

        var uploadedFiles = new List<FileObject>();
        var uploadedIds = new List<Guid>();

        // 1. 先上传所有新文件（不删除旧文件）
        foreach (var (stream, fileName, contentType) in files)
        {
            var file = await UploadCoreAsync(stream, fileName, ownerType, ownerId, contentType, deleteOld: false);
            uploadedFiles.Add(file);
            uploadedIds.Add(file.Id);
        }

        // 2. 再统一删除旧文件（排除刚上传的）
        var oldFiles = await _fileObjectRepository.GetListAsync(
            f => f.OwnerType == ownerType && f.OwnerId == ownerId && !uploadedIds.Contains(f.Id)
        );

        foreach (var oldFile in oldFiles)
        {
            try
            {
                await _blobContainer.DeleteAsync(oldFile.BlobPath);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "删除旧物理文件失败: {BlobPath}", oldFile.BlobPath);
            }
            await _fileObjectRepository.HardDeleteAsync(oldFile, true);
        }

        _logger.LogInformation(
            "批量上传完成: {OwnerType}/{OwnerId}, 新文件数: {NewCount}, 旧文件数: {OldCount}",
            ownerType, ownerId, files.Count, oldFiles.Count);

        return uploadedFiles;
    }

    /// <summary>
    /// 获取文件列表（ownerId 为 null 查系统文件，有值查业务文件）
    /// </summary>
    public virtual async Task<PagedResultDto<FileObject>> GetListAsync(
        string keyword = null,
        string ownerType = null,
        string ownerId = null,
        DateTime? startTime = null,
        DateTime? endTime = null,
        int skipCount = 0,
        int maxResultCount = 10)
    {

        return await _fileObjectRepository.GetListAsync(
            keyword: keyword,
            ownerType: ownerType,
            ownerId: ownerId,
            startTime: startTime,
            endTime: endTime,
            skipCount: skipCount,
            maxResultCount: maxResultCount
        );
    }

    /// <summary>
    /// 删除文件（ownerId 为 null 删系统文件，有值删业务文件）
    /// </summary>
    [UnitOfWork]
    public virtual async Task DeleteFilesAsync(string ownerType, string ownerId = null)
    {
        Check.NotNullOrWhiteSpace(ownerType, nameof(ownerType));

        // 系统文件校验
        if (string.IsNullOrEmpty(ownerId) && !SystemFileTypes.IsValid(ownerType))
        {
            throw new BusinessException($"非法的系统文件类型: {ownerType}，允许的类型: {string.Join(", ", SystemFileTypes.AllowedTypes)}");
        }

        var files = await _fileObjectRepository.GetListAsync(f => f.OwnerType == ownerType && f.OwnerId == ownerId);

        if (!files.Any())
        {
            _logger.LogInformation("没有找到要删除的文件: {OwnerType}/{OwnerId}", ownerType, ownerId ?? "(系统)");
            return;
        }

        foreach (var file in files)
        {
            try
            {
                await _blobContainer.DeleteAsync(file.BlobPath);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "删除物理文件失败: {BlobPath}", file.BlobPath);
            }
        }

        await _fileObjectRepository.HardDeleteAsync(files, true);

        var target = string.IsNullOrEmpty(ownerId) ? "系统文件" : "业务文件";
        _logger.LogInformation("已删除 {Target}: {OwnerType}/{OwnerId}, 数量: {Count}",
            target, ownerType, ownerId ?? "(系统)", files.Count);
    }

    /// <summary>
    /// 删除单个文件
    /// </summary>
    [UnitOfWork]
    public virtual async Task DeleteFileAsync(Guid fileId)
    {
        var file = await _fileObjectRepository.GetAsync(fileId);
        if (file == null)
            throw new UserFriendlyException($"文件不存在: {fileId}");

        try
        {
            await _blobContainer.DeleteAsync(file.BlobPath);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "删除物理文件失败: {BlobPath}", file.BlobPath);
        }

        await _fileObjectRepository.HardDeleteAsync(file, true);
        _logger.LogInformation("已删除文件: {FileId}", fileId);
    }

    /// <summary>
    /// 获取文件流（用于下载/预览）
    /// </summary>
    public virtual async Task<(Stream Content, string ContentType, string FileName)> GetFileAsync(Guid fileId)
    {
        var fileObject = await _fileObjectRepository.GetAsync(fileId);
        if (fileObject == null)
            throw new UserFriendlyException($"文件不存在: {fileId}");

        var stream = await _blobContainer.GetOrNullAsync(fileObject.BlobPath);
        if (stream == null)
            throw new UserFriendlyException($"物理文件不存在: {fileObject.BlobPath}");

        return (stream, fileObject.ContentType, fileObject.FileName);
    }

    /// <summary>
    /// 按 ownerType + ownerId 获取文件（ownerId 为 null 时获取系统文件，不存在返回 null）
    /// </summary>
    public virtual async Task<FileObject> GetFileObjectByOwnerAsync(
        string ownerType,
        string ownerId = null)
    {
        Check.NotNullOrWhiteSpace(ownerType, nameof(ownerType));

        var files = await _fileObjectRepository.GetListAsync(
            f => f.OwnerType == ownerType && f.OwnerId == ownerId);

        return files.FirstOrDefault();
    }

    /// <summary>
    /// 获取文件元数据
    /// </summary>
    public virtual async Task<FileObject> GetFileObjectAsync(Guid fileId)
    {
        var file = await _fileObjectRepository.GetAsync(fileId);
        if (file == null)
            throw new UserFriendlyException($"文件不存在: {fileId}");
        return file;
    }

    /// <summary>
    /// 核心上传逻辑
    /// </summary>
    private async Task<FileObject> UploadCoreAsync(Stream stream, string fileName, string ownerType, string ownerId, string contentType, bool deleteOld)
    {
        Check.NotNull(stream, nameof(stream));
        Check.NotNullOrWhiteSpace(fileName, nameof(fileName));
        Check.NotNullOrWhiteSpace(ownerType, nameof(ownerType));

        // 系统文件校验：ownerId 为空，检查是否在白名单中
        if (string.IsNullOrEmpty(ownerId) && !SystemFileTypes.IsValid(ownerType))
        {
            throw new BusinessException($"非法的系统文件类型: {ownerType}，允许的类型: {string.Join(", ", SystemFileTypes.AllowedTypes)}");
        }

        var fileId = _guidGenerator.Create();
        var fileSize = stream.Length;
        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        var finalContentType = contentType ?? GetContentTypeByExtension(ext);

        var blobPath = GenerateBlobPath(ownerType, ownerId, fileId, ext);

        stream.Position = 0;
        await _blobContainer.SaveAsync(blobPath, stream, true);

        var fileObject = new FileObject(
            id: fileId,
            blobPath: blobPath,
            fileName: fileName,
            fileSize: fileSize,
            contentType: finalContentType,
            ownerId: ownerId,
            ownerType: ownerType,
            tenantId: CurrentTenant.Id
        );

        await _fileObjectRepository.InsertAsync(fileObject);

        // 覆盖模式：删除同 ownerType + ownerId 的所有旧文件
        if (deleteOld)
        {
            var oldFiles = await _fileObjectRepository.GetListAsync(
                f => f.OwnerType == ownerType && f.OwnerId == ownerId && f.Id != fileId
            );

            foreach (var oldFile in oldFiles)
            {
                try
                {
                    await _blobContainer.DeleteAsync(oldFile.BlobPath);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "删除旧物理文件失败: {BlobPath}", oldFile.BlobPath);
                }
                await _fileObjectRepository.HardDeleteAsync(oldFile, true);
            }

            if (oldFiles.Any())
            {
                _logger.LogInformation("删除了 {Count} 个旧文件 (OwnerType={OwnerType}, OwnerId={OwnerId})",
                    oldFiles.Count, ownerType, ownerId ?? "(系统)");
            }
        }

        _logger.LogInformation("文件上传成功: {BlobPath}", blobPath);
        return fileObject;
    }

    /// <summary>
    /// 生成物理存储路径
    /// </summary>
    protected virtual string GenerateBlobPath(string ownerType, string ownerId, Guid fileId, string ext)
    {
        var now = DateTime.UtcNow;
        var lowerType = ownerType.ToLowerInvariant();

        if (string.IsNullOrEmpty(ownerId))
        {
            var tenantId = CurrentTenant.Id;
            var basePath = tenantId.HasValue ? $"system/tenants/{tenantId}" : "system/global";
            return $"{basePath}/{lowerType}/{fileId:N}{ext}";
        }

        // ===== 日志型文件 =====
        if (FileOwnerTypeCategories.IsLogData(ownerType))
        {
            return $"rawdata/{lowerType}/{now:yyyy}/{now:MM}/{now:dd}/{fileId:N}{ext}";
        }

        // ===== 业务文件：ownerId 有值 =====
        return $"business/{lowerType}/{ownerId}/{fileId:N}{ext}";
    }

    /// <summary>
    /// 根据扩展名推断 ContentType
    /// </summary>
    protected virtual string GetContentTypeByExtension(string ext)
    {
        return ext.ToLowerInvariant() switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".bmp" => "image/bmp",
            ".webp" => "image/webp",
            ".svg" => "image/svg+xml",
            ".pdf" => "application/pdf",
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".xls" => "application/vnd.ms-excel",
            ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            ".txt" => "text/plain",
            ".csv" => "text/csv",
            ".json" => "application/json",
            ".zip" => "application/zip",
            ".rar" => "application/x-rar-compressed",
            ".7z" => "application/x-7z-compressed",
            _ => "application/octet-stream"
        };
    }
}
