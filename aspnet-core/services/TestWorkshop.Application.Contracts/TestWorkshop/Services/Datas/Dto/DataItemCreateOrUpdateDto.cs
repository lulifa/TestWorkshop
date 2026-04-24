namespace TestWorkshop;

public class DataItemCreateOrUpdateDto : IValidatableObject
{
    [Required]
    [DynamicStringLength(typeof(TestWorkshopConsts), nameof(TestWorkshopConsts.MaxLength128))]
    public string DisplayName { get; set; }

    [DynamicStringLength(typeof(TestWorkshopConsts), nameof(TestWorkshopConsts.MaxLength128))]
    public string DefaultValue { get; set; }

    [DynamicStringLength(typeof(TestWorkshopConsts), nameof(TestWorkshopConsts.MaxLength1024))]
    public string Description { get; set; }

    public bool AllowBeNull { get; set; }

    public ValueType ValueType { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!AllowBeNull && DefaultValue.IsNullOrWhiteSpace())
        {
            var localizer = validationContext.GetRequiredService<IStringLocalizer<TestWorkshopResource>>();
            yield return new ValidationResult(
                localizer["The {0} field is required.", localizer["DisplayName:Value"]],
                new string[] { nameof(DefaultValue) });
        }
    }
}
