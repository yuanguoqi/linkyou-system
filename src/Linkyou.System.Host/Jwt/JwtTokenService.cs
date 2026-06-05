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
    /// 获取访问令牌有效期（秒）
    /// </summary>
    public int GetAccessTokenExpiresInSeconds()
    {
        var minutes = int.Parse(
            _configuration["Jwt:AccessTokenExpirationMinutes"] ?? "60");
        return minutes * 60;
    }
}
