namespace TestWorkshop;

public interface IDataBaseConnectionStringChecker
{
    Task<DataBaseConnectionStringCheckResult> CheckAsync(string connectionString);

}
