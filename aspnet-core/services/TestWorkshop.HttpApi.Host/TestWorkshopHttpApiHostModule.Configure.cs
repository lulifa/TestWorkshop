using Microsoft.Extensions.Configuration;
using Volo.Abp.Localization;

namespace TestWorkshop
{
    public partial class TestWorkshopHttpApiHostModule
    {


        private void ConfigureLocalization(IConfiguration configuration)
        {
            Configure<AbpLocalizationOptions>(options =>
            {
                options.Languages.Add(new LanguageInfo("en", "en", "English"));
                options.Languages.Add(new LanguageInfo("zh-Hans", "zh-Hans", "简体中文"));

            });

        }

    }
}
