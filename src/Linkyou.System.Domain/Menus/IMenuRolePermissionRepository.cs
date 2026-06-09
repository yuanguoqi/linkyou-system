using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace Linkyou.System.Menus;

public interface IMenuRolePermissionRepository : IRepository<MenuRolePermission, Guid>
{
    /// <summary>获取指定角色的所有菜单权限</summary>
    Task<List<MenuRolePermission>> GetByRoleAsync(
        string roleName,
        CancellationToken cancellationToken = default);

    /// <summary>获取指定菜单的所有角色权限</summary>
    Task<List<MenuRolePermission>> GetByMenuAsync(
        Guid menuId,
        CancellationToken cancellationToken = default);

    /// <summary>获取指定角色列表有权限的菜单 ID 集合（去重）</summary>
    Task<List<Guid>> GetMenuIdsByRolesAsync(
        string[] roleNames,
        CancellationToken cancellationToken = default);

    /// <summary>删除指定菜单的所有角色权限</summary>
    Task DeleteByMenuAsync(
        Guid menuId,
        CancellationToken cancellationToken = default);

    /// <summary>获取指定角色有权限的菜单 ID 集合</summary>
    Task<List<Guid>> GetMenuIdsByRoleAsync(
        string roleName,
        CancellationToken cancellationToken = default);

    /// <summary>删除指定角色的所有菜单权限</summary>
    Task DeleteByRoleAsync(
        string roleName,
        CancellationToken cancellationToken = default);
}
