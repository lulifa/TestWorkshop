using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestWorkshop.Migrations
{
    /// <inheritdoc />
    public partial class RemoveWorkshopOrderAuditOpinion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AuditOpinion",
                table: "AppWorkshopOrderAuditRecords");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AuditOpinion",
                table: "AppWorkshopOrderAuditRecords",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true,
                comment: "审核意见");
        }
    }
}
