using Linkyou.System;
using Serilog;
using Serilog.Events;

// Npgsql 8.x 默认要求 timestamp with time zone 必须是 UTC，
// 启用 legacy 模式以兼容 ABP 框架内部使用的 Local DateTime
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

// 配置 Serilog 启动日志（在 DI 容器初始化之前使用）
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Async(c => c.Console())
    .WriteTo.Async(c => c.File(
        "Logs/logs.txt",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30))
    .CreateLogger();

try
{
    Log.Information("启动领佑通用管理系统...");

    var builder = WebApplication.CreateBuilder(args);

    // 使用 Serilog 替换默认日志系统
    builder.Host.UseSerilog();

    // 使用 Autofac 替换默认 DI 容器（支持属性注入和拦截器）
    builder.Host.UseAutofac();

    // 注册 ABP 应用模块
    await builder.AddApplicationAsync<LinkyouSystemHostModule>();

    var app = builder.Build();

    // 初始化并启动 ABP 应用
    await app.InitializeApplicationAsync();

    await app.RunAsync();

    return 0;
}
catch (Exception ex)
{
    Log.Fatal(ex, "系统启动失败！");
    return 1;
}
finally
{
    // 确保所有日志都写入完毕
    await Log.CloseAndFlushAsync();
}
