using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestWorkshop.Migrations
{
    /// <inheritdoc />
    public partial class UseLocalTimesForWorkshopOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                ALTER TABLE "AppWorkshopOrders"
                ALTER COLUMN "TestStartedAt" TYPE timestamp without time zone
                USING "TestStartedAt" AT TIME ZONE 'Asia/Shanghai';
                """);

            migrationBuilder.Sql(
                """
                ALTER TABLE "AppWorkshopOrders"
                ALTER COLUMN "TestEndedAt" TYPE timestamp without time zone
                USING "TestEndedAt" AT TIME ZONE 'Asia/Shanghai';
                """);

            migrationBuilder.Sql(
                """
                COMMENT ON COLUMN "AppWorkshopOrders"."TestStartedAt"
                IS '实际测试开始时间（本地时间）';
                """);

            migrationBuilder.Sql(
                """
                COMMENT ON COLUMN "AppWorkshopOrders"."TestEndedAt"
                IS '实际测试结束时间（本地时间）';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                ALTER TABLE "AppWorkshopOrders"
                ALTER COLUMN "TestStartedAt" TYPE timestamp with time zone
                USING "TestStartedAt" AT TIME ZONE 'Asia/Shanghai';
                """);

            migrationBuilder.Sql(
                """
                ALTER TABLE "AppWorkshopOrders"
                ALTER COLUMN "TestEndedAt" TYPE timestamp with time zone
                USING "TestEndedAt" AT TIME ZONE 'Asia/Shanghai';
                """);

            migrationBuilder.Sql(
                """
                COMMENT ON COLUMN "AppWorkshopOrders"."TestStartedAt"
                IS '实际测试开始时间';
                """);

            migrationBuilder.Sql(
                """
                COMMENT ON COLUMN "AppWorkshopOrders"."TestEndedAt"
                IS '实际测试结束时间';
                """);
        }
    }
}
