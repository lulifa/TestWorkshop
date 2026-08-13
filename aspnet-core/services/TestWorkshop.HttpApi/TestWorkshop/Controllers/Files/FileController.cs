using Volo.Abp.Content;

namespace TestWorkshop.Controllers;

/// <summary>
/// 文件管理
/// </summary>
[Route("api/platform/files")]
public class FileController : TestWorkshopController
{
    private readonly IFileAppService Service;

    public FileController(IFileAppService fileAppService)
    {
        Service = fileAppService;
    }

    /// <summary>
    /// 上传文件
    /// </summary>
    [HttpPost("upload")]
    public async Task<FileObjectDto> UploadAsync(
        IRemoteStreamContent file,
        [FromForm] FileOwnerInput input)
    {
        return await Service.UploadAsync(file, input);
    }

    /// <summary>
    /// 批量上传文件
    /// </summary>
    [HttpPost("batch")]
    public async Task<List<FileObjectDto>> BatchUploadAsync(
        [FromForm] List<IRemoteStreamContent> files,
        [FromForm] FileOwnerInput input)
    {
        return await Service.BatchUploadAsync(files, input);
    }

    /// <summary>
    /// 获取文件列表（分页 + 多条件过滤）
    /// </summary>
    [HttpGet]
    public async Task<PagedResultDto<FileObjectDto>> GetFilesAsync(GetFileListInput input)
    {
        return await Service.GetFilesAsync(input);
    }

    /// <summary>
    /// 按业务对象下载文件
    /// </summary>
    [HttpGet("by-owner")]
    public async Task<IRemoteStreamContent> DownloadByOwnerAsync(FileOwnerInput input)
    {
        return await Service.DownloadByOwnerAsync(input);
    }

    /// <summary>
    /// 获取当前用户头像，未上传时返回空
    /// </summary>
    [HttpGet("user-avatar")]
    public async Task<IRemoteStreamContent> DownloadCurrentUserAvatarAsync()
    {
        return await Service.DownloadCurrentUserAvatarAsync();
    }

    /// <summary>
    /// 获取单个文件信息
    /// </summary>
    [HttpGet("{id}")]
    public async Task<FileObjectDto> GetAsync(Guid id)
    {
        return await Service.GetAsync(id);
    }

    /// <summary>
    /// 下载文件
    /// </summary>
    [HttpGet("{id}/download")]
    public async Task<IRemoteStreamContent> DownloadAsync(Guid id)
    {
        return await Service.DownloadAsync(id);
    }

    /// <summary>
    /// 删除单个文件
    /// </summary>
    [HttpDelete("{id}")]
    public async Task DeleteAsync(Guid id)
    {
        await Service.DeleteAsync(id);
    }

    /// <summary>
    /// 按业务对象删除文件
    /// </summary>
    [HttpDelete]
    public async Task DeleteFilesAsync(FileOwnerInput input)
    {
        await Service.DeleteFilesAsync(input);
    }
}