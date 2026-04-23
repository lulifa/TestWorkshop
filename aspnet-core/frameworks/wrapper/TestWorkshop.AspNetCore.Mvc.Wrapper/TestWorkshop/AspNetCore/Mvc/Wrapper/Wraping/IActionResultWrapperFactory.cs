namespace TestWorkshop.AspNetCore.Mvc.Wrapper;

public interface IActionResultWrapperFactory : ITransientDependency
{
    IActionResultWrapper CreateFor(FilterContext context);
}
