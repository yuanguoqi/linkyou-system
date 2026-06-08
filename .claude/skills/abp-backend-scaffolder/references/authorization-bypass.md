# ABP 授权绕过方案

## 问题描述

ABP 框架使用自己的 `MethodInvocationAuthorizationService` 拦截器检查权限，独立于 ASP.NET Core 标准的 `IAuthorizationHandler` 管道。因此：

- `IAuthorizationHandler` 方案**不生效**
- `AlwaysAllowAuthorizationHandler` 方案**不生效**

## 解决方案

替换 ABP 的 `IAuthorizationService` 实现，必须覆盖所有重载方法。

### DevAuthorizationService

```csharp
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace Linkyou.System.Authentication;

public class DevAuthorizationService : IAuthorizationService
{
    private static readonly AuthorizationResult Success = AuthorizationResult.Success();

    // ASP.NET Core 标准调用
    public Task<AuthorizationResult> AuthorizeAsync(
        ClaimsPrincipal user, object? resource,
        IEnumerable<IAuthorizationRequirement> requirements)
    {
        return Task.FromResult(Success);
    }

    // ABP 策略名称调用
    public Task<AuthorizationResult> AuthorizeAsync(
        ClaimsPrincipal user, object? resource, string policyName)
    {
        return Task.FromResult(Success);
    }

    // ABP 授权扩展调用（关键！）
    public Task<AuthorizationResult> AuthorizeAsync(
        ClaimsPrincipal user, object? resource, AuthorizationPolicy policy)
    {
        return Task.FromResult(Success);
    }
}
```

### 注册方式

```csharp
// LinkyouSystemHttpApiModule.cs
context.Services.Replace(
    ServiceDescriptor.Singleton<IAuthorizationService, DevAuthorizationService>());
```

## 关键点

1. **必须覆盖 `AuthorizationPolicy` 参数重载** — ABP 的 `AbpAuthorizationServiceExtensions.CheckAsync` 调用此重载
2. **使用 `Replace` 注册** — 确保替换 ABP 默认实现
3. **仅用于开发环境** — 生产环境应移除此服务，使用 ABP 默认授权

## 适用场景

- 开发环境快速调试
- 种子数据权限未正确授予时的临时方案
- 需要完全禁用权限检查的测试环境
