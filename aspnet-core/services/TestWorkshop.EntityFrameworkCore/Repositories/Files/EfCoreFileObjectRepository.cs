namespace TestWorkshop.FileManagement;

/// <summary>
/// 文件索引仓储实现 - EF Core
/// </summary>
public class EfCoreFileObjectRepository :
    EfCoreRepository<TestWorkshopDbContext, FileObject, Guid>,
    IFileObjectRepository
{
    public EfCoreFileObjectRepository(
        IDbContextProvider<TestWorkshopDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }


    /// <summary>
    /// 按条件查询文件列表
    /// </summary>
    public async Task<List<FileObject>> GetListAsync(Expression<Func<FileObject, bool>> predicate)
    {
        var dbSet = await GetDbSetAsync();
        var query = dbSet.Where(predicate);

        return await query.OrderByDescending(f => f.CreationTime).ToListAsync();
    }

    /// <summary>
    /// 按条件查询文件列表（自定义排序）
    /// </summary>
    public async Task<List<FileObject>> GetListAsync(Expression<Func<FileObject, bool>> predicate, Expression<Func<FileObject, object>> orderBy, bool descending = true)
    {
        var dbSet = await GetDbSetAsync();
        var query = dbSet.Where(predicate);

        query = descending ? query.OrderByDescending(orderBy) : query.OrderBy(orderBy);

        return await query.ToListAsync();
    }

    /// <summary>
    /// 统一分页查询（多条件过滤）
    /// </summary>
    public async Task<PagedResultDto<FileObject>> GetListAsync(
        string keyword = null,
        string ownerType = null,
        string ownerId = null,
        DateTime? startTime = null,
        DateTime? endTime = null,
        int skipCount = 0,
        int maxResultCount = 10)
    {
        var dbSet = await GetDbSetAsync();
        var query = dbSet.AsQueryable();

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            query = query.Where(f => f.FileName.Contains(keyword) || f.BlobPath.Contains(keyword));
        }

        if (!string.IsNullOrWhiteSpace(ownerType))
        {
            query = query.Where(f => f.OwnerType == ownerType);
        }

        if (!string.IsNullOrWhiteSpace(ownerId))
        {
            query = query.Where(f => f.OwnerId == ownerId);
        }

        // 时间范围过滤
        if (startTime.HasValue)
        {
            query = query.Where(f => f.CreationTime >= startTime.Value);
        }
        if (endTime.HasValue)
        {
            query = query.Where(f => f.CreationTime <= endTime.Value);
        }

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(f => f.CreationTime)
            .Skip(skipCount)
            .Take(maxResultCount)
            .ToListAsync();

        return new PagedResultDto<FileObject>(totalCount, items);
    }

}