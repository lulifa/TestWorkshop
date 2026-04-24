namespace TestWorkshop.EntityFrameworkCore;

public class EfCoreUserFavoriteMenuRepository :
    EfCoreRepository<TestWorkshopDbContext, UserFavoriteMenu, Guid>,
    IUserFavoriteMenuRepository
{
    public EfCoreUserFavoriteMenuRepository(
        IDbContextProvider<TestWorkshopDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }

    public async virtual Task<List<UserFavoriteMenu>> GetListByMenuIdAsync(
        Guid menuId,
        CancellationToken cancellationToken = default)
    {
        return await (await GetDbSetAsync())
            .Where(x => x.MenuId == menuId)
            .ToListAsync(GetCancellationToken(cancellationToken));
    }

    public async virtual Task<List<UserFavoriteMenu>> GetFavoriteMenusAsync(
        Guid userId,
        string framework = null,
        Guid? menuId = null,
        CancellationToken cancellationToken = default)
    {
        return await (await GetDbSetAsync())
            .Where(x => x.UserId == userId)
            .WhereIf(menuId.HasValue, x => x.MenuId == menuId)
            .WhereIf(!framework.IsNullOrWhiteSpace(), x => x.Framework == framework)
            .OrderBy(x => x.CreationTime)
            .ToListAsync(GetCancellationToken(cancellationToken));
    }

    public async virtual Task<bool> CheckExistsAsync(
        string framework,
        Guid userId,
        Guid menuId,
        CancellationToken cancellationToken = default)
    {
        return await (await GetDbSetAsync())
            .AnyAsync(x =>
                x.Framework == framework && x.UserId == userId && x.MenuId == menuId,
                GetCancellationToken(cancellationToken));
    }

    public async virtual Task<UserFavoriteMenu> FindByUserMenuAsync(
        Guid userId,
        Guid menuId,
        CancellationToken cancellationToken = default)
    {
        return await (await GetDbSetAsync())
            .Where(x => x.UserId == userId && x.MenuId == menuId)
            .FirstOrDefaultAsync(GetCancellationToken(cancellationToken));
    }
}
