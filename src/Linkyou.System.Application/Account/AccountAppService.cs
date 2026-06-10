using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Linkyou.System.Jwt;
using Linkyou.System.Localization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Caching.Distributed;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Authorization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.PermissionManagement;
using Volo.Abp.Caching;
using Volo.Abp.Identity;
using Volo.Abp.TenantManagement;
using Volo.Abp.Security.Claims;
using Volo.Abp.Settings;

namespace Linkyou.System.Account;

/// <summary>
/// 账户应用服务实现
/// 处理登录、令牌刷新、用户信息查询、密码修改、退出登录
/// </summary>
public class AccountAppService : ApplicationService, IAccountAppService
{
    private readonly IdentityUserManager _userManager;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IDistributedCache<string> _refreshTokenCache;
    private readonly IPermissionManager _permissionManager;
    private readonly IRepository<Tenant, Guid> _tenantRepository;
    private readonly ISettingProvider _settingProvider;

    // RefreshToken 缓存键前缀
    private const string RefreshTokenCachePrefix = "RefreshToken:";

    public AccountAppService(
        IdentityUserManager userManager,
        IJwtTokenService jwtTokenService,
        IDistributedCache<string> refreshTokenCache,
        IPermissionManager permissionManager,
        IRepository<Tenant, Guid> tenantRepository,
        ISettingProvider settingProvider)
    {
        _userManager = userManager;
        _jwtTokenService = jwtTokenService;
        _refreshTokenCache = refreshTokenCache;
        _permissionManager = permissionManager;
        _tenantRepository = tenantRepository;
        _settingProvider = settingProvider;
    }

    /// <summary>
    /// 登录：验证用户名/密码，返回 AccessToken + RefreshToken
    /// </summary>
    public async Task<LoginOutput> LoginAsync(LoginInput input)
    {
        // 支持用户名或邮箱登录
        var user = await _userManager.FindByNameAsync(input.UserNameOrEmailAddress)
                   ?? await _userManager.FindByEmailAsync(input.UserNameOrEmailAddress);

        if (user == null)
        {
            throw new UserFriendlyException(L["InvalidUserNameOrPassword"]);
        }

        // 检查账户是否被锁定
        if (await _userManager.IsLockedOutAsync(user))
        {
            throw new UserFriendlyException(L["AccountLocked"]);
        }

        // 验证密码
        var passwordValid = await _userManager.CheckPasswordAsync(user, input.Password);
        if (!passwordValid)
        {
            // 记录失败次数（ABP 内置锁定策略）
            await _userManager.AccessFailedAsync(user);
            throw new UserFriendlyException(L["InvalidUserNameOrPassword"]);
        }

        // 登录成功，重置失败计数
        await _userManager.ResetAccessFailedCountAsync(user);

        // 构建 JWT Claims
        var roles = await _userManager.GetRolesAsync(user);
        var claims = BuildClaims(user, roles);

        // 生成令牌对
        var accessToken = _jwtTokenService.GenerateAccessToken(claims);
        var refreshToken = _jwtTokenService.GenerateRefreshToken();

        // 缓存 RefreshToken（key = 前缀 + userId，value = refreshToken）
        var refreshExpireDays = 30;
        await _refreshTokenCache.SetAsync(
            RefreshTokenCachePrefix + user.Id,
            refreshToken,
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(refreshExpireDays)
            });

