# TestWorkshop

测试车间远程管理系统，基于 ABP Framework 与 Vue Vben Admin 构建。

仓库包含完整的后端和前端源码：

- 后端：`aspnet-core`，ABP 分层架构 + OpenIddict 认证 + SignalR 实时通知
- 前端：`vben5/vue-vben-admin-5.6.0`，Vue 3 + Vite + Ant Design Vue + Vxe Table
- 数据库：PostgreSQL + TimescaleDB
- 缓存：Redis

## 技术栈

| 端 | 主要技术 |
| --- | --- |
| 后端 | .NET 9、ABP 9.3.5、EF Core、PostgreSQL、TimescaleDB、Redis、OpenIddict、SignalR、ABP BlobStoring |
| 前端 | Vue 3、Vite、TypeScript、Ant Design Vue、Vxe Table、Pinia、Tailwind CSS |
| 其他 | Swagger API 文档、ABP 权限体系、多租户能力、健康检查 |

## 已实现功能

### 认证与权限

- OpenIddict OAuth2/OIDC，支持密码模式、授权码、刷新令牌
- 本地用户名密码登录、扫码登录、验证码登录、忘记密码页面
- 用户、角色、权限、组织机构管理
- 角色和用户菜单分配、启动菜单配置
- 三方登录已预留页面，当前未启用

### 仪表盘 / 工作台

- 工作台欢迎页、天气信息（UAPI，可配置 `VITE_UAPI_API_KEY`）
- 常用菜单快捷导航、收藏菜单管理
- 消息中心和通告趋势展示
- 待办事项当前为占位实现

### 消息与通告

- 普通消息：单发、已读、批量已读、删除、我的消息列表
- 广播通告：全量发送、订阅、已读、批量已读、删除、我的通告列表
- 右上角消息下拉：消息 Tab 和通告 Tab，统一“全部已读 / 查看所有”
- SignalR 实时推送：`SignalR/Notification`，支持 `ReceiveTextMessage` 和 `ReceiveBroadCastMessage`
- 消息等级：Warning / Information / Error

### 平台管理

- 菜单管理：动态菜单、角色菜单、用户菜单、菜单属性配置
- 布局管理：Vben 布局约束、页面属性配置
- 数据字典：字典分组、字典项、静态字典项
- 文件管理：上传、批量上传、覆盖删除旧文件、下载、按业务对象管理
- 通告管理：创建广播通告、查看订阅和已读状态
- 消息管理：创建普通消息、批量已读、删除

### 系统管理

- 用户管理：新增、编辑、重置密码、锁定/解锁、分配角色和组织机构
- 角色管理：角色权限、菜单分配
- 组织机构：树形结构、角色/用户分配
- 权限管理：权限树配置
- 审计日志：操作审计、实体变更记录
- 安全日志：登录等安全事件
- 租户管理：租户、连接字符串（Host 侧）

### 车间设备与遥测

- 已建立 `WorkshopDevice` 设备实体，支持设备编码、类型、关联组织机构
- 已建立 `WorkshopTelemetryTask` 遥测任务状态机：待处理、处理中、成功、失败、重试、过期
- 车间设备管理页面当前为组件演示占位，业务 CRUD 和遥测任务接口待接入

## 目录结构

```text
TestWorkshop/
├─ aspnet-core/
│  ├─ frameworks/                 # 自定义框架：SignalR、Wrapper、Authorization 等
│  └─ services/
│     ├─ TestWorkshop.Domain/                 # 领域层
│     ├─ TestWorkshop.Domain.Shared/          # 领域共享层
│     ├─ TestWorkshop.Application/            # 应用层
│     ├─ TestWorkshop.Application.Contracts/  # 应用服务契约
│     ├─ TestWorkshop.HttpApi/                # API 层
│     ├─ TestWorkshop.HttpApi.Host/           # 宿主项目
│     ├─ TestWorkshop.EntityFrameworkCore/    # EF Core 数据层
│     └─ TestWorkshop.DbMigrator/             # 数据库迁移与种子数据
├─ vben5/vue-vben-admin-5.6.0/
│  ├─ apps/web-antd/              # 前端应用
│  └─ packages/                   # Vben 前端包源码
├─ docs/                          # 架构/部署/版本等图片文档
└─ README.md
```

