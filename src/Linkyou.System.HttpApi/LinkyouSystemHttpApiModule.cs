using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.FeatureManagement;
using Volo.Abp.Identity;
using Volo.Abp.Modularity;
using Volo.Abp.PermissionManagement;
using Volo.Abp.PermissionManagement.HttpApi;
using Volo.Abp.SettingManagement;
using Volo.Abp.TenantManagement;

namespace Linkyou.System;

/// <summary>
/// HttpApi 模块：Controller 实现 IApplicationService 接口，直接委托给 Application 层
/// </summary>
[DependsOn(
    typeof(LinkyouSystemApplicationContractsModule),
    typeof(AbpAspNetCoreMvcModule),
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
        context.Services.AddMvc()
            .AddApplicationPart(typeof(LinkyouSystemHttpApiModule).Assembly);
    }
}
