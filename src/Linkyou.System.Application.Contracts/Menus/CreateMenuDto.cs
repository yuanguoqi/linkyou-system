using System;
using System.ComponentModel.DataAnnotations;

namespace Linkyou.System.Menus;

public class CreateMenuDto
{
    [Required]
    [MaxLength(MenuConsts.MaxNameLength)]
    public required string Name { get; set; }

    [Required]
    [MaxLength(MenuConsts.MaxPathLength)]
    public required string Path { get; set; }

    [MaxLength(MenuConsts.MaxIconLength)]
    public string? Icon { get; set; }

    public Guid? ParentId { get; set; }
    public int Sort { get; set; }
    public bool IsVisible { get; set; } = true;

    [MaxLength(MenuConsts.MaxPermissionLength)]
    public string? Permission { get; set; }
}
