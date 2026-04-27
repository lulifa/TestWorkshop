using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.Generic;
using System.Linq;
using Volo.Abp.AspNetCore.MultiTenancy;
using Volo.Abp.MultiTenancy;

namespace TestWorkshop;

public class AbpOperationFilter : IOperationFilter
{
    private readonly AbpMultiTenancyOptions _multiTenancyOptions;
    private readonly AbpAspNetCoreMultiTenancyOptions _aspNetCoreMultiTenancyOptions;
    public AbpOperationFilter(IOptions<AbpMultiTenancyOptions> multiTenancyOptions, IOptions<AbpAspNetCoreMultiTenancyOptions> aspNetCoreMultiTenancyOptions)
    {
        _multiTenancyOptions = multiTenancyOptions.Value;
        _aspNetCoreMultiTenancyOptions = aspNetCoreMultiTenancyOptions.Value;
    }

    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        operation.Parameters = operation.Parameters ?? new List<OpenApiParameter>();

        if (_multiTenancyOptions.IsEnabled)
        {
            if (operation.Parameters.All(p => p.Name != _aspNetCoreMultiTenancyOptions.TenantKey))
            {
                operation.Parameters.Add(new OpenApiParameter
                {
                    Name = _aspNetCoreMultiTenancyOptions.TenantKey,
                    In = ParameterLocation.Header,
                    Description = "Tenant Id in http header",
                    Required = false
                });
            }
        }

        if (operation.Parameters.All(p => p.Name != "Accept-Language"))
        {
            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "Accept-Language",
                In = ParameterLocation.Header,
                Required = false,
                Description = "Language setting: zh-Hans or en. Default is zh-Hans.",
                Schema = new OpenApiSchema
                {
                    Type = "string",
                    Default = new OpenApiString("zh-Hans")
                }
            });
        }
    }

}
