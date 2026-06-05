@echo off
REM =====================================================
REM Linkyou.System 数据库迁移工具 (Windows)
REM 用法: scripts\db-migrate.bat [命令] [迁移名称]
REM =====================================================

setlocal enabledelayedexpansion

set SCRIPT_DIR=%~dp0
set ROOT_DIR=%SCRIPT_DIR%..
set SRC_DIR=%ROOT_DIR%\src
set EF_PROJECT=%SRC_DIR%\Linkyou.System.EntityFrameworkCore
set STARTUP_PROJECT=%SRC_DIR%\Linkyou.System.Host

if "%1"=="" goto help
if "%1"=="help" goto help
if "%1"=="add" goto add
if "%1"=="update" goto update
if "%1"=="rollback" goto rollback
if "%1"=="remove" goto remove
if "%1"=="list" goto list
if "%1"=="script" goto script
if "%1"=="status" goto status
if "%1"=="fresh" goto fresh
goto help

:help
echo Linkyou.System 数据库迁移工具
echo.
echo 用法: %~nx0 ^<命令^> [参数]
echo.
echo 命令:
echo   add ^<名称^>     创建新迁移
echo   update          应用所有待执行的迁移
echo   rollback ^<名称^> 回滚到指定迁移
echo   remove          移除最后一次未应用的迁移
echo   list            列出所有迁移
echo   script [名称]   生成 SQL 脚本
echo   status          显示迁移状态
echo   fresh           删除数据库并重新迁移（开发环境）
echo.
echo 示例:
echo   %~nx0 add AddProducts
echo   %~nx0 update
echo   %~nx0 script AddMenus
goto :eof

:add
if "%2"=="" (
    echo 错误: 请提供迁移名称
    echo 用法: %~nx0 add ^<MigrationName^>
    exit /b 1
)
echo 创建迁移: %2
dotnet ef migrations add %2 --project "%EF_PROJECT%" --startup-project "%STARTUP_PROJECT%" --output-dir Migrations
if errorlevel 1 (
    echo 迁移创建失败
    exit /b 1
)
echo 迁移创建成功
goto :eof

:update
echo 应用数据库迁移...
dotnet ef database update --project "%EF_PROJECT%" --startup-project "%STARTUP_PROJECT%"
if errorlevel 1 (
    echo 迁移应用失败
    exit /b 1
)
echo 迁移应用完成
goto :eof

:rollback
if "%2"=="" (
    echo 错误: 请提供目标迁移名称
    echo 用法: %~nx0 rollback ^<MigrationName^>
    echo.
    echo 可用迁移:
    dotnet ef migrations list --project "%EF_PROJECT%" --startup-project "%STARTUP_PROJECT%"
    exit /b 1
)
echo 回滚到: %2
dotnet ef database update %2 --project "%EF_PROJECT%" --startup-project "%STARTUP_PROJECT%"
echo 回滚完成
goto :eof

:remove
echo 移除最后一次未应用的迁移...
dotnet ef migrations remove --project "%EF_PROJECT%" --startup-project "%STARTUP_PROJECT%"
echo 迁移已移除
goto :eof

:list
echo 迁移列表:
dotnet ef migrations list --project "%EF_PROJECT%" --startup-project "%STARTUP_PROJECT%"
goto :eof

:script
set OUTPUT=%ROOT_DIR%\scripts\migration.sql
if "%2"=="" (
    echo 生成完整 SQL 脚本...
    dotnet ef migrations script --project "%EF_PROJECT%" --startup-project "%STARTUP_PROJECT%" --output "%OUTPUT%" --idempotent
) else (
    echo 生成 SQL 脚本 (从 %2 开始^)...
    dotnet ef migrations script %2 --project "%EF_PROJECT%" --startup-project "%STARTUP_PROJECT%" --output "%OUTPUT%" --idempotent
)
echo SQL 脚本已生成: %OUTPUT%
goto :eof

:status
echo 数据库迁移状态:
echo.
dotnet ef migrations list --project "%EF_PROJECT%" --startup-project "%STARTUP_PROJECT%"
goto :eof

:fresh
echo 警告: 此操作将删除数据库并重新创建
set /p confirm="确认执行? (y/N): "
if /i not "%confirm%"=="y" (
    echo 已取消
    goto :eof
)
echo 删除数据库...
dotnet ef database drop --project "%EF_PROJECT%" --startup-project "%STARTUP_PROJECT%" --force
echo 重新应用迁移...
dotnet ef database update --project "%EF_PROJECT%" --startup-project "%STARTUP_PROJECT%"
echo 数据库重建完成
goto :eof
