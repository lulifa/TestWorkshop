using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestWorkshop.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkshopOrderSupervisorAndCompletionTime : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ProjectSupervisorUserId",
                table: "AppWorkshopOrders",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                comment: "单船项目主管用户ID");

            migrationBuilder.AddColumn<DateTime>(
                name: "RequiredCompletionTime",
                table: "AppWorkshopOrders",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                comment: "要求车间完成测试时间");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProjectSupervisorUserId",
                table: "AppWorkshopOrders");

            migrationBuilder.DropColumn(
                name: "RequiredCompletionTime",
                table: "AppWorkshopOrders");
        }
    }
}
