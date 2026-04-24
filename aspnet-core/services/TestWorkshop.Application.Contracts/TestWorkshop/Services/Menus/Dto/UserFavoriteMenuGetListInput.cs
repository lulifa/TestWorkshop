namespace TestWorkshop;
public class UserFavoriteMenuGetListInput
{
    [DynamicStringLength(typeof(TestWorkshopConsts), nameof(TestWorkshopConsts.MaxLength64))]
    public string Framework { get; set; }
}
