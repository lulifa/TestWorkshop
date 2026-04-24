namespace TestWorkshop;

public class TestWorkshopDomainMappingProfile : Profile
{
    public TestWorkshopDomainMappingProfile()
    {
        CreateMap<Layout, LayoutEto>();
        CreateMap<Menu, MenuEto>();
        CreateMap<UserMenu, UserMenuEto>();
        CreateMap<RoleMenu, RoleMenuEto>();
    }
}
