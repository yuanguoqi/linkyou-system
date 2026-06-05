using System;
using Volo.Abp.Application.Dtos;

namespace Linkyou.System.Menus;

public class MenuDto : FullAuditedEntityDto<Guid>
{
    public string Name { get; set; } = null!;
    public string Path { get; set; } = null!;
    public string? Icon { get; set; }
    public Guid? ParentId { get; set; }
    public int Sort { get; set; }
    public bool IsVisible { get; set; }
    public string? Permission { get; set; }
}
