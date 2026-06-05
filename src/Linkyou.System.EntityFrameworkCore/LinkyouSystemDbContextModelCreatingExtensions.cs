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
            b.ToTable("Menus", LinkyouSystemConsts.DbSchema);
            b.Property(x => x.Name).IsRequired().HasMaxLength(MenuConsts.MaxNameLength);
            b.Property(x => x.Path).IsRequired().HasMaxLength(MenuConsts.MaxPathLength);
            b.Property(x => x.Icon).HasMaxLength(MenuConsts.MaxIconLength);
            b.Property(x => x.Permission).HasMaxLength(MenuConsts.MaxPermissionLength);

            // 索引：租户内菜单名唯一
            b.HasIndex(x => new { x.TenantId, x.Name });
            // 索引：按父级查询子菜单
            b.HasIndex(x => new { x.TenantId, x.ParentId });
            // 索引：排序查询
            b.HasIndex(x => x.Sort);

            // 自引用外键：父级菜单
            b.HasOne<Menu>()
                .WithMany()
                .HasForeignKey(x => x.ParentId)
                .OnDelete(DeleteBehavior.Restrict)  // 禁止级联删除，防止误删子菜单
                .HasConstraintName("FK_Menus_ParentId");
        });

        builder.Entity<MenuRolePermission>(b =>
        {
            b.ToTable("MenuRolePermissions", LinkyouSystemConsts.DbSchema);
            b.Property(x => x.RoleName).IsRequired().HasMaxLength(256);

            // 索引：租户+角色查询（最常用）
            b.HasIndex(x => new { x.TenantId, x.RoleName });
            // 索引：租户+菜单+角色（唯一约束）
            b.HasIndex(x => new { x.TenantId, x.MenuId, x.RoleName }).IsUnique();
            // 索引：按菜单查询所有角色权限
            b.HasIndex(x => x.MenuId);

            // 外键：关联菜单表，级联删除
            b.HasOne<Menu>()
                .WithMany()
                .HasForeignKey(x => x.MenuId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_MenuRolePermissions_MenuId");
        });
    }
}
