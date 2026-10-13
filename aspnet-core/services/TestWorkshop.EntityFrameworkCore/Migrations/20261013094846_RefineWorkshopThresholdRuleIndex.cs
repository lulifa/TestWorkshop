using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestWorkshop.Migrations
{
    /// <inheritdoc />
    public partial class RefineWorkshopThresholdRuleIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AppWorkshopThresholdRules_DeviceType_DataItemType",
                table: "AppWorkshopThresholdRules");

            migrationBuilder.CreateIndex(
                name: "IX_AppWorkshopThresholdRules_DeviceType_DataItemType",
                table: "AppWorkshopThresholdRules",
                columns: new[] { "DeviceType", "DataItemType" },
                unique: true,
                filter: "\"IsDeleted\" = false");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AppWorkshopThresholdRules_DeviceType_DataItemType",
                table: "AppWorkshopThresholdRules");

            migrationBuilder.CreateIndex(
                name: "IX_AppWorkshopThresholdRules_DeviceType_DataItemType",
                table: "AppWorkshopThresholdRules",
                columns: new[] { "DeviceType", "DataItemType" },
                unique: true);
        }
    }
}
