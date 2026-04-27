namespace TestWorkshop;

/// <summary>
/// 字典管理
/// </summary>
[Route("api/platform/datas")]
public class DataController : TestWorkshopCrudController<
    IDataAppService,
    DataDto,
    GetDataListInput,
    DataCreateDto,
    DataUpdateDto>
{
    public DataController(IDataAppService appService) : base(appService)
    {
    }


    [HttpGet]
    [Route("by-name/{name}")]
    public async virtual Task<DataDto> GetAsync(string name)
    {
        return await AppService.GetAsync(name);
    }

    [HttpGet]
    [Route("all")]
    public async virtual Task<ListResultDto<DataDto>> GetAllAsync()
    {
        return await AppService.GetAllAsync();
    }

    [HttpPut]
    [Route("{id}/move")]
    public async virtual Task<DataDto> MoveAsync(Guid id, DataMoveDto input)
    {
        return await AppService.MoveAsync(id, input);
    }

    [HttpPost]
    [Route("{id}/items")]
    public async virtual Task CreateItemAsync(Guid id, DataItemCreateDto input)
    {
        await AppService.CreateItemAsync(id, input);
    }

    [HttpPut]
    [Route("{id}/items/{name}")]
    public async virtual Task UpdateItemAsync(Guid id, string name, DataItemUpdateDto input)
    {
        await AppService.UpdateItemAsync(id, name, input);
    }

    [HttpDelete]
    [Route("{id}/items/{name}")]
    public async virtual Task DeleteItemAsync(Guid id, string name)
    {
        await AppService.DeleteItemAsync(id, name);
    }

}
