namespace Microsoft.AspNetCore.Cors;

public static class AbpCorsPolicyBuilderExtensions
{
    public static CorsPolicyBuilder WithAbpProWrapExposedHeaders(this CorsPolicyBuilder corsPolicyBuilder)
    {
        return corsPolicyBuilder
            .WithExposedHeaders(
                AbpHttpWrapConsts.AbpWrapResult,
                AbpHttpWrapConsts.AbpDontWrapResult);
    }
}
