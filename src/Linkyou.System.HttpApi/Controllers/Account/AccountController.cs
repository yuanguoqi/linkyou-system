using System.Threading.Tasks;
using Linkyou.System.Account;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;

namespace Linkyou.System.Controllers.Account;

/// <summary>
/// 账户 API 控制器
/// 提供登录、刷新令牌、用户信息、修改密码、退出登录接口
/// </summary>
[RemoteService]
[Area("app")]
[Route("api/account")]
public class AccountController : LinkyouSystemController
{
    private readonly IAccountAppService _accountAppService;

    public AccountController(IAccountAppService accountAppService)
    {
        _accountAppService = accountAppService;
    }

    /// <summary>
    /// 登录，返回 AccessToken + RefreshToken
    /// POST /api/account/login
    /// </summary>
    [HttpPost("login")]
    public Task<LoginOutput> LoginAsync([FromBody] LoginInput input)
        => _accountAppService.LoginAsync(input);

    /// <summary>
    /// 刷新访问令牌
    /// POST /api/account/refresh-token
    /// </summary>
    [HttpPost("refresh-token")]
    public Task<LoginOutput> RefreshTokenAsync([FromBody] RefreshTokenInput input)
        => _accountAppService.RefreshTokenAsync(input);

    /// <summary>
    /// 获取当前登录用户信息（需 Bearer Token）
    /// GET /api/account/my-profile
    /// </summary>
    [HttpGet("my-profile")]
    public Task<CurrentUserDto> GetCurrentUserAsync()
        => _accountAppService.GetCurrentUserAsync();

    /// <summary>
    /// 修改密码
    /// POST /api/account/change-password
    /// </summary>
    [HttpPost("change-password")]
    public Task ChangePasswordAsync([FromBody] ChangePasswordInput input)
        => _accountAppService.ChangePasswordAsync(input);

    /// <summary>
    /// 退出登录（吊销 RefreshToken）
    /// POST /api/account/logout
    /// </summary>
    [HttpPost("logout")]
    public Task LogoutAsync()
        => _accountAppService.LogoutAsync();
}
