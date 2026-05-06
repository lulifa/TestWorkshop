using TestWorkshop.TimeScale;

namespace TestWorkshop;

public class TestWorkshopApplicationAutoMapperProfile : Profile
{
    public TestWorkshopApplicationAutoMapperProfile()
    {
        CreateMap<TelemetryTask, TelemetryTaskDto>();
        CreateMap<TelemetryTaskCreateDto, TelemetryTask>();


        CreateMap<DataItem, DataItemDto>();
        CreateMap<Data, DataDto>();
        CreateMap<Menu, MenuDto>()
            .ForMember(dto => dto.Meta, map => map.MapFrom(src => src.ExtraProperties))
            .ForMember(dto => dto.Startup, map => map.Ignore());
        CreateMap<Layout, LayoutDto>()
            .ForMember(dto => dto.Meta, map => map.MapFrom(src => src.ExtraProperties));
        CreateMap<UserFavoriteMenu, UserFavoriteMenuDto>();


        // abp拓展的字段或者额外属性都会存储在ExtraProperties属性中，需要手动映射
        CreateMap<Tenant, TenantDto>().MapExtraProperties();

        CreateMap<TenantConnectionString, TenantConnectionStringDto>();

        CreateMap<OrganizationUnit, OrganizationUnitDto>().MapExtraProperties();
    }
}
