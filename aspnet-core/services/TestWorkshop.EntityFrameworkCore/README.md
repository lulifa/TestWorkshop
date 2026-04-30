TestWorkshop 集成 TimescaleDB 说明（精简版）
一、EFCore 配置（ModelBuilder 扩展）
文件路径：TestWorkshop.EntityFrameworkCore
public static void ConfigureTimeScale(this ModelBuilder builder)
{
    builder.Entity<Device>(b =>
    {
        b.ToTable(TestWorkshopDbProperties.DbTablePrefix + "Devices", TestWorkshopDbProperties.DbSchema);

        b.HasKey(x => x.Id);

        b.Property(p => p.Code)
            .HasMaxLength(TestWorkshopConsts.MaxLength64)
            .HasColumnName(nameof(Device.Code))
            .IsRequired();
        b.Property(p => p.Name)
            .HasMaxLength(TestWorkshopConsts.MaxLength128)
            .HasColumnName(nameof(Device.Name))
            .IsRequired();

        b.ConfigureByConvention();

        b.HasIndex(x => x.Code).IsUnique();
    });

    builder.Entity<DeviceTelemetry>(b =>
    {
        b.ToTable(TestWorkshopDbProperties.DbTablePrefix + "DeviceTelemetries", TestWorkshopDbProperties.DbSchema);

        b.HasKey(x => x.Id);

        b.Property(p => p.Metric)
            .HasMaxLength(TestWorkshopConsts.MaxLength128)
            .HasColumnName(nameof(DeviceTelemetry.Metric))
            .IsRequired();
        b.Property(p => p.Timestamp)
            .HasColumnName(nameof(DeviceTelemetry.Timestamp))
            .HasColumnType("timestamp with time zone")
            .IsRequired();
        b.Property(p => p.DeviceId).IsRequired();
        b.Property(p => p.Value).IsRequired();

        b.ConfigureByConvention();
    });
}
二、迁移文件配置
1. 执行命令生成迁移：Add-Migration AddDeviceTelemetry
2. 在生成的迁移文件 Up 方法末尾，添加以下 TimescaleDB 核心脚本：
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

        // TimescaleDB 扩展核心配置（手动添加）
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