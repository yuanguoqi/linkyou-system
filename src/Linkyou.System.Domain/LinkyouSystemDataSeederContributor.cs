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
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using Volo.Abp.Identity;
using Volo.Abp.MultiTenancy;
using Volo.Abp.PermissionManagement;
using Volo.Abp.TenantManagement;

namespace Linkyou.System;

/// <summary>
/// 数据种子贡献者（幂等，重复执行不会重复插入）
/// 在应用首次启动时插入：
///   1. 默认 admin 账号（1343261694@qq.com / Admin@123456）
///   2. superadmin 角色，授予系统全量权限
///   3. 将 admin 用户加入 superadmin 角色（确保 admin 拥有全部权限）
///   4. 默认菜单数据 + superadmin 角色菜单权限
/// </summary>
public class LinkyouSystemDataSeederContributor
    : IDataSeedContributor, ITransientDependency
{
    private const string SuperAdminRoleName = "superadmin";
    private const string AdminUserName = "admin";

    private const string DefaultTenantName = "linkyou";

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
    private readonly IRepository<Tenant, Guid> _tenantRepository;
    private readonly TenantManager _tenantManager;

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
        IGuidGenerator guidGenerator,
        IRepository<Tenant, Guid> tenantRepository,
        TenantManager tenantManager)
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
        _tenantRepository = tenantRepository;
        _tenantManager = tenantManager;
    }

    public async Task SeedAsync(DataSeedContext context)
    {
        // 仅在有明确租户上下文时执行种子数据，避免宿主租户(null)重复生成
        if (!context.TenantId.HasValue)
        {
            // 宿主租户：获取或创建默认租户，然后用该租户上下文执行种子
            var tenantId = await GetOrCreateDefaultTenantAsync(context);
            if (!tenantId.HasValue) return;

            using (_currentTenant.Change(tenantId))
            {
                await SeedAllAsync(tenantId);
            }
            return;
        }

        // 有明确租户上下文：直接执行种子
        using (_currentTenant.Change(context.TenantId))
        {
            await SeedAllAsync(context.TenantId);
        }
    }

    /// <summary>
    /// 执行所有种子数据操作
    /// </summary>
    private async Task SeedAllAsync(Guid? tenantId)
    {
        await SeedAdminUserAsync(tenantId);
        await SeedSuperAdminRoleAsync();
        await AssignAdminToSuperAdminRoleAsync();
        await SeedMenusAsync(tenantId);
    }

    /// <summary>
    /// 获取或创建默认租户
    /// 优先使用 context.TenantId，否则从数据库获取第一个租户，
    /// 如果没有租户则创建一个默认租户
    /// </summary>
    private async Task<Guid?> GetOrCreateDefaultTenantAsync(DataSeedContext context)
    {
        // 优先使用 context 中的 TenantId
        if (context.TenantId.HasValue)
            return context.TenantId;

        // 从数据库获取第一个租户
        var tenants = await _tenantRepository.GetListAsync();
        var firstTenant = tenants.FirstOrDefault();

        if (firstTenant != null)
            return firstTenant.Id;

        // 没有租户数据，创建默认租户
        firstTenant = await _tenantManager.CreateAsync(DefaultTenantName);
        await _tenantRepository.InsertAsync(firstTenant);
        return firstTenant.Id;
    }

    /// <summary>
    /// 创建默认 admin 账号，并强制同步密码。
    /// ABP 的 IIdentityDataSeeder 是幂等的：若 admin 用户已存在则跳过创建，
    /// 不会更新密码。因此在调用 SeedAsync 后额外执行一次密码强制重置，
    /// 确保 admin 账号始终可以用 Admin@123456 登录。
    /// </summary>
    private async Task SeedAdminUserAsync(Guid? tenantId)
    {
        var adminPassword = "Admin@123456";

        await _identityDataSeeder.SeedAsync(
            adminEmail: "1343261694@qq.com",
            adminPassword: adminPassword,
            tenantId: tenantId
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
    private async Task<IdentityRole> SeedSuperAdminRoleAsync()
    {
        // 角色不存在时才创建（幂等）
        var superAdminRole = await _roleManager.FindByNameAsync(SuperAdminRoleName);
        if (superAdminRole == null)
        {
            superAdminRole = new IdentityRole(Guid.NewGuid(), SuperAdminRoleName)
            {
                IsStatic = true,
                IsDefault = false,
                IsPublic = true
            };
            var createResult = await _roleManager.CreateAsync(superAdminRole);
            if (!createResult.Succeeded)
            {
                throw new AbpException(
                    $"创建角色 '{SuperAdminRoleName}' 失败: " +
                    string.Join(", ", createResult.Errors.Select(e => e.Description)));
            }
        }

        // 获取所有权限定义（不限制 provider）
        var permissionNames = (await _permissionDefinitionManager.GetPermissionsAsync())
            .Select(p => p.Name)
            .ToArray();

        // 批量授予所有权限给 superadmin 角色
        await _permissionDataSeeder.SeedAsync(
            RolePermissionValueProvider.ProviderName,
            SuperAdminRoleName,
            permissionNames,
            _currentTenant.Id
        );

        // 同时授予 admin 角色所有权限
        await _permissionDataSeeder.SeedAsync(
            RolePermissionValueProvider.ProviderName,
            "admin",
            permissionNames,
            _currentTenant.Id
        );

        return superAdminRole;
    }

    /// <summary>
    /// 将 admin 用户绑定到 admin 和 superadmin 角色。
    /// 幂等：若已绑定则跳过。
    /// </summary>
    private async Task AssignAdminToSuperAdminRoleAsync()
    {
        var adminUser = await _userManager.FindByNameAsync(AdminUserName);
        if (adminUser == null) return;

        // 获取用户当前角色
        var currentRoles = await _userManager.GetRolesAsync(adminUser);
        var hasAdmin = currentRoles.Contains("admin", StringComparer.OrdinalIgnoreCase);
        var hasSuperAdmin = currentRoles.Contains(SuperAdminRoleName, StringComparer.OrdinalIgnoreCase);

        if (hasAdmin && hasSuperAdmin) return;

        // 使用仓储获取被跟踪的用户实体
        var trackedUser = await _userRepository.GetAsync(adminUser.Id);

        // 确保用户在 admin 角色中
        if (!hasAdmin)
        {
            var adminRole = await _roleManager.FindByNameAsync("admin");
            if (adminRole != null)
            {
                trackedUser.AddRole(adminRole.Id);
            }
        }

        // 确保用户在 superadmin 角色中
        if (!hasSuperAdmin)
        {
            var superAdminRole = await _roleManager.FindByNameAsync(SuperAdminRoleName);
            if (superAdminRole != null)
            {
                trackedUser.AddRole(superAdminRole.Id);
            }
        }

        // 使用仓储更新，确保角色关联被持久化
        await _userRepository.UpdateAsync(trackedUser);
    }

    /// <summary>
    /// 种子菜单数据 + superadmin 角色菜单权限（幂等）
    /// </summary>
    private async Task SeedMenusAsync(Guid? tenantId)
    {
        // 检查当前租户下是否已有菜单数据（幂等）
        var existingMenus = await _menuRepository.GetListAsync(maxResultCount: 1);
        if (existingMenus.Any()) return;

        // ── 仪表盘 ──
        var dashboard = new Menu(_guidGenerator.Create(), "仪表盘", "/dashboard", "Odometer", null, 0, true) { TenantId = tenantId };
        await _menuRepository.InsertAsync(dashboard);

        // ── 系统管理（父级菜单） ──
        var systemGroup = new Menu(_guidGenerator.Create(), "系统管理", "/system", "Setting", null, 100, true) { TenantId = tenantId };
        await _menuRepository.InsertAsync(systemGroup);

        // ── 系统管理子菜单 ──
        var systemChildren = new List<Menu>
        {
            new(_guidGenerator.Create(), "用户管理", "/system/users", "User", systemGroup.Id, 0, true, "AbpIdentity.Users") { TenantId = tenantId },
            new(_guidGenerator.Create(), "角色管理", "/system/roles", "UserFilled", systemGroup.Id, 1, true, "AbpIdentity.Roles") { TenantId = tenantId },
            new(_guidGenerator.Create(), "租户管理", "/system/tenants", "OfficeBuilding", systemGroup.Id, 2, true, "AbpTenantManagement.Tenants") { TenantId = tenantId },
            new(_guidGenerator.Create(), "审计日志", "/system/audit-logs", "Document", systemGroup.Id, 3, true) { TenantId = tenantId },
            new(_guidGenerator.Create(), "菜单管理", "/system/menus", "Menu", systemGroup.Id, 4, true, "Menus.MenuItems") { TenantId = tenantId },
            new(_guidGenerator.Create(), "系统设置", "/system/settings", "Tools", systemGroup.Id, 5, true, "SettingManagement.Settings") { TenantId = tenantId },
        };

        foreach (var child in systemChildren)
        {
            await _menuRepository.InsertAsync(child);
        }

        // ── 为 superadmin 和 admin 角色授予所有菜单权限 ──
        var allMenus = new List<Menu> { dashboard, systemGroup };
        allMenus.AddRange(systemChildren);

        foreach (var menu in allMenus)
        {
            // superadmin 角色
            await _menuRolePermissionRepository.InsertAsync(
                new MenuRolePermission(_guidGenerator.Create(), menu.Id, SuperAdminRoleName) { TenantId = tenantId });
            // admin 角色
            await _menuRolePermissionRepository.InsertAsync(
                new MenuRolePermission(_guidGenerator.Create(), menu.Id, "admin") { TenantId = tenantId });
        }
    }
}
