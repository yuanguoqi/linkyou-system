using Volo.Abp.Domain;
using Volo.Abp.Identity;
using Volo.Abp.Modularity;
using Volo.Abp.TenantManagement;
using Volo.Abp.FeatureManagement;
using Volo.Abp.SettingManagement;
using Volo.Abp.AuditLogging;
using Volo.Abp.PermissionManagement.Identity;

namespace Linkyou.System;

/// <summary>
/// Domain 模块：定义领域实体、领域服务、仓储接口
/// 核心业务逻辑所在层，只依赖 Domain.Shared
/// </summary>
[DependsOn(
    typeof(LinkyouSystemDomainSharedModule),
    // ABP 领域核心
    typeof(AbpDddDomainModule),
    // 身份管理（用户、角色）
    typeof(AbpIdentityDomainModule),
    // 多租户管理
    typeof(AbpTenantManagementDomainModule),
    // 功能管理
    typeof(AbpFeatureManagementDomainModule),
    // 设置管理
    typeof(AbpSettingManagementDomainModule),
    // 审计日志
    typeof(AbpAuditLoggingDomainModule),
    // 权限管理（身份集成）
    typeof(AbpPermissionManagementDomainIdentityModule)
)]
public class LinkyouSystemDomainModule : AbpModule
{
}
