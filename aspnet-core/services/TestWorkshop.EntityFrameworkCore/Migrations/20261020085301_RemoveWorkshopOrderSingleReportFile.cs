using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestWorkshop.Migrations
{
    /// <inheritdoc />
    public partial class RemoveWorkshopOrderSingleReportFile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TestReportFileId",
                table: "AppWorkshopOrders");

            migrationBuilder.DropColumn(
                name: "TestReportFileName",
                table: "AppWorkshopOrders");

            migrationBuilder.DropColumn(
                name: "ReportFileId",
                table: "AppWorkshopOrderAuditRecords");

            migrationBuilder.AddColumn<int>(
                name: "ReportFileCount",
                table: "AppWorkshopOrderAuditRecords",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                comment: "审核时的报告文件数量");

            migrationBuilder.Sql(
                """
                UPDATE "AppWorkshopOrderAuditRecords" AS a
                SET "ReportFileCount" = (
                    SELECT COUNT(*)
                    FROM "AppFileObjects" AS f
                    WHERE f."OwnerType" = 'WorkshopTestReport'
                      AND f."OwnerId" = a."OrderId"::text
                      AND f."IsDeleted" = false
                );
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReportFileCount",
                table: "AppWorkshopOrderAuditRecords");

            migrationBuilder.AddColumn<Guid>(
                name: "TestReportFileId",
                table: "AppWorkshopOrders",
                type: "uuid",
                nullable: true,
                comment: "测试报告文件ID");

            migrationBuilder.AddColumn<string>(
                name: "TestReportFileName",
                table: "AppWorkshopOrders",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true,
                comment: "测试报告文件名");

            migrationBuilder.AddColumn<Guid>(
                name: "ReportFileId",
                table: "AppWorkshopOrderAuditRecords",
                type: "uuid",
                nullable: true,
                comment: "审核时对应的报告文件ID");
        }
    }
}
