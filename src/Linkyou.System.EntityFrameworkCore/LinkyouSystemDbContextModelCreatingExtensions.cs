using Linkyou.System.Menus;
using Microsoft.EntityFrameworkCore;
using Volo.Abp;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Identity.EntityFrameworkCore;
using Volo.Abp.TenantManagement.EntityFrameworkCore;
using Volo.Abp.AuditLogging.EntityFrameworkCore;
using Volo.Abp.FeatureManagement.EntityFrameworkCore;
using Volo.Abp.PermissionManagement.EntityFrameworkCore;
using Volo.Abp.SettingManagement.EntityFrameworkCore;
using Volo.Abp.BackgroundJobs.EntityFrameworkCore;

namespace Linkyou.System.EntityFrameworkCore;

/// <summary>
/// DbContext 模型创建扩展方法
/// 集中管理所有表的 Fluent API 配置
/// </summary>
public static class LinkyouSystemDbContextModelCreatingExtensions
{
    public static void ConfigureLinkyouSystem(this ModelBuilder builder)
    {
        Check.NotNull(builder, nameof(builder));

        // 配置 ABP 内置模块数据表
        builder.ConfigureIdentity();
        builder.ConfigureTenantManagement();
        builder.ConfigureAuditLogging();
        builder.ConfigureFeatureManagement();
        builder.ConfigurePermissionManagement();
        builder.ConfigureSettingManagement();
        builder.ConfigureBackgroundJobs();

        builder.Entity<Menu>(b =>
        {
            b.ToTable(LinkyouSystemConsts.DbTablePrefix + "Menus", LinkyouSystemConsts.DbSchema);
            b.Property(x => x.Name).IsRequired().HasMaxLength(MenuConsts.MaxNameLength);
            b.Property(x => x.Path).IsRequired().HasMaxLength(MenuConsts.MaxPathLength);
            b.Property(x => x.Icon).HasMaxLength(MenuConsts.MaxIconLength);
            b.Property(x => x.Permission).HasMaxLength(MenuConsts.MaxPermissionLength);
            b.HasIndex(x => new { x.TenantId, x.Name });
        });
    }
}
