namespace TestWorkshop;

public class GetDataByNameInput
{
    [Required]
    [DynamicStringLength(typeof(TestWorkshopConsts), nameof(TestWorkshopConsts.MaxLength64))]
    public string Name { get; set; }
}
