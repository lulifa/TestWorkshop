# 订单与遥测数据关联方案讨论汇总

## 一、背景

系统目前有下位机上传的遥测数据，主要涉及两张表：

1. `WorkshopTelemetryTask`
   - 表示一个 CSV 文件对应的遥测处理任务
   - 当前主要字段包括：`Id`、`FileObjectId`、`Status`、`CreatedAt`、`ProcessedAt`、`ExpiresAt`
   - 当前没有 `OrderId`

2. `WorkshopDeviceTelemetry`
   - 表示解析后的实际遥测波形数据
   - 当前主要字段包括：`DeviceId`、`TaskId`、`Timestamp`、`StartTime`、`EndTime`、`StartTime2`、`EndTime2`、`ChannelType`、`Value`
   - 当前通过 `TaskId` 关联 `WorkshopTelemetryTask`

下位机上传数据时，数据中没有订单号，但有：

- `DeviceCode`
- `FileTime`
- `StartTime`
- `EndTime`
- `StartTime2`
- `EndTime2`
- `ChannelType`
- 被测设备信息等

业务现在的核心需求是：

> 遥测任务和遥测数据必须归属某个订单。没有明确订单归属的数据视为非法数据。

## 二、业务方确认的规则

业务方给出的规则可以归纳为：

1. 调度人员先在系统中手工录入订单。
2. 车间按照订单号开始生产或测试。
3. 数据要关联到订单。
4. 不使用订单的创建时间 `CreatedAt` 做关联。
5. 使用订单实际的开始测试时间。
6. 订单开始时间与开始测试时间一致。
7. 一个订单关联一台设备。
8. 同一台设备同时只能有一个订单处于测试中。

## 三、核心问题

下位机数据中没有订单号，那么系统如何确定某条遥测数据属于哪个订单？

初步结论：

- 订单号是解决关联问题最直接的方式。
- 如果下位机不能传订单号，则必须有等价的关联依据。
- 等价的关联依据可以是：
  - `DeviceCode`
  - 订单关联的设备
  - 订单的实际测试开始时间
  - 订单的实际测试结束时间
  - 上传数据中的测试时间

不能只使用订单创建时间或数据上传时间进行关联。

## 四、当前建议的数据模型

在没有以下复杂场景的前提下：

- 一个订单只关联一台设备
- 同一设备同一时间只执行一个订单
- 一个订单只有一次连续测试

可以只设计一张 `Order` 表，不需要额外的 `TestSession` 表。

建议订单表字段：

```text
Order
  Id
  OrderNo
  DeviceId
  Status
  TestStartedAt
  TestEndedAt
```

建议状态：

```text
Draft       已录入，未开始
Testing     测试中，占用设备
Completed   已完成，释放设备
Cancelled   已取消，释放设备
```

建议 `WorkshopTelemetryTask` 增加：

```text
OrderId
```

关系为：

```text
Order
  1 -> N WorkshopTelemetryTask
      1 -> N WorkshopDeviceTelemetry
```

实际外键关系：

```text
WorkshopTelemetryTask.OrderId
  -> Order.Id

WorkshopDeviceTelemetry.TaskId
  -> WorkshopTelemetryTask.Id
```

当前不一定需要在 `WorkshopDeviceTelemetry` 中再增加 `OrderId`。可以通过 `TaskId` 查询到任务，再通过任务的 `OrderId` 找到订单。

如果后续需要按订单高频查询大量波形数据，可以考虑在 `WorkshopDeviceTelemetry` 中冗余 `OrderId` 并增加索引，但订单归属的权威来源仍然是 `WorkshopTelemetryTask.OrderId`。

## 五、订单测试流程

### 1. 新建订单

调度人员在订单管理界面录入：

```text
订单号
关联设备
订单基本信息
```

此时订单状态为 `Draft`：

```text
TestStartedAt = null
TestEndedAt = null
```

### 2. 点击开始测试

开始测试时：

```text
校验该设备是否已经存在 Testing 状态订单
设置 DeviceId
设置 Status = Testing
设置 TestStartedAt = 当前时间
TestEndedAt = null
```

同一设备同时只能有一个 `Testing` 状态订单。

### 3. 下位机上传数据

数据到达后：

```text
根据 DeviceCode 找到设备
根据设备找到测试中的订单
根据数据时间判断是否属于该订单
```

由于 `TestEndedAt` 可能还没有值，测试中的订单时间区间可以视为：

```text
[TestStartedAt, +∞)
```

或者使用：

```text
[TestStartedAt, 当前时间 + 少量容差]
```

