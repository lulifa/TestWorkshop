namespace TestWorkshop;

public interface ILayoutAppService :
    ICrudAppService<
        LayoutDto,
        Guid,
        GetLayoutListInput,
        LayoutCreateDto,
        LayoutUpdateDto>
{
    Task<ListResultDto<LayoutDto>> GetAllListAsync();
}
