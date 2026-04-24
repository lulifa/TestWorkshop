namespace TestWorkshop;

public class OrganizationUnitAddUserDto
{
    [Required]
    public List<Guid> UserIds { get; set; }
}
