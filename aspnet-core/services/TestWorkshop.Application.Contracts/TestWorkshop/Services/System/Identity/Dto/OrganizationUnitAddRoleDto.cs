namespace TestWorkshop;
public class OrganizationUnitAddRoleDto
{
    [Required]
    public List<Guid> RoleIds { get; set; }
}
