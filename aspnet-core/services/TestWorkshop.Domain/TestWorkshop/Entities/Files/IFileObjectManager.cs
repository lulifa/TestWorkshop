namespace TestWorkshop;

/// <summary>
/// 文件管理领域服务
/// </summary>
public interface IFileObjectManager
{
    /// <summary>
    /// 上传单个文件（覆盖模式：上传新文件，自动删除同 ownerType + ownerId 的所有旧文件）
    /// </summary>
    Task<FileObject> UploadAsync(
        Stream stream,
        string fileName,
        string ownerType,
        string ownerId = null,
        string contentType = null);

    /// <summary>
    /// 批量上传文件（覆盖模式：上传所有新文件，再统一删除同 ownerType + ownerId 的所有旧文件）
    /// </summary>
    Task<List<FileObject>> BatchUploadAsync(
        List<(Stream Stream, string FileName, string ContentType)> files,
        string ownerType,
        string ownerId);

    /// <summary>
    /// 按业务类型查询所有文件（支持分页）
    /// </summary>
    Task<PagedResultDto<FileObject>> GetListAsync(
        string keyword = null,
        string ownerType = null,
        string ownerId = null,
        DateTime? startTime = null,
        DateTime? endTime = null,
        int skipCount = 0,
        int maxResultCount = 10);

    /// <summary>
    /// 按 ownerType + ownerId 删除文件
    /// </summary>
    Task DeleteFilesAsync(string ownerType, string ownerId);

    /// <summary>
    /// 删除单个文件
    /// </summary>
    Task DeleteFileAsync(Guid fileId);

    /// <summary>
    /// 获取文件流（用于下载/预览）
    /// </summary>
    Task<(Stream Content, string ContentType, string FileName)> GetFileAsync(Guid fileId);

    /// <summary>
    /// 按 ownerType + ownerId 获取文件（不存在返回 null）
    /// </summary>
    Task<FileObject> GetFileObjectByOwnerAsync(
        string ownerType,
        string ownerId);

    /// <summary>
    /// 获取文件元数据
    /// </summary>
    Task<FileObject> GetFileObjectAsync(Guid fileId);
}
