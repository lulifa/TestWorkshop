using Microsoft.AspNetCore.Cors.Infrastructure;
using TestWorkshop.Wrapper;

namespace Microsoft.AspNetCore.Cors;

public static class AbpCorsPolicyBuilderExtensions
{
    public static CorsPolicyBuilder WithAbpWrapExposedHeaders(this CorsPolicyBuilder corsPolicyBuilder)
    {
        return corsPolicyBuilder
            .WithExposedHeaders(
                AbpHttpWrapConsts.AbpWrapResult,
                AbpHttpWrapConsts.AbpDontWrapResult);
    }
}
