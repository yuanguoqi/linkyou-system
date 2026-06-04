using Linkyou.System.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Linkyou.System.EntityFrameworkCore;

/// <summary>
/// EF Core 设计时 DbContext 工厂
/// 用于执行 dotnet ef migrations add 等迁移命令时提供 DbContext 实例
/// 仅在开发时使用，不影响运行时
/// </summary>
public class LinkyouSystemDbContextFactory : IDesignTimeDbContextFactory<LinkyouSystemDbContext>
{
    public LinkyouSystemDbContext CreateDbContext(string[] args)
    {
        // 读取 Host 项目的 appsettings.json 作为设计时配置
        var configuration = BuildConfiguration();

        var builder = new DbContextOptionsBuilder<LinkyouSystemDbContext>()
            .UseNpgsql(configuration.GetConnectionString("Default"));

        return new LinkyouSystemDbContext(builder.Options);
    }

    private static IConfigurationRoot BuildConfiguration()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(
                Directory.GetCurrentDirectory(),
                "../Linkyou.System.Host"))
            .AddJsonFile("appsettings.json", optional: false);

        return builder.Build();
    }
}
