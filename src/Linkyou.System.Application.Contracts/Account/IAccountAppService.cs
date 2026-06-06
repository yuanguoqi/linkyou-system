using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace Linkyou.System.Account;

/// <summary>
/// 账户应用服务接口
/// </summary>
public interface IAccountAppService : IApplicationService
{
    /// <summary>登录，返回 JWT 令牌对</summary>
    Task<LoginOutput> LoginAsync(LoginInput input);

    /// <summary>刷新访问令牌</summary>
    Task<LoginOutput> RefreshTokenAsync(RefreshTokenInput input);

    /// <summary>获取当前登录用户信息</summary>
    Task<CurrentUserDto> GetCurrentUserAsync();

    /// <summary>修改密码</summary>
    Task ChangePasswordAsync(ChangePasswordInput input);

    /// <summary>退出登录（服务端吊销刷新令牌）</summary>
    Task LogoutAsync();

    /// <summary>获取租户列表（公开接口，供登录页使用）</summary>
    Task<List<TenantLookupDto>> GetTenantListAsync();
}
