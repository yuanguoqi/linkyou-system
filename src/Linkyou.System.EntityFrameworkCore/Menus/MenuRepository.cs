using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading;
using System.Threading.Tasks;
using Linkyou.System.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace Linkyou.System.Menus;

public class MenuRepository(IDbContextProvider<LinkyouSystemDbContext> dbContextProvider)
    : LinkyouRepositoryBase<Menu, Guid>(dbContextProvider), IMenuRepository
{
    public async Task<Menu?> FindByNameAsync(string name, CancellationToken ct = default) =>
        await (await GetDbSetAsync()).FirstOrDefaultAsync(x => x.Name == name, ct);

    public async Task<List<Menu>> GetListAsync(
        string? filter = null, int skipCount = 0, int maxResultCount = 10,
        string sorting = nameof(Menu.Sort), CancellationToken ct = default)
    {
        var query = (await GetDbSetAsync())
            .WhereIf(!filter.IsNullOrWhiteSpace(), x => x.Name.Contains(filter!));
        return await QueryListAsync(query, skipCount, maxResultCount, sorting, ct);
    }

    public async Task<long> GetCountAsync(string? filter = null, CancellationToken ct = default)
    {
        var query = (await GetDbSetAsync())
            .WhereIf(!filter.IsNullOrWhiteSpace(), x => x.Name.Contains(filter!));
        return await QueryCountAsync(query, ct);
    }

    public async Task<List<Menu>> GetAllVisibleAsync(CancellationToken ct = default) =>
        await (await GetDbSetAsync())
            .AsNoTracking()
            .Where(x => x.IsVisible)
            .OrderBy(x => x.Sort)
            .ToListAsync(ct);
}