### 4. 点击结束测试

结束测试时：

```text
Status = Completed
TestEndedAt = 当前时间
释放设备
```

### 5. 查看订单数据

订单详情页通过以下链路展示数据：

```text
Order
 -> WorkshopTelemetryTask
 -> WorkshopDeviceTelemetry
 -> 波形数据
```

订单列表页不建议直接加载完整 `Value` 数组。应先展示任务摘要，用户进入任务详情后再加载波形。

## 六、数据归属判断规则

建议条件：

```text
上传数据 DeviceCode == 订单关联设备 DeviceCode

并且

数据 StartTime >= Order.TestStartedAt

并且

数据 EndTime2 <= Order.TestEndedAt
```

当订单尚未结束时：

```text
TestEndedAt 为空
```

此时只能校验：

```text
data.StartTime >= Order.TestStartedAt
data.EndTime2 <= 当前时间 + 容差
```

订单结束后，再使用完整的开始和结束时间进行校验。

如果 `StartTime` 或 `EndTime2` 缺失，可以考虑使用：

```text
FileTime
EndTime
StartTime2
```

但业务规则需要明确优先级。

## 七、并发和数据库约束

仅靠界面校验或服务端查询不能完全防止并发问题。必须在数据库增加约束：

```text
同一 TenantId + DeviceId 下，最多只能存在一个 Status = Testing 的订单
```

PostgreSQL 可以使用部分唯一索引，概念如下：

```text
UNIQUE (TenantId, DeviceId)
WHERE Status = Testing
```

这样可以保证：

```text
已完成订单不占用设备
草稿订单不占用设备
测试中订单占用设备
同一设备不能同时有两个测试中订单
```

创建和更新时仍然应该做业务校验，用于给用户即时反馈，但数据库约束才是最终保障。

## 八、订单更新规则建议

```text
Draft：
  允许修改订单业务字段和设备

Testing：
  禁止修改 DeviceId
  禁止重复开始测试
  如需更换设备，必须先结束或取消当前测试

Completed：
  不允许修改测试字段
  如需重新测试，建议新建订单

Cancelled：
  不允许直接开始
```

## 九、订单管理界面建议

订单列表页：

- 新建订单
- 编辑订单
- 开始测试
- 结束测试
- 查看详情

订单详情页：

- 订单基本信息
- 关联设备
- 测试状态
- 测试开始时间
- 测试结束时间
- 遥测任务列表
- 遥测波形数据

设备选择时显示占用状态：

```text
D001  空闲
D002  测试中，当前订单 O100
D003  停用
```

用户不需要手工给订单选择 `WorkshopDeviceTelemetry` 记录。系统在数据上传时自动关联。

## 十、当前仍需要确认的问题

1. 下位机上传请求中是否真的完全没有 `OrderNo` 字段？
2. 订单是否可能在同一台设备上测试多次？
3. 一个订单是否可能关联多台设备？
4. 下位机数据是否可能延迟到订单结束后才上传？
5. 下位机的 `StartTime`、`EndTime2` 是否与服务器时间同一时区？
6. 下位机时钟是否能够保持与服务器时间同步？
7. 数据时间缺失时使用哪个字段作为回退？
8. 测试开始按钮点击后，下位机是否一定在开始时间之后产生数据？
9. 操作人员忘记点击结束测试时如何处理？
10. 数据匹配不到订单时是直接拒绝，还是进入人工处理？

## 十一、希望 DeepSeek 重点评审的问题

请从数据库设计、业务一致性、并发控制和数据关联可靠性角度评审以下方案：

1. 一个订单只关联一台设备，一个设备同时只能有一个测试中订单时，是否真的不需要 `TestSession` 表？
2. `Order` 表直接保存 `DeviceId`、`Status`、`TestStartedAt`、`TestEndedAt` 是否合理？
3. `TestEndedAt` 为空时，用开放时间区间接收数据是否可靠？
4. 仅通过设备编码和时间区间匹配订单，是否存在严重隐患？
5. 是否应该在 `WorkshopTelemetryTask` 中增加 `OrderId`？
6. 是否应该在 `WorkshopDeviceTelemetry` 中直接增加 `OrderId`，还是通过 `TaskId` 关联即可？
7. 同一设备只允许一个测试中订单，使用 PostgreSQL 部分唯一索引是否是最佳方案？
8. 延迟上传、重复上传和时钟漂移场景下，应该如何处理？
9. 如果数据匹配不到订单，应该拒绝还是人工归属？
10. 如果业务后续支持一个订单多次测试或一个订单多台设备，应该如何平滑演进？

