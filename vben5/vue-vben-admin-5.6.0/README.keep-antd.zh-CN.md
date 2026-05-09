# Vben5 精简为仅保留 Ant Design Vue 指南

这份文档适用于当前这个 `vue-vben-admin-5.6.0` 仓库，目标是：

- 只保留 `apps/web-antd`
- 删除其他 UI 版本应用
- 按需删除 `docs`、`playground`、`backend-mock`
- 把仓库收敛成更适合你自己业务开发的骨架

## 先说结论

如果你只是想尽快开始业务开发，最稳妥的做法是：

1. 只删除其他 UI 应用目录
2. 保留 `packages/*`、`internal/*`、`scripts/*`
3. 按需删除 `docs`、`playground`
4. 如果不用内置 mock，再删除 `apps/backend-mock`
5. 修改根目录脚本、开发代理、部署脚本
6. 最后再删 `web-antd` 内的 demo 页面和演示路由

不要一上来删 `packages` 和 `internal`。  
`web-antd` 的大量能力都依赖这些共享包。

## 当前仓库里和 UI 应用有关的目录

当前 `apps` 目录下有：

- `apps/web-antd`
- `apps/web-antdv-next`
- `apps/web-ele`
- `apps/web-naive`
- `apps/web-tdesign`
- `apps/backend-mock`

如果你只保留 Ant Design Vue，必删的是：

- `apps/web-antdv-next`
- `apps/web-ele`
- `apps/web-naive`
- `apps/web-tdesign`

可选删除的是：

- `apps/backend-mock`
- `docs`
- `playground`

## 推荐精简顺序

### 第 0 步：先留一个可回退点

建议先做一件事：

- 新建一个 git 分支，或者复制一份仓库备份

比如：

```powershell
git checkout -b chore/keep-only-antd
```

如果你当前目录不是 git 仓库，也至少先复制一个目录备份。

### 第 1 步：删除其他 UI 应用

先只删其他 UI 应用，不碰共享包。

PowerShell 示例：

```powershell
Remove-Item -Recurse -Force `
  apps/web-antdv-next, `
  apps/web-ele, `
  apps/web-naive, `
  apps/web-tdesign
```

手动删除也可以。

### 第 2 步：决定是否保留 docs、playground、mock

你可以按下面的原则判断：

- `docs`：如果你不需要本地文档站点，可以删
- `playground`：如果你不需要官方演示工程，可以删
- `apps/backend-mock`：如果你不用内置 mock 服务，可以删

PowerShell 示例：

```powershell
Remove-Item -Recurse -Force docs, playground
```

如果你也不用 mock：

```powershell
Remove-Item -Recurse -Force apps/backend-mock
```

## 哪些目录不要删

下面这些通常都要保留：

- `apps/web-antd`
- `internal/*`
- `packages/*`
- `scripts/*`
- 根目录的各种配置文件

特别说明：

- `packages/*` 不是“多余示例代码”，而是 `web-antd` 的共享能力层
- `internal/*` 里有 `vite-config`、`tsconfig`、eslint 配置等基础设施
- `scripts/*` 里有仓库自己的工具脚本

## 第 3 步：修改根目录 `package.json`

当前根目录 [package.json](/d:/ProCore/TestWorkshop/vben5/vue-vben-admin-5.6.0/package.json) 里还有很多多应用脚本：

- `build:docs`
- `build:ele`
- `build:naive`
- `build:tdesign`
- `build:play`
- `dev:antdv-next`
- `dev:docs`
- `dev:ele`
- `dev:naive`
- `dev:tdesign`
- `dev:play`

这些在你删完目录后，建议一起删掉。

### 推荐改法

把根脚本收敛成只面向 `web-antd`：

```json
{
  "scripts": {
    "dev": "pnpm dev:antd",
    "dev:antd": "pnpm -F @vben/web-antd run dev",
    "build": "pnpm build:antd",
    "build:antd": "pnpm run build --filter=@vben/web-antd",
    "preview": "pnpm -F @vben/web-antd run preview",
    "typecheck": "pnpm -F @vben/web-antd run typecheck",
    "lint": "vsh lint",
    "format": "vsh lint --format"
  }
}
```