        return new LoginOutput
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresIn = _jwtTokenService.GetAccessTokenExpiresInSeconds(),
        };
    }

    /// <summary>
    /// 刷新令牌：用 RefreshToken 换取新的令牌对
    /// </summary>
    public async Task<LoginOutput> RefreshTokenAsync(RefreshTokenInput input)
    {
        // 使用前端传入的 UserId 直接验证 RefreshToken，无需依赖过期的 AccessToken
        var isValid = await ValidateRefreshTokenAsync(input.UserId, input.RefreshToken);
        if (!isValid)
        {
            throw new UserFriendlyException(L["InvalidRefreshToken"]);
        }

        var user = await _userManager.GetByIdAsync(input.UserId);
        var roles = await _userManager.GetRolesAsync(user);
        var claims = BuildClaims(user, roles);

        // 生成新令牌对
        var newAccessToken = _jwtTokenService.GenerateAccessToken(claims);
        var newRefreshToken = _jwtTokenService.GenerateRefreshToken();

        // 更新缓存中的 RefreshToken（旧 token 自动失效）
        await _refreshTokenCache.SetAsync(
            RefreshTokenCachePrefix + user.Id,
            newRefreshToken,
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(30)
            });

        return new LoginOutput
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken,
            ExpiresIn = _jwtTokenService.GetAccessTokenExpiresInSeconds(),
        };
    }

    /// <summary>
    /// 获取当前登录用户信息（需要认证）
    /// </summary>
    [Authorize]
    public async Task<CurrentUserDto> GetCurrentUserAsync()
    {
        var userId = CurrentUser.Id
            ?? throw new AbpAuthorizationException(L["NotLoggedIn"]);

        var user = await _userManager.GetByIdAsync(userId);
        var roles = await _userManager.GetRolesAsync(user);

        // 收集所有已授予的权限（用户直接授权 + 角色授权）
        var grantedPermissions = new HashSet<string>();

        // 1. 用户直接拥有的权限
        var userPermissionGrants = await _permissionManager.GetAllAsync("User", userId.ToString());
        foreach (var grant in userPermissionGrants)
        {
            grantedPermissions.Add(grant.Name);
        }

        // 2. 通过角色拥有的权限
        foreach (var roleName in roles)
        {
            var rolePermissionGrants = await _permissionManager.GetAllAsync("Role", roleName);
            foreach (var grant in rolePermissionGrants)
            {
                grantedPermissions.Add(grant.Name);
            }
        }

        return new CurrentUserDto
        {
            Id = user.Id.ToString(),
            UserName = user.UserName!,
            Name = user.Name,
            SurName = user.Surname,
            Email = user.Email!,
            PhoneNumber = user.PhoneNumber,
            TenantId = user.TenantId,
            Roles = [.. roles],
            Permissions = [.. grantedPermissions],
        };
    }

    /// <summary>
    /// 修改密码（需要认证）
    /// 使用自定义校验规则（从系统设置读取），绕过 ABP Identity 内置的 PasswordOptions 校验
    /// </summary>
    [Authorize]
    public async Task ChangePasswordAsync(ChangePasswordInput input)
    {
        var userId = CurrentUser.Id
            ?? throw new AbpAuthorizationException(L["NotLoggedIn"]);

        var user = await _userManager.GetByIdAsync(userId);

        // 1. 校验当前密码
        if (!await _userManager.CheckPasswordAsync(user, input.CurrentPassword))
        {
            throw new UserFriendlyException(L["PasswordValidation:CurrentPasswordWrong"]);
        }

        // 2. 根据系统设置校验新密码
        var errors = await ValidatePasswordAsync(input.NewPassword);
        if (errors.Count > 0)
        {
            throw new UserFriendlyException(string.Join("; ", errors));
        }

        // 3. 反射设置 PasswordHash（ABP 设为 protected），绕过动态校验
        var hash = _userManager.PasswordHasher.HashPassword(user, input.NewPassword);
        var prop = typeof(IdentityUser).GetProperty("PasswordHash",
            global::System.Reflection.BindingFlags.Instance | global::System.Reflection.BindingFlags.Public | global::System.Reflection.BindingFlags.NonPublic);
        prop!.SetValue(user, hash);
        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
            throw new UserFriendlyException(string.Join("; ", result.Errors.Select(e => e.Description)));
    }

    /// <summary>
    /// 从系统设置读取密码策略并校验
    /// </summary>
    private async Task<List<string>> ValidatePasswordAsync(string password)
    {
        var errors = new List<string>();

        var len = await _settingProvider.GetAsync<int>("Abp.Identity.Password.RequiredLength");
        var upper = await _settingProvider.GetAsync<bool>("Abp.Identity.Password.RequireUppercase");
        var lower = await _settingProvider.GetAsync<bool>("Abp.Identity.Password.RequireLowercase");
        var digit = await _settingProvider.GetAsync<bool>("Abp.Identity.Password.RequireDigit");
        var nonAlpha = await _settingProvider.GetAsync<bool>("Abp.Identity.Password.RequireNonAlphanumeric");

        if (password.Length < len)
            errors.Add(L["PasswordValidation:RequiredLength", len]);

        if (upper && !password.Any(char.IsUpper))
            errors.Add(L["PasswordValidation:RequireUppercase"]);

        if (lower && !password.Any(char.IsLower))
            errors.Add(L["PasswordValidation:RequireLowercase"]);

        if (digit && !password.Any(char.IsDigit))
            errors.Add(L["PasswordValidation:RequireDigit"]);

        if (nonAlpha && password.All(char.IsLetterOrDigit))
            errors.Add(L["PasswordValidation:RequireNonAlphanumeric"]);

        return errors;
    }

    /// <summary>
    /// 退出登录：删除服务端 RefreshToken 缓存（需要认证）
    /// </summary>
    [Authorize]
    public async Task LogoutAsync()
    {
        if (CurrentUser.Id.HasValue)
        {
            await _refreshTokenCache.RemoveAsync(
                RefreshTokenCachePrefix + CurrentUser.Id.Value);
        }
    }

    /// <summary>
    /// 获取租户列表（公开接口，供登录页使用）
    /// </summary>
    [AllowAnonymous]
    public async Task<List<TenantLookupDto>> GetTenantListAsync()
    {
        var tenants = await _tenantRepository.GetListAsync();
        return tenants.Take(100).Select(t => new TenantLookupDto
        {
            Id = t.Id,
            Name = t.Name,
        }).ToList();
    }

    // ==================== 私有方法 ====================

    /// <summary>
    /// 构建 JWT Claims 列表
    /// </summary>
    private static List<Claim> BuildClaims(
        Volo.Abp.Identity.IdentityUser user,
        IList<string> roles)
    {
        var claims = new List<Claim>
        {
            new(AbpClaimTypes.UserId,   user.Id.ToString()),
            new(AbpClaimTypes.UserName, user.UserName!),
            new(AbpClaimTypes.Email,    user.Email ?? string.Empty),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
        };

        if (user.TenantId.HasValue)
        {
            claims.Add(new Claim(AbpClaimTypes.TenantId, user.TenantId.Value.ToString()));
        }

        if (!string.IsNullOrWhiteSpace(user.Name))
        {
            claims.Add(new Claim(AbpClaimTypes.Name, user.Name));
        }

        // 添加所有角色
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        return claims;
    }

    /// <summary>
    /// 根据 UserId 和 RefreshToken 验证令牌有效性
    /// UserId 由前端在刷新请求中明确传入，避免依赖过期 AccessToken 中的 CurrentUser
    /// </summary>
    private async Task<bool> ValidateRefreshTokenAsync(Guid userId, string refreshToken)
    {
        var cached = await _refreshTokenCache.GetAsync(
            RefreshTokenCachePrefix + userId);
        return cached == refreshToken;
    }
}
