namespace Linkyou.System;

/// <summary>
/// 全局常量定义
/// </summary>
public static class LinkyouSystemConsts
{
    /// <summary>
    /// ABP 内置模块表名前缀（如 AbpAuditLogs、AbpRoles）
    /// 业务表不使用前缀，直接使用 PascalCase 命名（如 Menus、MenuRolePermissions）
    /// </summary>
    public const string DbTablePrefix = "";

    /// <summary>
    /// 数据库 Schema，PostgreSQL 中默认为 public
    /// </summary>
    public const string? DbSchema = null;
}
