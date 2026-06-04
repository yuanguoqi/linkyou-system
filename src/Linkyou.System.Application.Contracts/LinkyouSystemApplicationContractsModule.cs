using Volo.Abp.Application;
using Volo.Abp.Identity;
using Volo.Abp.Modularity;
using Volo.Abp.ObjectExtending;
using Volo.Abp.TenantManagement;
using Volo.Abp.FeatureManagement;
using Volo.Abp.SettingManagement;
using Volo.Abp.AuditLogging;
using Volo.Abp.PermissionManagement;

namespace Linkyou.System;

/// <summary>
/// Application.Contracts 模块：定义应用服务接口、DTO、权限常量
/// 此层可被前端 HTTP API 客户端直接引用
/// </summary>
[DependsOn(
    typeof(LinkyouSystemDomainSharedModule),
    // ABP 应用契约核心
    typeof(AbpDddApplicationContractsModule),
    // 对象扩展（DTO 扩展）
    typeof(AbpObjectExtendingModule),
    // 各 ABP 模块的应用契约
    typeof(AbpIdentityApplicationContractsModule),
    typeof(AbpTenantManagementApplicationContractsModule),
    typeof(AbpFeatureManagementApplicationContractsModule),
    typeof(AbpSettingManagementApplicationContractsModule),
    typeof(AbpAuditLoggingApplicationContractsModule),
    typeof(AbpPermissionManagementApplicationContractsModule)
)]
public class LinkyouSystemApplicationContractsModule : AbpModule
{
}
