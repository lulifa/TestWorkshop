using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestWorkshop.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkshopDeviceHeartbeatRuntime : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "FirstHeartbeatAt",
                table: "AppWorkshopDeviceHeartbeats",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP",
                comment: "第一次心跳时间");

            migrationBuilder.AddColumn<long>(
                name: "TotalRuntimeSeconds",
                table: "AppWorkshopDeviceHeartbeats",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                comment: "累计运行秒数");

            migrationBuilder.Sql(
                """
                UPDATE "AppWorkshopDeviceHeartbeats"
                SET "FirstHeartbeatAt" = "LastHeartbeatAt";
                """);

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FirstHeartbeatAt",
                table: "AppWorkshopDeviceHeartbeats");

            migrationBuilder.DropColumn(
                name: "TotalRuntimeSeconds",
                table: "AppWorkshopDeviceHeartbeats");
        }
    }
}
