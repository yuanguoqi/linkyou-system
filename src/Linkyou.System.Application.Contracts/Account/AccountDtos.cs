using System.ComponentModel.DataAnnotations;

namespace Linkyou.System.Account;

/// <summary>
/// 登录请求 DTO
/// </summary>
public class LoginInput
{
    /// <summary>
    /// 用户名或邮箱
    /// </summary>
    [Required(ErrorMessage = "用户名不能为空")]
    [MaxLength(256)]
    public required string UserNameOrEmailAddress { get; set; }

    /// <summary>
    /// 密码
    /// </summary>
    [Required(ErrorMessage = "密码不能为空")]
    [MaxLength(128)]
    public required string Password { get; set; }

    /// <summary>
    /// 记住我（影响 RefreshToken 有效期）
    /// </summary>
    public bool RememberMe { get; set; }
}

/// <summary>
/// 登录响应 DTO
/// </summary>
public class LoginOutput
{
    /// <summary>访问令牌</summary>
    public required string AccessToken { get; set; }

    /// <summary>刷新令牌</summary>
    public required string RefreshToken { get; set; }

    /// <summary>访问令牌过期时间（秒）</summary>
    public int ExpiresIn { get; set; }

    /// <summary>令牌类型，固定为 Bearer</summary>
    public string TokenType { get; set; } = "Bearer";
}

/// <summary>
/// 刷新令牌请求 DTO
/// </summary>
public class RefreshTokenInput
{
    [Required]
    public required string RefreshToken { get; set; }

    /// <summary>
    /// 用户 ID（AccessToken 过期后无法从 CurrentUser 获取，由前端传入）
    /// </summary>
    [Required]
    public required Guid UserId { get; set; }
}

/// <summary>
/// 修改密码请求 DTO
/// </summary>
public class ChangePasswordInput
{
    [Required]
    [MaxLength(128)]
    public required string CurrentPassword { get; set; }

    [Required]
    [MinLength(6)]
    [MaxLength(128)]
    public required string NewPassword { get; set; }
}

/// <summary>
/// 当前用户信息 DTO
/// </summary>
public class CurrentUserDto
{
    public required string Id { get; set; }
    public required string UserName { get; set; }
    public string? Name { get; set; }
    public string? SurName { get; set; }
    public required string Email { get; set; }
    public string? PhoneNumber { get; set; }
    public Guid? TenantId { get; set; }
    public List<string> Roles { get; set; } = [];
    public List<string> Permissions { get; set; } = [];
}
