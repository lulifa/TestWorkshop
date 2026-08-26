namespace TestWorkshop.EntityFrameworkCore;

public static class TestWorkshopDbContextModelCreatingExtensions
{

    public static void ConfigureTestWorkshop(this ModelBuilder builder)
    {
        Check.NotNull(builder, nameof(builder));

        builder.ConfigureBusiness();

        builder.ConfigurePlatform();

        // 默认先注释掉这个 迁移平台功能，后续在打开这个 注册timescale相关迁移及readme里边的操作
        builder.ConfigureTimeScale();

    }

    public static void ConfigureBusiness(this ModelBuilder builder)
    {
        builder.Entity<WorkshopDevice>(b =>
        {
            b.ToTable(TestWorkshopDbProperties.DbTablePrefix + "WorkshopDevices", TestWorkshopDbProperties.DbSchema);

            b.HasKey(x => x.Id);

            b.Property(p => p.Code)
                .HasMaxLength(TestWorkshopConsts.MaxLength64)
                .IsRequired();
            b.Property(p => p.Name)
                .HasMaxLength(TestWorkshopConsts.MaxLength128)
                .IsRequired();

            b.ConfigureByConvention();

            b.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
        });

        // ✅ WorkshopTelemetryTask 配置（清爽版）
        builder.Entity<WorkshopTelemetryTask>(b =>
        {
            b.ToTable(TestWorkshopDbProperties.DbTablePrefix + "WorkshopTelemetryTasks", TestWorkshopDbProperties.DbSchema);

            b.HasKey(x => x.Id);

            // 关联 FileObject 的外键
            b.Property(p => p.FileObjectId)
                .IsRequired()
                .HasComment("关联的 FileObject ID");

            // 业务状态
            b.Property(p => p.Status)
                .IsRequired()
                .HasComment("处理状态 (0=Pending 1=Processing 2=Success 3=Failed)");

            b.Property(p => p.RetryCount)
                .IsRequired()
                .HasDefaultValue(0)
                .HasComment("重试次数");

            b.Property(p => p.NextRetryTime)
                .HasColumnType("timestamp with time zone")
                .HasComment("下次重试时间");

            b.Property(p => p.Error)
                .HasMaxLength(2000)
                .HasComment("错误信息");

            b.Property(p => p.RecordCount)
                .HasComment("解析的记录数");

            // 时间字段
            b.Property(p => p.CreatedAt)
                .HasColumnType("timestamp with time zone")
                .IsRequired()
                .HasComment("创建时间");

            b.Property(p => p.ProcessedAt)
                .HasColumnType("timestamp with time zone")
                .HasComment("处理完成时间");

            b.Property(p => p.ProcessingStartedAt)
                .HasColumnType("timestamp with time zone")
                .HasComment("任务开始处理的时间，用于判断是否卡死");

            b.Property(p => p.ExpiresAt)
                .HasColumnType("timestamp with time zone")
                .IsRequired()
                .HasComment("过期时间");

            // 多租户
            b.Property(p => p.TenantId)
                .HasComment("租户ID");

            b.ConfigureByConvention();

            b.HasIndex(x => x.FileObjectId);

            b.HasIndex(x => new { x.Status, x.CreatedAt });

            b.HasIndex(x => new { x.ExpiresAt, x.Status });

            b.HasIndex(x => x.CreatedAt);

        });
    }

    public static void ConfigureTimeScale(this ModelBuilder builder)
    {
        builder.Entity<WorkshopDeviceTelemetry>(b =>
        {
            b.ToTable(TestWorkshopDbProperties.DbTablePrefix + "WorkshopDeviceTelemetries", TestWorkshopDbProperties.DbSchema);

            b.HasKey(x => new { x.DeviceId, x.Timestamp, x.MetricType });

            b.Property(p => p.MetricType)
                .IsRequired();

            b.Property(p => p.Timestamp)
                .HasColumnType("timestamp with time zone")
                .IsRequired();

            b.Property(p => p.DeviceId).IsRequired();
            b.Property(p => p.Value).IsRequired();

            // 被测试产品（可空，因为不是所有采集都有产品）
            b.Property(p => p.TestedDeviceCode).HasMaxLength(64);
            b.Property(p => p.TestedDeviceName).HasMaxLength(128);

            b.ConfigureByConvention();
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

        builder.Entity<Notification>(b =>
        {
            b.ToTable(TestWorkshopDbProperties.DbTablePrefix + "Notifications", TestWorkshopDbProperties.DbSchema);

            // 租户id
            b.Property(e => e.TenantId)
                .HasComment("租户id");

            // 消息标题 - 必填，最大长度128
            b.Property(e => e.Title)
                .IsRequired()
                .HasMaxLength(TestWorkshopConsts.MaxLength128)
                .HasComment("消息标题");

            // 消息内容 - 必填，最大长度1024
            b.Property(e => e.Content)
                .IsRequired()
                .HasMaxLength(TestWorkshopConsts.MaxLength1024)
                .HasComment("消息内容");

            // 消息类型
            b.Property(e => e.MessageType)
                .HasComment("消息类型");

            // 消息等级
            b.Property(e => e.MessageLevel)
                .HasComment("消息等级");

            // 发送人用户Id - 必填
            b.Property(e => e.SenderUserId)
                .HasComment("发送人用户Id");

            // 发送人用户名 - 必填，最大长度128
            b.Property(e => e.SenderUserName)
                .IsRequired()
                .HasMaxLength(TestWorkshopConsts.MaxLength128)
                .HasComment("发送人用户名");

            // 接收人用户Id
            b.Property(e => e.ReceiveUserId)
                .HasComment("接收人用户Id");

            // 接收人用户名 - 最大长度128
            b.Property(e => e.ReceiveUserName)
                .HasMaxLength(TestWorkshopConsts.MaxLength128)
                .HasComment("接收人用户名");

            // 是否已读 - 必填
            b.Property(e => e.Read)
                .IsRequired()
                .HasComment("是否已读");

            // 已读时间
            b.Property(e => e.ReadTime)
                .HasComment("已读时间");

            b.ConfigureByConvention();

        });

        builder.Entity<NotificationSubscription>(b =>
        {
            b.ToTable(TestWorkshopDbProperties.DbTablePrefix + "NotificationSubscriptions", TestWorkshopDbProperties.DbSchema);

            // 租户id
            b.Property(e => e.TenantId)
                .HasComment("租户id");

            // 消息Id - 必填
            b.Property(e => e.NotificationId)
                .IsRequired()
                .HasComment("消息Id");

            // 接收人用户Id - 必填
            b.Property(e => e.ReceiveUserId)
                .IsRequired()
                .HasComment("接收人用户Id");

            // 接收人用户名 - 最大长度128
            b.Property(e => e.ReceiveUserName)
                .HasMaxLength(TestWorkshopConsts.MaxLength128)
                .HasComment("接收人用户名");

            // 是否已读 - 必填
            b.Property(e => e.Read)
                .IsRequired()
                .HasComment("是否已读");

            // 已读时间 - 必填
            b.Property(e => e.ReadTime)
                .IsRequired()
                .HasComment("已读时间");

            b.ConfigureByConvention();

            b.HasIndex(e => e.NotificationId);

            b.HasIndex(e => e.ReceiveUserId);

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
