using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Identity;
using Volo.Abp.Modularity;
using Volo.Abp.TenantManagement;
using Volo.Abp.FeatureManagement;
using Volo.Abp.SettingManagement;
using Volo.Abp.PermissionManagement;
using Volo.Abp.PermissionManagement.HttpApi;

namespace Linkyou.System;

/// <summary>
/// HttpApi 模块：定义 REST API 控制器
/// 控制器仅做请求转发，业务逻辑在 Application 层
/// </summary>
[DependsOn(
    typeof(LinkyouSystemApplicationContractsModule),
    // ABP MVC 核心
    typeof(AbpAspNetCoreMvcModule),
    // 各 ABP 模块的 HTTP API 控制器
    typeof(AbpIdentityHttpApiModule),
    typeof(AbpTenantManagementHttpApiModule),
    typeof(AbpFeatureManagementHttpApiModule),
    typeof(AbpSettingManagementHttpApiModule),
    typeof(AbpPermissionManagementHttpApiModule)
)]
public class LinkyouSystemHttpApiModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        // 配置 API 路由约定
        Configure<AbpAspNetCoreMvcOptions>(options =>
        {
            options
                .ConventionalControllers
                .Create(typeof(LinkyouSystemApplicationContractsModule).Assembly);
        });
    }
}
