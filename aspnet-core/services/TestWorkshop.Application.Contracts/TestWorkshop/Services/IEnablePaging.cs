namespace TestWorkshop;

public interface IEnablePaging
{
    /// <summary>
    /// 是否启用分页；true：分页查询，false：查询全部数据
    /// </summary>
    bool IsPaged { get; set; }
}
