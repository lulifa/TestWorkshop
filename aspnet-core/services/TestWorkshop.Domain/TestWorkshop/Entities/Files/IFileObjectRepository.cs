namespace TestWorkshop;

/// <summary>
/// 文件索引仓储接口
/// </summary>
public interface IFileObjectRepository : IBasicRepository<FileObject, Guid>
{
    /// <summary>
    /// 按条件查询文件列表
    /// </summary>
    Task<List<FileObject>> GetListAsync(Expression<Func<FileObject, bool>> predicate);

    /// <summary>
    /// 按条件查询文件列表
    /// </summary>
    Task<List<FileObject>> GetListAsync(Expression<Func<FileObject, bool>> predicate, Expression<Func<FileObject, object>> orderBy, bool descending = true);


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
}