using TestWorkshop.TimeScale;
using Volo.Abp.AuditLogging;

namespace TestWorkshop;

public class TestWorkshopApplicationAutoMapperProfile : Profile
{
    public TestWorkshopApplicationAutoMapperProfile()
    {
        CreateMap<WorkshopDevice, WorkshopDeviceDto>();

        CreateMap<WorkshopTelemetryTask, WorkshopTelemetryTaskDto>();
        CreateMap<Notification, NotificationOutput>();
        CreateMap<NotificationSubscription, NotificationSubscriptionOutput>();

        CreateMap<DataItem, DataItemDto>();
        CreateMap<Data, DataDto>();
        CreateMap<Menu, MenuDto>()
            .ForMember(dto => dto.Meta, map => map.MapFrom(src => src.ExtraProperties))
            .ForMember(dto => dto.Startup, map => map.Ignore());
        CreateMap<Layout, LayoutDto>()
            .ForMember(dto => dto.Meta, map => map.MapFrom(src => src.ExtraProperties));
        CreateMap<UserFavoriteMenu, UserFavoriteMenuDto>();
        CreateMap<FileObject, FileObjectDto>();


        // abp拓展的字段或者额外属性都会存储在ExtraProperties属性中，需要手动映射
        CreateMap<Tenant, TenantDto>().MapExtraProperties();

        CreateMap<TenantConnectionString, TenantConnectionStringDto>();

        CreateMap<OrganizationUnit, OrganizationUnitDto>().MapExtraProperties();

        CreateMap<AuditLog, AuditLogOutput>()
            .ForMember(dest => dest.ExecutionTime,
                opt => opt.MapFrom(s => s.ExecutionTime.ToString("O")));
        CreateMap<AuditLogAction, AuditLogActionOutput>()
            .ForMember(dest => dest.ExecutionTime,
                opt => opt.MapFrom(s => s.ExecutionTime.ToString("O")));
        CreateMap<EntityChange, EntityChangeOutput>()
            .ForMember(dest => dest.ChangeTypeDescription,
                opt => opt.MapFrom(s => s.ChangeType))
            .ForMember(dest => dest.ChangeTime,
                opt => opt.MapFrom(s => s.ChangeTime.ToString("O")));
        CreateMap<EntityPropertyChange, EntityPropertyChangeOutput>();

        CreateMap<IdentitySecurityLog, IdentitySecurityLogOutput>();
    }
}
