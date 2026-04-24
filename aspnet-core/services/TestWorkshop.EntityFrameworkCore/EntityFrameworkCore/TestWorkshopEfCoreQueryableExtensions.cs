namespace TestWorkshop.EntityFrameworkCore;

public static class TestWorkshopEfCoreQueryableExtensions
{

    public static IQueryable<Layout> IncludeDetails(this IQueryable<Layout> queryable, bool include = true)
    {
        if (!include)
        {
            return queryable;
        }

        return queryable;
    }

    public static IQueryable<Menu> IncludeDetails(this IQueryable<Menu> queryable, bool include = true)
    {
        if (!include)
        {
            return queryable;
        }

        return queryable;
    }

    public static IQueryable<Data> IncludeDetails(this IQueryable<Data> queryable, bool include = true)
    {
        if (!include)
        {
            return queryable;
        }

        return queryable
            .AsSplitQuery()
            .Include(x => x.Items);
    }
}
