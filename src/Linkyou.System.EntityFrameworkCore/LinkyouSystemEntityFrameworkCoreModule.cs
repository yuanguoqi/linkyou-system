using Microsoft.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.PostgreSql;
using Volo.Abp.Identity.EntityFrameworkCore;
using Volo.Abp.TenantManagement.EntityFrameworkCore;
using Volo.Abp.AuditLogging.EntityFrameworkCore;
using Volo.Abp.FeatureManagement.EntityFrameworkCore;
using Volo.Abp.PermissionManagement.EntityFrameworkCore;
using Volo.Abp.SettingManagement.EntityFrameworkCore;
using Volo.Abp.BackgroundJobs.EntityFrameworkCore;
using Volo.Abp.Modularity;

namespace Linkyou.System.EntityFrameworkCore;

/// <summary>
/// EF Core 模块：配置 DbContext、仓储实现、数据库迁移
/// </summary>
[DependsOn(
    typeof(LinkyouSystemDomainModule),
    // ABP EF Core 核心
    typeof(AbpEntityFrameworkCoreModule),
    // PostgreSQL 驱动
    typeof(AbpEntityFrameworkCorePostgreSqlModule),
    // 各 ABP 模块的 EF Core 集成
    typeof(AbpIdentityEntityFrameworkCoreModule),
    typeof(AbpTenantManagementEntityFrameworkCoreModule),
    typeof(AbpAuditLoggingEntityFrameworkCoreModule),
    typeof(AbpFeatureManagementEntityFrameworkCoreModule),
    typeof(AbpPermissionManagementEntityFrameworkCoreModule),
    typeof(AbpSettingManagementEntityFrameworkCoreModule),
    typeof(AbpBackgroundJobsEntityFrameworkCoreModule)
)]
public class LinkyouSystemEntityFrameworkCoreModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        // 配置主数据库连接（PostgreSQL）
        Configure<AbpDbContextOptions>(options =>
        {
            options.UseNpgsql();
        });

        // 注册 DbContext，ABP 会自动为所有实体添加默认仓储
        context.Services.AddAbpDbContext<LinkyouSystemDbContext>(options =>
        {
            // 为所有实体添加默认 CRUD 仓储（IRepository<T, TKey>）
            options.AddDefaultRepositories(includeAllEntities: true);
        });
    }
}
