namespace TestWorkshop;

public class MenuGetByUserInput
{
    [Required]
    public Guid UserId { get; set; }

    [DynamicStringLength(typeof(TestWorkshopConsts), nameof(TestWorkshopConsts.MaxLength64))]
    public string Framework { get; set; }
}
