namespace TestWorkshop;

public class DataCreateOrUpdateDto
{
    [Required]
    [DynamicStringLength(typeof(TestWorkshopConsts), nameof(TestWorkshopConsts.MaxLength64))]
    public string Name { get; set; }

    [Required]
    [DynamicStringLength(typeof(TestWorkshopConsts), nameof(TestWorkshopConsts.MaxLength128))]
    public string DisplayName { get; set; }

    [DynamicStringLength(typeof(TestWorkshopConsts), nameof(TestWorkshopConsts.MaxLength1024))]
    public string Description { get; set; }
}
