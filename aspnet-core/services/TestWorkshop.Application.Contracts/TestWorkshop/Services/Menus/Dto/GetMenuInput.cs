namespace TestWorkshop;

public class GetMenuInput
{
    [DynamicStringLength(typeof(TestWorkshopConsts), nameof(TestWorkshopConsts.MaxLength64))]
    public string Framework { get; set; }
}
