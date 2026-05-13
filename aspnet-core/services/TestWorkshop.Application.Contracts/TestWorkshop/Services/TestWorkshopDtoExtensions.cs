namespace TestWorkshop;

public static class TestWorkshopDtoExtensions
{
    private static readonly OneTimeRunner OneTimeRunner = new OneTimeRunner();

    public static void Configure()
    {
        OneTimeRunner.Run(() =>
        {
            /* You can add extension properties to DTOs
             * defined in the depended modules.
             *
             * Example:
             *
             * ObjectExtensionManager.Instance
             *   .AddOrUpdateProperty<IdentityRoleDto, string>("Title");
             *
             * See the documentation for more:
             * https://docs.abp.io/en/abp/latest/Object-Extensions
             */

            // 给 OrganizationUnit 的【创建 DTO】加 BusinessCode
            ObjectExtensionManager.Instance
                .AddOrUpdateProperty<OrganizationUnitCreateDto, string>(OrganizationUnitConstants.BusinessCode);

            // 给 OrganizationUnit 的【更新 DTO】加 BusinessCode
            ObjectExtensionManager.Instance
                .AddOrUpdateProperty<OrganizationUnitUpdateDto, string>(OrganizationUnitConstants.BusinessCode);

            // 给 OrganizationUnit 的【DTO】加 BusinessCode
            ObjectExtensionManager.Instance
                .AddOrUpdateProperty<OrganizationUnitDto, string>(OrganizationUnitConstants.BusinessCode);

        });
    }
}
