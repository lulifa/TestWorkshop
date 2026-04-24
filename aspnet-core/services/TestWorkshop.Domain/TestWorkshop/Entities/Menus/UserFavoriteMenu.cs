namespace TestWorkshop;

public class UserFavoriteMenu : AuditedEntity<Guid>, IMultiTenant
{
    public virtual Guid? TenantId { get; protected set; }

    public virtual Guid MenuId { get; protected set; }

    public virtual Guid UserId { get; protected set; }

    public virtual string AliasName { get; set; }

    public virtual string Color { get; set; }

    public virtual string Framework { get; set; }

    public virtual string Name { get; set; }

    public virtual string DisplayName { get; set; }

    public virtual string Path { get; set; }

    public virtual string Icon { get; set; }

    protected UserFavoriteMenu() { }
    public UserFavoriteMenu(
        Guid id,
        Guid menuId,
        Guid userId,
        string framework,
        string name,
        string displayName,
        string path,
        string icon,
        string color,
        string aliasName = null,
        Guid? tenantId = null)
        : base(id)
    {
        MenuId = menuId;
        UserId = userId;
        Framework = Check.NotNullOrWhiteSpace(framework, nameof(framework), TestWorkshopConsts.MaxLength64);
        Name = Check.NotNullOrWhiteSpace(name, nameof(name), TestWorkshopConsts.MaxLength64);
        DisplayName = Check.NotNullOrWhiteSpace(displayName, nameof(displayName), TestWorkshopConsts.MaxLength128);
        Path = Check.NotNullOrWhiteSpace(path, nameof(path), TestWorkshopConsts.MaxLength256);
        Icon = Check.Length(icon, nameof(icon), TestWorkshopConsts.MaxLength512);
        Color = Check.Length(color, nameof(color), TestWorkshopConsts.MaxLength64);
        AliasName = Check.Length(aliasName, nameof(aliasName), TestWorkshopConsts.MaxLength128);
        TenantId = tenantId;
    }
}
