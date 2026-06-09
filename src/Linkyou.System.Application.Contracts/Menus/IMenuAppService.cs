using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Linkyou.System.Menus;

public interface IMenuAppService : IApplicationService
{
    [HttpGet("{id}")]
    Task<MenuDto> GetAsync(Guid id);

    [HttpGet]
    Task<PagedResultDto<MenuDto>> GetListAsync(GetMenuListInput input);

    [HttpPost]
    Task<MenuDto> CreateAsync(CreateMenuDto input);

    [HttpPut("{id}")]
    Task<MenuDto> UpdateAsync(Guid id, UpdateMenuDto input);

    [HttpDelete("{id}")]
    Task DeleteAsync(Guid id);

    /// <summary>获取当前用户的菜单列表（根据角色权限过滤，树形结构）</summary>
    [HttpGet("user-menus")]
    Task<List<UserMenuDto>> GetUserMenusAsync();

    /// <summary>获取指定菜单的角色权限列表</summary>
    [HttpGet("{menuId}/role-permissions")]
    Task<List<MenuRolePermissionDto>> GetMenuRolePermissionsAsync(Guid menuId);

    /// <summary>设置菜单的角色权限（覆盖写入）</summary>
    [HttpPut("{menuId}/role-permissions")]
    Task SetMenuRolePermissionsAsync(Guid menuId, string[] roleNames);

    /// <summary>获取指定角色有权限的菜单 ID 列表</summary>
    [HttpGet("role/{roleName}/menu-ids")]
    Task<List<Guid>> GetRoleMenuIdsAsync(string roleName);

    /// <summary>设置角色的菜单权限（覆盖写入）</summary>
    [HttpPut("role/{roleName}/menu-ids")]
    Task SetRoleMenuIdsAsync(string roleName, Guid[] menuIds);
}
