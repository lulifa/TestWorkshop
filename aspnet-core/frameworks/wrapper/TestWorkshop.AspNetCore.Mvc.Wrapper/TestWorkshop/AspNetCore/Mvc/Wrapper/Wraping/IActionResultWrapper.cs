namespace TestWorkshop.AspNetCore.Mvc.Wrapper;

public interface IActionResultWrapper
{
    void Wrap(FilterContext context);
}