## 环境要求

- .NET 9 SDK
- Node.js 20+
- pnpm 10+
- PostgreSQL，并启用 TimescaleDB 扩展
- Redis
- 现代浏览器，推荐 Chrome

## 本地启动

### 1. 初始化数据库

先修改连接字符串：

```json
{
  "ConnectionStrings": {
    "Default": "Host=localhost;Port=5432;Database=TestWorkshop;User ID=postgres;Password=123456;"
  }
}
```

涉及文件：

- `aspnet-core/services/TestWorkshop.HttpApi.Host/appsettings.json`
- `aspnet-core/services/TestWorkshop.DbMigrator/appsettings.json`

执行数据库迁移和种子数据：

```bash
cd aspnet-core/services/TestWorkshop.DbMigrator
dotnet run
```

### 2. 启动后端

```bash
cd aspnet-core/services/TestWorkshop.HttpApi.Host
dotnet run
```

启动后可访问：

- Swagger：`http://localhost:44349/swagger`
- 健康检查：`http://localhost:44349/health-status`

### 3. 启动前端

```bash
cd vben5/vue-vben-admin-5.6.0
pnpm install
pnpm dev:antd
```

前端地址：`http://localhost:4200`

默认本地账号（如未修改种子数据）：

```text
用户名：admin
密码：1q2w3E*
```

内置角色：`admin`、`supervisor`、`tester`、`auditor`、`guest`。

## 主要配置

### 后端

`aspnet-core/services/TestWorkshop.HttpApi.Host/appsettings.json`：

- `ConnectionStrings:Default`：PostgreSQL 连接字符串
- `Redis:IsEnabled`：是否启用 Redis，未安装时可设为 `false`
- `Redis:Configuration`：Redis 连接配置
- `Blob:Path`：文件物理存储目录，例如 `/data/telemetry`
- `AuthServer:Authority`：认证中心地址
- `App:CorsOrigins`：前端跨域地址

`aspnet-core/services/TestWorkshop.DbMigrator/appsettings.json`：

- `OpenIddict:Applications`：Vue、Swagger、OAuth 客户端配置

### 前端

`vben5/vue-vben-admin-5.6.0/apps/web-antd/.env.development`：

- `VITE_PORT`：前端端口，默认 `4200`
- `VITE_GLOB_AUTHORITY`：认证中心地址，默认 `http://localhost:44349`
- `VITE_GLOB_CLIENT_ID`：OpenIddict 客户端 ID
- `VITE_GLOB_CLIENT_SECRET`：客户端密钥
- `VITE_UAPI_API_KEY`：工作台天气接口密钥，可选

## 当前状态

| 模块 | 状态 |
| --- | --- |
| 认证、权限、用户、角色、组织机构 | 已实现 |
| 菜单、布局、数据字典、文件管理 | 已实现 |
| 消息、通告、SignalR 实时通知 | 已实现 |
| 工作台、天气、收藏菜单 | 已实现 |
| 审计日志、安全日志、租户管理 | 已实现 |
| 车间设备管理、遥测任务 | 实体和任务模型已建立，页面/接口待接入 |
| 三方登录 | 预留，未启用 |
| 待办事项 | 占位 |

## 其他说明

- 框架层已集成 ABP 多租户能力，当前登录端未启用租户选择，业务主要按主机/全局方式使用。
- 天气接口使用第三方 UAPI，未配置密钥时前端会降级展示默认天气。
- 消息、通告通过 SignalR 实时推送，同时持久化到数据库，右上角下拉和工作台会同步刷新。
