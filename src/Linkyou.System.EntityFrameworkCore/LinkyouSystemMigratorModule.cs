using Linkyou.System.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp;
using Volo.Abp.Autofac;
using Volo.Abp.Modularity;

namespace Linkyou.System;

/// <summary>
/// 数据库迁移控制台程序模块（可单独运行迁移）
/// 在 CI/CD 管道中使用：dotnet run -- migrate
/// </summary>
[DependsOn(
    typeof(AbpAutofacModule),
    typeof(LinkyouSystemEntityFrameworkCoreModule)
)]
public class LinkyouSystemMigratorModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        // 注册迁移服务
        context.Services.AddTransient<LinkyouSystemDbMigrationService>();
        context.Services.AddTransient<ILinkyouSystemDbSchemaMigrator,
            EntityFrameworkCoreLinkyouSystemDbSchemaMigrator>();
    }
}
