# 认证令牌（access_token / refresh_token）有效期记录

> 记录时间：2026-10-15
> 数据来源：本机运行中的认证服务 `http://localhost:44349` 与数据库 `TestWorkshop` 中 OpenIddict 相关表的实测结果，不是文档里的默认值推测。

## 一、当前生效值

| 令牌 | 用途 | 有效期 | 说明 |
| --- | --- | --- | --- |
| `access_token` | 访问接口 | 1 小时 | 放在请求头 `Authorization` 中，过期后接口返回 401 |
| `refresh_token` | 换取新令牌 | 14 天 | 从签发时刻起算，不能直接访问接口 |
| `id_token` | 身份信息 | 20 分钟 | OIDC 登录（授权码模式）返回，仅承载用户身份声明 |

实测结果：`OpenIddictTokens` 表中 158 条 `access_token` 的 `ExpirationDate - CreationDate` 全部为 `01:00:00`，100 条 `refresh_token` 全部为 `14 days`，100 条 `id_token` 全部为 `00:20:00`（表内时间为 UTC）。

## 二、需要注意的运行时行为

- access_token 1 小时后过期。前端不做本地倒计时，完全依赖后端返回 401 来触发续期。
- refresh_token 有效期为 14 天，且每次刷新都会签发一枚新的 refresh_token（旧的被标记为 `redeemed`）。因此只要 14 天内产生过请求，会话就能一直续下去；连续 14 天没有任何请求，则需要重新登录。
- 前端把令牌存在 access store 并做持久化（见 `packages/stores/src/modules/access.ts` 的 `pick` 字段）。开发环境写 `localStorage`，生产环境经 SecureLS（AES）加密后写入。
- 刷新失败时按 `preferences.app.loginExpiredMode` 处理：`page` 跳登录页，`modal` 弹出重新登录框。当前 `apps/web-antd/src/preferences.ts` 中 `enableRefreshToken: true`。
- 能拿到 refresh_token 的前提是授权范围包含 `offline_access`，`.env` 里的 `VITE_GLOB_AUDIENCE` 已包含该范围。

## 三、这几个值是从哪里来的

本项目没有配置令牌有效期，用的全是框架默认值：

- `aspnet-core/services/TestWorkshop.HttpApi.Host/appsettings.json` 的 `AuthServer` 段没有有效期配置项。
- `TestWorkshopHttpApiHostModule.Configure.cs` 的 `PreConfigureOpenIddict` 只配置了校验、证书和 Issuer，没有设置有效期。
- `TestWorkshop.Domain/TestWorkshop/Seeds/OpenIddictDataSeedContributor.cs` 创建应用时没有设置 `AccessTokenLifetime` / `RefreshTokenLifetime` / `IdentityTokenLifetime`。
- 数据库 `OpenIddictApplications.ExtraProperties` 为空。

结论：取值来自 ABP 9.3.5 + OpenIddict 6.4.0 的服务端默认值，即访问令牌 1 小时、刷新令牌 14 天、身份令牌 20 分钟。

## 四、要调整的话改哪里

改服务端默认值，在 `TestWorkshopHttpApiHostModule.Configure.cs` 的 `PreConfigureOpenIddict` 中追加：

```csharp
PreConfigure<OpenIddictServerBuilder>(serverBuilder =>
{
    serverBuilder.SetAccessTokenLifetime(TimeSpan.FromHours(2));
    serverBuilder.SetRefreshTokenLifetime(TimeSpan.FromDays(30));
    serverBuilder.SetIdentityTokenLifetime(TimeSpan.FromMinutes(20));
});
```

注意现有代码里证书和 Issuer 的配置包在 `if (!environment.IsDevelopment())` 中，上面这段如果写在同一个分支里就只在非开发环境生效，按需求选择放置位置。

改完后重启 `TestWorkshop.HttpApi.Host`。已签发的旧令牌仍按旧有效期执行，需要重新登录才会拿到新有效期的令牌。

如果只想针对某个客户端调整，可在种子数据里给 `AbpApplicationDescriptor` 设置 `AccessTokenLifetime` / `RefreshTokenLifetime`，该配置会写入 `OpenIddictApplications` 表并只对该客户端生效。

## 五、自查方法

查库统计各类令牌的实际有效期：

```sql
SELECT "Type",
       min("ExpirationDate" - "CreationDate") AS min_lifetime,
       max("ExpirationDate" - "CreationDate") AS max_lifetime,
       count(*)                               AS tokens
FROM "OpenIddictTokens"
GROUP BY "Type"
ORDER BY "Type";
```

或看登录响应：`POST http://localhost:44349/connect/token`（`grant_type=password`）返回的 `expires_in` 即 access_token 的剩余秒数，当前应为 `3600`。

## 六、前端相关代码位置

| 位置 | 作用 |
| --- | --- |
| `apps/web-antd/src/store/auth.ts` | `_loginSuccess`、`refreshSession` 写入新令牌；注意 `_loginSuccess` 未使用后端返回的 `expires_in` |
| `apps/web-antd/src/adapter/request/index.ts` | `doRefreshToken` / `doReAuthenticate` 组装刷新与重登录逻辑 |
| `packages/effects/request/src/request-client/preset-interceptors.ts` | 401 时用 refresh_token 换新令牌并重放请求，刷新期间其余请求排队 |
| `packages/stores/src/modules/access.ts` | `accessToken` / `refreshToken` 的持久化字段 |
| `packages/@abp/core/src/utils/auth.ts` | oidc-client-ts 的 `UserManager` 配置与令牌存储方式 |
