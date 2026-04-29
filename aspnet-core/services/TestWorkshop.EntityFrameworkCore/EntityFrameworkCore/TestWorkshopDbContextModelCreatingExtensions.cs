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
