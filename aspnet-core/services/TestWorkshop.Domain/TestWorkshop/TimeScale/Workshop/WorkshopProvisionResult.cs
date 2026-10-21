namespace TestWorkshop.TimeScale;

public class WorkshopProvisionResult
{
    public Guid OrganizationUnitId { get; set; }

    public Guid? ParentOrganizationUnitId { get; set; }

    public string DisplayName { get; set; }

    public string BusinessCode { get; set; }

    public List<WorkshopProvisionedAccount> Accounts { get; set; } = [];
}

public class WorkshopProvisionedAccount
{
    public Guid UserId { get; set; }

    public string UserName { get; set; }

    public string DisplayName { get; set; }

    public string RoleName { get; set; }
}
