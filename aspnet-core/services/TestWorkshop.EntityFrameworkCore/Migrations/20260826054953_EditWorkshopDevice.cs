using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestWorkshop.Migrations
{
    /// <inheritdoc />
    public partial class EditWorkshopDevice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AppWorkshopDevices_Code",
                table: "AppWorkshopDevices");

            migrationBuilder.AddColumn<string>(
                name: "ConcurrencyStamp",
                table: "AppWorkshopDevices",
                type: "character varying(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreationTime",
                table: "AppWorkshopDevices",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "CreatorId",
                table: "AppWorkshopDevices",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeleterId",
                table: "AppWorkshopDevices",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletionTime",
                table: "AppWorkshopDevices",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExtraProperties",
                table: "AppWorkshopDevices",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "AppWorkshopDevices",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModificationTime",
                table: "AppWorkshopDevices",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LastModifierId",
                table: "AppWorkshopDevices",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppWorkshopDevices_TenantId_Code",
                table: "AppWorkshopDevices",
                columns: new[] { "TenantId", "Code" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AppWorkshopDevices_TenantId_Code",
                table: "AppWorkshopDevices");

            migrationBuilder.DropColumn(
                name: "ConcurrencyStamp",
                table: "AppWorkshopDevices");

            migrationBuilder.DropColumn(
                name: "CreationTime",
                table: "AppWorkshopDevices");

            migrationBuilder.DropColumn(
                name: "CreatorId",
                table: "AppWorkshopDevices");

            migrationBuilder.DropColumn(
                name: "DeleterId",
                table: "AppWorkshopDevices");

            migrationBuilder.DropColumn(
                name: "DeletionTime",
                table: "AppWorkshopDevices");

            migrationBuilder.DropColumn(
                name: "ExtraProperties",
                table: "AppWorkshopDevices");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "AppWorkshopDevices");

            migrationBuilder.DropColumn(
                name: "LastModificationTime",
                table: "AppWorkshopDevices");

            migrationBuilder.DropColumn(
                name: "LastModifierId",
                table: "AppWorkshopDevices");

            migrationBuilder.CreateIndex(
                name: "IX_AppWorkshopDevices_Code",
                table: "AppWorkshopDevices",
                column: "Code",
                unique: true);
        }
    }
}
