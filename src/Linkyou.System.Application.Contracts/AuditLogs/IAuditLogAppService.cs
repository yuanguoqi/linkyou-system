using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Linkyou.System.AuditLogs;

public interface IAuditLogAppService : IApplicationService
{
    [HttpGet("{id}")]
    Task<AuditLogDto> GetAsync(Guid id);

    [HttpGet]
    Task<PagedResultDto<AuditLogDto>> GetListAsync(GetAuditLogListInput input);
}
