using System.Collections.Generic;
using System.Threading.Tasks;
using Linkyou.System.Account;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;

namespace Linkyou.System.Controllers.Account;

[RemoteService]
[Area("app")]
[Route("api/account")]
public class AccountController : LinkyouSystemController, IAccountAppService
{
    private readonly IAccountAppService _appService;

    public AccountController(IAccountAppService appService)
        => _appService = appService;

    [HttpPost("login")]
    public Task<LoginOutput> LoginAsync([FromBody] LoginInput input)
        => _appService.LoginAsync(input);

    [HttpPost("refresh-token")]
    public Task<LoginOutput> RefreshTokenAsync([FromBody] RefreshTokenInput input)
        => _appService.RefreshTokenAsync(input);

    [HttpGet("my-profile")]
    public Task<CurrentUserDto> GetCurrentUserAsync()
        => _appService.GetCurrentUserAsync();

    [HttpPost("change-password")]
    public Task ChangePasswordAsync([FromBody] ChangePasswordInput input)
        => _appService.ChangePasswordAsync(input);

    [HttpPost("logout")]
    public Task LogoutAsync()
        => _appService.LogoutAsync();

    [HttpGet("tenants")]
    public Task<List<TenantLookupDto>> GetTenantListAsync()
        => _appService.GetTenantListAsync();
}
