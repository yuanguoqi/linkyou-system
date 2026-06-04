namespace Linkyou.System;

/// <summary>
/// 全局常量定义
/// </summary>
public static class LinkyouSystemConsts
{
    /// <summary>
    /// 数据库表名前缀，所有业务表统一以 Lys_ 开头
    /// </summary>
    public const string DbTablePrefix = "Lys_";

    /// <summary>
    /// 数据库 Schema，PostgreSQL 中默认为 public
    /// </summary>
    public const string? DbSchema = null;
}
