namespace TestWorkshop;

public class IdentityUserChangePasswordInput
{
    [Required]
    [DisableAuditing]
    public string CurrentPassword { get; set; }

    [Required]
    [DisableAuditing]
    [DynamicStringLength(typeof(IdentityUserConsts), nameof(IdentityUserConsts.MaxPasswordLength))]
    public string NewPassword { get; set; }
}