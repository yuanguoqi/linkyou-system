using System;
using Volo.Abp.Application.Dtos;

namespace Linkyou.System.AuditLogs;

public class GetAuditLogListInput : PagedAndSortedResultRequestDto
{
    /// <summary>按 URL 过滤</summary>
    public string? Url { get; set; }
    /// <summary>按用户名过滤</summary>
    public string? UserName { get; set; }
    public string? HttpMethod { get; set; }
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
}
