namespace TestWorkshop;

public class MenuGetAllInput : ISortedResultRequest
{
    [DynamicStringLength(typeof(TestWorkshopConsts), nameof(TestWorkshopConsts.MaxLength64))]
    public string Framework { get; set; }

    public string Filter { get; set; }

    public bool Reverse { get; set; }

    public Guid? ParentId { get; set; }

    public string Sorting { get; set; }

    public Guid? LayoutId { get; set; }
}
