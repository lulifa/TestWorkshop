using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestWorkshop.Migrations
{
    /// <inheritdoc />
    public partial class RemoveWorkshopAlarmNotificationState : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastNotifiedAt",
                table: "AppWorkshopAlarms");

            migrationBuilder.DropColumn(
                name: "Level",
                table: "AppWorkshopAlarms");

            migrationBuilder.AlterColumn<DateTime>(
                name: "StartedAt",
                table: "AppWorkshopAlarms",
                type: "timestamp with time zone",
                nullable: false,
                comment: "报警成立时间",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldComment: "报警成立时间，升级时长从这里算");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "StartedAt",
                table: "AppWorkshopAlarms",
                type: "timestamp with time zone",
                nullable: false,
                comment: "报警成立时间，升级时长从这里算",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldComment: "报警成立时间");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastNotifiedAt",
                table: "AppWorkshopAlarms",
                type: "timestamp with time zone",
                nullable: true,
                comment: "最近一次通知时间");

            migrationBuilder.AddColumn<int>(
                name: "Level",
                table: "AppWorkshopAlarms",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                comment: "已通知级别 0=未通知 1/2/3");
        }
    }
}
