using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestWorkshop.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkshopTestReportFetchRecords : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TestReportError",
                table: "AppWorkshopOrders");

            migrationBuilder.CreateTable(
                name: "AppWorkshopOrderReportFetchRecords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderId = table.Column<Guid>(type: "uuid", nullable: false, comment: "订单ID"),
                    TriggerType = table.Column<int>(type: "integer", nullable: false, comment: "触发方式 (0=自动 1=手动)"),
                    RequestedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false, comment: "请求状态 (0=排队中 1=获取中 2=无报告 3=成功 4=失败)"),
                    AttemptNo = table.Column<int>(type: "integer", nullable: false, comment: "请求序号"),
                    ScheduledAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, comment: "允许执行本次请求的时间"),
                    StartedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, comment: "开始请求时间"),
                    CompletedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, comment: "请求完成时间"),
                    HttpStatusCode = table.Column<int>(type: "integer", nullable: true, comment: "外部接口HTTP状态码"),
                    ExternalReportId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true, comment: "外部报告ID（接口确定后使用）"),
                    ErrorCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true, comment: "错误码"),
                    ErrorMessage = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true, comment: "错误信息"),
                    FileCount = table.Column<int>(type: "integer", nullable: false, comment: "本次获取的文件数量"),
                    IsRetryable = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true, comment: "失败后是否允许自动重试"),
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
                    table.PrimaryKey("PK_AppWorkshopOrderReportFetchRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppWorkshopOrderReportFetchRecords_AppWorkshopOrders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "AppWorkshopOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppWorkshopOrders_Status_TestReportStatus",
                table: "AppWorkshopOrders",
                columns: new[] { "Status", "TestReportStatus" });

            migrationBuilder.CreateIndex(
                name: "IX_AppWorkshopOrderReportFetchRecords_OrderId_CreationTime",
                table: "AppWorkshopOrderReportFetchRecords",
                columns: new[] { "OrderId", "CreationTime" });

            migrationBuilder.CreateIndex(
                name: "IX_AppWorkshopOrderReportFetchRecords_Status_ScheduledAt",
                table: "AppWorkshopOrderReportFetchRecords",
                columns: new[] { "Status", "ScheduledAt" });

            migrationBuilder.CreateIndex(
                name: "UX_AppWorkshopOrderReportFetchRecords_ActiveOrder",
                table: "AppWorkshopOrderReportFetchRecords",
                column: "OrderId",
                unique: true,
                filter: "\"Status\" IN (0, 1) AND \"IsDeleted\" = false");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppWorkshopOrderReportFetchRecords");

            migrationBuilder.DropIndex(
                name: "IX_AppWorkshopOrders_Status_TestReportStatus",
                table: "AppWorkshopOrders");

            migrationBuilder.AddColumn<string>(
                name: "TestReportError",
                table: "AppWorkshopOrders",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true,
                comment: "测试报告获取失败原因");
        }
    }
}
