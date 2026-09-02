namespace TestWorkshop;

/// <summary>
/// 文件管理应用服务接口
/// </summary>
public interface IFileAppService : IApplicationService
{
    /// <summary>
    /// 上传文件
    /// </summary>
    Task<FileObjectDto> UploadAsync(IRemoteStreamContent file, FileOwnerInput input);

    /// <summary>
    /// 批量上传文件
    /// </summary>
    Task<List<FileObjectDto>> BatchUploadAsync(List<IRemoteStreamContent> files, FileOwnerInput input);

    /// <summary>
    /// 获取文件列表（分页 + 多条件过滤）
    /// </summary>
    Task<PagedResultDto<FileObjectDto>> GetFilesAsync(GetFileListInput input);

    /// <summary>
    /// 获取单个文件信息
    /// </summary>
    Task<FileObjectDto> GetAsync(Guid id);

    /// <summary>
    /// 下载文件
    /// </summary>
    Task<IRemoteStreamContent> DownloadAsync(Guid id);

    /// <summary>
    /// 按业务对象下载文件
    /// </summary>
    Task<IRemoteStreamContent> DownloadByOwnerAsync(FileOwnerInput input);

    /// <summary>
    /// 获取当前用户头像，未上传时返回空
    /// </summary>
    Task<IRemoteStreamContent> DownloadCurrentUserAvatarAsync();

    /// <summary>
    /// 删除单个文件
    /// </summary>
    Task DeleteAsync(Guid id);

    /// <summary>
    /// 按业务对象删除文件
    /// </summary>
    Task DeleteFilesAsync(FileOwnerInput input);
}