namespace TestWorkshop;

public class FileObject : FullAuditedAggregateRoot<Guid>, IMultiTenant
{

    /// <summary>
    /// 多租户 ID
    /// </summary>
    public virtual Guid? TenantId { get; protected set; }

    /// <summary>
    /// 物理存储的相对路径（核心！）
    /// 示例: "business/VesselImage/vsl-123/a1b2c3d4.jpg"
    /// 不包含 Blob:Path 根目录
    /// </summary>
    public virtual string BlobPath { get; protected set; }

    /// <summary>
    /// 文件原始名称（用于前端显示和下载）
    /// </summary>
    public virtual string FileName { get; protected set; }

    /// <summary>
    /// 文件大小（字节）
    /// </summary>
    public virtual long FileSize { get; protected set; }

    /// <summary>
    /// MIME 类型（如 image/jpeg, application/pdf）
    /// </summary>
    public virtual string ContentType { get; protected set; }

    /// <summary>
    /// 业务ID（核心！）
    /// 例如: Vessel 表的 Id, Unit 表的 Id, 用户的 Id
    /// </summary>
    public virtual string OwnerId { get; protected set; }

    /// <summary>
    /// 业务归属类型（核心！）
    /// 例如: "VesselImage", "UnitAttachment", "UserAvatar"
    /// </summary>
    public virtual string OwnerType { get; protected set; }

    protected FileObject() { }

    public FileObject(
        Guid id,
        string blobPath,
        string fileName,
        long fileSize,
        string contentType,
        string ownerId = null,
        string ownerType = null,
        Guid? tenantId = null)
        : base(id)
    {
        Check.NotNullOrWhiteSpace(blobPath, nameof(blobPath));
        Check.NotNullOrWhiteSpace(fileName, nameof(fileName));
        Check.NotNullOrWhiteSpace(contentType, nameof(contentType));

        BlobPath = blobPath;
        FileName = fileName;
        FileSize = fileSize;
        ContentType = contentType;
        OwnerId = ownerId;
        OwnerType = ownerType;
        TenantId = tenantId;
    }

    /// <summary>
    /// 更新文件大小（覆盖/替换场景）
    /// </summary>
    public void UpdateFileSize(long newSize)
    {
        FileSize = newSize;
    }

    /// <summary>
    /// 更新文件路径（数据迁移场景）
    /// </summary>
    public void UpdateBlobPath(string newPath)
    {
        BlobPath = newPath;
    }

}
