using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Linkyou.System.Menus;

[Authorize(MenusPermissions.MenuItems.Default)]
public class MenuAppService : ApplicationService, IMenuAppService
{
    private readonly MenuManager _manager;
    private readonly IMenuRepository _repository;
    private readonly IMenuRolePermissionRepository _permissionRepository;

    public MenuAppService(
        MenuManager manager,
        IMenuRepository repository,
        IMenuRolePermissionRepository permissionRepository)
    {
        _manager = manager;
        _repository = repository;
        _permissionRepository = permissionRepository;
    }

    public async Task<MenuDto> GetAsync(Guid id)
    {
        var entity = await _repository.GetAsync(id);
        return ObjectMapper.Map<Menu, MenuDto>(entity);
    }

    public async Task<PagedResultDto<MenuDto>> GetListAsync(GetMenuListInput input)
    {
        var (items, count) = await _repository.GetPagedListAsync(input);
        return new PagedResultDto<MenuDto>(
            count, ObjectMapper.Map<List<Menu>, List<MenuDto>>(items));
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
        // 删除菜单时同步删除角色权限
        await _permissionRepository.DeleteByMenuAsync(id);
        await _repository.DeleteAsync(id);
    }

    /// <summary>
    /// 获取当前用户的菜单列表（根据角色权限过滤，树形结构）
    /// </summary>
    [Authorize]
    public async Task<List<UserMenuDto>> GetUserMenusAsync()
    {
        var userRoles = CurrentUser.Roles ?? Array.Empty<string>();

        // 超级管理员看到所有可见菜单
        var isSuperAdmin = userRoles.Contains("superadmin", StringComparer.OrdinalIgnoreCase);

        List<Menu> allMenus;

        var visibleMenus = await _repository.GetAllVisibleAsync();

        if (isSuperAdmin)
        {
            allMenus = visibleMenus;
        }
        else
        {
            var permissions = await _permissionRepository.GetByRolesAsync(userRoles);
            var allowedMenuIds = permissions.Select(p => p.MenuId).ToHashSet();
            allMenus = visibleMenus.Where(m => allowedMenuIds.Contains(m.Id)).ToList();
        }

        // 构建树形结构
        return BuildTree(allMenus, null);
    }

    /// <summary>
    /// 获取指定菜单的角色权限列表
    /// </summary>
    public async Task<List<MenuRolePermissionDto>> GetMenuRolePermissionsAsync(Guid menuId)
    {
        var permissions = await _permissionRepository.GetByMenuAsync(menuId);
        return permissions.Select(p => new MenuRolePermissionDto
        {
            MenuId = p.MenuId,
            RoleName = p.RoleName,
        }).ToList();
    }

    /// <summary>
    /// 设置菜单的角色权限（覆盖写入）
    /// </summary>
    [Authorize(MenusPermissions.MenuItems.Update)]
    public async Task SetMenuRolePermissionsAsync(Guid menuId, string[] roleNames)
    {
        // 验证菜单存在
        await _repository.GetAsync(menuId);

        // 删除旧权限
        await _permissionRepository.DeleteByMenuAsync(menuId);

        // 写入新权限
        foreach (var roleName in roleNames)
        {
            await _permissionRepository.InsertAsync(
                new MenuRolePermission(GuidGenerator.Create(), menuId, roleName));
        }
    }

    /// <summary>构建菜单树</summary>
    private static List<UserMenuDto> BuildTree(List<Menu> allMenus, Guid? parentId)
    {
        return allMenus
            .Where(m => m.ParentId == parentId)
            .OrderBy(m => m.Sort)
            .Select(m => new UserMenuDto
            {
                Id = m.Id,
                Name = m.Name,
                Path = m.Path,
                Icon = m.Icon,
                ParentId = m.ParentId,
                Sort = m.Sort,
                Children = BuildTree(allMenus, m.Id),
            })
            .ToList();
    }
}
