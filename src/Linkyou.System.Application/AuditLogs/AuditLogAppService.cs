using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.AuditLogging;

namespace Linkyou.System.AuditLogs;

[Authorize]
public class AuditLogAppService(IAuditLogRepository auditLogRepository)
    : ApplicationService, IAuditLogAppService
{
    public async Task<AuditLogDto> GetAsync(Guid id)
    {
        var log = await auditLogRepository.GetAsync(id);
        return Map(log);
    }

    public async Task<PagedResultDto<AuditLogDto>> GetListAsync(GetAuditLogListInput input)
    {
        var count = await auditLogRepository.GetCountAsync(
            startTime: input.StartTime,
            endTime: input.EndTime,
            httpMethod: input.HttpMethod,
            url: input.Filter,
            userName: input.Filter);

        var items = await auditLogRepository.GetListAsync(
            sorting: input.Sorting ?? "executionTime desc",
            maxResultCount: input.MaxResultCount,
            skipCount: input.SkipCount,
            startTime: input.StartTime,
            endTime: input.EndTime,
            httpMethod: input.HttpMethod,
            url: input.Filter,
            userName: input.Filter);

        return new PagedResultDto<AuditLogDto>(count, items.ConvertAll(Map));
    }

    private static AuditLogDto Map(Volo.Abp.AuditLogging.AuditLog log) => new()
    {
        Id = log.Id,
        UserId = log.UserId?.ToString(),
        UserName = log.UserName,
        TenantId = log.TenantId?.ToString(),
        ApplicationName = log.ApplicationName,
        ClientIpAddress = log.ClientIpAddress,
        HttpMethod = log.HttpMethod,
        Url = log.Url,
        HttpStatusCode = log.HttpStatusCode,
        ExecutionDuration = log.ExecutionDuration,
        ExecutionTime = log.ExecutionTime,
        Exceptions = log.Exceptions,
        BrowserInfo = log.BrowserInfo,
    };
}
