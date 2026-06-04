using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Http.Client;
using Volo.Abp.Identity;
using Volo.Abp.Modularity;
using Volo.Abp.TenantManagement;
using Volo.Abp.FeatureManagement;
using Volo.Abp.SettingManagement;

namespace Linkyou.System;

/// <summary>
/// HttpApi.Client 模块：通过动态 HTTP 代理调用后端 API
/// 可在其他微服务或测试项目中引用，无需编写 HTTP 调用代码
/// </summary>
[DependsOn(
    typeof(LinkyouSystemApplicationContractsModule),
    // ABP HTTP 客户端核心（动态代理）
    typeof(AbpHttpClientModule),
    // 各 ABP 模块的 HTTP 客户端
    typeof(AbpIdentityHttpApiClientModule),
    typeof(AbpTenantManagementHttpApiClientModule),
    typeof(AbpFeatureManagementHttpApiClientModule),
    typeof(AbpSettingManagementHttpApiClientModule)
)]
public class LinkyouSystemHttpApiClientModule : AbpModule
{
    public const string RemoteServiceName = "Default";

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        // 为当前程序集中所有 IApplicationService 接口创建动态 HTTP 代理
        context.Services.AddHttpClientProxies(
            typeof(LinkyouSystemApplicationContractsModule).Assembly,
            RemoteServiceName
        );
    }
}
