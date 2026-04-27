namespace TestWorkshop;

/// <summary>
/// 所有控制器的基类（只做本地化资源设置）
/// </summary>
public abstract class TestWorkshopController : AbpControllerBase
{
    protected TestWorkshopController()
    {
        LocalizationResource = typeof(TestWorkshopResource);
    }
}

/// <summary>
/// 轻量级 CRUD 基类（只封装标准的 5 个方法）
/// </summary>
/// <typeparam name="TAppService">AppService 接口类型</typeparam>
/// <typeparam name="TEntityDto">实体 Dto 类型</typeparam>
/// <typeparam name="TKey">主键类型</typeparam>
/// <typeparam name="TGetListInput">列表查询输入类型</typeparam>
/// <typeparam name="TCreateInput">创建输入类型</typeparam>
/// <typeparam name="TUpdateInput">更新输入类型</typeparam>
public abstract class TestWorkshopCrudController<TAppService, TEntityDto, TKey, TGetListInput, TCreateInput, TUpdateInput>
    : TestWorkshopController
    where TAppService : ICrudAppService<TEntityDto, TKey, TGetListInput, TCreateInput, TUpdateInput>
{
    protected readonly TAppService AppService;

    protected TestWorkshopCrudController(TAppService appService)
    {
        AppService = appService;
    }

    [HttpPost]
    public virtual Task<TEntityDto> CreateAsync(TCreateInput input)
        => AppService.CreateAsync(input);

    [HttpDelete("{id}")]
    public virtual Task DeleteAsync(TKey id)
        => AppService.DeleteAsync(id);

    [HttpGet("{id}")]
    public virtual Task<TEntityDto> GetAsync(TKey id)
        => AppService.GetAsync(id);

    [HttpPut("{id}")]
    public virtual Task<TEntityDto> UpdateAsync(TKey id, TUpdateInput input)
        => AppService.UpdateAsync(id, input);

    [HttpGet]
    public virtual Task<PagedResultDto<TEntityDto>> GetListAsync(TGetListInput input)
        => AppService.GetListAsync(input);
}

/// <summary>
/// Guid 主键的便利版本（减少一个泛型参数）
/// </summary>
public abstract class TestWorkshopCrudController<TAppService, TEntityDto, TGetListInput, TCreateInput, TUpdateInput>
    : TestWorkshopCrudController<TAppService, TEntityDto, Guid, TGetListInput, TCreateInput, TUpdateInput>
    where TAppService : ICrudAppService<TEntityDto, Guid, TGetListInput, TCreateInput, TUpdateInput>
{
    protected TestWorkshopCrudController(TAppService appService) : base(appService)
    {
    }
}