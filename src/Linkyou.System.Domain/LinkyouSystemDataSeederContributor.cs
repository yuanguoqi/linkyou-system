using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Volo.Abp;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Identity;
using Volo.Abp.MultiTenancy;
using Volo.Abp.PermissionManagement;

namespace Linkyou.System;

/// <summary>
/// 数据种子贡献者（幂等，重复执行不会重复插入）
/// 在应用首次启动时插入：
///   1. 默认 admin 账号（admin@linkyou.com / Admin@123456）
///   2. superadmin 角色，并授予系统全量权限
/// </summary>
public class LinkyouSystemDataSeederContributor
    : IDataSeedContributor, ITransientDependency
{
    private readonly IIdentityDataSeeder _identityDataSeeder;
    private readonly ICurrentTenant _currentTenant;
    private readonly IdentityRoleManager _roleManager;
    private readonly IdentityUserManager _userManager;
    private readonly IPermissionDataSeeder _permissionDataSeeder;
    private readonly IPermissionDefinitionManager _permissionDefinitionManager;

    public LinkyouSystemDataSeederContributor(
        IIdentityDataSeeder identityDataSeeder,
        ICurrentTenant currentTenant,
        IdentityRoleManager roleManager,
        IdentityUserManager userManager,
        IPermissionDataSeeder permissionDataSeeder,
        IPermissionDefinitionManager permissionDefinitionManager)
    {
        _identityDataSeeder = identityDataSeeder;
        _currentTenant = currentTenant;
        _roleManager = roleManager;
        _userManager = userManager;
        _permissionDataSeeder = permissionDataSeeder;
        _permissionDefinitionManager = permissionDefinitionManager;
    }

    public async Task SeedAsync(DataSeedContext context)
    {
        using (_currentTenant.Change(context?.TenantId))
        {
            // 1. 创建默认管理员账号和内置 admin 角色
            await SeedAdminUserAsync(context!);

            // 2. 创建 superadmin 角色并授予所有权限
            await SeedSuperAdminRoleAsync(context);
        }
    }

    /// <summary>
    /// 创建默认 admin 账号，并强制同步密码。
    /// ABP 的 IIdentityDataSeeder 是幂等的：若 admin 用户已存在则跳过创建，
    /// 不会更新密码。因此在调用 SeedAsync 后额外执行一次密码强制重置，
    /// 确保 admin 账号始终可以用 Admin@123456 登录。
    /// </summary>
    private async Task SeedAdminUserAsync(DataSeedContext context)
    {
        var adminPassword = context.Properties.GetOrDefault("AdminPassword") as string
            ?? "Admin@123456";

        await _identityDataSeeder.SeedAsync(
            adminEmail: context.Properties.GetOrDefault("AdminEmail") as string
                ?? "admin@linkyou.com",
            adminPassword: adminPassword,
            tenantId: context.TenantId
        );

        // 强制重置密码：无论 admin 是新建还是已存在，均同步为目标密码。
        // 使用 RemovePassword + AddPassword 避免依赖 TwoFactorTokenProvider。
        var adminUser = await _userManager.FindByNameAsync("admin");
        if (adminUser != null && !await _userManager.CheckPasswordAsync(adminUser, adminPassword))
        {
            var removeResult = await _userManager.RemovePasswordAsync(adminUser);
            if (!removeResult.Succeeded)
            {
                throw new AbpException(
                    "移除 admin 旧密码失败: " +
                    string.Join(", ", removeResult.Errors.Select(e => e.Description)));
            }

            var addResult = await _userManager.AddPasswordAsync(adminUser, adminPassword);
            if (!addResult.Succeeded)
            {
                throw new AbpException(
                    "设置 admin 新密码失败: " +
                    string.Join(", ", addResult.Errors.Select(e => e.Description)));
            }
        }
    }

    /// <summary>
    /// 创建 superadmin 角色并授予当前租户侧所有可用权限
    /// </summary>
    private async Task SeedSuperAdminRoleAsync(DataSeedContext? context)
    {
        const string superAdminRoleName = "superadmin";

        // 角色不存在时才创建（幂等）
        var superAdminRole = await _roleManager.FindByNameAsync(superAdminRoleName);
        if (superAdminRole == null)
        {
            superAdminRole = new IdentityRole(Guid.NewGuid(), superAdminRoleName)
            {
                IsStatic = true,   // 静态角色不可被删除
                IsDefault = false,
                IsPublic = true
            };
            var result = await _roleManager.CreateAsync(superAdminRole);
            if (!result.Succeeded)
            {
                throw new AbpException(
                    $"创建角色 '{superAdminRoleName}' 失败: " +
                    string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }

        // 获取当前租户侧、允许角色提供者的所有权限名
        var multiTenancySide = _currentTenant.GetMultiTenancySide();
        var permissionNames = (await _permissionDefinitionManager.GetPermissionsAsync())
            .Where(p => p.MultiTenancySide.HasFlag(multiTenancySide))
            .Where(p => !p.Providers.Any()
                        || p.Providers.Contains(RolePermissionValueProvider.ProviderName))
            .Select(p => p.Name)
            .ToArray();

        // 批量授予所有权限（IPermissionDataSeeder 内部幂等处理重复授权）
        await _permissionDataSeeder.SeedAsync(
            RolePermissionValueProvider.ProviderName,
            superAdminRoleName,
            permissionNames,
            context?.TenantId
        );
    }
}
