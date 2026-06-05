using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace Linkyou.System.Menus;

public class Menu : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }
    public string Name { get; private set; } = null!;
    public string Path { get; private set; } = null!;
    public string? Icon { get; private set; }
    public Guid? ParentId { get; private set; }
    public int Sort { get; private set; }
    public bool IsVisible { get; private set; }
    public string? Permission { get; private set; }

    protected Menu() { }

    internal Menu(Guid id, string name, string path, string? icon = null,
        Guid? parentId = null, int sort = 0, bool isVisible = true, string? permission = null)
        : base(id)
    {
        SetName(name);
        SetPath(path);
        Icon = icon;
        ParentId = parentId;
        Sort = sort;
        IsVisible = isVisible;
        Permission = permission;
    }

    internal void SetName(string name)
    {
        Name = Check.NotNullOrWhiteSpace(name, nameof(name), MenuConsts.MaxNameLength);
    }

    internal void SetPath(string path)
    {
        Path = Check.NotNullOrWhiteSpace(path, nameof(path), MenuConsts.MaxPathLength);
    }

    internal void Update(string name, string path, string? icon, Guid? parentId,
        int sort, bool isVisible, string? permission)
    {
        SetName(name);
        SetPath(path);
        Icon = icon;
        ParentId = parentId;
        Sort = sort;
        IsVisible = isVisible;
        Permission = permission;
    }
}
