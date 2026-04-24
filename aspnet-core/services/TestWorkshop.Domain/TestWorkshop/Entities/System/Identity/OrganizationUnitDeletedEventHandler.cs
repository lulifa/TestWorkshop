namespace TestWorkshop;

public class OrganizationUnitDeletedEventHandler :
    IDistributedEventHandler<EntityDeletedEto<OrganizationUnitEto>>,
    ITransientDependency
{
    protected IPermissionManager PermissionManager { get; }

    public OrganizationUnitDeletedEventHandler(IPermissionManager permissionManager)
    {
        PermissionManager = permissionManager;
    }

    public async Task HandleEventAsync(EntityDeletedEto<OrganizationUnitEto> eventData)
    {
        await PermissionManager.DeleteAsync(OrganizationUnitPermissionValueProvider.ProviderName, eventData.Entity.Code);
        await PermissionManager.DeleteAsync(OrganizationUnitPermissionValueProvider.ProviderName, eventData.Entity.Id.ToString());
    }
}
