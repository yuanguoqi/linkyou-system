using System;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace Linkyou.System.Menus;

/// <summary>
/// 菜单角色权限关联表
/// 定义哪些角色可以访问哪些菜单
/// </summary>
public class MenuRolePermission : CreationAuditedEntity<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }

    /// <summary>菜单ID</summary>
    public Guid MenuId { get; private set; }

    /// <summary>角色名称（如 admin、superadmin）</summary>
    public string RoleName { get; private set; } = null!;

    protected MenuRolePermission() { }

    public MenuRolePermission(Guid id, Guid menuId, string roleName) : base(id)
    {
        MenuId = menuId;
        RoleName = roleName;
    }
}
