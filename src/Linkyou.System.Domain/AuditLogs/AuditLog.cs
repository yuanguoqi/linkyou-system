using System;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace Linkyou.System.AuditLogs;

/// <summary>
/// 审计日志实体
/// </summary>
public class AuditLog : CreationAuditedEntity<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }

    /// <summary>HTTP 方法</summary>
    public string HttpMethod { get; set; } = null!;

    /// <summary>请求 URL</summary>
    public string Url { get; set; } = null!;

    /// <summary>HTTP 状态码</summary>
    public int HttpStatusCode { get; set; }

    /// <summary>执行耗时（毫秒）</summary>
    public int ExecutionDuration { get; set; }

    /// <summary>客户端 IP</summary>
    public string? ClientIpAddress { get; set; }

    /// <summary>用户名</summary>
    public string? UserName { get; set; }

    /// <summary>异常信息</summary>
    public string? Exceptions { get; set; }

    protected AuditLog() { }

    public AuditLog(
        Guid id,
        string httpMethod,
        string url,
        int httpStatusCode,
        int executionDuration,
        string? clientIpAddress = null,
        string? userName = null,
        string? exceptions = null) : base(id)
    {
        HttpMethod = httpMethod;
        Url = url;
        HttpStatusCode = httpStatusCode;
        ExecutionDuration = executionDuration;
        ClientIpAddress = clientIpAddress;
        UserName = userName;
        Exceptions = exceptions;
    }
}
