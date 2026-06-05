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

## 代码生成规范

### 1. 领域层实体（Domain Layer）

实体必须继承 `FullAuditedAggregateRoot<Guid>` 或 `AuditedAggregateRoot<Guid>`，支持多租户时实现 `IMultiTenant`。

```csharp
// 文件路径: src/Linkyou.System.Domain/{模块名}/{实体名}.cs
using System;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace Linkyou.System.{模块名};

/// <summary>
/// {实体描述}
/// </summary>
public class {实体名} : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    /// <summary>
    /// 租户ID（多租户支持）
    /// </summary>
    public Guid? TenantId { get; set; }

    // 业务属性（private set 保护封装性，通过构造函数或领域方法赋值）
    public string Name { get; private set; } = null!;

    protected {实体名}() { /* EF Core 需要无参构造，不可删除 */ }

    internal {实体名}(Guid id, string name) : base(id)
    {
        SetName(name);
    }

    /// <summary>
    /// 更新名称（领域方法，包含业务规则验证）
    /// </summary>
    internal void SetName(string name)
    {
        Name = Check.NotNullOrWhiteSpace(name, nameof(name), {实体名}Consts.MaxNameLength);
    }
}
```

### 2. 业务异常（Domain.Shared Layer）

所有业务错误使用结构化错误码，而非硬编码字符串消息。

```csharp
// 文件路径: src/Linkyou.System.Domain.Shared/{模块名}/{实体名}ErrorCodes.cs
namespace Linkyou.System.{模块名};

public static class {实体名}ErrorCodes
{
    // 错误码格式：{模块名}:{实体名}:{问题描述}
    public const string NameAlreadyExists = "{模块名}:{实体名}:010001";
    public const string NotFound          = "{模块名}:{实体名}:010002";
}
```

在本地化文件中注册对应消息（`zh-Hans.json`）：
```json
"{模块名}:{实体名}:010001": "名称 '{0}' 已存在，请使用不同的名称。",
"{模块名}:{实体名}:010002": "{实体名} 不存在。"
```

### 3. 领域服务（Domain Service）

```csharp
// 文件路径: src/Linkyou.System.Domain/{模块名}/{实体名}Manager.cs
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Services;

namespace Linkyou.System.{模块名};

/// <summary>
/// {实体名} 领域服务，负责跨聚合操作和唯一性等业务约束校验
/// </summary>
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

    public async Task UpdateNameAsync({实体名} entity, string name)
    {
        if (entity.Name == name) return;
        await EnsureNameNotExistsAsync(name);
        entity.SetName(name);
    }

    private async Task EnsureNameNotExistsAsync(string name)
    {
        var existing = await _repository.FindByNameAsync(name);
        if (existing != null)
        {
            throw new BusinessException({实体名}ErrorCodes.NameAlreadyExists)
                .WithData("name", name);
        }
    }
}
```

### 4. 仓储接口（Repository Interface）

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
    Task<{实体名}?> FindByNameAsync(
        string name,
        CancellationToken cancellationToken = default);

    Task<List<{实体名}>> GetListAsync(
        string? filter = null,
        int skipCount = 0,
        int maxResultCount = 10,
        string sorting = nameof({实体名}.CreationTime) + " desc",
        CancellationToken cancellationToken = default);

    Task<long> GetCountAsync(
        string? filter = null,
        CancellationToken cancellationToken = default);
}
```

### 5. DTO定义（Application.Contracts Layer）

```csharp
// 文件路径: src/Linkyou.System.Application.Contracts/{模块名}/{实体名}Dto.cs
using System;
using Volo.Abp.Application.Dtos;

namespace Linkyou.System.{模块名};

public class {实体名}Dto : FullAuditedEntityDto<Guid>
{
    public string Name { get; set; } = null!;
    // 按实际字段补充
}

// Create{实体名}Dto.cs
public class Create{实体名}Dto
{
    [Required]
    [MaxLength({实体名}Consts.MaxNameLength)]
    public required string Name { get; set; }
}

// Update{实体名}Dto.cs
public class Update{实体名}Dto : Create{实体名}Dto { }

// Get{实体名}ListInput.cs
public class Get{实体名}ListInput : PagedAndSortedResultRequestDto
{
    public string? Filter { get; set; }
}
```

### 6. 权限定义（Permissions）

```csharp
// {模块名}Permissions.cs
namespace Linkyou.System.{模块名};

public static class {模块名}Permissions
{
    public const string GroupName = "{模块名}";

    public static class {实体名}s
    {
        public const string Default = GroupName + ".{实体名}s";
        public const string Create  = Default + ".Create";
        public const string Update  = Default + ".Update";
        public const string Delete  = Default + ".Delete";
    }
}

