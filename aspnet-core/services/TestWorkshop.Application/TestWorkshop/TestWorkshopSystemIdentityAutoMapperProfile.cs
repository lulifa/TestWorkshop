using AutoMapper;

namespace TestWorkshop;

public class TestWorkshopSystemIdentityAutoMapperProfile : Profile
{
    public TestWorkshopSystemIdentityAutoMapperProfile()
    {
        CreateMap<IdentityUserDto, SystemIdentityUserDto>();
    }
}
