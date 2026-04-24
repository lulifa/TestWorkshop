namespace TestWorkshop;

public class TenantConnectionStringCheckOptions
{
    public IDictionary<string, IDataBaseConnectionStringChecker> ConnectionStringCheckers { get; } 

    public TenantConnectionStringCheckOptions()
    {
        ConnectionStringCheckers = new Dictionary<string, IDataBaseConnectionStringChecker>(StringComparer.InvariantCultureIgnoreCase);
    }
}
