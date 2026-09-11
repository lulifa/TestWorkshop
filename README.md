# TestWorkshop

测试车间远程管理系统，基于 ABP Framework 与 Vue Vben Admin 构建。

仓库包含完整的后端和前端源码：

- 后端：`aspnet-core`，ABP 分层架构 + OpenIddict 认证 + SignalR 实时通知
- 前端：`vben5/vue-vben-admin-5.6.0`，Vue 3 + Vite + Ant Design Vue + Vxe Table
- 数据库：PostgreSQL + TimescaleDB
- 缓存：Redis

## 技术栈

| 端   | 主要技术                                                                                         |
| ---- | ------------------------------------------------------------------------------------------------ |
| 后端 | .NET 9、ABP 9.3.5、EF Core、PostgreSQL、TimescaleDB、Redis、OpenIddict、SignalR、ABP BlobStoring |
| 前端 | Vue 3、Vite、TypeScript、Ant Design Vue、Vxe Table、Pinia、Tailwind CSS                          |
| 其他 | Swagger API 文档、ABP 权限体系、多租户能力、健康检查                                             |

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

- 车间设备管理：分页查询、筛选、新增、编辑、删除，维护设备编码、名称、类型和所属组织机构
- 遥测任务管理：CSV 上传、模拟数据上传、任务列表和统计、失败重试、删除、文件预览与下载
- 后台处理链路：上传文件后创建遥测任务，后台 Worker 定时认领任务，解析 CSV 并批量写入 TimescaleDB
- 数据存储：设备遥测写入 `WorkshopDeviceTelemetries` 超级表，按设备、时间和指标建立主键/索引
- 可靠性处理：任务处理中卡死自动恢复，失败任务指数退避重试，过期任务和物理文件定时清理
- 日志清理：审计日志和安全日志默认保留 365 天

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
│     │  └─ TimeScale/Workshop/               # 遥测数据、仓储和后台 Worker
│     └─ TestWorkshop.DbMigrator/             # 数据库迁移与种子数据
├─ vben5/vue-vben-admin-5.6.0/
│  ├─ apps/web-antd/              # 前端应用
│  └─ packages/                   # Vben 前端包源码
├─ docs/
│  ├─ current/                    # 基于当前代码生成的架构、数据流和部署图
│  └─ *.jpg                       # 早期架构/部署等历史图片
└─ README.md
```

## 环境要求

- .NET 9 SDK
- Node.js 20.19+，当前仓库推荐 Node.js 22.22.0
- pnpm 10.28.2
- PostgreSQL，并启用 TimescaleDB 扩展
- Redis
- 现代浏览器，推荐 Chrome

## 本地启动

### 1. 初始化数据库

先确保 PostgreSQL 已安装 TimescaleDB 扩展，并允许当前数据库用户创建扩展。然后修改下面两个项目中的连接字符串：

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

开发环境默认连接本机 `PostgreSQL:5432`，数据库名为 `TestWorkshop`。

执行数据库迁移和种子数据：

```bash
dotnet run --project aspnet-core/services/TestWorkshop.DbMigrator
```

迁移程序会创建业务表、ABP/Identity/OpenIddict 表，并写入默认管理员、角色、菜单等种子数据。

### 2. 启动后端

```bash
dotnet run --project aspnet-core/services/TestWorkshop.HttpApi.Host
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

首次启动时，`TestWorkshop.DbMigrator` 会根据 `appsettings.json` 创建 OpenIddict 客户端。如果修改了客户端 ID、回调地址或端口，需要重新执行迁移程序。

## 主要配置

### 后端

`aspnet-core/services/TestWorkshop.HttpApi.Host/appsettings.json`：

- `ConnectionStrings:Default`：PostgreSQL 连接字符串
- `Redis:IsEnabled`：是否启用 Redis，未安装时可设为 `false`
- `Redis:Configuration`：Redis 连接配置
- `Blob:Path`：文件物理存储目录，例如 Linux 下的 `/data/telemetry`，Windows 本地可改为 `D:\\data\\telemetry`，并确保目录可写
- `AuthServer:Authority`：认证中心地址
- `App:CorsOrigins`：前端跨域地址
- `App:VueUrl`：OIDC 登录完成后回跳的前端地址

`aspnet-core/services/TestWorkshop.DbMigrator/appsettings.json`：

- `OpenIddict:Applications`：Vue、Swagger、OAuth 客户端配置

### 前端

`vben5/vue-vben-admin-5.6.0/apps/web-antd/.env.development`：

- `VITE_PORT`：前端端口，默认 `4200`
- `VITE_GLOB_AUTHORITY`：认证中心地址，默认 `http://localhost:44349`
- `VITE_GLOB_CLIENT_ID`：OpenIddict 客户端 ID
- `VITE_GLOB_CLIENT_SECRET`：客户端密钥
- `VITE_UAPI_API_KEY`：工作台天气接口密钥，可选

生产环境还需要配置 `apps/web-antd/.env.production`，重点检查 API 地址、认证中心地址、OIDC 客户端 ID 和回调地址。不要把生产客户端密钥写入仓库。

### 开发检查

后端：

```bash
dotnet build aspnet-core/TestWorkshop.sln
```

前端：

```bash
cd vben5/vue-vben-admin-5.6.0
pnpm lint
pnpm check:type
pnpm test:unit
```

## 当前状态

| 模块                             | 状态                                                     |
| -------------------------------- | -------------------------------------------------------- |
| 认证、权限、用户、角色、组织机构 | 已实现                                                   |
| 菜单、布局、数据字典、文件管理   | 已实现                                                   |
| 消息、通告、SignalR 实时通知     | 已实现                                                   |
| 工作台、天气、收藏菜单           | 已实现                                                   |
| 审计日志、安全日志、租户管理     | 已实现                                                   |
| 车间设备管理、遥测任务           | 已实现，包含 CSV 上传、后台解析、重试和 TimescaleDB 写入 |
| 三方登录                         | 预留，未启用                                             |
| 待办事项                         | 占位                                                     |

## 其他说明

- 框架层已集成 ABP 多租户能力，当前登录端未启用租户选择，业务主要按主机/全局方式使用。
- 天气接口使用第三方 UAPI，未配置密钥时前端会降级展示默认天气。
- 消息、通告通过 SignalR 实时推送，同时持久化到数据库，右上角下拉和工作台会同步刷新。
- 开发配置中的连接字符串、客户端密钥、证书口令等仅用于本地环境，部署前必须替换。
- 遥测文件默认保存在后端配置的 `Blob:Path` 下，数据库只保存文件对象元数据；迁移或扩容时需要同步处理文件存储。

## 当前设计图

以下图片基于当前 `master` 代码生成，生成日期为 2026-09-11：

- [系统架构](docs/current/01-architecture.png)
- [遥测数据流](docs/current/02-telemetry-flow.png)
- [部署拓扑](docs/current/03-deployment.png)
- [版本与运行要求](docs/current/04-versions-and-requirements.png)

图片源文件和本地渲染脚本分别位于 `docs/current-project.html` 与 `docs/render-diagrams.mjs`。历史图片保留在 `docs` 根目录，不再作为当前实现依据。

## 许可证

[MIT](LICENSE)
