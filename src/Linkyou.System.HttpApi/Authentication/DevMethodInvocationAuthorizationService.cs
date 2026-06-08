using System.Threading.Tasks;
using Volo.Abp.Authorization;

namespace Linkyou.System.Authentication;

/// <summary>
/// 开发环境方法调用授权服务：允许所有已认证用户访问
/// 替换 ABP 的 MethodInvocationAuthorizationService
/// </summary>
public class DevMethodInvocationAuthorizationService : IMethodInvocationAuthorizationService
{
    public Task CheckAsync(MethodInvocationAuthorizationContext context)
    {
        // 始终放行，不检查权限
        return Task.CompletedTask;
    }
}
