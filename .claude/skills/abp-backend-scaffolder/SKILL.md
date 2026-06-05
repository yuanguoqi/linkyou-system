---
name: abp-backend-scaffolder
description: |
  ABP vNext 10.4 后端代码生成器，专为 Linkyou.System 项目设计。当用户需要创建新的后端
  实体、模块、领域对象、应用服务、仓储、API控制器时，必须使用此skill。触发场景包括：
  "创建实体"、"新增模块"、"生成CRUD"、"添加领域服务"、"创建应用服务"、"生成仓储"、
  "创建控制器"、"scaffolding"、"脚手架后端"、"生成后端代码"等。只要用户提到ABP分层
  架构中任何一层的代码生成需求，都应主动使用此skill，不要等用户明确说"使用abp-backend-scaffolder"。
---

# ABP vNext 10.4 后端代码生成器

## 项目上下文

- **项目名称**: Linkyou.System（领佑通用管理系统）
- **后端框架**: ABP vNext 10.4
- **.NET版本**: .NET Core 10.0
- **数据库**: PostgreSQL
- **ORM**: Entity Framework Core 10.0
- **多租户**: 单数据库多租户模式
- **根命名空间**: `Linkyou.System`

## 分层架构说明

```
src/
├── Linkyou.System.Domain/                  # 领域层
├── Linkyou.System.Domain.Shared/           # 领域共享层（枚举、常量、错误码）
├── Linkyou.System.Application.Contracts/   # 应用契约层（DTO、接口、权限）
├── Linkyou.System.Application/             # 应用层（应用服务实现）
├── Linkyou.System.EntityFrameworkCore/     # EF Core层（DbContext、迁移、仓储）
├── Linkyou.System.HttpApi/                 # HTTP API层（控制器）
├── Linkyou.System.HttpApi.Client/          # HTTP API客户端层
└── Linkyou.System.Host/                    # 宿主层（入口）
```

## 已有实体参考

项目中已有以下实体，新实体应参考其模式：

| 实体 | 说明 | 关键特征 |
|------|------|---------|
| `Menu` | 菜单管理 | FullAuditedAggregateRoot, IMultiTenant, private set, 领域方法 |
| `MenuRolePermission` | 菜单角色权限 | CreationAuditedEntity, IMultiTenant, 外键关联 |

## 代码生成规范

### 1. 领域层实体（Domain Layer）

实体必须继承 `FullAuditedAggregateRoot<Guid>` 或 `AuditedAggregateRoot<Guid>`，支持多租户时实现 `IMultiTenant`。

```csharp
// 文件路径: src/Linkyou.System.Domain/{模块名}/{实体名}.cs
using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace Linkyou.System.{模块名};

public class {实体名} : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }
    public string Name { get; private set; } = null!;

    protected {实体名}() { }

    internal {实体名}(Guid id, string name) : base(id)
    {
        SetName(name);
    }

    internal void SetName(string name)
    {
        Name = Check.NotNullOrWhiteSpace(name, nameof(name), {实体名}Consts.MaxNameLength);
    }
}
```

### 2. 常量定义（Domain.Shared Layer）

```csharp
// 文件路径: src/Linkyou.System.Domain.Shared/{模块名}/{实体名}Consts.cs
namespace Linkyou.System.{模块名};

public static class {实体名}Consts
{
    public const int MaxNameLength = 128;
}
```

### 3. 错误码（Domain.Shared Layer）

```csharp
// 文件路径: src/Linkyou.System.Domain.Shared/{模块名}/{实体名}ErrorCodes.cs
namespace Linkyou.System.{模块名};

public static class {实体名}ErrorCodes
{
    public const string NameAlreadyExists = "{模块名}:{实体名}:010001";
    public const string NotFound = "{模块名}:{实体名}:010002";
}
```

### 4. 仓储接口（Domain Layer）

```csharp
// 文件路径: src/Linkyou.System.Domain/{模块名}/I{实体名}Repository.cs
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace Linkyou.System.{模块名};

public interface I{实体名}Repository : IRepository<{实体名}, Guid>
{
    Task<{实体名}?> FindByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<List<{实体名}>> GetListAsync(string? filter = null, int skipCount = 0,
        int maxResultCount = 10, string sorting = nameof({实体名}.CreationTime) + " desc",
        CancellationToken cancellationToken = default);
    Task<long> GetCountAsync(string? filter = null, CancellationToken cancellationToken = default);
}
```

### 5. 领域服务（Domain Layer）

```csharp
// 文件路径: src/Linkyou.System.Domain/{模块名}/{实体名}Manager.cs
using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Services;

namespace Linkyou.System.{模块名};

public class {实体名}Manager : DomainService
{
    private readonly I{实体名}Repository _repository;

    public {实体名}Manager(I{实体名}Repository repository)
    {
        _repository = repository;
    }

    public async Task<{实体名}> CreateAsync(string name)
    {
        await EnsureNameNotExistsAsync(name);
        return new {实体名}(GuidGenerator.Create(), name);
    }

    private async Task EnsureNameNotExistsAsync(string name)
    {
        var existing = await _repository.FindByNameAsync(name);
        if (existing != null)
            throw new BusinessException({实体名}ErrorCodes.NameAlreadyExists).WithData("name", name);
    }
}
```

