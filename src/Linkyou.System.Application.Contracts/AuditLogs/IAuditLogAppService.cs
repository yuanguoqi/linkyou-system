using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Linkyou.System.AuditLogs;

public interface IAuditLogAppService : IApplicationService
{
    Task<AuditLogDto> GetAsync(Guid id);
    Task<PagedResultDto<AuditLogDto>> GetListAsync(GetAuditLogListInput input);
}
