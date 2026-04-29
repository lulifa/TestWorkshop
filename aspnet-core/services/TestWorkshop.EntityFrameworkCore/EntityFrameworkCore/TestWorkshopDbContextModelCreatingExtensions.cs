namespace TestWorkshop.EntityFrameworkCore;

public static class TestWorkshopDbContextModelCreatingExtensions
{

    public static void ConfigureTestWorkshop(this ModelBuilder builder)
    {

        builder.ConfigurePlatform();

    }

    public static void ConfigurePlatform(
        this ModelBuilder builder)
    {
        Check.NotNull(builder, nameof(builder));

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

        builder.Entity<RoleMenu>(x =>
        {
            x.ToTable(TestWorkshopDbProperties.DbTablePrefix + "RoleMenus", TestWorkshopDbProperties.DbSchema);

            x.Property(p => p.RoleName)
                .IsRequired()
                .HasMaxLength(TestWorkshopConsts.MaxLength256)
                .HasColumnName(nameof(RoleMenu.RoleName));

            x.ConfigureByConvention();

            x.HasIndex(i => new { i.RoleName, i.MenuId });
        });

        builder.Entity<UserMenu>(x =>
        {
            x.ToTable(TestWorkshopDbProperties.DbTablePrefix + "UserMenus", TestWorkshopDbProperties.DbSchema);

            x.ConfigureByConvention();

            x.HasIndex(i => new { i.UserId, i.MenuId });
        });

        builder.Entity<UserFavoriteMenu>(x =>
        {
            x.ToTable(TestWorkshopDbProperties.DbTablePrefix + "UserFavoriteMenus", TestWorkshopDbProperties.DbSchema);

            x.Property(p => p.Framework)
                .HasMaxLength(TestWorkshopConsts.MaxLength64)
                .HasColumnName(nameof(Menu.Framework))
                .IsRequired();
            x.Property(p => p.DisplayName)
                .HasMaxLength(TestWorkshopConsts.MaxLength128)
                .HasColumnName(nameof(Route.DisplayName))
                .IsRequired();
            x.Property(p => p.Name)
                .HasMaxLength(TestWorkshopConsts.MaxLength64)
                .HasColumnName(nameof(Route.Name))
                .IsRequired();
            x.Property(p => p.Path)
                .HasMaxLength(TestWorkshopConsts.MaxLength256)
                .HasColumnName(nameof(Route.Path))
                .IsRequired();

            x.Property(p => p.Icon)
                .HasMaxLength(TestWorkshopConsts.MaxLength512)
                .HasColumnName(nameof(UserFavoriteMenu.Icon));
            x.Property(p => p.Color)
                .HasMaxLength(TestWorkshopConsts.MaxLength64)
                .HasColumnName(nameof(UserFavoriteMenu.Color));
            x.Property(p => p.AliasName)
                .HasMaxLength(TestWorkshopConsts.MaxLength128)
                .HasColumnName(nameof(UserFavoriteMenu.AliasName));

            x.ConfigureByConvention();

            x.HasIndex(i => new { i.UserId, i.MenuId });
        });

        builder.Entity<Data>(x =>
        {
            x.ToTable(TestWorkshopDbProperties.DbTablePrefix + "Datas", TestWorkshopDbProperties.DbSchema);

            x.Property(p => p.Code)
                .HasMaxLength(TestWorkshopConsts.MaxLength1024)
                .HasColumnName(nameof(Data.Code))
                .IsRequired();
            x.Property(p => p.Name)
                .HasMaxLength(TestWorkshopConsts.MaxLength64)
                .HasColumnName(nameof(Data.Name))
                .IsRequired();
            x.Property(p => p.DisplayName)
               .HasMaxLength(TestWorkshopConsts.MaxLength128)
               .HasColumnName(nameof(Data.DisplayName))
               .IsRequired();
            x.Property(p => p.Description)
                .HasMaxLength(TestWorkshopConsts.MaxLength1024)
                .HasColumnName(nameof(Data.Description));

            x.ConfigureByConvention();

            x.HasMany(p => p.Items)
                .WithOne()
                .HasForeignKey(fk => fk.DataId)
                .IsRequired();

            x.HasIndex(i => new { i.Name });
        });

        builder.Entity<DataItem>(x =>
        {
            x.ToTable(TestWorkshopDbProperties.DbTablePrefix + "DataItems", TestWorkshopDbProperties.DbSchema);

            x.Property(p => p.DefaultValue)
                .HasMaxLength(TestWorkshopConsts.MaxLength128)
                .HasColumnName(nameof(DataItem.DefaultValue));
            x.Property(p => p.Name)
                .HasMaxLength(TestWorkshopConsts.MaxLength64)
                .HasColumnName(nameof(DataItem.Name))
                .IsRequired();
            x.Property(p => p.DisplayName)
               .HasMaxLength(TestWorkshopConsts.MaxLength128)
               .HasColumnName(nameof(DataItem.DisplayName))
               .IsRequired();
            x.Property(p => p.Description)
                .HasMaxLength(TestWorkshopConsts.MaxLength1024)
                .HasColumnName(nameof(DataItem.Description));

            x.Property(p => p.AllowBeNull).HasDefaultValue(true);

            x.ConfigureByConvention();

            x.HasIndex(i => new { i.Name });
        });
    }

    public static EntityTypeBuilder<TRoute> ConfigureRoute<TRoute>(
        this EntityTypeBuilder<TRoute> builder)
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
