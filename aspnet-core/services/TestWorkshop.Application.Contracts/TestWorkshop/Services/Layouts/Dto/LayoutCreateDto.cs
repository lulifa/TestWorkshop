namespace TestWorkshop;

public class LayoutCreateDto : LayoutCreateOrUpdateDto
{
    public Guid DataId { get; set; }

    [Required]
    [DynamicStringLength(typeof(TestWorkshopConsts), nameof(TestWorkshopConsts.MaxLength64))]
    public string Framework { get; set; }
}
