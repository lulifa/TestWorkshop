using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestWorkshop.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkshopOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "OrderId",
                table: "AppWorkshopTelemetryTasks",
                type: "uuid",
                nullable: true,
                comment: "关联订单ID，历史数据可能为空");

            migrationBuilder.AlterColumn<DateTime>(
                name: "TestDate",
                table: "AppWorkshopDeviceTelemetries",
                type: "timestamp without time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "date");

            migrationBuilder.CreateTable(
                name: "AppWorkshopOrders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderNo = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false, comment: "订单号"),
                    DeviceId = table.Column<Guid>(type: "uuid", nullable: false, comment: "关联设备"),
                    Status = table.Column<int>(type: "integer", nullable: false, comment: "测试状态 (0=Draft 1=Testing 2=Completed 3=Cancelled)"),
                    TestStartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "实际测试开始时间"),
                    TestEndedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "实际测试结束时间"),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true, comment: "租户ID"),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppWorkshopOrders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppWorkshopOrders_AppWorkshopDevices_DeviceId",
                        column: x => x.DeviceId,
                        principalTable: "AppWorkshopDevices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppWorkshopTelemetryTasks_OrderId",
                table: "AppWorkshopTelemetryTasks",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_AppWorkshopOrders_DeviceId_CreationTime",
                table: "AppWorkshopOrders",
                columns: new[] { "DeviceId", "CreationTime" });

            migrationBuilder.CreateIndex(
                name: "IX_AppWorkshopOrders_OrderNo",
                table: "AppWorkshopOrders",
                column: "OrderNo",
                unique: true,
                filter: "\"TenantId\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AppWorkshopOrders_Status_TestStartedAt",
                table: "AppWorkshopOrders",
                columns: new[] { "Status", "TestStartedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_AppWorkshopOrders_TenantId_OrderNo",
                table: "AppWorkshopOrders",
                columns: new[] { "TenantId", "OrderNo" },
                unique: true,
                filter: "\"TenantId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "UX_AppWorkshopOrders_DeviceId_Testing",
                table: "AppWorkshopOrders",
                column: "DeviceId",
                unique: true,
                filter: "\"Status\" = 1");

            migrationBuilder.AddForeignKey(
                name: "FK_AppWorkshopTelemetryTasks_AppWorkshopOrders_OrderId",
                table: "AppWorkshopTelemetryTasks",
                column: "OrderId",
                principalTable: "AppWorkshopOrders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppWorkshopTelemetryTasks_AppWorkshopOrders_OrderId",
                table: "AppWorkshopTelemetryTasks");

            migrationBuilder.DropTable(
                name: "AppWorkshopOrders");

            migrationBuilder.DropIndex(
                name: "IX_AppWorkshopTelemetryTasks_OrderId",
                table: "AppWorkshopTelemetryTasks");

            migrationBuilder.DropColumn(
                name: "OrderId",
                table: "AppWorkshopTelemetryTasks");

            migrationBuilder.AlterColumn<DateTime>(
                name: "TestDate",
                table: "AppWorkshopDeviceTelemetries",
                type: "date",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldNullable: true);
        }
    }
}
