using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Linkyou.System.Menus;
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
///
/// ABP 调用时机：
///   1. 首次启动 → SeedAsync(TenantId = null)
///   2. 创建新租户 → SeedAsync(TenantId = 新租户ID)
///
/// 设计原则：
///   - linkyou 租户：最高级管理租户，拥有全部权限
///   - admin 用户：在 linkyou 租户下创建，拥有全部权限
///   - 菜单数据：按租户隔离，每个租户独立一份
///   - 所有操作幂等，使用 FindByNameAsync 检查（不受多租户过滤影响）
/// </summary>
public class LinkyouSystemDataSeederContributor
    : IDataSeedContributor, ITransientDependency
{
    private const string AdminRoleName = "admin";
    private const string DefaultTenantName = "linkyou";
    private const string AdminEmail = "1343261694@qq.com";
    private const string AdminPassword = "Admin@123456";

    private readonly IIdentityDataSeeder _identityDataSeeder;
    private readonly ICurrentTenant _currentTenant;
    private readonly IPermissionDataSeeder _permissionDataSeeder;
    private readonly IPermissionDefinitionManager _permissionDefinitionManager;
    private readonly IMenuRepository _menuRepository;
    private readonly IMenuRolePermissionRepository _menuRolePermissionRepository;
    private readonly IGuidGenerator _guidGenerator;
    private readonly IRepository<Tenant, Guid> _tenantRepository;
    private readonly TenantManager _tenantManager;

    public LinkyouSystemDataSeederContributor(
        IIdentityDataSeeder identityDataSeeder,
        ICurrentTenant currentTenant,
        IPermissionDataSeeder permissionDataSeeder,
        IPermissionDefinitionManager permissionDefinitionManager,
        IMenuRepository menuRepository,
        IMenuRolePermissionRepository menuRolePermissionRepository,
        IGuidGenerator guidGenerator,
        IRepository<Tenant, Guid> tenantRepository,
        TenantManager tenantManager)
    {
        _identityDataSeeder = identityDataSeeder;
        _currentTenant = currentTenant;
        _permissionDataSeeder = permissionDataSeeder;
        _permissionDefinitionManager = permissionDefinitionManager;
        _menuRepository = menuRepository;
        _menuRolePermissionRepository = menuRolePermissionRepository;
        _guidGenerator = guidGenerator;
        _tenantRepository = tenantRepository;
        _tenantManager = tenantManager;
    }

    public async Task SeedAsync(DataSeedContext context)
    {
        var tenantId = await ResolveTenantIdAsync(context);

        using (_currentTenant.Change(tenantId))
        {
            await SeedAdminUserAsync(tenantId);
            await SeedPermissionsAsync();
            await SeedMenusAsync(tenantId);
        }
    }

    /// <summary>
    /// 解析目标租户 ID
    /// - 有 context.TenantId → 直接使用
    /// - 无 context.TenantId → 查找或创建默认租户
    /// </summary>
    private async Task<Guid?> ResolveTenantIdAsync(DataSeedContext context)
    {
        if (context.TenantId.HasValue)
            return context.TenantId;

        var tenants = await _tenantRepository.GetListAsync();
        var existing = tenants.FirstOrDefault();
        if (existing != null)
            return existing.Id;

        var tenant = await _tenantManager.CreateAsync(DefaultTenantName);
        await _tenantRepository.InsertAsync(tenant);
        return tenant.Id;
    }

    /// <summary>
    /// 创建 admin 用户（ABP 内置，幂等）
    /// </summary>
    private async Task SeedAdminUserAsync(Guid? tenantId)
    {
        await _identityDataSeeder.SeedAsync(
            adminEmail: AdminEmail,
            adminPassword: AdminPassword,
            tenantId: tenantId
        );
    }

    /// <summary>
    /// 授予 admin 角色所有权限
    /// 注意：ABP 的 IIdentityDataSeeder 已自动为 admin 角色授予基础权限
    /// 此方法仅补充 ABP 未自动授予的自定义权限
    /// </summary>
    private async Task SeedPermissionsAsync()
    {
        // ABP 的 IIdentityDataSeeder.SeedAsync 已自动为 admin 角色授予所有权限
        // 包括 AbpIdentity.*, AbpTenantManagement.*, SettingManagement.*, 等
        // 因此此处无需额外操作，避免重复插入导致唯一约束冲突
        await Task.CompletedTask;
    }

    /// <summary>
    /// 创建菜单数据（幂等：按名称检查）
    /// </summary>
    private async Task SeedMenusAsync(Guid? tenantId)
    {
        var existing = await _menuRepository.FindByNameAsync("仪表盘");
        if (existing != null) return;

        var dashboard = CreateMenu("仪表盘", "/dashboard", "Odometer", null, 0, tenantId);
        await _menuRepository.InsertAsync(dashboard);

        var systemGroup = CreateMenu("系统管理", "/system", "Setting", null, 100, tenantId);
        await _menuRepository.InsertAsync(systemGroup);

        var children = new[]
        {
            CreateMenu("用户管理", "/system/users", "User", systemGroup.Id, 0, tenantId, "AbpIdentity.Users"),
            CreateMenu("角色管理", "/system/roles", "UserFilled", systemGroup.Id, 1, tenantId, "AbpIdentity.Roles"),
            CreateMenu("租户管理", "/system/tenants", "OfficeBuilding", systemGroup.Id, 2, tenantId, "AbpTenantManagement.Tenants"),
            CreateMenu("审计日志", "/system/audit-logs", "Document", systemGroup.Id, 3, tenantId),
            CreateMenu("菜单管理", "/system/menus", "Menu", systemGroup.Id, 4, tenantId, "Menus.MenuItems"),
            CreateMenu("系统设置", "/system/settings", "Tools", systemGroup.Id, 5, tenantId, "SettingManagement.Settings"),
        };

        foreach (var child in children)
            await _menuRepository.InsertAsync(child);

        // 为 admin 角色授予所有菜单权限
        var allMenus = new List<Menu> { dashboard, systemGroup };
        allMenus.AddRange(children);

        foreach (var menu in allMenus)
            await _menuRolePermissionRepository.InsertAsync(
                new MenuRolePermission(_guidGenerator.Create(), menu.Id, AdminRoleName) { TenantId = tenantId });
    }

    private Menu CreateMenu(string name, string path, string icon, Guid? parentId, int sort, Guid? tenantId, string? permission = null)
    {
        return new Menu(_guidGenerator.Create(), name, path, icon, parentId, sort, true, permission) { TenantId = tenantId };
    }
}