// {模块名}PermissionDefinitionProvider.cs
public class {模块名}PermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var group = context.AddGroup({模块名}Permissions.GroupName, L("{模块名}"));
        var perm  = group.AddPermission({模块名}Permissions.{实体名}s.Default, L("Permission:{实体名}s"));
        perm.AddChild({模块名}Permissions.{实体名}s.Create, L("Permission:{实体名}s.Create"));
        perm.AddChild({模块名}Permissions.{实体名}s.Update, L("Permission:{实体名}s.Update"));
        perm.AddChild({模块名}Permissions.{实体名}s.Delete, L("Permission:{实体名}s.Delete"));
    }
    private static LocalizableString L(string name) =>
        LocalizableString.Create<LinkyouSystemResource>(name);
}
```

### 7. 应用服务接口和实现

```csharp
// I{实体名}AppService.cs
public interface I{实体名}AppService : IApplicationService
{
    Task<{实体名}Dto> GetAsync(Guid id);
    Task<PagedResultDto<{实体名}Dto>> GetListAsync(Get{实体名}ListInput input);
    Task<{实体名}Dto> CreateAsync(Create{实体名}Dto input);
    Task<{实体名}Dto> UpdateAsync(Guid id, Update{实体名}Dto input);
    Task DeleteAsync(Guid id);
}

// {实体名}AppService.cs
[Authorize({模块名}Permissions.{实体名}s.Default)]
public class {实体名}AppService : ApplicationService, I{实体名}AppService
{
    private readonly {实体名}Manager _manager;
    private readonly I{实体名}Repository _repository;

    public {实体名}AppService({实体名}Manager manager, I{实体名}Repository repository)
    {
        _manager    = manager;
        _repository = repository;
    }

    public async Task<{实体名}Dto> GetAsync(Guid id)
    {
        var entity = await _repository.GetAsync(id);
        return ObjectMapper.Map<{实体名}, {实体名}Dto>(entity);
    }

    public async Task<PagedResultDto<{实体名}Dto>> GetListAsync(Get{实体名}ListInput input)
    {
        var count = await _repository.GetCountAsync(input.Filter);
        var list  = await _repository.GetListAsync(
            input.Filter, input.SkipCount, input.MaxResultCount,
            input.Sorting ?? nameof({实体名}.CreationTime) + " desc");
        return new PagedResultDto<{实体名}Dto>(
            count, ObjectMapper.Map<List<{实体名}>, List<{实体名}Dto>>(list));
    }

    [Authorize({模块名}Permissions.{实体名}s.Create)]
    public async Task<{实体名}Dto> CreateAsync(Create{实体名}Dto input)
    {
        var entity = await _manager.CreateAsync(input.Name);
        await _repository.InsertAsync(entity);
        return ObjectMapper.Map<{实体名}, {实体名}Dto>(entity);
    }

    [Authorize({模块名}Permissions.{实体名}s.Update)]
    public async Task<{实体名}Dto> UpdateAsync(Guid id, Update{实体名}Dto input)
    {
        var entity = await _repository.GetAsync(id);
        await _manager.UpdateNameAsync(entity, input.Name);
        return ObjectMapper.Map<{实体名}, {实体名}Dto>(entity);
    }

    [Authorize({模块名}Permissions.{实体名}s.Delete)]
    public async Task DeleteAsync(Guid id)
    {
        await _repository.DeleteAsync(id);
    }
}
```

### 8. EF Core 仓储实现

性能要点：使用 `AsNoTracking()` 做只读查询，用 `WhereIf` 避免空判断嵌套，`OrderBy` 借助 `System.Linq.Dynamic.Core` 支持运行时排序字符串。

```csharp
// 文件路径: src/Linkyou.System.EntityFrameworkCore/{模块名}/Ef{实体名}Repository.cs
public class Ef{实体名}Repository :
    EfCoreRepository<LinkyouSystemDbContext, {实体名}, Guid>,
    I{实体名}Repository
{
    public Ef{实体名}Repository(IDbContextProvider<LinkyouSystemDbContext> dbContextProvider)
        : base(dbContextProvider) { }

    public async Task<{实体名}?> FindByNameAsync(string name, CancellationToken ct = default)
    {
        var dbSet = await GetDbSetAsync();
        return await dbSet.FirstOrDefaultAsync(x => x.Name == name, ct);
    }

    public async Task<List<{实体名}>> GetListAsync(
        string? filter = null, int skipCount = 0, int maxResultCount = 10,
        string sorting = "CreationTime desc", CancellationToken ct = default)
    {
        var dbSet = await GetDbSetAsync();
        return await dbSet
            .AsNoTracking()
            .WhereIf(!filter.IsNullOrWhiteSpace(), x => x.Name.Contains(filter!))
            .OrderBy(sorting)
            .Skip(skipCount).Take(maxResultCount)
            .ToListAsync(ct);
    }

    public async Task<long> GetCountAsync(string? filter = null, CancellationToken ct = default)
    {
        var dbSet = await GetDbSetAsync();
        return await dbSet
            .WhereIf(!filter.IsNullOrWhiteSpace(), x => x.Name.Contains(filter!))
            .LongCountAsync(ct);
    }
}
```

### 9. HTTP API 控制器

```csharp
// 文件路径: src/Linkyou.System.HttpApi/{模块名}/{实体名}Controller.cs
[RemoteService]
[Area("app")]
[ControllerName("{实体名}")]
[Route("api/app/{实体名小写复数}")]
public class {实体名}Controller : LinkyouSystemController, I{实体名}AppService
{
    private readonly I{实体名}AppService _appService;

