namespace TestWorkshop;

public class GetLayoutListInput : PagedAndSortedResultRequestDto, IEnablePaging
{
    public string Filter { get; set; }

    [DynamicStringLength(typeof(TestWorkshopConsts), nameof(TestWorkshopConsts.MaxLength64))]
    public string Framework { get; set; }

    public bool IsPaged { get; set; } = true;
}
