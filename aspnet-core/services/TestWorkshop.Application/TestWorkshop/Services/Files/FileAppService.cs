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
    public virtual async Task<FileObjectDto> UploadAsync(IRemoteStreamContent file, FileOwnerInput input)
    {
        if (file == null || file.GetStream().Length == 0)
            throw new UserFriendlyException("Please select a valid file");

        using var stream = file.GetStream();

        var fileObject = await _fileObjectManager.UploadAsync(
            stream: stream,
            fileName: file.FileName,
            ownerType: input.OwnerType,
            ownerId: input.OwnerId,
            contentType: file.ContentType
        );

        return ObjectMapper.Map<FileObject, FileObjectDto>(fileObject);
    }

    /// <summary>
    /// 批量上传文件
    /// </summary>
    public virtual async Task<List<FileObjectDto>> BatchUploadAsync(List<IRemoteStreamContent> files, FileOwnerInput input)
    {
        if (files == null || files.Count == 0)
            throw new UserFriendlyException("Please select at least one valid file");

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
            ownerType: input.OwnerType,
            ownerId: input.OwnerId
        );

        if (input.OwnerType == SystemFileTypes.UserAvatar)
        {
            var avatar = await _fileObjectManager.GetFileObjectByOwnerAsync(input.OwnerType, input.OwnerId);
            if (avatar != null)
            {
                var extraFiles = fileObjects.Where(f => f.Id != avatar.Id).ToList();
                foreach (var extraFile in extraFiles)
                {
                    await _fileObjectManager.DeleteFileAsync(extraFile.Id);
                }
            }
        }

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
    /// 按业务对象下载文件
    /// </summary>
    public virtual async Task<IRemoteStreamContent> DownloadByOwnerAsync(FileOwnerInput input)
    {
        var fileObject = await _fileObjectManager.GetFileObjectByOwnerAsync(input.OwnerType, input.OwnerId);
        if (fileObject == null)
            return null;

        var (stream, contentType, fileName) = await _fileObjectManager.GetFileAsync(fileObject.Id);
        return new RemoteStreamContent(stream, fileName, contentType);
    }

    /// <summary>
    /// 获取当前用户头像，未上传时返回空
    /// </summary>
    public virtual async Task<IRemoteStreamContent> DownloadCurrentUserAvatarAsync()
    {
        var userId = CurrentUser.Id?.ToString();
        if (string.IsNullOrWhiteSpace(userId))
            throw new UserFriendlyException("当前用户不存在");

        var fileObject = await _fileObjectManager.GetFileObjectByOwnerAsync(SystemFileTypes.UserAvatar, userId);
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
    /// 按业务对象删除文件
    /// </summary>
    public virtual async Task DeleteFilesAsync(FileOwnerInput input)
    {
        await _fileObjectManager.DeleteFilesAsync(input.OwnerType, input.OwnerId);
    }
}