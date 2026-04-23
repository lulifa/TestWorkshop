namespace TestWorkshop.AspNetCore.Mvc.Wrapper;

public interface IWrapResultChecker
{
    bool WrapOnAction(ActionDescriptor actionDescriptor);

    bool WrapOnExecution(FilterContext context);

    bool WrapOnException(ExceptionContext context);

    bool WrapOnException(PageHandlerExecutedContext context);
}
