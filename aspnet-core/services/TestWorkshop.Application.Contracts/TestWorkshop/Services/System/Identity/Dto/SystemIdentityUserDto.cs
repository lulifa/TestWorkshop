namespace TestWorkshop;

public class SystemIdentityUserDto : IdentityUserDto
{
    public List<string> RoleNames { get; set; } = new();
}
