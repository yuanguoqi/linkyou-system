using System;
using Volo.Abp.Application.Dtos;

namespace Linkyou.System.AuditLogs;

public class AuditLogDto : EntityDto<Guid>
{
    public string? UserId { get; set; }
    public string? UserName { get; set; }
    public string? TenantId { get; set; }
    public string? ApplicationName { get; set; }
    public string? ClientIpAddress { get; set; }
    public string? HttpMethod { get; set; }
    public string? Url { get; set; }
    public int? HttpStatusCode { get; set; }
    public int ExecutionDuration { get; set; }
    public DateTime ExecutionTime { get; set; }
    public string? Exceptions { get; set; }
    public string? BrowserInfo { get; set; }
}
