using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestWorkshop.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkshopAlarmRetentionIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_AppWorkshopAlarms_Status_ClearedAt",
                table: "AppWorkshopAlarms",
                columns: new[] { "Status", "ClearedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AppWorkshopAlarms_Status_ClearedAt",
                table: "AppWorkshopAlarms");
        }
    }
}
