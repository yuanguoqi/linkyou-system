using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using Volo.Abp.Application.Dtos;

namespace Linkyou.System.AuditLogs;

[RemoteService]
[Area("app")]
[Route("api/audit-logging/audit-logs")]
public class AuditLogController : Linkyou.System.Controllers.LinkyouSystemController, IAuditLogAppService
{
    private readonly IAuditLogAppService _appService;

    public AuditLogController(IAuditLogAppService appService)
        => _appService = appService;

    [HttpGet("{id}")]
    public Task<AuditLogDto> GetAsync(Guid id) => _appService.GetAsync(id);

    [HttpGet]
    public Task<PagedResultDto<AuditLogDto>> GetListAsync([FromQuery] GetAuditLogListInput input)
        => _appService.GetListAsync(input);
}
