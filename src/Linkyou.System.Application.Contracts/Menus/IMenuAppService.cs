using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Linkyou.System.Menus;

public interface IMenuAppService : IApplicationService
{
    Task<MenuDto> GetAsync(Guid id);
    Task<PagedResultDto<MenuDto>> GetListAsync(GetMenuListInput input);
    Task<MenuDto> CreateAsync(CreateMenuDto input);
    Task<MenuDto> UpdateAsync(Guid id, UpdateMenuDto input);
    Task DeleteAsync(Guid id);

    /// <summary>获取当前用户的菜单列表（根据角色权限过滤，树形结构）</summary>
    Task<List<UserMenuDto>> GetUserMenusAsync();

    /// <summary>获取指定菜单的角色权限列表</summary>
    Task<List<MenuRolePermissionDto>> GetMenuRolePermissionsAsync(Guid menuId);

    /// <summary>设置菜单的角色权限（覆盖写入）</summary>
    Task SetMenuRolePermissionsAsync(Guid menuId, string[] roleNames);
}
