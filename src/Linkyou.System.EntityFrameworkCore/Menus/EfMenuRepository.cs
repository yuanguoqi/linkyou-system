using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading;
using System.Threading.Tasks;
using Linkyou.System.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace Linkyou.System.Menus;

public class EfMenuRepository :
    EfCoreRepository<LinkyouSystemDbContext, Menu, Guid>,
    IMenuRepository
{
    public EfMenuRepository(IDbContextProvider<LinkyouSystemDbContext> dbContextProvider)
        : base(dbContextProvider) { }

    public async Task<Menu?> FindByNameAsync(string name, CancellationToken ct = default)
    {
        var dbSet = await GetDbSetAsync();
        return await dbSet.FirstOrDefaultAsync(x => x.Name == name, ct);
    }

    public async Task<List<Menu>> GetListAsync(
        string? filter = null, int skipCount = 0, int maxResultCount = 10,
        string sorting = "Sort", CancellationToken ct = default)
    {
        var dbSet = await GetDbSetAsync();
        return await dbSet
            .AsNoTracking()
            .WhereIf(!filter.IsNullOrWhiteSpace(), x => x.Name.Contains(filter!))
            .OrderBy(sorting)
            .Skip(skipCount).Take(maxResultCount)
            .ToListAsync(ct);
    }

    public async Task<long> GetCountAsync(string? filter = null, CancellationToken ct = default)
    {
        var dbSet = await GetDbSetAsync();
        return await dbSet
            .WhereIf(!filter.IsNullOrWhiteSpace(), x => x.Name.Contains(filter!))
            .LongCountAsync(ct);
    }
}
