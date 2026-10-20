using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestWorkshop.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkshopOrderReportAudit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "AppWorkshopOrders",
                type: "integer",
                nullable: false,
                comment: "测试状态 (0=已录入未测试 1=测试中 2=测试完成未审核 3=已取消 4=审核通过 5=审核不通过)",
                oldClrType: typeof(int),
                oldType: "integer",
                oldComment: "测试状态 (0=Draft 1=Testing 2=Completed 3=Cancelled)");

            migrationBuilder.AddColumn<string>(
                name: "TestReportError",
                table: "AppWorkshopOrders",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true,
                comment: "测试报告获取失败原因");

            migrationBuilder.AddColumn<DateTime>(
                name: "TestReportFetchedAt",
                table: "AppWorkshopOrders",
                type: "timestamp without time zone",
                nullable: true,
                comment: "测试报告获取完成时间");

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

            migrationBuilder.AddColumn<int>(
                name: "TestReportStatus",
                table: "AppWorkshopOrders",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                comment: "测试报告获取状态 (0=未获取 1=获取中 2=已就绪 3=获取失败)");

            migrationBuilder.CreateTable(
                name: "AppWorkshopOrderAuditRecords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderId = table.Column<Guid>(type: "uuid", nullable: false, comment: "订单ID"),
                    Result = table.Column<int>(type: "integer", nullable: false, comment: "审核结果 (0=不通过 1=通过)"),
                    AuditOpinion = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true, comment: "审核意见"),
                    MaintenanceSuggestion = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true, comment: "维保建议"),
                    AuditorUserId = table.Column<Guid>(type: "uuid", nullable: false, comment: "审核人用户ID"),
                    AuditorName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false, comment: "审核人姓名"),
                    AuditedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, comment: "审核时间"),
                    ReportFileId = table.Column<Guid>(type: "uuid", nullable: true, comment: "审核时对应的报告文件ID"),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true, comment: "租户ID"),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppWorkshopOrderAuditRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppWorkshopOrderAuditRecords_AppWorkshopOrders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "AppWorkshopOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppWorkshopOrderAuditRecords_OrderId_AuditedAt",
                table: "AppWorkshopOrderAuditRecords",
                columns: new[] { "OrderId", "AuditedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppWorkshopOrderAuditRecords");

            migrationBuilder.DropColumn(
                name: "TestReportError",
                table: "AppWorkshopOrders");

            migrationBuilder.DropColumn(
                name: "TestReportFetchedAt",
                table: "AppWorkshopOrders");

            migrationBuilder.DropColumn(
                name: "TestReportFileId",
                table: "AppWorkshopOrders");

            migrationBuilder.DropColumn(
                name: "TestReportFileName",
                table: "AppWorkshopOrders");

            migrationBuilder.DropColumn(
                name: "TestReportStatus",
                table: "AppWorkshopOrders");

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "AppWorkshopOrders",
                type: "integer",
                nullable: false,
                comment: "测试状态 (0=Draft 1=Testing 2=Completed 3=Cancelled)",
                oldClrType: typeof(int),
                oldType: "integer",
                oldComment: "测试状态 (0=已录入未测试 1=测试中 2=测试完成未审核 3=已取消 4=审核通过 5=审核不通过)");
        }
    }
}
