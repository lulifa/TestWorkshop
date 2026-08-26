using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestWorkshop.Migrations
{
    /// <inheritdoc />
    public partial class AddTaskIdToWorkshopDeviceTelemetry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "TaskId",
                table: "AppWorkshopDeviceTelemetries",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateIndex(
                name: "IX_AppWorkshopDeviceTelemetries_TaskId_Timestamp",
                table: "AppWorkshopDeviceTelemetries",
                columns: new[] { "TaskId", "Timestamp" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AppWorkshopDeviceTelemetries_TaskId_Timestamp",
                table: "AppWorkshopDeviceTelemetries");

            migrationBuilder.DropColumn(
                name: "TaskId",
                table: "AppWorkshopDeviceTelemetries");
        }
    }
}
