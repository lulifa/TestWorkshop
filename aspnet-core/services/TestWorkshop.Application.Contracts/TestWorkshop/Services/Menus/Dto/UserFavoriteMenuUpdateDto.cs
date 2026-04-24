namespace TestWorkshop;

public class UserFavoriteMenuUpdateDto : UserFavoriteMenuCreateOrUpdateDto, IHasConcurrencyStamp
{

    public string ConcurrencyStamp { get; set; }
}
