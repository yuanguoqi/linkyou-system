using System;

namespace Linkyou.System.Menus;

/// <summary>
/// 菜单角色权限 DTO
/// </summary>
public class MenuRolePermissionDto
{
    public Guid MenuId { get; set; }
    public string RoleName { get; set; } = null!;
}
