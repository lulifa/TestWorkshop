using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestWorkshop.Migrations
{
    /// <inheritdoc />
    public partial class AddTimeScaleDB : Migration
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
                    ChannelType = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    TaskId = table.Column<long>(type: "bigint", nullable: false),
                    StartTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    EndTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    StartTime2 = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    EndTime2 = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    TestDate = table.Column<DateTime>(type: "date", nullable: false),
                    Value = table.Column<double[]>(type: "double precision[]", nullable: false),
                    TestedDeviceCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    TestedDeviceName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    PilotSN = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    ShipName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    TestBy = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppWorkshopDeviceTelemetries", x => new { x.DeviceId, x.Timestamp, x.ChannelType });
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppWorkshopDeviceTelemetries_TaskId_Timestamp",
                table: "AppWorkshopDeviceTelemetries",
                columns: new[] { "TaskId", "Timestamp" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppWorkshopDeviceTelemetries");
        }
    }
}
