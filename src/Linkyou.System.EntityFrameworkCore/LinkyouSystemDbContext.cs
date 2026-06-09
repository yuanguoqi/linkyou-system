using Linkyou.System.Menus;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Identity;
using Volo.Abp.Identity.EntityFrameworkCore;
using Volo.Abp.TenantManagement;
using Volo.Abp.TenantManagement.EntityFrameworkCore;

namespace Linkyou.System.EntityFrameworkCore;

/// <summary>
/// 应用主 DbContext
/// 集成所有 ABP 模块的数据表，同时承载业务实体
/// </summary>
[ConnectionStringName("Default")]
public class LinkyouSystemDbContext :
    AbpDbContext<LinkyouSystemDbContext>,
    IIdentityDbContext,
    ITenantManagementDbContext
{
    // ==================== ABP 身份管理表 ====================

    /// <summary>用户表</summary>
    public DbSet<IdentityUser> Users { get; set; }

    /// <summary>角色表</summary>
    public DbSet<IdentityRole> Roles { get; set; }

    /// <summary>用户声明表</summary>
    public DbSet<IdentityUserClaim> UserClaims { get; set; }

    /// <summary>用户角色关联表</summary>
    public DbSet<IdentityUserRole> UserRoles { get; set; }

    /// <summary>用户登录信息表</summary>
    public DbSet<IdentityUserLogin> UserLogins { get; set; }

    /// <summary>用户令牌表</summary>
    public DbSet<IdentityUserToken> UserTokens { get; set; }

    /// <summary>声明类型表</summary>
    public DbSet<IdentityClaimType> ClaimTypes { get; set; }

    /// <summary>用户委托表</summary>
    public DbSet<IdentityUserDelegation> UserDelegations { get; set; }

    /// <summary>安全日志表</summary>
    public DbSet<IdentitySecurityLog> SecurityLogs { get; set; }

    /// <summary>用户会话表</summary>
    public DbSet<IdentitySession> Sessions { get; set; }

    /// <summary>用户链接表</summary>
    public DbSet<IdentityLinkUser> LinkUsers { get; set; }

    /// <summary>组织单元表</summary>
    public DbSet<OrganizationUnit> OrganizationUnits { get; set; }

    // ==================== ABP 多租户表 ====================

    /// <summary>租户表</summary>
    public DbSet<Tenant> Tenants { get; set; }

    /// <summary>租户连接字符串表</summary>
    public DbSet<TenantConnectionString> TenantConnectionStrings { get; set; }

    // ==================== 业务实体表（在此追加） ====================
    public DbSet<Menu> Menus { get; set; }
    public DbSet<MenuRolePermission> MenuRolePermissions { get; set; }

    public LinkyouSystemDbContext(DbContextOptions<LinkyouSystemDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // 配置所有 ABP 内置模块的数据模型
        builder.ConfigureLinkyouSystem();
    }
}
