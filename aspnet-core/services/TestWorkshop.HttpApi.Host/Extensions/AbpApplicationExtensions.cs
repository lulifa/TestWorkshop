namespace TestWorkshop;

public static class AbpApplicationExtensions
{

    public static IApplicationBuilder UseTestWorkshopRequestLocalization(this IApplicationBuilder app)
    {
        return app.UseAbpRequestLocalization(options =>
        {
            options.RequestCultureProviders.RemoveAll(provider => provider is AcceptLanguageHeaderRequestCultureProvider);

            options.RequestCultureProviders.Add(new AbpCultureProvider());

        });
    }

}