### 6. DTO定义（Application.Contracts Layer）

```csharp
// {实体名}Dto.cs
public class {实体名}Dto : FullAuditedEntityDto<Guid>
{
    public string Name { get; set; } = null!;
}

// Create{实体名}Dto.cs
public class Create{实体名}Dto
{
    [Required]
    [MaxLength({实体名}Consts.MaxNameLength)]
    public required string Name { get; set; }
}

// Update{实体名}Dto.cs : Create{实体名}Dto { }

// Get{实体名}ListInput.cs
public class Get{实体名}ListInput : PagedAndSortedResultRequestDto
{
    public string? Filter { get; set; }
}
```

### 7. 权限定义（Application.Contracts Layer）

```csharp
// {模块名}Permissions.cs
public static class {模块名}Permissions
{
    public const string GroupName = "{模块名}";
    public static class {实体名}s
    {
        public const string Default = GroupName + ".{实体名}s";
        public const string Create = Default + ".Create";
        public const string Update = Default + ".Update";
        public const string Delete = Default + ".Delete";
    }
}
```

权限定义提供者需要引入 `Linkyou.System.Localization`：

```csharp
// {模块名}PermissionDefinitionProvider.cs
using Linkyou.System.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

public class {模块名}PermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var group = context.AddGroup({模块名}Permissions.GroupName, L("Permission:{模块名}"));
        var perm = group.AddPermission({模块名}Permissions.{实体名}s.Default, L("Permission:{实体名}s"));
        perm.AddChild({模块名}Permissions.{实体名}s.Create, L("Permission:{实体名}s.Create"));
        perm.AddChild({模块名}Permissions.{实体名}s.Update, L("Permission:{实体名}s.Update"));
        perm.AddChild({模块名}Permissions.{实体名}s.Delete, L("Permission:{实体名}s.Delete"));
    }

    private static LocalizableString L(string name) =>
        LocalizableString.Create<LinkyouSystemResource>(name);
}
```

### 8. 应用服务（Application Layer）

```csharp
[Authorize({模块名}Permissions.{实体名}s.Default)]
public class {实体名}AppService : ApplicationService, I{实体名}AppService
{
    // 注入 Manager 和 Repository
    // 实现 CRUD 方法
    // 写操作加 [Authorize(...Create/Update/Delete)]
}
```

### 9. EF Core 仓储（EntityFrameworkCore Layer）

```csharp
public class Ef{实体名}Repository :
    EfCoreRepository<LinkyouSystemDbContext, {实体名}, Guid>,
    I{实体名}Repository
{
    // 使用 AsNoTracking() 做只读查询
    // 使用 WhereIf 避免空判断嵌套
}
```

### 10. 控制器（HttpApi Layer）

```csharp
using Linkyou.System.Controllers;

[RemoteService]
[Area("app")]
[Route("api/app/{实体名小写复数}")]
public class {实体名}Controller : LinkyouSystemController, I{实体名}AppService
{
    // 注意：不使用 [ControllerName] 属性
    // 基类使用 LinkyouSystemController（在 Linkyou.System.Controllers 命名空间）
}
```

### 11. DbContext 配置

```csharp
// LinkyouSystemDbContext.cs
public DbSet<{实体名}> {实体名}s { get; set; }

// LinkyouSystemDbContextModelCreatingExtensions.cs
builder.Entity<{实体名}(b =>
{
    b.ToTable("{实体名}s", LinkyouSystemConsts.DbSchema);  // 业务表不加 Lys_ 前缀
    b.Property(x => x.Name).IsRequired().HasMaxLength({实体名}Consts.MaxNameLength);
    b.HasIndex(x => new { x.TenantId, x.Name });
});
```

### 12. AutoMapper 配置

```csharp
// LinkyouSystemApplicationAutoMapperProfile.cs
CreateMap<{实体名}, {实体名}Dto>();
```

### 13. 本地化资源

```json
// zh-Hans.json
"Permission:{模块名}": "{模块名}管理",
"Permission:{实体名}s": "{实体名}管理",
"Permission:{实体名}s.Create": "创建{实体名}",
"{模块名}:{实体名}:010001": "名称 '{0}' 已存在。"
```

### 14. 依赖注入

仓储通过 `AddDefaultRepositories(includeAllEntities: true)` 自动注册。
领域服务、应用服务通过 ABP 的依赖注入约定自动注册。

### 15. 数据库迁移

```bash
dotnet ef migrations add Add{实体名}s \
  --project src/Linkyou.System.EntityFrameworkCore \
  --startup-project src/Linkyou.System.Host
dotnet ef database update \
  --project src/Linkyou.System.EntityFrameworkCore \
  --startup-project src/Linkyou.System.Host
```
