using System.ComponentModel.DataAnnotations;

namespace TestWorkshop;

public class InitializeWorkshopInput
{
    [Required]
    [StringLength(128)]
    public string DisplayName { get; set; }

    [Required]
    [StringLength(64)]
    public string BusinessCode { get; set; }

    [StringLength(64)]
    public string AccountPrefix { get; set; }

    [Required]
    [StringLength(128, MinimumLength = 6)]
    public string InitialPassword { get; set; }

    public Guid? ParentOrganizationUnitId { get; set; }
}
