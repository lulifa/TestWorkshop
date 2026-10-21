namespace TestWorkshop;

public class InitializeWorkshopResultDto
{
    public Guid OrganizationUnitId { get; set; }

    public Guid? ParentOrganizationUnitId { get; set; }

    public string DisplayName { get; set; }

    public string BusinessCode { get; set; }

    public List<InitializeWorkshopAccountDto> Accounts { get; set; } = [];
}

public class InitializeWorkshopAccountDto
{
    public Guid UserId { get; set; }

    public string UserName { get; set; }

    public string DisplayName { get; set; }

    public string RoleName { get; set; }
}
