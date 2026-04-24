namespace TestWorkshop;

public class DataCreateDto : DataCreateOrUpdateDto
{
    [DisplayName("DisplayName:ParentData")]
    public Guid? ParentId { get; set; }
}
