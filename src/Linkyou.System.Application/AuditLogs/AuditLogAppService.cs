using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Linkyou.System.AuditLogs;

[Authorize]
public class AuditLogAppService : ApplicationService, IAuditLogAppService
{
    private readonly IAuditLogRepository _repository;

    public AuditLogAppService(IAuditLogRepository repository)
    {
        _repository = repository;
    }

    public async Task<AuditLogDto> GetAsync(Guid id)
    {
        var entity = await _repository.GetAsync(id);
        return ObjectMapper.Map<AuditLog, AuditLogDto>(entity);
    }

    public async Task<PagedResultDto<AuditLogDto>> GetListAsync(GetAuditLogListInput input)
    {
        var count = await _repository.GetCountAsync(
            input.Filter, input.HttpMethod, input.StartTime, input.EndTime);

        var list = await _repository.GetListAsync(
            input.Filter, input.HttpMethod, input.StartTime, input.EndTime,
            input.SkipCount, input.MaxResultCount,
            input.Sorting ?? "CreationTime desc");

        return new PagedResultDto<AuditLogDto>(
            count, ObjectMapper.Map<List<AuditLog>, List<AuditLogDto>>(list));
    }
}
