using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Linkyou.System.Menus;

[Authorize(MenusPermissions.MenuItems.Default)]
public class MenuAppService : ApplicationService, IMenuAppService
{
    private readonly MenuManager _manager;
    private readonly IMenuRepository _repository;

    public MenuAppService(MenuManager manager, IMenuRepository repository)
    {
        _manager = manager;
        _repository = repository;
    }

    public async Task<MenuDto> GetAsync(Guid id)
    {
        var entity = await _repository.GetAsync(id);
        return ObjectMapper.Map<Menu, MenuDto>(entity);
    }

    public async Task<PagedResultDto<MenuDto>> GetListAsync(GetMenuListInput input)
    {
        var count = await _repository.GetCountAsync(input.Filter);
        var list = await _repository.GetListAsync(
            input.Filter, input.SkipCount, input.MaxResultCount,
            input.Sorting ?? nameof(Menu.Sort));
        return new PagedResultDto<MenuDto>(
            count, ObjectMapper.Map<List<Menu>, List<MenuDto>>(list));
    }

    [Authorize(MenusPermissions.MenuItems.Create)]
    public async Task<MenuDto> CreateAsync(CreateMenuDto input)
    {
        var entity = await _manager.CreateAsync(
            input.Name, input.Path, input.Icon, input.ParentId,
            input.Sort, input.IsVisible, input.Permission);
        await _repository.InsertAsync(entity);
        return ObjectMapper.Map<Menu, MenuDto>(entity);
    }

    [Authorize(MenusPermissions.MenuItems.Update)]
    public async Task<MenuDto> UpdateAsync(Guid id, UpdateMenuDto input)
    {
        var entity = await _repository.GetAsync(id);
        await _manager.UpdateAsync(entity,
            input.Name, input.Path, input.Icon, input.ParentId,
            input.Sort, input.IsVisible, input.Permission);
        return ObjectMapper.Map<Menu, MenuDto>(entity);
    }

    [Authorize(MenusPermissions.MenuItems.Delete)]
    public async Task DeleteAsync(Guid id)
    {
        await _repository.DeleteAsync(id);
    }
}
