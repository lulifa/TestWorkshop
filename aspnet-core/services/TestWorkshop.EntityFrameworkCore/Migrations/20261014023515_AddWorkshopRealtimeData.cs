using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestWorkshop.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkshopRealtimeData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TestedDeviceCode",
                table: "AppWorkshopAlarms");

            migrationBuilder.DropColumn(
                name: "TestedDeviceName",
                table: "AppWorkshopAlarms");

            migrationBuilder.CreateTable(
                name: "AppWorkshopRealtimeData",
                columns: table => new
                {
                    DeviceId = table.Column<Guid>(type: "uuid", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DeviceCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Pressure = table.Column<decimal>(type: "numeric(18,4)", nullable: true),
                    Flow = table.Column<decimal>(type: "numeric(18,4)", nullable: true),
                    Temperature = table.Column<decimal>(type: "numeric(18,4)", nullable: true),
                    Torque = table.Column<decimal>(type: "numeric(18,4)", nullable: true),
                    Vibration = table.Column<decimal>(type: "numeric(18,4)", nullable: true),
                    EmergencyStop = table.Column<bool>(type: "boolean", nullable: true),
                    ReceivedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppWorkshopRealtimeData", x => new { x.DeviceId, x.Timestamp });
                });

            // 转成 TimescaleDB 超级表，按 Timestamp 分区（与遥测表保持一致）
            migrationBuilder.Sql("""
                SELECT create_hypertable(
                    '"AppWorkshopRealtimeData"',
                    'Timestamp',
                    if_not_exists => TRUE
                );
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppWorkshopRealtimeData");

            migrationBuilder.AddColumn<string>(
                name: "TestedDeviceCode",
                table: "AppWorkshopAlarms",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true,
                comment: "被测设备编码（上下文）");

            migrationBuilder.AddColumn<string>(
                name: "TestedDeviceName",
                table: "AppWorkshopAlarms",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true,
                comment: "被测设备名称（上下文）");
        }
    }
}
