using System;
using Volo.Abp.Application.Dtos;

namespace Linkyou.System.AuditLogs;

public class GetAuditLogListInput : PagedAndSortedResultRequestDto
{
    public string? Filter { get; set; }
    public string? HttpMethod { get; set; }
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
}
