namespace TestWorkshop;

public class GetLayoutListInput : PagedAndSortedResultRequestDto
{
    public string Filter { get; set; }

    [DynamicStringLength(typeof(TestWorkshopConsts), nameof(TestWorkshopConsts.MaxLength64))]
    public string Framework { get; set; }
}
