namespace TestWorkshop;

public class UserMenuStartupInput
{
    public Guid UserId { get; set; }


    [DynamicStringLength(typeof(TestWorkshopConsts), nameof(TestWorkshopConsts.MaxLength64))]
    public string Framework { get; set; }
}
