namespace TestWorkshop;

public static class RoleConstants
{
    /// <summary>
    /// 管理员
    /// </summary>
    public const string admin = nameof(admin);

    /// <summary>
    /// 车间主管
    /// </summary>
    public const string supervisor = nameof(supervisor);

    /// <summary>
    /// 测试工程师
    /// </summary>
    public const string tester = nameof(tester);

    /// <summary>
    /// 质量审核员
    /// </summary>
    public const string auditor = nameof(auditor);

    /// <summary>
    /// 访客
    /// </summary>
    public const string guest = nameof(guest);

    /// <summary>
    /// 所有业务角色（除了管理员）
    /// </summary>
    public static readonly string[] BusinessRoles =
    {
        supervisor,
        tester,
        auditor,
        guest
    };

    /// <summary>
    /// 所有角色（包含管理员）
    /// </summary>
    public static readonly string[] AllRoles =
    {
        admin,
        supervisor,
        tester,
        auditor,
        guest
    };

}
