using Volo.Abp.BlobStoring;

namespace TestWorkshop;

public class FileObjectManager : DomainService, IFileObjectManager
{

    private readonly IBlobContainer _blobContainer;
    private readonly IFileObjectRepository _fileObjectRepository;
    private readonly IGuidGenerator _guidGenerator;
    private readonly IUnitOfWorkManager _unitOfWorkManager;
    private readonly ILogger<FileObjectManager> _logger;

    public FileObjectManager(
        IBlobContainer blobContainer,
        IFileObjectRepository fileObjectRepository,
        IGuidGenerator guidGenerator,
        IUnitOfWorkManager unitOfWorkManager,
        ILogger<FileObjectManager> logger)
    {
        _blobContainer = blobContainer;
        _fileObjectRepository = fileObjectRepository;
        _guidGenerator = guidGenerator;
        _unitOfWorkManager = unitOfWorkManager;
        _logger = logger;
    }

    public virtual async Task<FileObject> UploadAsync(
        Stream stream,
        string fileName,
        string ownerType = null,
        string ownerId = null,
        string contentType = null)
    {
        Check.NotNull(stream, nameof(stream));
        Check.NotNullOrWhiteSpace(fileName, nameof(fileName));

        if (!string.IsNullOrEmpty(ownerType) && string.IsNullOrEmpty(ownerId))
        {
            throw new BusinessException("业务文件必须同时指定 OwnerType 和 OwnerId");
        }

        var fileId = _guidGenerator.Create();
        var fileSize = stream.Length;
        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        var finalContentType = contentType ?? GetContentTypeByExtension(ext);

        var blobPath = GenerateBlobPath(fileName, ownerType, ownerId, fileId, ext);

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

        _logger.LogInformation("文件上传成功: {BlobPath}", blobPath);
        return fileObject;
    }

    [UnitOfWork]
    public virtual async Task DeleteBusinessFilesAsync(string ownerType, string ownerId)
    {
        Check.NotNullOrWhiteSpace(ownerType, nameof(ownerType));
        Check.NotNullOrWhiteSpace(ownerId, nameof(ownerId));

        var files = await _fileObjectRepository.GetListAsync(f => f.OwnerType == ownerType && f.OwnerId == ownerId
        );

        if (!files.Any()) return;

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

        await _fileObjectRepository.DeleteManyAsync(files);
        _logger.LogInformation("已删除业务对象所有文件: {OwnerType}/{OwnerId}, 数量: {Count}", ownerType, ownerId, files.Count);
    }

    [UnitOfWork]
    public virtual async Task DeleteFileAsync(Guid fileId)
    {
        var file = await _fileObjectRepository.GetAsync(fileId);
        if (file == null) throw new UserFriendlyException($"文件不存在: {fileId}");

        try
        {
            await _blobContainer.DeleteAsync(file.BlobPath);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "删除物理文件失败: {BlobPath}", file.BlobPath);
        }

        await _fileObjectRepository.DeleteAsync(file);
    }

    public virtual async Task<(Stream Content, string ContentType, string FileName)> GetFileAsync(Guid fileId)
    {
        var fileObject = await _fileObjectRepository.GetAsync(fileId);
        if (fileObject == null) throw new UserFriendlyException($"文件不存在: {fileId}");

        var stream = await _blobContainer.GetAsync(fileObject.BlobPath);
        if (stream == null) throw new UserFriendlyException($"物理文件不存在: {fileObject.BlobPath}");

        return (stream, fileObject.ContentType, fileObject.FileName);
    }

    public virtual async Task<List<FileObject>> GetFilesByOwnerAsync(string ownerType, string ownerId)
    {
        Check.NotNullOrWhiteSpace(ownerType, nameof(ownerType));
        Check.NotNullOrWhiteSpace(ownerId, nameof(ownerId));

        return await _fileObjectRepository.GetListAsync(f => f.OwnerType == ownerType && f.OwnerId == ownerId);
    }

    [UnitOfWork]
    public virtual async Task ReplaceFilesAsync(
        string ownerType,
        string ownerId,
        List<(Stream Stream, string FileName, string ContentType)> newFiles)
    {
        await DeleteBusinessFilesAsync(ownerType, ownerId);

        foreach (var (stream, fileName, contentType) in newFiles)
        {
            await UploadAsync(stream, fileName, ownerType, ownerId, contentType);
        }

        _logger.LogInformation("已替换业务对象所有文件: {OwnerType}/{OwnerId}, 新文件数: {Count}", ownerType, ownerId, newFiles.Count);
    }

    protected virtual string GenerateBlobPath(
        string fileName,
        string ownerType,
        string ownerId,
        Guid fileId,
        string ext)
    {
        var now = DateTime.UtcNow;

        // 系统级文件
        if (string.IsNullOrEmpty(ownerType))
        {
            var tenantId = CurrentTenant.Id;
            return tenantId.HasValue
                ? $"system/tenants/{tenantId}/{fileId:N}{ext}"
                : $"system/global/{fileId:N}{ext}";
        }

        // 日志型（注册表）
        if (FileOwnerTypeCategories.IsLogData(ownerType))
        {
            return $"rawdata/{ownerType}/{now:yyyy}/{now:MM}/{now:dd}/{fileId:N}{ext}";
        }

        // 状态型（默认）
        if (string.IsNullOrEmpty(ownerId))
        {
            throw new BusinessException($"状态型数据必须指定 OwnerId，OwnerType: {ownerType}");
        }
        return $"business/{ownerType}/{ownerId}/{fileId:N}{ext}";
    }

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
            ".zip" => "application/zip",
            ".rar" => "application/x-rar-compressed",
            ".7z" => "application/x-7z-compressed",
            _ => "application/octet-stream"
        };
    }

}
