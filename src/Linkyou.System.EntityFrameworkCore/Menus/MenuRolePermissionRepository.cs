using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Linkyou.System.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace Linkyou.System.Menus;

public class MenuRolePermissionRepository(IDbContextProvider<LinkyouSystemDbContext> dbContextProvider)
    : LinkyouRepositoryBase<MenuRolePermission, Guid>(dbContextProvider), IMenuRolePermissionRepository
{
    public async Task<List<MenuRolePermission>> GetByRoleAsync(
        string roleName, CancellationToken ct = default) =>
        await (await GetDbSetAsync())
            .Where(x => x.RoleName == roleName)
            .ToListAsync(ct);

    public async Task<List<MenuRolePermission>> GetByMenuAsync(
        Guid menuId, CancellationToken ct = default) =>
        await (await GetDbSetAsync())
            .Where(x => x.MenuId == menuId)
            .ToListAsync(ct);

    public async Task<List<MenuRolePermission>> GetByRolesAsync(
        string[] roleNames, CancellationToken ct = default) =>
        await (await GetDbSetAsync())
            .Where(x => roleNames.Contains(x.RoleName))
            .GroupBy(x => x.MenuId)
            .Select(g => g.First())
            .ToListAsync(ct);

    public async Task DeleteByMenuAsync(Guid menuId, CancellationToken ct = default) =>
        await (await GetDbSetAsync())
            .Where(x => x.MenuId == menuId)
            .ExecuteDeleteAsync(ct);
}
