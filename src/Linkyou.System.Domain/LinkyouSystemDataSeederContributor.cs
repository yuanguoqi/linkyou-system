using System;
using System.Linq;
using System.Threading.Tasks;
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
    private readonly IPermissionDataSeeder _permissionDataSeeder;
    private readonly IPermissionDefinitionManager _permissionDefinitionManager;

    public LinkyouSystemDataSeederContributor(
        IIdentityDataSeeder identityDataSeeder,
        ICurrentTenant currentTenant,
        IdentityRoleManager roleManager,
        IPermissionDataSeeder permissionDataSeeder,
        IPermissionDefinitionManager permissionDefinitionManager)
    {
        _identityDataSeeder = identityDataSeeder;
        _currentTenant = currentTenant;
        _roleManager = roleManager;
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
    /// 创建默认 admin 账号（ABP 内置 IIdentityDataSeeder 负责幂等处理）
    /// 默认：admin@linkyou.com / Admin@123456
    /// </summary>
    private async Task SeedAdminUserAsync(DataSeedContext context)
    {
        await _identityDataSeeder.SeedAsync(
            adminEmail: context.Properties.GetOrDefault("AdminEmail") as string
                ?? "admin@linkyou.com",
            adminPassword: context.Properties.GetOrDefault("AdminPassword") as string
                ?? "Admin@123456",
            tenantId: context.TenantId
        );
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
