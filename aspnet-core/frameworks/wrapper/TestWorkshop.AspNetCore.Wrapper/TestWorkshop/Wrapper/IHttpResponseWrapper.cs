namespace TestWorkshop.AspNetCore.Wrapper;

public interface IHttpResponseWrapper
{
    void Wrap(HttpResponseWrapperContext context);
}
