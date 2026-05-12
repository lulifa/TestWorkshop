using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestWorkshop.Migrations
{
    /// <inheritdoc />
    public partial class AddTimeScaleEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppWorkshopDeviceTelemetries",
                columns: table => new
                {
                    DeviceId = table.Column<Guid>(type: "uuid", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Metric = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Value = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppWorkshopDeviceTelemetries", x => new { x.DeviceId, x.Timestamp, x.Metric });
                });

            // TimescaleDB 扩展核心配置（手动添加）
            migrationBuilder.Sql(@"CREATE EXTENSION IF NOT EXISTS timescaledb;");

            migrationBuilder.Sql(@"
                SELECT create_hypertable(
                    '""AppWorkshopDeviceTelemetries""',
                    'Timestamp',
                    if_not_exists => TRUE
                );
            ");

            migrationBuilder.Sql(@"
                CREATE INDEX IF NOT EXISTS idx_device_time
                ON ""AppWorkshopDeviceTelemetries"" (""DeviceId"", ""Timestamp"" DESC);
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppWorkshopDeviceTelemetries");
        }
    }
}
