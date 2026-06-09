using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace Linkyou.System.AuditLogs;

public interface IAuditLogRepository : IRepository<AuditLog, Guid>
{
    Task<List<AuditLog>> GetListAsync(
        string? filter = null,
        string? httpMethod = null,
        DateTime? startTime = null,
        DateTime? endTime = null,
        int skipCount = 0,
        int maxResultCount = 10,
        string sorting = "CreationTime desc",
        CancellationToken cancellationToken = default);

    Task<long> GetCountAsync(
        string? filter = null,
        string? httpMethod = null,
        DateTime? startTime = null,
        DateTime? endTime = null,
        CancellationToken cancellationToken = default);
}
