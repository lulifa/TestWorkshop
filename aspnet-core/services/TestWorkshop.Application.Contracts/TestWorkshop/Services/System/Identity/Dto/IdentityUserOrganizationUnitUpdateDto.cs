namespace TestWorkshop;

public class IdentityUserOrganizationUnitUpdateDto
{
    [Required]
    public Guid[] OrganizationUnitIds { get; set; }
}
