namespace Linkyou.System;

/// <summary>
/// 业务异常错误码常量
/// 使用 LinkyouSystem: 前缀，与本地化资源映射
/// </summary>
public static class LinkyouSystemErrorCodes
{
    // 通用错误
    public const string NotFound = "LinkyouSystem:NotFound";
    public const string AlreadyExists = "LinkyouSystem:AlreadyExists";
    public const string InvalidInput = "LinkyouSystem:InvalidInput";

    // 用户相关
    public const string UserNotFound = "LinkyouSystem:UserNotFound";
    public const string UserAlreadyExists = "LinkyouSystem:UserAlreadyExists";

    // 租户相关
    public const string TenantNotFound = "LinkyouSystem:TenantNotFound";
}
