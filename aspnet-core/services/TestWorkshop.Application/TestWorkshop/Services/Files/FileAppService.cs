using System.IO;
using Volo.Abp.Content;

namespace TestWorkshop;

[Authorize]
public class FileAppService : ApplicationService, IFileAppService
{
    private readonly IFileObjectManager _fileObjectManager;

    public FileAppService(IFileObjectManager fileObjectManager)
    {
        _fileObjectManager = fileObjectManager;
    }

    /// <summary>
    /// 上传文件
    /// </summary>
    public virtual async Task<FileObjectDto> UploadAsync(IRemoteStreamContent file, string ownerType, string ownerId = null)
    {
        if (file == null || file.GetStream().Length == 0)
            throw new UserFriendlyException("Please select a valid file");

        if (string.IsNullOrWhiteSpace(ownerType))
            throw new UserFriendlyException("ownerType 不能为空");

        using var stream = file.GetStream();

        var fileObject = await _fileObjectManager.UploadAsync(
            stream: stream,
            fileName: file.FileName,
            ownerType: ownerType,
            ownerId: ownerId,
            contentType: file.ContentType
        );

        return ObjectMapper.Map<FileObject, FileObjectDto>(fileObject);
    }

    /// <summary>
    /// 批量上传文件
    /// </summary>
    public virtual async Task<List<FileObjectDto>> BatchUploadAsync(List<IRemoteStreamContent> files, string ownerType, string ownerId)
    {
        if (files == null || files.Count == 0)
            throw new UserFriendlyException("Please select at least one valid file");

        if (string.IsNullOrWhiteSpace(ownerType))
            throw new UserFriendlyException("ownerType 不能为空");

        var fileTuples = new List<(Stream Stream, string FileName, string ContentType)>();
        foreach (var file in files)
        {
            if (file == null || file.ContentLength == 0)
                continue;

            fileTuples.Add((file.GetStream(), file.FileName, file.ContentType));
        }

        if (fileTuples.Count == 0)
            throw new UserFriendlyException("没有有效的文件");

        var fileObjects = await _fileObjectManager.BatchUploadAsync(
            files: fileTuples,
            ownerType: ownerType,
            ownerId: ownerId
        );

        return ObjectMapper.Map<List<FileObject>, List<FileObjectDto>>(fileObjects);
    }

    /// <summary>
    /// 获取文件列表（分页 + 多条件过滤）
    /// </summary>
    public virtual async Task<PagedResultDto<FileObjectDto>> GetFilesAsync(GetFileListInput input)
    {

        if (!input.IsPaged)
        {
            input.SkipCount = 0;
            input.MaxResultCount = int.MaxValue;
        }

        var result = await _fileObjectManager.GetListAsync(
            input.Keyword,
            input.OwnerType,
            input.OwnerId,
            input.StartTime,
            input.EndTime,
            input.SkipCount,
            input.MaxResultCount);

        var totalCount = result.TotalCount;

        var items = ObjectMapper.Map<List<FileObject>, List<FileObjectDto>>(result.Items.ToList());

        return new PagedResultDto<FileObjectDto>(totalCount, items);

    }

    /// <summary>
    /// 获取单个文件信息
    /// </summary>
    public virtual async Task<FileObjectDto> GetAsync(Guid id)
    {
        var file = await _fileObjectManager.GetFileObjectAsync(id);
        return ObjectMapper.Map<FileObject, FileObjectDto>(file);
    }

    /// <summary>
    /// 下载文件
    /// </summary>
    public virtual async Task<IRemoteStreamContent> DownloadAsync(Guid id)
    {
        var (stream, contentType, fileName) = await _fileObjectManager.GetFileAsync(id);
        return new RemoteStreamContent(stream, fileName, contentType);
    }

    /// <summary>
    /// 按 ownerType + ownerId 下载文件（ownerId 为 null 时获取系统文件）
    /// </summary>
    public virtual async Task<IRemoteStreamContent> DownloadByOwnerAsync(string ownerType, string ownerId = null)
    {
        if (string.IsNullOrWhiteSpace(ownerType))
            throw new UserFriendlyException("ownerType 不能为空");

        var fileObject = await _fileObjectManager.GetFileObjectByOwnerAsync(ownerType, ownerId);
        if (fileObject == null)
            return null;

        var (stream, contentType, fileName) = await _fileObjectManager.GetFileAsync(fileObject.Id);
        return new RemoteStreamContent(stream, fileName, contentType);
    }

    /// <summary>
    /// 删除单个文件
    /// </summary>
    public virtual async Task DeleteAsync(Guid id)
    {
        await _fileObjectManager.DeleteFileAsync(id);
    }

    /// <summary>
    /// 删除文件
    /// </summary>
    public virtual async Task DeleteFilesAsync(string ownerType, string ownerId = null)
    {
        if (string.IsNullOrWhiteSpace(ownerType))
            throw new UserFriendlyException("ownerType 不能为空");

        await _fileObjectManager.DeleteFilesAsync(ownerType, ownerId);
    }

}
