namespace TestWorkshop;

public class RoleMenuInput
{
    [Required]
    [StringLength(80)]
    public string RoleName { get; set; }

    [DynamicStringLength(typeof(TestWorkshopConsts), nameof(TestWorkshopConsts.MaxLength64))]
    public string Framework { get; set; }

    public Guid? StartupMenuId { get; set; }

    [Required]
    public List<Guid> MenuIds { get; set; } = new List<Guid>();
}
