using Volo.Abp.Application;
using Volo.Abp.AutoMapper;
using Volo.Abp.Identity;
using Volo.Abp.Modularity;
using Volo.Abp.TenantManagement;
using Volo.Abp.FeatureManagement;
using Volo.Abp.SettingManagement;
using Volo.Abp.PermissionManagement;

namespace Linkyou.System;

/// <summary>
/// Application 模块：实现应用服务，处理用例逻辑
/// 依赖 Domain 层和 Application.Contracts 层
/// </summary>
[DependsOn(
    typeof(LinkyouSystemDomainModule),
    typeof(LinkyouSystemApplicationContractsModule),
    // ABP 应用层核心
    typeof(AbpDddApplicationModule),
    // AutoMapper 自动对象映射
    typeof(AbpAutoMapperModule),
    // 各 ABP 模块的应用服务实现
    typeof(AbpIdentityApplicationModule),
    typeof(AbpTenantManagementApplicationModule),
    typeof(AbpFeatureManagementApplicationModule),
    typeof(AbpSettingManagementApplicationModule),
    typeof(AbpPermissionManagementApplicationModule)
)]
public class LinkyouSystemApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        // 配置 AutoMapper，自动扫描当前程序集中的 Profile
        Configure<AbpAutoMapperOptions>(options =>
        {
            options.AddMaps<LinkyouSystemApplicationModule>();
        });
    }
}
