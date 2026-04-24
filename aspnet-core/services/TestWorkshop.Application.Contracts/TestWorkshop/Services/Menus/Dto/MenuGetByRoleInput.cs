namespace TestWorkshop;

public class MenuGetByRoleInput
{
    [Required]
    [StringLength(80)]
    public string Role { get; set; }

    [DynamicStringLength(typeof(TestWorkshopConsts), nameof(TestWorkshopConsts.MaxLength64))]
    public string Framework { get; set; }
}
