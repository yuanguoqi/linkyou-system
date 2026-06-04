using System.Collections.Generic;
using System.Security.Claims;

namespace Linkyou.System.Jwt;

/// <summary>
/// JWT Token 服务接口（定义在 Application 层，Host 层实现）
/// </summary>
public interface IJwtTokenService
{
    /// <summary>生成访问令牌</summary>
    string GenerateAccessToken(IEnumerable<Claim> claims);

    /// <summary>生成刷新令牌（随机字符串）</summary>
    string GenerateRefreshToken();

    /// <summary>获取访问令牌有效期（秒）</summary>
    int GetAccessTokenExpiresInSeconds();
}
