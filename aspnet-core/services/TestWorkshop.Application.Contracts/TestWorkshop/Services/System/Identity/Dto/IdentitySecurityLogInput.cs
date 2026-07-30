namespace TestWorkshop;

public class IdentitySecurityLogInput : PagedAndSortedResultRequestDto, IEnablePaging
{

    /// <summary>
    /// 开始时间
    /// </summary>
    public DateTime? StartTime { get; set; }

    /// <summary>
    /// 结束时间
    /// </summary>
    public DateTime? EndTime { get; set; }

    public string Identity { get; set; }

    public string ActionName { get; set; }

    /// <summary>
    /// 用户Id
    /// </summary>
    public Guid? UserId { get; set; }

    /// <summary>
    /// 用户名
    /// </summary>
    public string UserName { get; set; }

    /// <summary>
    /// 应用程序名称
    /// </summary>
    public string ApplicationName { get; set; }

    /// <summary>
    /// RequestId
    /// </summary>
    public string CorrelationId { get; set; }

    /// <summary>
    /// ClientId
    /// </summary>
    public string ClientId { get; set; }

    /// <summary>
    /// 客户端IP
    /// </summary>
    public string ClientIpAddress { get; set; }

    public bool IsPaged { get; set; } = true;

}
