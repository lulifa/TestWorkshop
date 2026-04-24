namespace TestWorkshop;

public class OrganizationUnitGetChildrenDto : IEntityDto<Guid>
{
    [Required]
    public Guid Id { get; set; }
    public bool Recursive { get; set; }
}
