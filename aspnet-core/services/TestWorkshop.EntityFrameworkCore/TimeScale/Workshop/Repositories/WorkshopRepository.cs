namespace TestWorkshop.EntityFrameworkCore;

public class WorkshopRepository : EfCoreRepository<TestWorkshopDbContext, Workshop, Guid>, IWorkshopRepository
{
    public WorkshopRepository(IDbContextProvider<TestWorkshopDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }
}
