TestWorkshop 集成 TimescaleDB 说明（精简版）
一、EFCore 配置（ModelBuilder 扩展）
文件路径：TestWorkshop.EntityFrameworkCore
public static void ConfigureTimeScale(this ModelBuilder builder)
{
    builder.Entity<WorkshopDeviceTelemetry>(b =>
    {
        b.ToTable(TestWorkshopDbProperties.DbTablePrefix + "WorkshopDeviceTelemetries", TestWorkshopDbProperties.DbSchema);

        b.HasKey(x => new { x.DeviceId, x.Timestamp, x.Metric });

        b.Property(p => p.Metric)
            .HasMaxLength(TestWorkshopConsts.MaxLength128)
            .HasColumnName(nameof(WorkshopDeviceTelemetry.Metric))
            .IsRequired();
        b.Property(p => p.Timestamp)
            .HasColumnName(nameof(WorkshopDeviceTelemetry.Timestamp))
            .HasColumnType("timestamp with time zone")
            .IsRequired();
        b.Property(p => p.DeviceId).IsRequired();
        b.Property(p => p.Value).IsRequired();

        b.ConfigureByConvention();
    });

}
二、迁移文件配置
1. 执行命令生成迁移：Add-Migration AddTimeScaleEntities
2. 在生成的迁移文件 Up 方法末尾，添加以下 TimescaleDB 核心脚本：
public partial class AddTimeScaleEntities : Migration
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
                Metric = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                Value = table.Column<double>(type: "double precision", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AppWorkshopDeviceTelemetries", x => new { x.DeviceId, x.Timestamp, x.Metric });
            });

        // TimescaleDB 扩展核心配置（手动添加）
        migrationBuilder.Sql(@"CREATE EXTENSION IF NOT EXISTS timescaledb;");

        migrationBuilder.Sql(@"
            SELECT create_hypertable(
                '""AppWorkshopDeviceTelemetries""',
                'Timestamp',
                if_not_exists => TRUE
            );
        ");

        migrationBuilder.Sql(@"
            CREATE INDEX IF NOT EXISTS idx_device_time
            ON ""AppWorkshopDeviceTelemetries"" (""DeviceId"", ""Timestamp"" DESC);
        ");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "AppWorkshopDeviceTelemetries");
    }
}