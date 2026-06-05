using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Services;

namespace Linkyou.System.Menus;

public class MenuManager : DomainService
{
    private readonly IMenuRepository _repository;

    public MenuManager(IMenuRepository repository)
    {
        _repository = repository;
    }

    public async Task<Menu> CreateAsync(string name, string path, string? icon = null,
        Guid? parentId = null, int sort = 0, bool isVisible = true, string? permission = null)
    {
        await EnsureNameNotExistsAsync(name);
        return new Menu(GuidGenerator.Create(), name, path, icon, parentId, sort, isVisible, permission);
    }

    public async Task UpdateAsync(Menu entity, string name, string path, string? icon,
        Guid? parentId, int sort, bool isVisible, string? permission)
    {
        if (entity.Name != name)
            await EnsureNameNotExistsAsync(name);
        entity.Update(name, path, icon, parentId, sort, isVisible, permission);
    }

    private async Task EnsureNameNotExistsAsync(string name)
    {
        var existing = await _repository.FindByNameAsync(name);
        if (existing != null)
            throw new BusinessException(MenuErrorCodes.NameAlreadyExists).WithData("name", name);
    }
}
