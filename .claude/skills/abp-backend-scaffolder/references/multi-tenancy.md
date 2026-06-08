# 多租户与种子数据完整规范

## 架构总览

```
┌─────────────────────────────────────────────────────────────┐
│                        请求流程                               │
├─────────────────────────────────────────────────────────────┤
│  登录页选择租户 → localStorage 存储 tenant_id               │
│  → axios 拦截器附加 __tenant 请求头                         │
│  → ABP 中间件解析租户 → 设置 CurrentTenant                  │
│  → EF Core 自动过滤 WHERE TenantId = @currentTenant         │
└─────────────────────────────────────────────────────────────┘
```

## 租户体系

### linkyou 租户（最高级管理租户）
- 名称：`linkyou`
- 角色：拥有系统全部权限
- 用途：系统管理员的专属工作空间

### 租户解析优先级
1. **Header** — `__tenant` 请求头（前端主要方式）
2. **CurrentUser** — JWT Token 中的 `tenantid` claim
3. **QueryString** — URL 参数 `__tenant`
4. **Cookie** — Cookie 中的租户信息

## 种子数据

### 执行时机
```
ABP 启动 → IDataSeeder.SeedAsync()
├── context.TenantId = null → 查找/创建 linkyou 租户
└── context.TenantId = xxx → 直接使用
```

### 种子内容
| 数据 | 说明 |
|------|------|
| 租户 | `linkyou`（默认租户） |
| 用户 | `admin` / `Admin@123456` |
| 角色 | `admin`（拥有全部权限） |
| 权限 | 所有 ABP 模块权限 + 核心权限兜底 |
| 菜单 | 仪表盘 + 系统管理（6个子菜单） |

### 幂等检查
- 用户：`_identityDataSeeder.SeedAsync()` 内置幂等
- 权限：`_permissionDataSeeder.SeedAsync()` 内置幂等
- 菜单：`_menuRepository.FindByNameAsync("仪表盘")` 按名称检查

## 实体多租户配置

### 需要租户隔离的实体
```csharp
public class MyEntity : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }
    // ...
}
```

### 全局共享的实体
```csharp
public class Menu : FullAuditedAggregateRoot<Guid>
{
    // 不实现 IMultiTenant，全局共享
    public Guid? TenantId { get; set; } // 仍保留字段
}
```

## 前端多租户

### 租户选择流程
```
1. 登录页加载 → GET /api/account/tenants → 显示租户列表
2. 用户选择租户 → authStore.setTenant(tenantId)
3. 登录 → axios 拦截器自动附加 __tenant 头
4. 后续所有 API 请求自动携带租户信息
```

### 关键代码
```typescript
// api/index.ts - 请求拦截器
if (authStore.currentTenantId) {
  config.headers['__tenant'] = authStore.currentTenantId
}

// auth store - 租户切换
function setTenant(tenantId: string | null) {
  currentTenantId.value = tenantId
  localStorage.setItem('tenant_id', tenantId ?? '')
}
```

## 数据库迁移

```bash
# 重置数据库
dotnet ef database drop --project src/Linkyou.System.EntityFrameworkCore --startup-project src/Linkyou.System.Host --force

# 应用迁移
dotnet ef database update --project src/Linkyou.System.EntityFrameworkCore --startup-project src/Linkyou.System.Host

# 创建新迁移
dotnet ef migrations add <名称> --project src/Linkyou.System.EntityFrameworkCore --startup-project src/Linkyou.System.Host
```

## 最佳实践

1. 所有业务实体实现 `IMultiTenant`
2. 索引包含 `TenantId`：`HasIndex(x => new { x.TenantId, x.Name })`
3. 种子数据设置 `TenantId`
4. 使用 `FindByNameAsync` 做幂等检查
5. 异常消息使用 `L["Key"]` 国际化
6. 前端 axios 拦截器自动携带租户
