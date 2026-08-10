using System.Linq.Expressions;
using TestWorkshop.EntityFrameworkCore;

namespace TestWorkshop.FileManagement
{
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
        public async Task<List<FileObject>> GetListAsync(
            Expression<Func<FileObject, bool>> predicate,
            bool includeDeleted = false)
        {
            var dbSet = await GetDbSetAsync();
            var query = dbSet.Where(predicate);

            if (!includeDeleted && IsSoftDeleteEnabled())
            {
                query = query.Where(f => !f.IsDeleted);
            }

            return await query
                .OrderByDescending(f => f.CreationTime)
                .ToListAsync();
        }

        /// <summary>
        /// 按条件查询文件列表（自定义排序）
        /// </summary>
        public async Task<List<FileObject>> GetListAsync(
            Expression<Func<FileObject, bool>> predicate,
            Expression<Func<FileObject, object>> orderBy,
            bool descending = true,
            bool includeDeleted = false)
        {
            var dbSet = await GetDbSetAsync();
            var query = dbSet.Where(predicate);

            if (!includeDeleted && IsSoftDeleteEnabled())
            {
                query = query.Where(f => !f.IsDeleted);
            }

            query = descending
                ? query.OrderByDescending(orderBy)
                : query.OrderBy(orderBy);

            return await query.ToListAsync();
        }

        /// <summary>
        /// 按业务归属查询文件列表
        /// </summary>
        public async Task<List<FileObject>> GetListByOwnerAsync(
            string ownerType,
            string ownerId,
            bool includeDeleted = false)
        {
            var dbSet = await GetDbSetAsync();
            var query = dbSet
                .Where(f => f.OwnerType == ownerType && f.OwnerId == ownerId);

            if (!includeDeleted && IsSoftDeleteEnabled())
            {
                query = query.Where(f => !f.IsDeleted);
            }

            return await query
                .OrderByDescending(f => f.CreationTime)
                .ToListAsync();
        }

        /// <summary>
        /// 按业务归属查询文件列表（带自定义排序）
        /// </summary>
        public async Task<List<FileObject>> GetListByOwnerAsync(
            string ownerType,
            string ownerId,
            Expression<Func<FileObject, object>> orderBy,
            bool descending = true,
            bool includeDeleted = false)
        {
            var dbSet = await GetDbSetAsync();
            var query = dbSet
                .Where(f => f.OwnerType == ownerType && f.OwnerId == ownerId);

            if (!includeDeleted && IsSoftDeleteEnabled())
            {
                query = query.Where(f => !f.IsDeleted);
            }

            query = descending
                ? query.OrderByDescending(orderBy)
                : query.OrderBy(orderBy);

            return await query.ToListAsync();
        }

        /// <summary>
        /// 按业务类型查询所有文件（支持分页和关键词搜索）
        /// </summary>
        public async Task<PagedResultDto<FileObject>> GetPagedListByTypeAsync(
            string ownerType,
            string? keyword = null,
            int skipCount = 0,
            int maxResultCount = 10,
            bool includeDeleted = false)
        {
            var dbSet = await GetDbSetAsync();
            var query = dbSet.Where(f => f.OwnerType == ownerType);

            if (!includeDeleted && IsSoftDeleteEnabled())
            {
                query = query.Where(f => !f.IsDeleted);
            }

            // 关键词搜索（文件名或路径）
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(f =>
                    f.FileName.Contains(keyword) ||
                    f.BlobPath.Contains(keyword));
            }

            var totalCount = await query.CountAsync();
            var items = await query
                .OrderByDescending(f => f.CreationTime)
                .Skip(skipCount)
                .Take(maxResultCount)
                .ToListAsync();

