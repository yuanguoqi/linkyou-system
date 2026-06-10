using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Volo.Abp.Auditing;
using Volo.Abp.Authorization;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Volo.Abp;
using Volo.Abp.AspNetCore.Authentication.JwtBearer;
using Volo.Abp.AspNetCore.MultiTenancy;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc.AntiForgery;
using Volo.Abp.AspNetCore.Serilog;
using Volo.Abp.Autofac;
using Volo.Abp.Caching;
using Volo.Abp.Caching.StackExchangeRedis;
using Volo.Abp.Localization;
using Volo.Abp.Modularity;
using Volo.Abp.Swashbuckle;
using Linkyou.System.EntityFrameworkCore;
using Linkyou.System.Localization;

namespace Linkyou.System;

/// <summary>
/// Host 模块：应用程序入口，组装所有模块，配置中间件管道
/// </summary>
[DependsOn(
    // 业务层模块
    typeof(LinkyouSystemHttpApiModule),
    typeof(LinkyouSystemApplicationModule),
    typeof(LinkyouSystemEntityFrameworkCoreModule),
    // ABP 基础设施
    typeof(AbpAutofacModule),                          // Autofac IOC 容器
    typeof(AbpCachingStackExchangeRedisModule),         // Redis 分布式缓存
    typeof(AbpAspNetCoreMultiTenancyModule),            // 多租户（从请求头识别）
    typeof(AbpAspNetCoreAuthenticationJwtBearerModule), // JWT Bearer 认证
    typeof(AbpAspNetCoreSerilogModule),                 // Serilog 结构化日志
    typeof(AbpSwashbuckleModule)                        // Swagger 文档
)]
public class LinkyouSystemHostModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var configuration = context.Services.GetConfiguration();
        var hostingEnvironment = context.Services.GetHostingEnvironment();

        ConfigureAuthentication(context, configuration);
        ConfigureLocalization();
        ConfigureCors(context, configuration);
        ConfigureSwagger(context, configuration);
        ConfigureAntiForgery();
        ConfigureAuditing();
        ConfigureCache(context);

        // 开发环境：替换方法调用授权服务，允许所有已认证用户访问
        // 注意：必须在所有模块初始化之后执行，确保覆盖 ABP 默认实现
        context.Services.Replace(
            ServiceDescriptor.Singleton<IMethodInvocationAuthorizationService, Linkyou.System.Authentication.DevMethodInvocationAuthorizationService>());
    }

    /// <summary>
    /// 配置 JWT Bearer 认证（本系统自签 JWT，直接验证签名密钥，无需外部授权服务器）
    /// </summary>
    private static void ConfigureAuthentication(
        ServiceConfigurationContext context,
        IConfiguration configuration)
    {
        var signingKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(configuration["Jwt:SigningKey"]!));

        context.Services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata =
                    Convert.ToBoolean(configuration["AuthServer:RequireHttpsMetadata"]);

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    // 验证签名密钥
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = signingKey,
                    // 验证颁发者
                    ValidateIssuer = true,
                    ValidIssuer = configuration["Jwt:Issuer"],
                    // 验证受众
                    ValidateAudience = true,
                    ValidAudience = configuration["Jwt:Audience"],
                    // 验证过期时间
                    ValidateLifetime = true,
                    // 允许 1 分钟的时钟偏差
                    ClockSkew = TimeSpan.FromMinutes(1),
                };
            });
    }

    /// <summary>
    /// 配置多语言本地化（中文、英文）
    /// </summary>
    private void ConfigureLocalization()
    {
        Configure<AbpLocalizationOptions>(options =>
        {
            // 支持的语言列表（ABP 10.x LanguageInfo 构造函数：cultureCode, uiCultureCode, displayName）
            options.Languages.Add(new LanguageInfo("zh-Hans", "zh-Hans", "简体中文"));
            options.Languages.Add(new LanguageInfo("en", "en", "English"));
        });
    }

    /// <summary>
    /// 配置跨域（CORS）
    /// </summary>
    private void ConfigureCors(ServiceConfigurationContext context, IConfiguration configuration)
    {
        context.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(builder =>
            {
                builder
                    // 允许的前端域名（从配置文件读取）
                    .WithOrigins(
                        configuration["App:CorsOrigins"]!
                            .Split(",", StringSplitOptions.RemoveEmptyEntries)
                            .Select(o => o.RemovePostFix("/"))
                            .ToArray()
                    )
                    .WithExposedHeaders("grpc-status", "grpc-message", "Grpc-Status", "Grpc-Message", "Grpc-Encoding", "Grpc-Accept-Encoding")
                    .SetIsOriginAllowedToAllowWildcardSubdomains()
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });
    }

    /// <summary>
    /// 配置 Swagger API 文档（JWT Bearer 认证）
    /// </summary>
    private void ConfigureSwagger(ServiceConfigurationContext context, IConfiguration configuration)
    {
        context.Services.AddAbpSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "领佑通用管理系统 API",
                Version = "v1",
                Description = "基于 ABP vNext 10.4 的企业级管理系统接口文档"
            });
            options.DocInclusionPredicate((_, __) => true);
            options.CustomSchemaIds(type => type.FullName);

            // 添加 JWT Bearer 认证支持
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "请输入 JWT Token，格式：Bearer {token}"
            });
            options.AddSecurityRequirement(_ => new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecuritySchemeReference("Bearer", null),
                    new List<string>()
                }
            });
        });
    }

    /// <summary>
    /// 关闭 CSRF 防护（API 模式下使用 JWT，不需要 Cookie CSRF 保护）
    /// </summary>
    private void ConfigureAntiForgery()
    {
        Configure<AbpAntiForgeryOptions>(options =>
        {
            options.AutoValidate = false;
        });
    }

    /// <summary>
    /// 配置审计日志：记录所有 API 调用和实体变更
    /// </summary>
    private void ConfigureAuditing()
    {
        Configure<AbpAuditingOptions>(options =>
        {
            options.IsEnabled = true;
            options.IsEnabledForAnonymousUsers = false;
            options.IsEnabledForGetRequests = true;
        });
    }

    /// <summary>
    /// 配置 Redis 分布式缓存
    /// </summary>
    private void ConfigureCache(ServiceConfigurationContext context)
    {
        var configuration = context.Services.GetConfiguration();
        var redis = configuration["Redis:Configuration"];
        if (!string.IsNullOrEmpty(redis))
        {
            Configure<AbpDistributedCacheOptions>(options =>
            {
                options.KeyPrefix = "LinkyouSystem:";
            });
        }
    }

    public override void OnApplicationInitialization(ApplicationInitializationContext context)
    {
        var app = context.GetApplicationBuilder();
        var env = context.GetEnvironment();

        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        // ABP 安全日志（记录认证相关操作）
        app.UseAbpSecurityHeaders();

        // 静态文件（用于 Swagger UI 等）
        app.UseStaticFiles();

        // 路由
        app.UseRouting();

        // 跨域（必须在 UseAuthentication 之前）
        app.UseCors();

        // 多租户识别（从请求头 __tenant 读取）
        app.UseMultiTenancy();

        // 本地化（从请求头 Accept-Language 读取）
        app.UseAbpRequestLocalization();

        // 认证 & 授权
        app.UseAuthentication();
        app.UseAuthorization();

        // ABP 审计日志
        app.UseAuditing();

        // Swagger 文档（开发环境显示）
        if (env.IsDevelopment())
        {
            app.UseSwagger();
            app.UseAbpSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "领佑通用管理系统 API v1");
            });
        }

        // Serilog 请求日志
        app.UseAbpSerilogEnrichers();

        // 注册端点（MVC 控制器路由）
        app.UseConfiguredEndpoints();
    }
}
