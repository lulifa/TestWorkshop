namespace TestWorkshop;

public interface IVben5NavigationManager
{
    Task<IReadOnlyCollection<ApplicationMenu>> GetAll();
}
