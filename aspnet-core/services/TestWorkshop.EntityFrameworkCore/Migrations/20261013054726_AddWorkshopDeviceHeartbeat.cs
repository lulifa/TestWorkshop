using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestWorkshop.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkshopDeviceHeartbeat : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppWorkshopDeviceHeartbeats",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DeviceId = table.Column<Guid>(type: "uuid", nullable: false, comment: "关联设备ID"),
                    DeviceCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false, comment: "设备编码（下位机上报）"),
                    LastHeartbeatAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "最近一次心跳时间"),
                    AlarmLevel = table.Column<int>(type: "integer", nullable: false, defaultValue: 0, comment: "已通知报警级别 0=正常 1=5分钟 2=10分钟 3=20分钟"),
                    LastAlarmAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "最近一次发送报警的时间")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppWorkshopDeviceHeartbeats", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppWorkshopDeviceHeartbeats_AppWorkshopDevices_DeviceId",
                        column: x => x.DeviceId,
                        principalTable: "AppWorkshopDevices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppWorkshopDeviceHeartbeats_DeviceCode",
                table: "AppWorkshopDeviceHeartbeats",
                column: "DeviceCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppWorkshopDeviceHeartbeats_DeviceId",
                table: "AppWorkshopDeviceHeartbeats",
                column: "DeviceId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppWorkshopDeviceHeartbeats");
        }
    }
}
