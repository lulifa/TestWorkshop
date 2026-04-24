namespace TestWorkshop;

public class DataItemCreateDto : DataItemCreateOrUpdateDto
{
    [Required]
    [DynamicStringLength(typeof(TestWorkshopConsts), nameof(TestWorkshopConsts.MaxLength64))]
    public string Name { get; set; }
}
