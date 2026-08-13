namespace TestWorkshop;

public class IdentityUserProfileInput
{
    [Required]
    public string Email { get; set; }

    public string Name { get; set; }

    public string Surname { get; set; }
}