    public {实体名}Controller(I{实体名}AppService appService)
        => _appService = appService;

    [HttpGet("{id}")]
    public Task<{实体名}Dto> GetAsync(Guid id) => _appService.GetAsync(id);

    [HttpGet]
    public Task<PagedResultDto<{实体名}Dto>> GetListAsync([FromQuery] Get{实体名}ListInput input)
        => _appService.GetListAsync(input);

    [HttpPost]
    public Task<{实体名}Dto> CreateAsync(Create{实体名}Dto input) => _appService.CreateAsync(input);

    [HttpPut("{id}")]
    public Task<{实体名}Dto> UpdateAsync(Guid id, Update{实体名}Dto input)
        => _appService.UpdateAsync(id, input);

    [HttpDelete("{id}")]
    public Task DeleteAsync(Guid id) => _appService.DeleteAsync(id);
}
```

## AutoMapper 配置规范

```csharp
// LinkyouSystemApplicationAutoMapperProfile.cs 中添加
CreateMap<{实体名}, {实体名}Dto>();
```

## DbContext 配置规范

```csharp
// LinkyouSystemDbContext.cs
public DbSet<{实体名}> {实体名}s { get; set; }

// LinkyouSystemDbContextModelCreatingExtensions.cs
builder.Entity<{实体名}>(b =>
{
    b.ToTable(LinkyouSystemConsts.DbTablePrefix + "{实体名}s", LinkyouSystemConsts.DbSchema);
    b.ConfigureByConvention();
    b.Property(x => x.Name).IsRequired().HasMaxLength({实体名}Consts.MaxNameLength);
    b.HasIndex(x => new { x.TenantId, x.Name }).IsUnique(); // 租户内唯一
});
```

## 常量定义规范

```csharp
// 文件路径: src/Linkyou.System.Domain.Shared/{模块名}/{实体名}Consts.cs
namespace Linkyou.System.{模块名};

public static class {实体名}Consts
{
    public const int MaxNameLength        = 128;
    public const int MaxDescriptionLength = 1024;
}
```

## 数据库迁移

每次新增实体或修改模型后，需要添加 EF Core 迁移：

```bash
# 在项目根目录执行
dotnet ef migrations add Add{实体名}s \
  --project src/Linkyou.System.EntityFrameworkCore \
  --startup-project src/Linkyou.System.Host

# 更新数据库（开发环境）
dotnet ef database update \
  --project src/Linkyou.System.EntityFrameworkCore \
  --startup-project src/Linkyou.System.Host
```

## 安全规范

1. **输入验证**：所有 DTO 属性使用 DataAnnotations（`[Required]`、`[MaxLength]`），ABP 会在控制器层自动验证。
2. **权限分层**：`AppService` 类级别打 `[Authorize(Default)]`，写操作方法级别单独打 `Create/Update/Delete`。
3. **防止批量赋值**：实体属性用 `private set`，仅通过领域方法修改，杜绝外部直接赋值。
4. **唯一索引**：为 `Name` 等有唯一性要求的字段加 `HasIndex(...).IsUnique()`，并在领域服务中做应用层唯一校验（给出友好错误码）。
5. **软删除**：继承 `FullAuditedAggregateRoot` 自动支持软删除，EF Core 全局过滤器自动排除已删除记录。

## 依赖注入注册规范

仓储通过 `LinkyouSystemEntityFrameworkCoreModule` 中的 `AddDefaultRepositories` 自动注册。
领域服务、应用服务通过 ABP 的依赖注入约定（继承 `DomainService`、`ApplicationService`）自动注册，无需手动 `AddTransient`。

## 生成步骤

接收到代码生成请求后，按以下顺序输出（每个文件完整代码，不可省略）：

1. `{实体名}Consts.cs`（Domain.Shared）
2. `{实体名}ErrorCodes.cs`（Domain.Shared）
3. `{实体名}.cs`（Domain - 实体）
4. `I{实体名}Repository.cs`（Domain - 仓储接口）
5. `{实体名}Manager.cs`（Domain - 领域服务）
6. `{实体名}Dto.cs` / `Create{实体名}Dto.cs` / `Update{实体名}Dto.cs` / `Get{实体名}ListInput.cs`（Application.Contracts）
7. `{模块名}Permissions.cs` + `{模块名}PermissionDefinitionProvider.cs`（Application.Contracts）
8. `I{实体名}AppService.cs`（Application.Contracts）
9. `{实体名}AppService.cs`（Application）
10. `Ef{实体名}Repository.cs`（EntityFrameworkCore）
11. DbContext / ModelCreating 更新说明 + 迁移命令
12. `{实体名}Controller.cs`（HttpApi）
13. AutoMapper 配置更新说明
14. 本地化资源更新说明（zh-Hans.json / en.json）