### 哪些脚本可以继续保留

这些通常可以保留：

- `lint`
- `format`
- `check`
- `check:type`
- `check:dep`
- `test:unit`

只是它们的检查范围会变成你剩下的工作区内容。

## 第 4 步：修改 `pnpm-workspace.yaml`

当前 [pnpm-workspace.yaml](/d:/ProCore/TestWorkshop/vben5/vue-vben-admin-5.6.0/pnpm-workspace.yaml) 里是大范围工作区：

- `apps/*`
- `docs`
- `playground`

### 最稳妥的改法

把它改成“显式列出你要保留的包”，这样最清楚。

如果你保留 mock：

```yaml
packages:
  - internal/*
  - internal/lint-configs/*
  - packages/*
  - packages/@core/base/*
  - packages/@core/ui-kit/*
  - packages/@core/forward/*
  - packages/@core/*
  - packages/effects/*
  - packages/business/*
  - apps/web-antd
  - apps/backend-mock
  - scripts/*
```

如果你不保留 mock：

```yaml
packages:
  - internal/*
  - internal/lint-configs/*
  - packages/*
  - packages/@core/base/*
  - packages/@core/ui-kit/*
  - packages/@core/forward/*
  - packages/@core/*
  - packages/effects/*
  - packages/business/*
  - apps/web-antd
  - scripts/*
```

### 是否一定要改

不是绝对必须。  
就算你保留 `apps/*`，只要目录已经删掉，通常也能工作。

但从“长期维护”和“防止误加应用”的角度，建议改成显式列表。

## 第 5 步：如果删除 `backend-mock`，一定要同时改 API 配置

这一步非常关键。

当前 [apps/web-antd/vite.config.mts](/d:/ProCore/TestWorkshop/vben5/vue-vben-admin-5.6.0/apps/web-antd/vite.config.mts) 里，开发代理固定写的是：

- `/api` -> `http://localhost:5320/api`

也就是说：

- 即使你删了 `apps/backend-mock`
- 只要你没改这个代理
- 登录、菜单、用户信息请求还是会继续打到 `5320`

这就是最容易出现的“删完 mock 还是报错”的原因。

### 方案 A：你有本地后端，继续用 `/api` 代理

这种情况最推荐。

1. 修改 [apps/web-antd/.env.development](/d:/ProCore/TestWorkshop/vben5/vue-vben-admin-5.6.0/apps/web-antd/.env.development)

```env
VITE_GLOB_API_URL=/api
VITE_NITRO_MOCK=false
```

2. 修改 [apps/web-antd/vite.config.mts](/d:/ProCore/TestWorkshop/vben5/vue-vben-admin-5.6.0/apps/web-antd/vite.config.mts)

把：

```ts
target: 'http://localhost:5320/api',
```

改成你的真实后端，例如：

```ts
target: 'http://localhost:8080/api',
```

这样前端仍然请求 `/api/auth/login`，但会被 Vite 代理到你的本地后端。

### 方案 B：开发环境直接请求完整后端地址

如果你不想走 Vite 代理，也可以把 [apps/web-antd/.env.development](/d:/ProCore/TestWorkshop/vben5/vue-vben-admin-5.6.0/apps/web-antd/.env.development) 改成：

```env
VITE_GLOB_API_URL=http://localhost:8080/api
VITE_NITRO_MOCK=false
```

然后删除 `vite.config.mts` 里的 `/api` 代理配置。

这种方式要注意：

- 后端必须处理 CORS
- 本地跨域问题要自己承担

### 生产环境也要一起改

当前 [apps/web-antd/.env.production](/d:/ProCore/TestWorkshop/vben5/vue-vben-admin-5.6.0/apps/web-antd/.env.production) 默认还是：

```env
VITE_GLOB_API_URL=https://mock-napi.vben.pro/api
```

如果你要接自己的后端，记得一起改成你自己的生产 API 地址，例如：

```env
VITE_GLOB_API_URL=https://api.your-company.com/api
```

## 第 6 步：修改 `turbo.json`

