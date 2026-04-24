namespace TestWorkshop;

public abstract class UserFavoriteMenuCreateOrUpdateDto
{
    [Required]
    public Guid MenuId { get; set; }

    [DynamicStringLength(typeof(TestWorkshopConsts), nameof(TestWorkshopConsts.MaxLength64))]
    public string Color { get; set; }

    [DynamicStringLength(typeof(TestWorkshopConsts), nameof(TestWorkshopConsts.MaxLength128))]
    public string AliasName { get; set; }

    [DynamicStringLength(typeof(TestWorkshopConsts), nameof(TestWorkshopConsts.MaxLength512))]
    public string Icon { get; set; }
}
