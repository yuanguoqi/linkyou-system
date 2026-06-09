using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading;
using System.Threading.Tasks;
using Linkyou.System.AuditLogs;
using Linkyou.System.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace Linkyou.System.AuditLogs;

public class EfAuditLogRepository :
    EfCoreRepository<LinkyouSystemDbContext, AuditLog, Guid>,
    IAuditLogRepository
{
    public EfAuditLogRepository(IDbContextProvider<LinkyouSystemDbContext> dbContextProvider)
        : base(dbContextProvider) { }

    public async Task<List<AuditLog>> GetListAsync(
        string? filter = null,
        string? httpMethod = null,
        DateTime? startTime = null,
        DateTime? endTime = null,
        int skipCount = 0,
        int maxResultCount = 10,
        string sorting = "CreationTime desc",
        CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();
        return await dbSet
            .AsNoTracking()
            .WhereIf(!filter.IsNullOrWhiteSpace(),
                x => x.Url.Contains(filter!) || x.UserName!.Contains(filter!))
            .WhereIf(!httpMethod.IsNullOrWhiteSpace(),
                x => x.HttpMethod == httpMethod)
            .WhereIf(startTime.HasValue,
                x => x.CreationTime >= startTime!.Value)
            .WhereIf(endTime.HasValue,
                x => x.CreationTime <= endTime!.Value)
            .OrderBy(sorting)
            .Skip(skipCount).Take(maxResultCount)
            .ToListAsync(cancellationToken);
    }

    public async Task<long> GetCountAsync(
        string? filter = null,
        string? httpMethod = null,
        DateTime? startTime = null,
        DateTime? endTime = null,
        CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();
        return await dbSet
            .WhereIf(!filter.IsNullOrWhiteSpace(),
                x => x.Url.Contains(filter!) || x.UserName!.Contains(filter!))
            .WhereIf(!httpMethod.IsNullOrWhiteSpace(),
                x => x.HttpMethod == httpMethod)
            .WhereIf(startTime.HasValue,
                x => x.CreationTime >= startTime!.Value)
            .WhereIf(endTime.HasValue,
                x => x.CreationTime <= endTime!.Value)
            .LongCountAsync(cancellationToken);
    }
}
