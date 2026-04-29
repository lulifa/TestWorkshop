using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TestWorkshop.Migrations
{
    /// <inheritdoc />
    public partial class AddDeviceTelemetry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppDevices",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    OrganizationUnitId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppDevices", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppDeviceTelemetries",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DeviceId = table.Column<Guid>(type: "uuid", nullable: false),
                    Metric = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Value = table.Column<double>(type: "double precision", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppDeviceTelemetries", x => new { x.DeviceId, x.Timestamp, x.Metric });
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppDevices_Code",
                table: "AppDevices",
                column: "Code",
                unique: true);


            migrationBuilder.Sql(@"CREATE EXTENSION IF NOT EXISTS timescaledb;");

            migrationBuilder.Sql(@"
                SELECT create_hypertable(
                    '""AppDeviceTelemetries""',
                    'Timestamp',
                    if_not_exists => TRUE
                );
            ");

            migrationBuilder.Sql(@"
                CREATE INDEX IF NOT EXISTS idx_device_time 
                ON ""AppDeviceTelemetries"" (""DeviceId"", ""Timestamp"" DESC);
            ");


        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppDevices");

            migrationBuilder.DropTable(
                name: "AppDeviceTelemetries");
        }
    }
}
