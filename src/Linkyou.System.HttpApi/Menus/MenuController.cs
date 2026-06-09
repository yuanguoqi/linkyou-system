using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Linkyou.System.Controllers;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using Volo.Abp.Application.Dtos;

namespace Linkyou.System.Menus;

[RemoteService]
[Area("app")]
[Route("api/app/menus")]
public class MenuController : LinkyouSystemController, IMenuAppService
{
    private readonly IMenuAppService _appService;

    public MenuController(IMenuAppService appService)
        => _appService = appService;

    [HttpGet("{id}")]
    public Task<MenuDto> GetAsync(Guid id) => _appService.GetAsync(id);

    [HttpGet]
    public Task<PagedResultDto<MenuDto>> GetListAsync([FromQuery] GetMenuListInput input)
        => _appService.GetListAsync(input);

    [HttpPost]
    public Task<MenuDto> CreateAsync(CreateMenuDto input) => _appService.CreateAsync(input);

    [HttpPut("{id}")]
    public Task<MenuDto> UpdateAsync(Guid id, UpdateMenuDto input)
        => _appService.UpdateAsync(id, input);

    [HttpDelete("{id}")]
    public Task DeleteAsync(Guid id) => _appService.DeleteAsync(id);

    [HttpGet("user-menus")]
    public Task<List<UserMenuDto>> GetUserMenusAsync()
        => _appService.GetUserMenusAsync();

    [HttpGet("{menuId}/role-permissions")]
    public Task<List<MenuRolePermissionDto>> GetMenuRolePermissionsAsync(Guid menuId)
        => _appService.GetMenuRolePermissionsAsync(menuId);

    [HttpPut("{menuId}/role-permissions")]
    public Task SetMenuRolePermissionsAsync(Guid menuId, [FromBody] string[] roleNames)
        => _appService.SetMenuRolePermissionsAsync(menuId, roleNames);

    [HttpGet("role/{roleName}/menu-ids")]
    public Task<List<Guid>> GetRoleMenuIdsAsync(string roleName)
        => _appService.GetRoleMenuIdsAsync(roleName);

    [HttpPut("role/{roleName}/menu-ids")]
    public Task SetRoleMenuIdsAsync(string roleName, [FromBody] Guid[] menuIds)
        => _appService.SetRoleMenuIdsAsync(roleName, menuIds);
}
