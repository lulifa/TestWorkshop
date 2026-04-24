namespace TestWorkshop;

public class MenuGetListInput : PagedAndSortedResultRequestDto
{
    [DynamicStringLength(typeof(TestWorkshopConsts), nameof(TestWorkshopConsts.MaxLength64))]
    public string Framework { get; set; }

    public string Filter { get; set; }

    public Guid? ParentId { get; set; }

    public Guid? LayoutId { get; set; }
}
