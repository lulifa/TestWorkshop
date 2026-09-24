# TestWorkshop 遥测（TimescaleDB）实现说明

本文档说明当前遥测上传、异步解析、超级表存储、保留策略和后台 Worker 的实现。后续调整字段或清理逻辑时，应同步更新本文档。

## 一、整体数据流

```text
下位机上传 CSV + WorkshopTelemetryFileInput
        ↓
AppService 校验设备和参数
        ↓
参数扁平化写入 FileObject.ExtraProperties
        ↓
创建 WorkshopTelemetryTask（Pending）
        ↓
WorkshopTelemetryWorker 认领并解析 CSV
        ↓
index,value 汇总为 double[] Value
        ↓
按 DeviceId + Timestamp + ChannelType 写入/更新超级表
        ↓
任务标记 Success 或 Failed
```

CSV 不再包含设备、通道、测试信息等列，文件内容只有：

```csv
0,20
1,20
2,20
3,20
```

`index` 必须从 `0` 开始连续递增。空行会被忽略，单文件最多解析 1,000,000 个采样点。

## 二、核心实体

### FileObject

物理 CSV 文件和文件级动态参数。

关键字段：

- `BlobPath`：文件物理存储路径
- `FileName`、`FileSize`、`ContentType`
- `ExtraProperties`：保存接口接收到的全部遥测参数

`ExtraProperties` 使用统一前缀 `TelemetryInput.`，例如：

```json
{
  "TelemetryInput.DeviceCode": "HD-FIVA-001",
  "TelemetryInput.ChannelType": "FivaFB",
  "TelemetryInput.FileTime": "2025-06-13 11:14:42",
  "TelemetryInput.TestInfo.StartTime": "2025-06-13 11:13:15",
  "TelemetryInput.TestInfo.EndTime": "2025-06-13 11:13:52",
  "TelemetryInput.TestInfo.TestDate": "2025-06-13"
}
```

简单参数即使没有值也会保留 key，值写为 `null`，便于参数详情页展示完整请求结构。

### WorkshopTelemetryTask

管理文件处理状态和任务生命周期。

状态：

```text
0 = Pending
1 = Processing
2 = Success
3 = Failed
```

主要时间：

- `CreatedAt`：任务创建时间
- `ProcessingStartedAt`：开始处理时间
- `ProcessedAt`：处理完成时间
- `NextRetryTime`：下一次重试时间
- `ExpiresAt`：任务和关联文件的统一过期时间

### WorkshopDeviceTelemetry

TimescaleDB 超级表，每个 CSV 文件、每个通道通常对应一行波形记录。

主键：

```text
DeviceId + Timestamp + ChannelType
```

字段说明：

| 字段 | 类型 | 说明 |
| --- | --- | --- |
| `DeviceId` | `uuid` | 采集设备，关联 `WorkshopDevice` |
| `TaskId` | `bigint` | 遥测任务 ID |
| `Timestamp` | `timestamp with time zone` | 来自 `FileTime`，同时是 hypertable 时间分区列 |
| `StartTime` | `timestamp without time zone` | 测试开始时间1，可空 |
| `EndTime` | `timestamp without time zone` | 测试结束时间1，可空 |
| `StartTime2` | `timestamp without time zone` | 测试开始时间2，可空 |
| `EndTime2` | `timestamp without time zone` | 测试结束时间2，可空 |
| `TestDate` | `timestamp without time zone` | 测试业务日期，可空 |
| `ChannelType` | `varchar(128)` | 通道类型，波形主键维度 |
| `Value` | `double precision[]` | 一个文件的完整波形数组 |
| `TestedDeviceCode` | `varchar(64)` | 被测设备编码 |
| `TestedDeviceName` | `varchar(128)` | 被测设备名称 |
| `PilotSN` | `varchar(64)` | 先导部件序列号 |
| `ShipName` | `varchar(128)` | 船名 |
| `TestBy` | `varchar(64)` | 测试人 |

时间语义：

- `Timestamp` 是带时区的时间点，来自必填参数 `FileTime`。
- 四个测试时间和 `TestDate` 来自下位机本地文本，按无时区时间保存。
- `TestDate` 虽然名字叫 Date，但接口可能传 `06/13/2025 00:00:00`，因此不使用 `date` 类型，也不使用 `DateOnly`。
- 当前 `FileTime` 如果未携带时区，会按 UTC 解析。若下位机传的是北京时间，建议传递 `+08:00` 偏移，或后续在 Worker 中明确配置业务时区进行转换。

