using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Identity;
using Volo.Abp.MultiTenancy;

namespace Linkyou.System;

/// <summary>
/// 数据种子贡献者
/// 在首次启动或执行 abp seed 时插入默认数据（管理员账号、默认角色等）
/// </summary>
public class LinkyouSystemDataSeederContributor
    : IDataSeedContributor, ITransientDependency
{
    private readonly IIdentityDataSeeder _identityDataSeeder;
    private readonly ICurrentTenant _currentTenant;

    public LinkyouSystemDataSeederContributor(
        IIdentityDataSeeder identityDataSeeder,
        ICurrentTenant currentTenant)
    {
        _identityDataSeeder = identityDataSeeder;
        _currentTenant = currentTenant;
    }

    public async Task SeedAsync(DataSeedContext context)
    {
        // 在宿主（Host）租户上下文中执行种子数据
        using (_currentTenant.Change(context?.TenantId))
        {
            // 创建默认管理员账号和角色
            await _identityDataSeeder.SeedAsync(
                adminEmail: context!.Properties.GetOrDefault("AdminEmail") as string
                    ?? "admin@linkyou.com",
                adminPassword: context.Properties.GetOrDefault("AdminPassword") as string
                    ?? "Admin@123456",
                tenantId: context.TenantId
            );
        }
    }
}
