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
/// 数据种子贡献者
///
/// ABP 调用规则：
///   - 启动时调用一次 SeedAsync(context.TenantId = null)
///   - 每次创建新租户时调用 SeedAsync(context.TenantId = 新租户ID)
///
/// 本实现：
///   1. context.TenantId 为 null 时 → 跳过（由 ABP 内置 seeder 处理宿主租户）
///   2. context.TenantId 有值时 → 在该租户下创建 admin、权限、菜单
/// </summary>
public class LinkyouSystemDataSeederContributor
    : IDataSeedContributor, ITransientDependency
{
    private const string AdminRoleName = "admin";

    private readonly IIdentityDataSeeder _identityDataSeeder;
    private readonly ICurrentTenant _currentTenant;
    private readonly IPermissionDataSeeder _permissionDataSeeder;
    private readonly IPermissionDefinitionManager _permissionDefinitionManager;
    private readonly IMenuRepository _menuRepository;
    private readonly IMenuRolePermissionRepository _menuRolePermissionRepository;
    private readonly IGuidGenerator _guidGenerator;

    public LinkyouSystemDataSeederContributor(
        IIdentityDataSeeder identityDataSeeder,
        ICurrentTenant currentTenant,
        IPermissionDataSeeder permissionDataSeeder,
        IPermissionDefinitionManager permissionDefinitionManager,
        IMenuRepository menuRepository,
        IMenuRolePermissionRepository menuRolePermissionRepository,
        IGuidGenerator guidGenerator)
    {
        _identityDataSeeder = identityDataSeeder;
        _currentTenant = currentTenant;
        _permissionDataSeeder = permissionDataSeeder;
        _permissionDefinitionManager = permissionDefinitionManager;
        _menuRepository = menuRepository;
        _menuRolePermissionRepository = menuRolePermissionRepository;
        _guidGenerator = guidGenerator;
    }

    public async Task SeedAsync(DataSeedContext context)
    {
        // 获取目标租户 ID
        var tenantId = context.TenantId;

        using (_currentTenant.Change(tenantId))
        {
            // 1. 创建 admin 用户和角色（ABP 内置，幂等）
            await _identityDataSeeder.SeedAsync(
                adminEmail: "1343261694@qq.com",
                adminPassword: "Admin@123456",
                tenantId: tenantId
            );

            // 2. 授予 admin 角色所有权限
            await SeedPermissionsAsync();

            // 3. 创建菜单数据（幂等）
            await SeedMenusAsync(tenantId);
        }
    }

    private async Task SeedPermissionsAsync()
    {
        var permissionNames = (await _permissionDefinitionManager.GetPermissionsAsync())
            .Select(p => p.Name)
            .ToArray();

        await _permissionDataSeeder.SeedAsync(
            RolePermissionValueProvider.ProviderName,
            AdminRoleName,
            permissionNames,
            _currentTenant.Id
        );
    }

    private async Task SeedMenusAsync(Guid? tenantId)
    {
        // 按名称检查是否已存在
        var existing = await _menuRepository.FindByNameAsync("仪表盘");
        if (existing != null) return;

        var dashboard = new Menu(_guidGenerator.Create(), "仪表盘", "/dashboard", "Odometer", null, 0, true) { TenantId = tenantId };
        await _menuRepository.InsertAsync(dashboard);

        var systemGroup = new Menu(_guidGenerator.Create(), "系统管理", "/system", "Setting", null, 100, true) { TenantId = tenantId };
        await _menuRepository.InsertAsync(systemGroup);

        var children = new List<Menu>
        {
            new(_guidGenerator.Create(), "用户管理", "/system/users", "User", systemGroup.Id, 0, true, "AbpIdentity.Users") { TenantId = tenantId },
            new(_guidGenerator.Create(), "角色管理", "/system/roles", "UserFilled", systemGroup.Id, 1, true, "AbpIdentity.Roles") { TenantId = tenantId },
            new(_guidGenerator.Create(), "租户管理", "/system/tenants", "OfficeBuilding", systemGroup.Id, 2, true, "AbpTenantManagement.Tenants") { TenantId = tenantId },
            new(_guidGenerator.Create(), "审计日志", "/system/audit-logs", "Document", systemGroup.Id, 3, true) { TenantId = tenantId },
            new(_guidGenerator.Create(), "菜单管理", "/system/menus", "Menu", systemGroup.Id, 4, true, "Menus.MenuItems") { TenantId = tenantId },
            new(_guidGenerator.Create(), "系统设置", "/system/settings", "Tools", systemGroup.Id, 5, true, "SettingManagement.Settings") { TenantId = tenantId },
        };

        foreach (var child in children)
            await _menuRepository.InsertAsync(child);

        // 授予 admin 角色所有菜单权限
        var allMenus = new List<Menu> { dashboard, systemGroup };
        allMenus.AddRange(children);

        foreach (var menu in allMenus)
            await _menuRolePermissionRepository.InsertAsync(
                new MenuRolePermission(_guidGenerator.Create(), menu.Id, AdminRoleName) { TenantId = tenantId });
    }
}
