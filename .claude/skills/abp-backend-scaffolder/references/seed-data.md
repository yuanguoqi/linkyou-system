# 种子数据规范

## 架构概述

项目使用 ABP 的 `IDataSeedContributor` 接口实现种子数据。种子数据在以下时机自动执行：
- 应用首次启动时
- 每次创建新租户时

## 种子数据结构

```
LinkyouSystemDataSeederContributor
├── EnsureDefaultTenantAsync()    # 确保存在默认租户
├── SeedAdminUserAsync()          # 创建 admin 用户
├── SeedPermissionsAsync()        # 授予 admin 角色所有权限
└── SeedMenusAsync()              # 创建菜单数据
```

## ABP 调用规则

```csharp
public async Task SeedAsync(DataSeedContext context)
{
    // context.TenantId = null  → 宿主租户（首次启动）
    // context.TenantId = xxx   → 指定租户（创建新租户时）

    var tenantId = context.TenantId;

    // 宿主租户调用时，创建/查找默认租户
    if (!tenantId.HasValue)
    {
        tenantId = await EnsureDefaultTenantAsync();
    }

    // 在目标租户上下文中执行种子
    using (_currentTenant.Change(tenantId))
    {
        await SeedAdminUserAsync(tenantId);
        await SeedPermissionsAsync();
        await SeedMenusAsync(tenantId);
    }
}
```

## 关键设计原则

### 1. 幂等性
所有种子数据操作必须是幂等的，重复执行不会产生重复数据：

```csharp
// 用户：ABP 内置 IIdentityDataSeeder 自动幂等
await _identityDataSeeder.SeedAsync(...);

// 权限：ABP 内置 IPermissionDataSeeder 自动幂等
await _permissionDataSeeder.SeedAsync(...);

// 菜单：按名称检查
var existing = await _menuRepository.FindByNameAsync("仪表盘");
if (existing != null) return;
```

### 2. 租户隔离
所有实体必须设置 `TenantId`：

```csharp
var menu = new Menu(...)
{
    TenantId = tenantId  // 必须设置
};
```

### 3. 权限授予
使用 ABP 的 `IPermissionDataSeeder` 批量授予权限：

```csharp
// 获取所有已注册权限
var permissionNames = (await _permissionDefinitionManager.GetPermissionsAsync())
    .Select(p => p.Name)
    .ToArray();

// 批量授予
await _permissionDataSeeder.SeedAsync(
    RolePermissionValueProvider.ProviderName,  // 提供者
    "admin",                                    // 角色名
    permissionNames,                           // 权限列表
    _currentTenant.Id                          // 租户 ID
);
```

## 添加新种子数据

### 步骤 1：注入依赖

```csharp
private readonly IMyRepository _myRepository;

public LinkyouSystemDataSeederContributor(
    // ... 现有依赖
    IMyRepository myRepository)
{
    _myRepository = myRepository;
}
```

### 步骤 2：创建种子方法

```csharp
private async Task SeedMyDataAsync(Guid? tenantId)
{
    // 幂等检查
    var existing = await _myRepository.FindAsync(...);
    if (existing != null) return;

    // 创建数据
    var entity = new MyEntity(...)
    {
        TenantId = tenantId  // 必须设置
    };
    await _myRepository.InsertAsync(entity);
}
```

### 步骤 3：在 SeedAsync 中调用

```csharp
public async Task SeedAsync(DataSeedContext context)
{
    var tenantId = context.TenantId;
    if (!tenantId.HasValue)
    {
        tenantId = await EnsureDefaultTenantAsync();
    }

    using (_currentTenant.Change(tenantId))
    {
        await SeedAdminUserAsync(tenantId);
        await SeedPermissionsAsync();
        await SeedMenusAsync(tenantId);
        await SeedMyDataAsync(tenantId);  // 新增
    }
}
```

## 默认种子数据

### 租户
- 名称：`linkyou`
- 类型：最高级管理租户，拥有所有权限

### 用户
- 用户名：`admin`
- 邮箱：`1343261694@qq.com`
- 密码：`Admin@123456`
- 角色：`superadmin``admin`

### 权限
- 授予 `superadmin` 角色所有已注册权限
- 包含 ABP 核心模块权限（兜底）

### 菜单
| 名称 | 路径 | 图标 | 权限 |
|------|------|------|------|
| 仪表盘 | /dashboard | Odometer | - |
| 系统管理 | /system | Setting | - |
| 用户管理 | /system/users | User | AbpIdentity.Users |
| 角色管理 | /system/roles | UserFilled | AbpIdentity.Roles |
| 租户管理 | /system/tenants | OfficeBuilding | AbpTenantManagement.Tenants |
| 审计日志 | /system/audit-logs | Document | - |
| 菜单管理 | /system/menus | Menu | Menus.MenuItems |
| 系统设置 | /system/settings | Tools | SettingManagement.Settings |

## 数据库迁移命令

```bash
# 重置数据库
dotnet ef database drop --project src/Linkyou.System.EntityFrameworkCore --startup-project src/Linkyou.System.Host --force

# 应用迁移
dotnet ef database update --project src/Linkyou.System.EntityFrameworkCore --startup-project src/Linkyou.System.Host

# 创建新迁移
dotnet ef migrations add <名称> --project src/Linkyou.System.EntityFrameworkCore --startup-project src/Linkyou.System.Host
```
