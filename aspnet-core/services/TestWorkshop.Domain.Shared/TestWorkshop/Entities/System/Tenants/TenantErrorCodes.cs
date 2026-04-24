namespace TestWorkshop;

public static class TenantErrorCodes
{

    public const string Namespace = "Tenant";

    /// <summary>
    /// 已经存在名为 {Name} 的租户!
    /// </summary>
    public const string DuplicateTenantName = Namespace + ":020001";
    /// <summary>
    /// 租户: {Tenant} 不存在!
    /// </summary>
    public const string TenantIdOrNameNotFound = Namespace + ":020002";
    /// <summary>
    /// 无法打开默认连接字符串指向的数据库连接
    /// </summary>
    public const string InvalidDefaultConnectionString = Namespace + ":020101";
    /// <summary>
    /// 默认连接字符串指向的数据库已经存在
    /// </summary>
    public const string DefaultConnectionStringDatabaseExists = Namespace + ":020102";
    /// <summary>
    /// {Name} 的连接字符串无效
    /// </summary>
    public const string InvalidConnectionString = Namespace + ":020103";
    /// <summary>
    /// 不支持 {Name} 类型的数据库连接检查
    /// </summary>
    public const string ConnectionStringProviderNotSupport = Namespace + ":020104";

}
