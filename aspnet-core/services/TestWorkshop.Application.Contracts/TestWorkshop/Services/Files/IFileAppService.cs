using System.IO;
using Volo.Abp.Content;

namespace TestWorkshop;

/// <summary>
/// 文件管理应用服务接口
/// </summary>
public interface IFileAppService : IApplicationService
{
    /// <summary>
    /// 上传文件
    /// </summary>
    Task<FileObjectDto> UploadAsync(IRemoteStreamContent file, string ownerType, string ownerId = null);

    /// <summary>
    /// 批量上传文件
    /// </summary>
    /// <returns></returns>
    Task<List<FileObjectDto>> BatchUploadAsync(List<IRemoteStreamContent> files, string ownerType, string ownerId);

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
    /// 按 ownerType + ownerId 下载文件（ownerId 为 null 时获取系统文件）
    /// </summary>
    Task<IRemoteStreamContent> DownloadByOwnerAsync(string ownerType, string ownerId = null);

    /// <summary>
    /// 删除单个文件
    /// </summary>
    Task DeleteAsync(Guid id);

    /// <summary>
    /// 删除文件
    /// </summary>
    Task DeleteFilesAsync(string ownerType, string ownerId = null);
}
