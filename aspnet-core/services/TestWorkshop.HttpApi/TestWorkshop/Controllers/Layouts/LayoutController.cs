namespace TestWorkshop;

/// <summary>
/// 布局管理
/// </summary>
[Route("api/platform/layouts")]
public class LayoutController : TestWorkshopCrudController<
    ILayoutAppService,
    LayoutDto,
    GetLayoutListInput,
    LayoutCreateDto,
    LayoutUpdateDto>
{
    public LayoutController(ILayoutAppService appService) : base(appService)
    {
    }

    [HttpGet]
    [Route("all")]
    public async virtual Task<ListResultDto<LayoutDto>> GetAllListAsync()
    {
        return await AppService.GetAllListAsync();
    }

}
