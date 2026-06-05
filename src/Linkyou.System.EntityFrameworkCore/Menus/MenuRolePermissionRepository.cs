
using Linkyou.System.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace Linkyou.System.Menus;

public class MenuRolePermissionRepository :
    EfCoreRepository<LinkyouSystemDbContext, MenuRolePermission, Guid>,
    IMenuRolePermissionRepository
{
    public MenuRolePermissionRepository(IDbContextProvider<LinkyouSystemDbContext> dbContextProvider)
        : base(dbContextProvider) { }

    public async Task<List<MenuRolePermission>> GetByRoleAsync(
        string roleName, CancellationToken ct = default)
    {
        var dbSet = await GetDbSetAsync();
        return await dbSet
            .Where(x => x.RoleName == roleName)
            .ToListAsync(ct);
    }

    public async Task<List<MenuRolePermission>> GetByMenuAsync(
        Guid menuId, CancellationToken ct = default)
    {
        var dbSet = await GetDbSetAsync();
        return await dbSet
            .Where(x => x.MenuId == menuId)
            .ToListAsync(ct);
    }

    public async Task<List<MenuRolePermission>> GetByRolesAsync(
        string[] roleNames, CancellationToken ct = default)
    {
        var dbSet = await GetDbSetAsync();
        return await dbSet
            .Where(x => roleNames.Contains(x.RoleName))
            .GroupBy(x => x.MenuId)
            .Select(g => g.First())
            .ToListAsync(ct);
    }

    public async Task DeleteByMenuAsync(Guid menuId, CancellationToken ct = default)
    {
        var dbSet = await GetDbSetAsync();
        var items = await dbSet.Where(x => x.MenuId == menuId).ToListAsync(ct);
        dbSet.RemoveRange(items);
    }
}