            return new PagedResultDto<FileObject>(totalCount, items);
        }

        /// <summary>
        /// 按业务归属统计文件数量
        /// </summary>
        public async Task<int> CountByOwnerAsync(string ownerType, string ownerId)
        {
            var dbSet = await GetDbSetAsync();
            var query = dbSet
                .Where(f => f.OwnerType == ownerType && f.OwnerId == ownerId);

            if (IsSoftDeleteEnabled())
            {
                query = query.Where(f => !f.IsDeleted);
            }

            return await query.CountAsync();
        }

        /// <summary>
        /// 按业务类型统计文件数量和总大小
        /// </summary>
        public async Task<(int Count, long TotalSize)> GetStatisticsByTypeAsync(string ownerType)
        {
            var dbSet = await GetDbSetAsync();
            var query = dbSet.Where(f => f.OwnerType == ownerType);

            if (IsSoftDeleteEnabled())
            {
                query = query.Where(f => !f.IsDeleted);
            }

            var count = await query.CountAsync();
            var totalSize = await query.SumAsync(f => f.FileSize);

            return (count, totalSize);
        }

        /// <summary>
        /// 删除指定业务归属的所有文件
        /// </summary>
        public async Task DeleteByOwnerAsync(string ownerType, string ownerId, bool permanent = false)
        {
            var dbSet = await GetDbSetAsync();
            var query = dbSet
                .Where(f => f.OwnerType == ownerType && f.OwnerId == ownerId)
                .IgnoreQueryFilters(); // 忽略全局过滤，确保找到所有记录（包括已软删除的）

            if (permanent)
            {
                // 物理删除：直接删除记录
                var files = await query.ToListAsync();
                if (files.Any())
                {
                    dbSet.RemoveRange(files);
                    var dbContext = await GetDbContextAsync();
                    await dbContext.SaveChangesAsync();
                }
            }
            else
            {
                // 软删除：只更新未删除的记录
                var files = await query
                    .Where(f => !f.IsDeleted)
                    .ToListAsync();

                if (files.Any())
                {
                    foreach (var file in files)
                    {
                        // 设置软删除标记（如果实体继承 ISoftDelete）
                        file.IsDeleted = true;
                        file.DeletionTime = DateTime.UtcNow;
                        // 注意：如果 FileObject 继承 FullAuditedAggregateRoot，
                        // DeleterId 会自动被 ABP 审计拦截器填充
                    }

                    // EF Core 的 UpdateRange 是同步方法，后面调用 SaveChangesAsync 异步保存
                    dbSet.UpdateRange(files);
                    var dbContext = await GetDbContextAsync();
                    await dbContext.SaveChangesAsync();
                }
            }
        }

        /// <summary>
        /// 获取指定路径的文件记录
        /// </summary>
        public async Task<FileObject> FindByBlobPathAsync(string blobPath)
        {
            var dbSet = await GetDbSetAsync();
            return await dbSet
                .FirstOrDefaultAsync(f => f.BlobPath == blobPath);
        }

        /// <summary>
        /// 批量插入
        /// </summary>
        public async Task InsertManyAsync(IEnumerable<FileObject> fileObjects)
        {
            var dbSet = await GetDbSetAsync();
            await dbSet.AddRangeAsync(fileObjects);
            var dbContext = await GetDbContextAsync();
            await dbContext.SaveChangesAsync();
        }

        /// <summary>
        /// 批量删除（物理删除）
        /// </summary>
        public async Task DeleteManyAsync(IEnumerable<Guid> ids)
        {
            var dbSet = await GetDbSetAsync();
            var files = await dbSet
                .Where(f => ids.Contains(f.Id))
                .ToListAsync();

            if (files.Any())
            {
                dbSet.RemoveRange(files);
                var dbContext = await GetDbContextAsync();
                await dbContext.SaveChangesAsync();
            }
        }

        /// <summary>
        /// ✅ 批量删除（物理删除，传入实体集合）—— 你缺少的这个！
        /// </summary>
        public async Task DeleteManyAsync(IEnumerable<FileObject> entities)
        {
            var files = entities.ToList();
            if (!files.Any()) return;

            var dbSet = await GetDbSetAsync();
            dbSet.RemoveRange(files);
            var dbContext = await GetDbContextAsync();
            await dbContext.SaveChangesAsync();
        }

        /// <summary>
        /// 判断是否启用软删除
        /// </summary>
        private bool IsSoftDeleteEnabled()
        {
            // 如果 FileObject 继承 ISoftDelete 接口，返回 true
            return typeof(ISoftDelete).IsAssignableFrom(typeof(FileObject));
        }
    }
}