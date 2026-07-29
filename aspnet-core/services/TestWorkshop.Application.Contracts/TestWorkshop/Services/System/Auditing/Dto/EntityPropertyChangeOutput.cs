namespace TestWorkshop;

public class EntityPropertyChangeOutput
{

    public Guid Id { get; set; }

    public Guid? TenantId { get; set; }

    public Guid EntityChangeId { get; set; }

    public string NewValue { get; set; }

    public string OriginalValue { get; set; }

    public string PropertyName { get; set; }

    public string PropertyTypeFullName { get; set; }

}
