namespace TestWorkshop.EntityFrameworkCore;

public static class TestWorkshopDbContextModelCreatingExtensions
{

    public static void ConfigureTestWorkshop(this ModelBuilder builder)
    {
        Check.NotNull(builder, nameof(builder));

        builder.ConfigureTimeScale();

        builder.ConfigurePlatform();

    }

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

        // ✅ 新增：TelemetryTask 配置
        builder.Entity<TelemetryTask>(b =>
        {
            b.ToTable(TestWorkshopDbProperties.DbTablePrefix + "TelemetryTasks", TestWorkshopDbProperties.DbSchema);

            b.HasKey(x => x.Id);

            b.Property(p => p.FileId)
                .HasColumnName(nameof(TelemetryTask.FileId))
                .IsRequired()
                .HasComment("文件ID");

            b.Property(p => p.FileName)
                .HasMaxLength(TestWorkshopConsts.MaxLength256)
                .HasColumnName(nameof(TelemetryTask.FileName))
                .IsRequired()
                .HasComment("原始文件名");

            b.Property(p => p.FileSize)
                .HasColumnName(nameof(TelemetryTask.FileSize))
                .IsRequired()
                .HasComment("文件大小（字节）");

            b.Property(p => p.BlobName)
                .HasMaxLength(TestWorkshopConsts.MaxLength256)
                .HasColumnName(nameof(TelemetryTask.BlobName))
                .HasComment("Blob存储文件名");

            b.Property(p => p.Status)
                .HasColumnName(nameof(TelemetryTask.Status))
                .IsRequired()
                .HasComment("处理状态 (0=Pending 1=Processing 2=Success 3=Failed)");

            b.Property(p => p.RetryCount)
                .HasColumnName(nameof(TelemetryTask.RetryCount))
                .IsRequired()
                .HasDefaultValue(0)
                .HasComment("重试次数");

            b.Property(p => p.NextRetryTime)
                .HasColumnName(nameof(TelemetryTask.NextRetryTime))
                .HasColumnType("timestamp with time zone")
                .HasComment("下次重试时间");

            b.Property(p => p.Error)
                .HasColumnName(nameof(TelemetryTask.Error))
                .HasComment("错误信息");

            b.Property(p => p.RecordCount)
                .HasColumnName(nameof(TelemetryTask.RecordCount))
                .HasComment("解析的记录数");

            b.Property(p => p.CreatedAt)
                .HasColumnName(nameof(TelemetryTask.CreatedAt))
                .HasColumnType("timestamp with time zone")
                .IsRequired()
                .HasComment("创建时间");

            b.Property(p => p.ProcessedAt)
                .HasColumnName(nameof(TelemetryTask.ProcessedAt))
                .HasColumnType("timestamp with time zone")
                .HasComment("处理完成时间");

            b.Property(p => p.ProcessingStartedAt)
                .HasColumnName(nameof(TelemetryTask.ProcessingStartedAt))
                .HasColumnType("timestamp with time zone")
                .HasComment("任务开始处理的时间，用于判断是否卡死");

            b.Property(p => p.ExpiresAt)
                .HasColumnName(nameof(TelemetryTask.ExpiresAt))
                .HasColumnType("timestamp with time zone")
                .IsRequired()
                .HasComment("过期时间");

            b.Property(p => p.IsDeleted)
                .HasColumnName(nameof(TelemetryTask.IsDeleted))
                .IsRequired()
                .HasDefaultValue(false)
                .HasComment("是否已删除");

            b.Property(p => p.DeletedAt)
                .HasColumnName(nameof(TelemetryTask.DeletedAt))
                .HasColumnType("timestamp with time zone")
                .HasComment("删除时间");

            b.Property(p => p.TenantId)
                .HasColumnName(nameof(TelemetryTask.TenantId))
                .HasComment("租户ID");

            b.ConfigureByConvention();

            // ✅ 创建索引以提高查询性能
            b.HasIndex(x => x.FileName);

            b.HasIndex(x => x.Status);

            b.HasIndex(x => new { x.ExpiresAt, x.IsDeleted });

            b.HasIndex(x => x.CreatedAt);

            b.HasIndex(x => x.FileId).IsUnique();

            b.HasIndex(x => new { x.Status, x.NextRetryTime, x.CreatedAt });

        });
    }

    public static void ConfigurePlatform(this ModelBuilder builder)
    {
        builder.Entity<Layout>(b =>
        {
            b.ToTable(TestWorkshopDbProperties.DbTablePrefix + "Layouts", TestWorkshopDbProperties.DbSchema);

            b.Property(p => p.Framework)
                .HasMaxLength(TestWorkshopConsts.MaxLength64)
                .HasColumnName(nameof(Layout.Framework))
                .IsRequired();

            b.ConfigureRoute();
        });

        builder.Entity<Menu>(b =>
        {
            b.ToTable(TestWorkshopDbProperties.DbTablePrefix + "Menus", TestWorkshopDbProperties.DbSchema);

            b.ConfigureRoute();

            b.Property(p => p.Framework)
                .HasMaxLength(TestWorkshopConsts.MaxLength64)
                .HasColumnName(nameof(Menu.Framework))
                .IsRequired();
            b.Property(p => p.Component)
                .HasMaxLength(TestWorkshopConsts.MaxLength256)
                .HasColumnName(nameof(Menu.Component))
                .IsRequired();
            b.Property(p => p.Code)
                .HasMaxLength(TestWorkshopConsts.MaxCodeLength)
                .HasColumnName(nameof(Menu.Code))
                .IsRequired();
        });

        builder.Entity<RoleMenu>(b =>
        {
            b.ToTable(TestWorkshopDbProperties.DbTablePrefix + "RoleMenus", TestWorkshopDbProperties.DbSchema);

            b.Property(p => p.RoleName)
                .IsRequired()
                .HasMaxLength(TestWorkshopConsts.MaxLength256)
                .HasColumnName(nameof(RoleMenu.RoleName));

            b.ConfigureByConvention();

            b.HasIndex(i => new { i.RoleName, i.MenuId });
        });

        builder.Entity<UserMenu>(b =>
        {
            b.ToTable(TestWorkshopDbProperties.DbTablePrefix + "UserMenus", TestWorkshopDbProperties.DbSchema);

            b.ConfigureByConvention();

            b.HasIndex(i => new { i.UserId, i.MenuId });
        });

        builder.Entity<UserFavoriteMenu>(b =>
        {
            b.ToTable(TestWorkshopDbProperties.DbTablePrefix + "UserFavoriteMenus", TestWorkshopDbProperties.DbSchema);

            b.Property(p => p.Framework)
                .HasMaxLength(TestWorkshopConsts.MaxLength64)
                .HasColumnName(nameof(Menu.Framework))
                .IsRequired();
            b.Property(p => p.DisplayName)
                .HasMaxLength(TestWorkshopConsts.MaxLength128)
                .HasColumnName(nameof(Route.DisplayName))
                .IsRequired();
            b.Property(p => p.Name)
                .HasMaxLength(TestWorkshopConsts.MaxLength64)
                .HasColumnName(nameof(Route.Name))
                .IsRequired();
            b.Property(p => p.Path)
                .HasMaxLength(TestWorkshopConsts.MaxLength256)
                .HasColumnName(nameof(Route.Path))
                .IsRequired();

            b.Property(p => p.Icon)
                .HasMaxLength(TestWorkshopConsts.MaxLength512)
                .HasColumnName(nameof(UserFavoriteMenu.Icon));
            b.Property(p => p.Color)
                .HasMaxLength(TestWorkshopConsts.MaxLength64)
                .HasColumnName(nameof(UserFavoriteMenu.Color));
            b.Property(p => p.AliasName)
                .HasMaxLength(TestWorkshopConsts.MaxLength128)
                .HasColumnName(nameof(UserFavoriteMenu.AliasName));

            b.ConfigureByConvention();

            b.HasIndex(i => new { i.UserId, i.MenuId });
        });

        builder.Entity<Data>(b =>
        {
            b.ToTable(TestWorkshopDbProperties.DbTablePrefix + "Datas", TestWorkshopDbProperties.DbSchema);

            b.Property(p => p.Code)
                .HasMaxLength(TestWorkshopConsts.MaxLength1024)
                .HasColumnName(nameof(Data.Code))
                .IsRequired();
            b.Property(p => p.Name)
                .HasMaxLength(TestWorkshopConsts.MaxLength64)
                .HasColumnName(nameof(Data.Name))
                .IsRequired();
            b.Property(p => p.DisplayName)
               .HasMaxLength(TestWorkshopConsts.MaxLength128)
               .HasColumnName(nameof(Data.DisplayName))
               .IsRequired();
            b.Property(p => p.Description)
                .HasMaxLength(TestWorkshopConsts.MaxLength1024)
                .HasColumnName(nameof(Data.Description));

            b.ConfigureByConvention();

            b.HasMany(p => p.Items)
                .WithOne()
                .HasForeignKey(fk => fk.DataId)
                .IsRequired();

            b.HasIndex(i => new { i.Name });
        });

        builder.Entity<DataItem>(b =>
        {
            b.ToTable(TestWorkshopDbProperties.DbTablePrefix + "DataItems", TestWorkshopDbProperties.DbSchema);

            b.Property(p => p.DefaultValue)
                .HasMaxLength(TestWorkshopConsts.MaxLength128)
                .HasColumnName(nameof(DataItem.DefaultValue));
            b.Property(p => p.Name)
                .HasMaxLength(TestWorkshopConsts.MaxLength64)
                .HasColumnName(nameof(DataItem.Name))
                .IsRequired();
            b.Property(p => p.DisplayName)
               .HasMaxLength(TestWorkshopConsts.MaxLength128)
               .HasColumnName(nameof(DataItem.DisplayName))
               .IsRequired();
            b.Property(p => p.Description)
                .HasMaxLength(TestWorkshopConsts.MaxLength1024)
                .HasColumnName(nameof(DataItem.Description));

            b.Property(p => p.AllowBeNull).HasDefaultValue(true);

            b.ConfigureByConvention();

            b.HasIndex(i => new { i.Name });
        });
    }

    public static EntityTypeBuilder<TRoute> ConfigureRoute<TRoute>(this EntityTypeBuilder<TRoute> builder)
        where TRoute : Route
    {
        builder
            .Property(p => p.DisplayName)
            .HasMaxLength(TestWorkshopConsts.MaxLength128)
            .HasColumnName(nameof(Route.DisplayName))
            .IsRequired();
        builder
            .Property(p => p.Name)
            .HasMaxLength(TestWorkshopConsts.MaxLength64)
            .HasColumnName(nameof(Route.Name))
            .IsRequired();
        builder
            .Property(p => p.Path)
            .HasMaxLength(TestWorkshopConsts.MaxLength256)
            .HasColumnName(nameof(Route.Path));
        builder
            .Property(p => p.Redirect)
            .HasMaxLength(TestWorkshopConsts.MaxLength256)
            .HasColumnName(nameof(Route.Redirect));

        builder.ConfigureByConvention();

        return builder;
    }

}
