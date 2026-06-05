using System;
using System.Collections.Generic;
using Volo.Abp.Application.Dtos;

namespace Linkyou.System.Menus;

/// <summary>
/// 用户菜单 DTO（树形结构）
/// </summary>
public class UserMenuDto : EntityDto<Guid>
{
    public string Name { get; set; } = null!;
    public string Path { get; set; } = null!;
    public string? Icon { get; set; }
    public Guid? ParentId { get; set; }
    public int Sort { get; set; }
    public List<UserMenuDto> Children { get; set; } = new();
}
