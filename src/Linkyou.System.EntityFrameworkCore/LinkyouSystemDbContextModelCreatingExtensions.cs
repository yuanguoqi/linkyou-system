using Microsoft.EntityFrameworkCore;
using Volo.Abp;
using Volo.Abp.EntityFrameworkCore.Modeling;
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

        /* 在此追加业务实体的 Fluent API 配置
         *
         * 示例：
         * builder.Entity<Product>(b =>
         * {
         *     b.ToTable(LinkyouSystemConsts.DbTablePrefix + "Products",
         *         LinkyouSystemConsts.DbSchema);
         *     b.ConfigureByConvention();  // 自动配置软删除、审计字段等
         *     b.Property(x => x.Name)
         *         .IsRequired()
         *         .HasMaxLength(ProductConsts.MaxNameLength)
         *         .HasColumnName(nameof(Product.Name));
         *     b.HasIndex(x => x.Name);
         * });
         */
    }
}
