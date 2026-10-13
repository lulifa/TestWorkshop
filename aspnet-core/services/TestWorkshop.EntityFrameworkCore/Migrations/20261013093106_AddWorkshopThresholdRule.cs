using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestWorkshop.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkshopThresholdRule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppWorkshopThresholdRules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DeviceType = table.Column<int>(type: "integer", nullable: false, comment: "设备类型 (1=FIVA 2=PUMP 3=ICU 4=SENSOR)"),
                    DataItemType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false, comment: "数据类型/物理量编码"),
                    UpperLimit = table.Column<decimal>(type: "numeric(18,4)", nullable: true, comment: "上限阈值，超过视为偏高"),
                    LowerLimit = table.Column<decimal>(type: "numeric(18,4)", nullable: true, comment: "下限阈值，低于视为偏低"),
                    AlarmContent = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true, comment: "报警内容"),
                    Enabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true, comment: "是否启用"),
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
                    table.PrimaryKey("PK_AppWorkshopThresholdRules", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppWorkshopThresholdRules_DeviceType_DataItemType",
                table: "AppWorkshopThresholdRules",
                columns: new[] { "DeviceType", "DataItemType" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppWorkshopThresholdRules");
        }
    }
}
