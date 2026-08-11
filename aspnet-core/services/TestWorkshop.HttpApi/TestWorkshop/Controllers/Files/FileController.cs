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
        string ownerType,
        string ownerId = null)
    {
        return await Service.UploadAsync(file, ownerType, ownerId);
    }

    /// <summary>
    /// 批量上传文件
    /// </summary>
    [HttpPost("batch")]
    public async Task<List<FileObjectDto>> BatchUploadAsync(
        List<IRemoteStreamContent> files,
        string ownerType,
        string ownerId)
    {
        return await Service.BatchUploadAsync(files, ownerType, ownerId);
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
    /// 批量删除文件
    /// </summary>
    [HttpDelete]
    public async Task DeleteFilesAsync(string ownerType, string ownerId = null)
    {
        await Service.DeleteFilesAsync(ownerType, ownerId);
    }
}