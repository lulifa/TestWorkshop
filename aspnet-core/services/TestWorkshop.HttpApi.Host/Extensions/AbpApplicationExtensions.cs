using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Localization;
using System.Collections.Generic;

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