## 三、EF Core 配置

超级表配置位于 `TestWorkshopDbContextModelCreatingExtensions.ConfigureTimeScale`：

```csharp
builder.Entity<WorkshopDeviceTelemetry>(b =>
{
    b.ToTable(
        TestWorkshopDbProperties.DbTablePrefix + "WorkshopDeviceTelemetries",
        TestWorkshopDbProperties.DbSchema);

    b.HasKey(x => new { x.DeviceId, x.Timestamp, x.ChannelType });

    b.Property(p => p.Timestamp)
        .HasColumnType("timestamp with time zone")
        .IsRequired();

    b.Property(p => p.StartTime)
        .HasColumnType("timestamp without time zone");

    b.Property(p => p.EndTime)
        .HasColumnType("timestamp without time zone");

    b.Property(p => p.StartTime2)
        .HasColumnType("timestamp without time zone");

    b.Property(p => p.EndTime2)
        .HasColumnType("timestamp without time zone");

    b.Property(p => p.TestDate)
        .HasColumnType("timestamp without time zone");

    b.Property(p => p.ChannelType)
        .HasMaxLength(TestWorkshopConsts.MaxLength128)
        .IsRequired();

    b.Property(p => p.Value)
        .HasColumnType("double precision[]")
        .IsRequired();

    b.HasIndex(x => new { x.TaskId, x.Timestamp });
});
```

## 四、TimescaleDB 初始化

在生成的迁移文件中，确保 `AppWorkshopDeviceTelemetries` 表已经创建，然后在 `Up` 方法末尾直接复制下面代码：

```csharp
migrationBuilder.Sql("""
    CREATE EXTENSION IF NOT EXISTS timescaledb;
    """);

migrationBuilder.Sql("""
    SELECT create_hypertable(
        '"AppWorkshopDeviceTelemetries"',
        'Timestamp',
        if_not_exists => TRUE
    );
    """);

migrationBuilder.Sql("""
    CREATE INDEX IF NOT EXISTS idx_device_time
    ON "AppWorkshopDeviceTelemetries" ("DeviceId", "Timestamp" DESC);
    """);
```

注意：`CREATE EXTENSION` 通常需要数据库管理员权限。如果扩展由 DBA 预装，可以删除第一条 `migrationBuilder.Sql`。

## 五、Worker 策略

### WorkshopTelemetryWorker

```text
扫描周期：5 秒
每次最多认领：5 个任务
单文件最大采样点：1,000,000
卡死判断：Processing 超过 10 分钟
卡死恢复后重试延迟：30 秒
```

处理原则：

- 先完成设备查找、文件读取和 CSV 解析。
- 数据库事务只覆盖最终 Upsert 和任务状态提交。
- 解析失败、未知设备、文件缺失等都会把任务标记为 Failed。
- 已存在相同 `DeviceId + Timestamp + ChannelType` 时执行 Upsert。
- `RecordCount` 保存的是 `Value` 数组长度，即采样点数。

### WorkshopTelemetryFileCleanupWorker

```text
执行周期：60 分钟
每批任务：100 个
单次最多处理：10 批
```

该 Worker 到期后删除：

1. 物理 CSV 文件
2. `FileObject` 记录
3. `WorkshopTelemetryTask` 记录

它不会删除 `WorkshopDeviceTelemetries` 超级表数据。

### LogCleanupBackgroundWorker

```text
执行周期：24 小时
审计日志保留：365 天
安全日志保留：365 天
```

日志保留与遥测文件保留是两套独立策略。

## 六、文件与任务保留策略

配置位置：`TestWorkshop.HttpApi.Host/appsettings.json`

```json
"WorkshopTelemetry": {
  "RetentionDays": 180,
  "FileCleanupIntervalMinutes": 60
}
```

含义：

- `RetentionDays`：遥测任务、FileObject 和原始 CSV 文件的统一保留天数。
- `FileCleanupIntervalMinutes`：过期文件清理频率。
- 新任务写入 `ExpiresAt = CreatedAt + RetentionDays`。
- 修改配置只影响新任务，已有任务仍使用数据库里已有的 `ExpiresAt`。

实际删除时间约为：

