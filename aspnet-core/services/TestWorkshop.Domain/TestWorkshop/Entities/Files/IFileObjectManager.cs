namespace TestWorkshop;

public interface IFileObjectManager
{

    /// <summary>
    /// 上传文件（自动识别存储策略）
    /// </summary>
    Task<FileObject> UploadAsync(
        Stream stream,
        string fileName,
        string ownerType = null,
        string ownerId = null,
        string contentType = null);

    /// <summary>
    /// 删除业务对象关联的所有文件
    /// </summary>
    Task DeleteBusinessFilesAsync(string ownerType, string ownerId);

    /// <summary>
    /// 删除单个文件
    /// </summary>
    Task DeleteFileAsync(Guid fileId);

    /// <summary>
    /// 获取文件内容
    /// </summary>
    Task<(Stream Content, string ContentType, string FileName)> GetFileAsync(Guid fileId);

    /// <summary>
    /// 获取指定业务对象的所有文件列表
    /// </summary>
    Task<List<FileObject>> GetFilesByOwnerAsync(string ownerType, string ownerId);

    /// <summary>
    /// 替换业务对象的所有文件（全量替换）
    /// </summary>
    Task ReplaceFilesAsync(
        string ownerType,
        string ownerId,
        List<(Stream Stream, string FileName, string ContentType)> newFiles);

}
