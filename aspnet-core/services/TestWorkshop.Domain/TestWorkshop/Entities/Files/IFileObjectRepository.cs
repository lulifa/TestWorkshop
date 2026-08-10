using System.Linq.Expressions;

namespace TestWorkshop;

/// <summary>
/// 文件索引仓储接口
/// </summary>
public interface IFileObjectRepository : IBasicRepository<FileObject, Guid>
{
    /// <summary>
    /// 按条件查询文件列表（默认按创建时间降序）—— 这是你缺的方法！
    /// </summary>
    Task<List<FileObject>> GetListAsync(
        Expression<Func<FileObject, bool>> predicate,
        bool includeDeleted = false);

    /// <summary>
    /// 按条件查询文件列表（自定义排序）
    /// </summary>
    Task<List<FileObject>> GetListAsync(
        Expression<Func<FileObject, bool>> predicate,
        Expression<Func<FileObject, object>> orderBy,
        bool descending = true,
        bool includeDeleted = false);

    /// <summary>
    /// 按业务归属查询文件列表
    /// </summary>
    Task<List<FileObject>> GetListByOwnerAsync(
        string ownerType,
        string ownerId,
        bool includeDeleted = false);

    /// <summary>
    /// 按业务归属查询文件列表（带排序）
    /// </summary>
    Task<List<FileObject>> GetListByOwnerAsync(
        string ownerType,
        string ownerId,
        Expression<Func<FileObject, object>> orderBy,
        bool descending = true,
        bool includeDeleted = false);

    /// <summary>
    /// 按业务类型查询所有文件（支持分页）
    /// </summary>
    Task<PagedResultDto<FileObject>> GetPagedListByTypeAsync(
        string ownerType,
        string keyword = null,
        int skipCount = 0,
        int maxResultCount = 10,
        bool includeDeleted = false);

    /// <summary>
    /// 按业务归属统计文件数量
    /// </summary>
    Task<int> CountByOwnerAsync(string ownerType, string ownerId);

    /// <summary>
    /// 按业务类型统计文件数量和总大小
    /// </summary>
    Task<(int Count, long TotalSize)> GetStatisticsByTypeAsync(string ownerType);

    /// <summary>
    /// 删除指定业务归属的所有文件（软删除或物理删除）
    /// </summary>
    Task DeleteByOwnerAsync(string ownerType, string ownerId, bool permanent = false);

    /// <summary>
    /// 获取指定路径的文件记录
    /// </summary>
    Task<FileObject?> FindByBlobPathAsync(string blobPath);

    /// <summary>
    /// 批量插入
    /// </summary>
    Task InsertManyAsync(IEnumerable<FileObject> fileObjects);

    /// <summary>
    /// 批量删除（物理删除，传入 ID 集合）
    /// </summary>
    Task DeleteManyAsync(IEnumerable<Guid> ids);

    /// <summary>
    /// 批量删除（物理删除，传入实体集合）—— 这是你缺的方法！
    /// </summary>
    Task DeleteManyAsync(IEnumerable<FileObject> entities);
}