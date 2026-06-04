using Linkyou.System.Localization;
using Volo.Abp.Localization;
using Volo.Abp.Localization.ExceptionHandling;
using Volo.Abp.Modularity;
using Volo.Abp.Validation.Localization;
using Volo.Abp.VirtualFileSystem;

namespace Linkyou.System;

/// <summary>
/// Domain.Shared 模块：定义跨层共享的常量、枚举、错误码、本地化资源
/// 此层不依赖任何其他业务层，可被所有层引用
/// </summary>
[DependsOn(
    typeof(AbpLocalizationModule)
)]
public class LinkyouSystemDomainSharedModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        // 配置本地化资源
        Configure<AbpVirtualFileSystemOptions>(options =>
        {
            // 将本地化文件嵌入虚拟文件系统
            options.FileSets.AddEmbedded<LinkyouSystemDomainSharedModule>();
        });

        Configure<AbpLocalizationOptions>(options =>
        {
            // 注册本地化资源
            options.Resources
                .Add<LinkyouSystemResource>("zh-Hans")
                .AddBaseTypes(typeof(AbpValidationResource))
                .AddVirtualJson("/Localization/Resources");

            // 设置默认语言
            options.DefaultResourceType = typeof(LinkyouSystemResource);
        });

        Configure<AbpExceptionLocalizationOptions>(options =>
        {
            // 将业务异常映射到本地化资源
            options.MapCodeNamespace("LinkyouSystem", typeof(LinkyouSystemResource));
        });
    }
}