当前 [turbo.json](/d:/ProCore/TestWorkshop/vben5/vue-vben-admin-5.6.0/turbo.json) 里有一段：

```json
"@vben/backend-mock#build": {
  "dependsOn": ["^build"],
  "outputs": [".nitro/**", ".output/**"]
}
```

如果你已经删掉 `apps/backend-mock`，这段建议也删掉。

如果你保留 mock，这段不用动。

## 第 7 步：修改 IDE 工作区文件

当前 [vben-admin.code-workspace](/d:/ProCore/TestWorkshop/vben5/vue-vben-admin-5.6.0/vben-admin.code-workspace) 里还包含：

- `@vben/backend-mock`
- `@vben/web-antdv-next`
- `@vben/web-ele`
- `@vben/web-naive`
- `@vben/web-tdesign`
- `@vben/docs`
- `@vben/playground`

如果你已经删掉这些目录，建议把这些 `folders` 条目一起删掉，不然 IDE 工作区里会残留无效路径。

## 第 8 步：如果你用 Docker，修改部署脚本

当前 [scripts/deploy/Dockerfile](/d:/ProCore/TestWorkshop/vben5/vue-vben-admin-5.6.0/scripts/deploy/Dockerfile) 最后复制的是：

```dockerfile
COPY --from=builder /app/playground/dist /usr/share/nginx/html
```

如果你删了 `playground`，这里必须改。

### 推荐改法

把构建和复制目标都改成 `web-antd`：

```dockerfile
RUN pnpm run build:antd
COPY --from=builder /app/apps/web-antd/dist /usr/share/nginx/html
```

否则 Docker 构建会直接失败。

## 第 9 步：如果你用 GitHub Actions，修改工作流

当前 [.github/workflows/deploy.yml](/d:/ProCore/TestWorkshop/vben5/vue-vben-admin-5.6.0/.github/workflows/deploy.yml) 里还有这些部署任务：

- `deploy-playground-ftp`
- `deploy-docs-ftp`
- `deploy-antd-ftp`
- `deploy-ele-ftp`
- `deploy-naive-ftp`

如果你已经删了：

- `playground`
- `docs`
- `web-ele`
- `web-naive`

那这些对应 job 也应该删掉，只保留 `deploy-antd-ftp` 或者直接按你的实际部署方式重写。

## 第 10 步：重新安装依赖，不要手改 lock 文件

删除目录和修改工作区后，不建议手动改 `pnpm-lock.yaml`。

正确顺序是：

```powershell
pnpm install
```

让 pnpm 自动重新收敛锁文件。

如果安装过程中觉得残留太多，也可以在确认无误后再清理一次依赖缓存：

```powershell
Remove-Item -Recurse -Force node_modules
pnpm install
```

## 第 11 步：验证精简结果

建议至少执行下面几个命令：

```powershell
pnpm dev:antd
pnpm -F @vben/web-antd run typecheck
pnpm lint
```

如果你已经删除了 mock，又改成了自己的后端，还要重点验证：

- 登录是否正常
- 获取用户信息是否正常
- 获取菜单是否正常
- 页面刷新后 token 和路由守卫是否正常

## 第 12 步：开始精简 `web-antd` 内部业务骨架

前面只是“删多应用”。  
如果你还想把 `web-antd` 本身精简成业务骨架，可以继续做下面这些。

### 1. 删除 demo 路由模块

当前 [apps/web-antd/src/router/routes](/d:/ProCore/TestWorkshop/vben5/vue-vben-admin-5.6.0/apps/web-antd/src/router/routes/index.ts) 会通过 `import.meta.glob('./modules/**/*.ts')` 自动加载：

- `modules/dashboard.ts`
- `modules/demos.ts`
- `modules/vben.ts`

如果你不需要这些演示模块，可以直接删掉对应路由文件。

通常最先删的是：

- `apps/web-antd/src/router/routes/modules/demos.ts`
- `apps/web-antd/src/router/routes/modules/vben.ts`

如果连默认看板也不要，再删：

- `apps/web-antd/src/router/routes/modules/dashboard.ts`

### 2. 删除对应页面目录

