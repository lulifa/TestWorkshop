using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestWorkshop.Migrations
{
    /// <inheritdoc />
    public partial class AddNotificationTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppNotifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true, comment: "租户id"),
                    Title = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false, comment: "消息标题"),
                    Content = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false, comment: "消息内容"),
                    MessageType = table.Column<int>(type: "integer", nullable: false, comment: "消息类型"),
                    MessageLevel = table.Column<int>(type: "integer", nullable: false, comment: "消息等级"),
                    SenderUserId = table.Column<Guid>(type: "uuid", nullable: false, comment: "发送人用户Id"),
                    SenderUserName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false, comment: "发送人用户名"),
                    ReceiveUserId = table.Column<Guid>(type: "uuid", nullable: true, comment: "接收人用户Id"),
                    ReceiveUserName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true, comment: "接收人用户名"),
                    Read = table.Column<bool>(type: "boolean", nullable: false, comment: "是否已读"),
                    ReadTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, comment: "已读时间"),
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
                    table.PrimaryKey("PK_AppNotifications", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppNotificationSubscriptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true, comment: "租户id"),
                    NotificationId = table.Column<Guid>(type: "uuid", nullable: false, comment: "消息Id"),
                    ReceiveUserId = table.Column<Guid>(type: "uuid", nullable: false, comment: "接收人用户Id"),
                    ReceiveUserName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true, comment: "接收人用户名"),
                    Read = table.Column<bool>(type: "boolean", nullable: false, comment: "是否已读"),
                    ReadTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, comment: "已读时间"),
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
                    table.PrimaryKey("PK_AppNotificationSubscriptions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppNotificationSubscriptions_NotificationId",
                table: "AppNotificationSubscriptions",
                column: "NotificationId");

            migrationBuilder.CreateIndex(
                name: "IX_AppNotificationSubscriptions_ReceiveUserId",
                table: "AppNotificationSubscriptions",
                column: "ReceiveUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppNotifications");

            migrationBuilder.DropTable(
                name: "AppNotificationSubscriptions");
        }
    }
}
