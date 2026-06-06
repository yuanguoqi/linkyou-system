using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;

namespace Linkyou.System.EntityFrameworkCore;

/// <summary>
/// 数据库初始化器
/// 在应用启动时自动创建/迁移数据库，并执行数据种子
/// </summary>
public class LinkyouSystemDbMigrationService : ITransientDependency
{
    private readonly ILogger<LinkyouSystemDbMigrationService> _logger;
    private readonly IDataSeeder _dataSeeder;
    private readonly IEnumerable<ILinkyouSystemDbSchemaMigrator> _dbSchemaMigrators;

    public LinkyouSystemDbMigrationService(
        ILogger<LinkyouSystemDbMigrationService> logger,
        IDataSeeder dataSeeder,
        IEnumerable<ILinkyouSystemDbSchemaMigrator> dbSchemaMigrators)
    {
        _logger = logger;
        _dataSeeder = dataSeeder;
        _dbSchemaMigrators = dbSchemaMigrators;
    }

    /// <summary>
    /// 执行数据库迁移和初始化种子数据
    /// </summary>
    public async Task MigrateAsync()
    {
        _logger.LogInformation("开始执行数据库迁移...");

        foreach (var migrator in _dbSchemaMigrators)
        {
            await migrator.MigrateAsync();
        }

        _logger.LogInformation("数据库迁移完成，开始初始化种子数据...");

        await _dataSeeder.SeedAsync(new DataSeedContext()
            .WithProperty("AdminEmail", "1343261694@qq.com")
            .WithProperty("AdminPassword", "Admin@123456"));

        _logger.LogInformation("种子数据初始化完成。");
    }
}

/// <summary>
/// 数据库 Schema 迁移接口（可扩展多个迁移器）
/// </summary>
public interface ILinkyouSystemDbSchemaMigrator
{
    Task MigrateAsync();
}

/// <summary>
/// EF Core 数据库迁移实现
/// </summary>
public class EntityFrameworkCoreLinkyouSystemDbSchemaMigrator
    : ILinkyouSystemDbSchemaMigrator, ITransientDependency
{
    private readonly IServiceProvider _serviceProvider;

    public EntityFrameworkCoreLinkyouSystemDbSchemaMigrator(
        IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task MigrateAsync()
    {
        // 使用新的 DI 作用域执行迁移，避免影响当前请求上下文
        await using var scope = _serviceProvider.CreateAsyncScope();
        await scope.ServiceProvider
            .GetRequiredService<LinkyouSystemDbContext>()
            .Database
            .MigrateAsync();
    }
}