删了路由后，再删页面：

- `apps/web-antd/src/views/demos`
- `apps/web-antd/src/views/dashboard`

如果你删了 `vben.ts`，再根据那个路由文件里引用的页面继续删对应目录。

### 3. 精简认证页

当前 [apps/web-antd/src/router/routes/core.ts](/d:/ProCore/TestWorkshop/vben5/vue-vben-admin-5.6.0/apps/web-antd/src/router/routes/core.ts) 默认保留了：

- `login`
- `code-login`
- `qrcode-login`
- `forget-password`
- `register`

如果你只要最基础的账号密码登录，建议在 `core.ts` 里删除：

- `CodeLogin`
- `QrCodeLogin`
- `ForgetPassword`
- `Register`

然后再删对应页面：

- `apps/web-antd/src/views/_core/authentication/code-login.vue`
- `apps/web-antd/src/views/_core/authentication/qrcode-login.vue`
- `apps/web-antd/src/views/_core/authentication/forget-password.vue`
- `apps/web-antd/src/views/_core/authentication/register.vue`

一般要保留：

- `apps/web-antd/src/views/_core/authentication/login.vue`
- `apps/web-antd/src/views/_core/fallback/not-found.vue`

### 4. 替换最核心的业务接口

真正接你自己业务时，最先要改的通常是这几个文件：

- [apps/web-antd/src/api/core/auth.ts](/d:/ProCore/TestWorkshop/vben5/vue-vben-admin-5.6.0/apps/web-antd/src/api/core/auth.ts)
- [apps/web-antd/src/api/core/user.ts](/d:/ProCore/TestWorkshop/vben5/vue-vben-admin-5.6.0/apps/web-antd/src/api/core/user.ts)
- [apps/web-antd/src/api/core/menu.ts](/d:/ProCore/TestWorkshop/vben5/vue-vben-admin-5.6.0/apps/web-antd/src/api/core/menu.ts)
- [apps/web-antd/src/store/auth.ts](/d:/ProCore/TestWorkshop/vben5/vue-vben-admin-5.6.0/apps/web-antd/src/store/auth.ts)

对应关系大致是：

- `auth.ts`：登录、登出、刷新 token、权限码
- `user.ts`：当前用户信息
- `menu.ts`：菜单和动态路由
- `store/auth.ts`：登录流程编排

### 5. 如果后端菜单暂时没准备好

可以先只打通：

- 登录
- 获取用户信息
- 获取菜单

即便菜单数据还是临时 mock，也能先把后台骨架跑起来。

不要一开始就同时重构权限、菜单、首页、缓存、国际化，成本会很高。

## 一套推荐的“业务接入顺序”

如果你打算尽快开始做自己的系统，我建议顺序是：

1. 只保留 `web-antd`
2. 删除其他 UI 应用
3. 决定是否删除 `docs`、`playground`、`backend-mock`
4. 改 `package.json`、`pnpm-workspace.yaml`、部署脚本
5. 接通你自己的登录接口
6. 接通用户信息接口
7. 接通菜单接口
8. 删除 demo 路由和 demo 页面
9. 再开始写自己的业务模块

## 一套最小可用目录目标

如果你要的是“只保留业务开发必须内容”，最终仓库可以接近这样：

```text
apps/
  web-antd/
  backend-mock/        # 可选
internal/
packages/
scripts/
package.json
pnpm-workspace.yaml
turbo.json
vitest.config.ts
```

如果你连 mock 也不用，那就只保留：

```text
apps/
  web-antd/
internal/
packages/
scripts/
package.json
pnpm-workspace.yaml
turbo.json
vitest.config.ts
```

## 最后提醒

最容易踩坑的不是“删少了”，而是这两类：

1. 删了目录，但没删脚本、工作区、CI、Docker 配置
2. 删了 `backend-mock`，但没改 `apps/web-antd/vite.config.mts` 的代理目标

如果你准备真的动手删，我更推荐按下面两阶段做：

- 第一阶段：只删其他 UI 应用，项目还能跑
- 第二阶段：再删 docs、playground、mock，并同步改配置

这样出问题时更容易定位。
