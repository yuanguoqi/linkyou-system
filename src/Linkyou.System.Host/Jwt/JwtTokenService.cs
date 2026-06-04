using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Volo.Abp.DependencyInjection;
using Linkyou.System.Jwt;

namespace Linkyou.System.Host.Jwt;

/// <summary>
/// JWT Token 生成服务
/// 负责签发 AccessToken 和 RefreshToken
/// </summary>
public class JwtTokenService : IJwtTokenService, ITransientDependency
{
    private readonly IConfiguration _configuration;

    public JwtTokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    /// <summary>
    /// 生成访问令牌（短期，默认 60 分钟）
    /// </summary>
    public string GenerateAccessToken(IEnumerable<Claim> claims)
    {
        var signingKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration["Jwt:SigningKey"]!));

        var expireMinutes = int.Parse(
            _configuration["Jwt:AccessTokenExpirationMinutes"] ?? "60");

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.AddMinutes(expireMinutes),
            signingCredentials: new SigningCredentials(
                signingKey,
                SecurityAlgorithms.HmacSha256));

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>
    /// 生成刷新令牌（长期，随机字符串，存入数据库）
    /// </summary>
    public string GenerateRefreshToken()
    {
        var bytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes);
    }

    /// <summary>
    /// 从已过期的 AccessToken 中读取 Claims（用于刷新令牌时验证身份）
    /// </summary>
    public ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
    {
        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = _configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = _configuration["Jwt:Audience"],
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["Jwt:SigningKey"]!)),
            // 允许已过期的 Token，刷新时必须关闭此校验
            ValidateLifetime = false,
        };

        var handler = new JwtSecurityTokenHandler();
        var principal = handler.ValidateToken(token, validationParameters, out var securityToken);

        if (securityToken is not JwtSecurityToken jwtToken
            || !jwtToken.Header.Alg.Equals(
                SecurityAlgorithms.HmacSha256,
                StringComparison.InvariantCultureIgnoreCase))
        {
            throw new SecurityTokenException("无效的令牌");
        }

        return principal;
    }

    /// <summary>
    /// 获取访问令牌有效期（秒）
    /// </summary>
    public int GetAccessTokenExpiresInSeconds()
    {
        var minutes = int.Parse(
            _configuration["Jwt:AccessTokenExpirationMinutes"] ?? "60");
        return minutes * 60;
    }
}
