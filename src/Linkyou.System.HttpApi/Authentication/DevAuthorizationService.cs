using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace Linkyou.System.Authentication;

/// <summary>
/// 开发环境授权服务：允许所有已认证用户访问所有端点
/// 替换 ABP 默认的 IAuthorizationService，跳过权限检查
/// 生产环境应移除此服务
/// </summary>
public class DevAuthorizationService : IAuthorizationService
{
    private static readonly AuthorizationResult Success = AuthorizationResult.Success();

    public Task<AuthorizationResult> AuthorizeAsync(
        ClaimsPrincipal user,
        object? resource,
        IEnumerable<IAuthorizationRequirement> requirements)
    {
        return Task.FromResult(Success);
    }

    public Task<AuthorizationResult> AuthorizeAsync(
        ClaimsPrincipal user,
        object? resource,
        string policyName)
    {
        return Task.FromResult(Success);
    }

    // ABP 调用此重载：AuthorizeAsync(user, resource, AuthorizationPolicy)
    public Task<AuthorizationResult> AuthorizeAsync(
        ClaimsPrincipal user,
        object? resource,
        AuthorizationPolicy policy)
    {
        return Task.FromResult(Success);
    }
}
