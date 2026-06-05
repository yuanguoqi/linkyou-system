using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Linkyou.System.Menus;
using Microsoft.AspNetCore.Identity;
using Volo.Abp;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Guids;
using Volo.Abp.Identity;
using Volo.Abp.MultiTenancy;
using Volo.Abp.PermissionManagement;

namespace Linkyou.System;

/// <summary>
/// 数据种子贡献者（幂等，重复执行不会重复插入）
/// 在应用首次启动时插入：
///   1. 默认 admin 账号（admin@linkyou.com / Admin@123456）
///   2. superadmin 角色，授予系统全量权限
///   3. 将 admin 用户加入 superadmin 角色（确保 admin 拥有全部权限）
///   4. 默认菜单数据 + superadmin 角色菜单权限
/// </summary>
public class LinkyouSystemDataSeederContributor
    : IDataSeedContributor, ITransientDependency
{
    private const string SuperAdminRoleName = "superadmin";
    private const string AdminUserName = "admin";

    private readonly IIdentityDataSeeder _identityDataSeeder;
    private readonly ICurrentTenant _currentTenant;
    private readonly IdentityRoleManager _roleManager;
    private readonly IdentityUserManager _userManager;
    private readonly IPermissionDataSeeder _permissionDataSeeder;
    private readonly IPermissionDefinitionManager _permissionDefinitionManager;
    private readonly IIdentityUserRepository _userRepository;
    private readonly IMenuRepository _menuRepository;
    private readonly IMenuRolePermissionRepository _menuRolePermissionRepository;
    private readonly IGuidGenerator _guidGenerator;

    public LinkyouSystemDataSeederContributor(
        IIdentityDataSeeder identityDataSeeder,
        ICurrentTenant currentTenant,
        IdentityRoleManager roleManager,
        IdentityUserManager userManager,
        IPermissionDataSeeder permissionDataSeeder,
        IPermissionDefinitionManager permissionDefinitionManager,
        IIdentityUserRepository userRepository,
        IMenuRepository menuRepository,
        IMenuRolePermissionRepository menuRolePermissionRepository,
        IGuidGenerator guidGenerator)
    {
        _identityDataSeeder = identityDataSeeder;
        _currentTenant = currentTenant;
        _roleManager = roleManager;
        _userManager = userManager;
        _permissionDataSeeder = permissionDataSeeder;
        _permissionDefinitionManager = permissionDefinitionManager;
        _userRepository = userRepository;
        _menuRepository = menuRepository;
        _menuRolePermissionRepository = menuRolePermissionRepository;
        _guidGenerator = guidGenerator;
    }

    public async Task SeedAsync(DataSeedContext context)
    {
        using (_currentTenant.Change(context?.TenantId))
        {
            // 1. 创建默认管理员账号
            await SeedAdminUserAsync(context!);

            // 2. 创建 superadmin 角色并授予所有权限
            await SeedSuperAdminRoleAsync(context);

            // 3. 将 admin 用户绑定到 superadmin 角色
            await AssignAdminToSuperAdminRoleAsync(context);

            // 4. 种子菜单 + 角色菜单权限
            await SeedMenusAsync();
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
        var adminUser = await _userManager.FindByNameAsync(AdminUserName);
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
    private async Task<IdentityRole> SeedSuperAdminRoleAsync(DataSeedContext? context)
    {
        // 角色不存在时才创建（幂等）
        var superAdminRole = await _roleManager.FindByNameAsync(SuperAdminRoleName);
        if (superAdminRole == null)
        {
            superAdminRole = new IdentityRole(Guid.NewGuid(), SuperAdminRoleName)
            {
                IsStatic = true,   // 静态角色不可被删除
                IsDefault = false,
                IsPublic = true
            };
            var result = await _roleManager.CreateAsync(superAdminRole);
            if (!result.Succeeded)
            {
                throw new AbpException(
                    $"创建角色 '{SuperAdminRoleName}' 失败: " +
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
            SuperAdminRoleName,
            permissionNames,
            context?.TenantId
        );

        return superAdminRole;
    }

    /// <summary>
    /// 将 admin 用户绑定到 superadmin 角色，确保 admin 拥有全部权限。
    /// 幂等：若已绑定则跳过。
    /// </summary>
    private async Task AssignAdminToSuperAdminRoleAsync(DataSeedContext? context)
    {
        var adminUser = await _userManager.FindByNameAsync(AdminUserName);
        if (adminUser == null) return;

        // 检查是否已在角色中（幂等）
        var userRoles = await _userRepository.GetRolesAsync(adminUser.Id);
        if (userRoles.Any(r => r.NormalizedName == SuperAdminRoleName.ToUpperInvariant()))
            return;

        // 加入 superadmin 角色
        var addResult = await _userManager.AddToRoleAsync(adminUser, SuperAdminRoleName);
        if (!addResult.Succeeded)
        {
            throw new AbpException(
                $"将 admin 用户加入 '{SuperAdminRoleName}' 角色失败: " +
                string.Join(", ", addResult.Errors.Select(e => e.Description)));
        }
    }

    /// <summary>
    /// 种子菜单数据 + superadmin 角色菜单权限（幂等）
    /// </summary>
    private async Task SeedMenusAsync()
    {
        // 检查是否已有菜单数据（幂等）
        var existingMenus = await _menuRepository.GetListAsync(maxResultCount: 1);
        if (existingMenus.Any()) return;

        // ── 仪表盘 ──
        var dashboard = new Menu(_guidGenerator.Create(), "仪表盘", "/dashboard", "Odometer", null, 0, true);
        await _menuRepository.InsertAsync(dashboard);

        // ── 系统管理（父级菜单，path 为空，仅做分组） ──
        var systemGroup = new Menu(_guidGenerator.Create(), "系统管理", "/system", "Setting", null, 100, true);
        await _menuRepository.InsertAsync(systemGroup);

        // ── 系统管理子菜单 ──
        var systemChildren = new List<Menu>
        {
            new(_guidGenerator.Create(), "用户管理", "/system/users", "User", systemGroup.Id, 0, true, "AbpIdentity.Users"),
            new(_guidGenerator.Create(), "角色管理", "/system/roles", "UserFilled", systemGroup.Id, 1, true, "AbpIdentity.Roles"),
            new(_guidGenerator.Create(), "租户管理", "/system/tenants", "OfficeBuilding", systemGroup.Id, 2, true, "AbpTenantManagement.Tenants"),
            new(_guidGenerator.Create(), "审计日志", "/system/audit-logs", "Document", systemGroup.Id, 3, true),
            new(_guidGenerator.Create(), "菜单管理", "/system/menus", "Menu", systemGroup.Id, 4, true, "Menus.MenuItems"),
            new(_guidGenerator.Create(), "系统设置", "/system/settings", "Tools", systemGroup.Id, 5, true, "SettingManagement.Settings"),
        };

        foreach (var child in systemChildren)
        {
            await _menuRepository.InsertAsync(child);
        }

        // ── 为 superadmin 角色授予所有菜单权限 ──
        var allMenus = new List<Menu> { dashboard, systemGroup };
        allMenus.AddRange(systemChildren);

        foreach (var menu in allMenus)
        {
            await _menuRolePermissionRepository.InsertAsync(
                new MenuRolePermission(_guidGenerator.Create(), menu.Id, SuperAdminRoleName));
        }
    }
}