```text
RetentionDays 到 RetentionDays + FileCleanupIntervalMinutes
```

当前默认值下约为 180 天到 180 天零 1 小时。

## 七、超级表数据清理

任务清理不会删除超级表数据，因此超级表仍需单独制定归档和清理策略。

### 1. 查看当前存储量

```sql
SELECT pg_size_pretty(
    hypertable_size('"AppWorkshopDeviceTelemetries"')
);
```

### 2. 查看时间分块

```sql
SELECT
    chunk_name,
    range_start,
    range_end,
    is_compressed
FROM timescaledb_information.chunks
WHERE hypertable_name = 'AppWorkshopDeviceTelemetries'
ORDER BY range_start;
```

### 3. 归档指定时间范围

以下 SQL 会将 2026-01-01 到 2028-12-31 的数据导出为 CSV：

```sql
COPY (
    SELECT
        "DeviceId",
        "TaskId",
        "Timestamp",
        "StartTime",
        "EndTime",
        "StartTime2",
        "EndTime2",
        "TestDate",
        "ChannelType",
        "Value",
        "TestedDeviceCode",
        "TestedDeviceName",
        "PilotSN",
        "ShipName",
        "TestBy"
    FROM "AppWorkshopDeviceTelemetries"
    WHERE "Timestamp" >= TIMESTAMPTZ '2026-01-01'
      AND "Timestamp" <  TIMESTAMPTZ '2029-01-01'
    ORDER BY "Timestamp"
) TO '/backup/workshop-telemetry-2026-2028.csv'
WITH (FORMAT csv, HEADER true);
```

`COPY ... TO '/path'` 会把文件写到数据库服务器所在机器，并要求数据库账号拥有服务端文件写权限。如果只能从客户端落盘，可以在 `psql` 中使用 `\copy` 执行相同查询。

归档文件建议继续压缩并计算校验值：

```bash
zstd /backup/workshop-telemetry-2026-2028.csv
sha256sum /backup/workshop-telemetry-2026-2028.csv.zst
```

### 4. 校验归档数据量

```sql
SELECT
    COUNT(*) AS record_count,
    MIN("Timestamp") AS min_timestamp,
    MAX("Timestamp") AS max_timestamp
FROM "AppWorkshopDeviceTelemetries"
WHERE "Timestamp" >= TIMESTAMPTZ '2026-01-01'
  AND "Timestamp" <  TIMESTAMPTZ '2029-01-01';
```

确认导出文件可读取、记录数和时间范围正确后，再删除数据库数据。

### 5. 删除旧时间分块

```sql
SELECT drop_chunks(
    '"AppWorkshopDeviceTelemetries"',
    older_than => TIMESTAMPTZ '2029-01-01'
);
```

### 6. 删除后再次检查

```sql
SELECT pg_size_pretty(
    hypertable_size('"AppWorkshopDeviceTelemetries"')
);
```

如果只需要删除某个任务的数据：

```sql
DELETE FROM "AppWorkshopDeviceTelemetries"
WHERE "TaskId" = 123;
```

如果需要按非 chunk 边界精确删除：

```sql
DELETE FROM "AppWorkshopDeviceTelemetries"
WHERE "Timestamp" >= TIMESTAMPTZ '2026-01-01'
  AND "Timestamp" <  TIMESTAMPTZ '2027-01-01';
```

如果确认不再保留任何遥测数据：

```sql
TRUNCATE TABLE "AppWorkshopDeviceTelemetries";
```

不要直接依赖任务列表的删除按钮清理超级表；任务删除只处理任务和原始文件。

## 八、前端管理能力

遥测任务页面当前支持：

- 文件名、状态、创建时间范围搜索
- 单条和批量删除任务
- 失败任务重试
- CSV 文件预览
- 查看接口接收参数
- 将参数复制为格式化 JSON
- 展示设备、通道、FileTime、采样点数和错误信息

参数详情直接读取 `FileObject.ExtraProperties`，后续新增上传参数不需要修改数据库表结构。

## 九、设备与组织单元

- `OrganizationUnit` 表示车间或公司组织。
- `WorkshopDevice` 通过 `OrganizationUnitId` 归属车间。
- 遥测数据通过 `DeviceId` 关联采集设备。
- 设备型号、设备名等基础信息不重复存入超级表，通过 `DeviceId` 查询即可。
