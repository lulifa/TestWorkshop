using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestWorkshop.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkshopAlarm : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AlarmLevel",
                table: "AppWorkshopDeviceHeartbeats");

            migrationBuilder.DropColumn(
                name: "LastAlarmAt",
                table: "AppWorkshopDeviceHeartbeats");

            migrationBuilder.CreateTable(
                name: "AppWorkshopAlarms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AlarmType = table.Column<int>(type: "integer", nullable: false, comment: "报警类型 (1=阈值 2=关联 3=设备 4=通信)"),
                    DeviceId = table.Column<Guid>(type: "uuid", nullable: false, comment: "测试台设备ID"),
                    DeviceCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false, comment: "测试台设备编码"),
                    OrganizationUnitId = table.Column<Guid>(type: "uuid", nullable: false, comment: "所属车间，报警可见性按此过滤"),
                    DataItemType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false, comment: "数据类型/物理量编码，空字符串表示不区分"),
                    Content = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true, comment: "报警内容"),
                    TestedDeviceCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true, comment: "被测设备编码（上下文）"),
                    TestedDeviceName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true, comment: "被测设备名称（上下文）"),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "报警成立时间，升级时长从这里算"),
                    Level = table.Column<int>(type: "integer", nullable: false, defaultValue: 0, comment: "已通知级别 0=未通知 1/2/3"),
                    LastNotifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "最近一次通知时间"),
                    Status = table.Column<int>(type: "integer", nullable: false, comment: "状态 (1=进行中 2=已恢复)"),
                    ClearedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "恢复时间"),
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
                    table.PrimaryKey("PK_AppWorkshopAlarms", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppWorkshopAlarms_OrganizationUnitId_Status_StartedAt",
                table: "AppWorkshopAlarms",
                columns: new[] { "OrganizationUnitId", "Status", "StartedAt" });

            migrationBuilder.CreateIndex(
                name: "UX_AppWorkshopAlarms_Active",
                table: "AppWorkshopAlarms",
                columns: new[] { "AlarmType", "DeviceId", "DataItemType" },
                unique: true,
                filter: "\"Status\" = 1 AND \"IsDeleted\" = false");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppWorkshopAlarms");

            migrationBuilder.AddColumn<int>(
                name: "AlarmLevel",
                table: "AppWorkshopDeviceHeartbeats",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                comment: "已通知报警级别 0=正常 1=5分钟 2=10分钟 3=20分钟");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastAlarmAt",
                table: "AppWorkshopDeviceHeartbeats",
                type: "timestamp with time zone",
                nullable: true,
                comment: "最近一次发送报警的时间");
        }
    }
}
