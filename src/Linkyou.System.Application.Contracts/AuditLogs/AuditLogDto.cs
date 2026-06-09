using System;
using Volo.Abp.Application.Dtos;

namespace Linkyou.System.AuditLogs;

public class AuditLogDto : CreationAuditedEntityDto<Guid>
{
    public string HttpMethod { get; set; } = null!;
    public string Url { get; set; } = null!;
    public int HttpStatusCode { get; set; }
    public int ExecutionDuration { get; set; }
    public string? ClientIpAddress { get; set; }
    public string? UserName { get; set; }
    public string? Exceptions { get; set; }
}
