namespace TestWorkshop;

public class AbpHideDefaultApiFilter : IDocumentFilter
{
    private readonly List<string> _hiddenPathPatterns = new()
    {
        "/api/abp",
        "/api/account",
        "/api/identity",
        "/api/setting-management",
        "/api/permission-management",
        "/api/feature-management",
        "/api/multi-tenancy"
    };

    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        var removeKeys = swaggerDoc.Paths
            .Where(p => _hiddenPathPatterns.Any(pattern => p.Key.Contains(pattern)))
            .Select(p => p.Key)
            .ToList();

        foreach (var key in removeKeys)
        {
            swaggerDoc.Paths.Remove(key);
        }
    }
}
