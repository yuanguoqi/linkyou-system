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

    // 业务属性（通过构造函数或工厂方法赋值，保持封装性）
    protected {实体名}() { /* EF Core 需要无参构造 */ }

    internal {实体名}(Guid id, /* 必填参数 */) : base(id)
    {
        // 在领域层做业务验证
    }
}
```

### 2. 领域服务（Domain Service）

```csharp
// 文件路径: src/Linkyou.System.Domain/{模块名}/{实体名}Manager.cs
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Services;

namespace Linkyou.System.{模块名};

/// <summary>
/// {实体名} 领域服务，负责复杂业务逻辑和跨聚合操作
/// </summary>
public class {实体名}Manager : DomainService
{
    private readonly I{实体名}Repository _repository;

    public {实体名}Manager(I{实体名}Repository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// 创建{实体名}（领域层工厂方法）
    /// </summary>
    public async Task<{实体名}> CreateAsync(/* 参数 */)
    {
        // 业务规则校验
        // 唯一性检查等
        return new {实体名}(GuidGenerator.Create(), /* 参数 */);
    }
}
```

### 3. 仓储接口（Repository Interface）

```csharp
// 文件路径: src/Linkyou.System.Domain/{模块名}/I{实体名}Repository.cs
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace Linkyou.System.{模块名};

/// <summary>
/// {实体名} 仓储接口
/// </summary>
public interface I{实体名}Repository : IRepository<{实体名}, Guid>
{
    Task<{实体名}?> FindByNameAsync(
        string name,
        CancellationToken cancellationToken = default);

    Task<List<{实体名}>> GetListAsync(
        string? filter = null,
        int skipCount = 0,
        int maxResultCount = 10,
        string sorting = nameof({实体名}.CreationTime),
        CancellationToken cancellationToken = default);

    Task<long> GetCountAsync(
        string? filter = null,
        CancellationToken cancellationToken = default);
}
```

### 4. DTO定义（Application.Contracts Layer）

```csharp
// 文件路径: src/Linkyou.System.Application.Contracts/{模块名}/{实体名}Dto.cs
using System;
using Volo.Abp.Application.Dtos;

namespace Linkyou.System.{模块名};

/// <summary>
/// {实体名} 数据传输对象（用于查询返回）
/// </summary>
public class {实体名}Dto : FullAuditedEntityDto<Guid>
{
    // 对应实体的公开属性
}

// 文件路径: src/Linkyou.System.Application.Contracts/{模块名}/Create{实体名}Dto.cs
using System.ComponentModel.DataAnnotations;

namespace Linkyou.System.{模块名};

/// <summary>
/// 创建 {实体名} 请求DTO
/// </summary>
public class Create{实体名}Dto
{
    [Required]
    [MaxLength({实体名}Consts.MaxNameLength)]
    public required string Name { get; set; }
}

// 文件路径: src/Linkyou.System.Application.Contracts/{模块名}/Update{实体名}Dto.cs
namespace Linkyou.System.{模块名};

/// <summary>
/// 更新 {实体名} 请求DTO
/// </summary>
public class Update{实体名}Dto : Create{实体名}Dto
{
    // 继承创建DTO，可添加更新特有字段
}

// 文件路径: src/Linkyou.System.Application.Contracts/{模块名}/Get{实体名}ListInput.cs
using Volo.Abp.Application.Dtos;

namespace Linkyou.System.{模块名};

/// <summary>
/// 获取 {实体名} 列表查询参数
/// </summary>
public class Get{实体名}ListInput : PagedAndSortedResultRequestDto
{
    /// <summary>
    /// 关键词过滤
    /// </summary>
    public string? Filter { get; set; }
}
```

### 5. 权限定义（Permissions）

```csharp
// 文件路径: src/Linkyou.System.Application.Contracts/{模块名}/{模块名}Permissions.cs
namespace Linkyou.System.{模块名};

/// <summary>
/// {模块名} 权限常量定义
/// </summary>
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

// 文件路径: src/Linkyou.System.Application.Contracts/{模块名}/{模块名}PermissionDefinitionProvider.cs
using Linkyou.System.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace Linkyou.System.{模块名};

/// <summary>
/// {模块名} 权限定义提供者
/// </summary>
public class {模块名}PermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var group = context.AddGroup(
            {模块名}Permissions.GroupName,
            L("{模块名}"));

        var {实体名小写}Permission = group.AddPermission(
            {模块名}Permissions.{实体名}s.Default,
            L("Permission:{实体名}s"));

        {实体名小写}Permission.AddChild(
            {模块名}Permissions.{实体名}s.Create,
            L("Permission:{实体名}s.Create"));

        {实体名小写}Permission.AddChild(
            {模块名}Permissions.{实体名}s.Update,
            L("Permission:{实体名}s.Update"));

        {实体名小写}Permission.AddChild(
            {模块名}Permissions.{实体名}s.Delete,
            L("Permission:{实体名}s.Delete"));
    }

    private static LocalizableString L(string name)
        => LocalizableString.Create<LinkyouSystemResource>(name);
}
```

### 6. 应用服务接口和实现

```csharp
// 文件路径: src/Linkyou.System.Application.Contracts/{模块名}/I{实体名}AppService.cs
using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Linkyou.System.{模块名};

/// <summary>
/// {实体名} 应用服务接口
/// </summary>
public interface I{实体名}AppService : IApplicationService
{
    Task<{实体名}Dto> GetAsync(Guid id);
    Task<PagedResultDto<{实体名}Dto>> GetListAsync(Get{实体名}ListInput input);
    Task<{实体名}Dto> CreateAsync(Create{实体名}Dto input);
    Task<{实体名}Dto> UpdateAsync(Guid id, Update{实体名}Dto input);
    Task DeleteAsync(Guid id);
}

// 文件路径: src/Linkyou.System.Application/{模块名}/{实体名}AppService.cs
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace Linkyou.System.{模块名};

/// <summary>
/// {实体名} 应用服务实现
/// </summary>
[Authorize({模块名}Permissions.{实体名}s.Default)]
public class {实体名}AppService :
    ApplicationService,
    I{实体名}AppService
{
    private readonly {实体名}Manager _{实体名小写}Manager;
    private readonly I{实体名}Repository _{实体名小写}Repository;

    public {实体名}AppService(
        {实体名}Manager {实体名小写}Manager,
        I{实体名}Repository {实体名小写}Repository)
    {
        _{实体名小写}Manager = {实体名小写}Manager;
        _{实体名小写}Repository = {实体名小写}Repository;
    }

    public async Task<{实体名}Dto> GetAsync(Guid id)
    {
        var entity = await _{实体名小写}Repository.GetAsync(id);
        return ObjectMapper.Map<{实体名}, {实体名}Dto>(entity);
    }

    public async Task<PagedResultDto<{实体名}Dto>> GetListAsync(Get{实体名}ListInput input)
    {
        var count = await _{实体名小写}Repository.GetCountAsync(input.Filter);
        var list = await _{实体名小写}Repository.GetListAsync(
            input.Filter,
            input.SkipCount,
            input.MaxResultCount,
            input.Sorting ?? nameof({实体名}.CreationTime));

        return new PagedResultDto<{实体名}Dto>(
            count,
            ObjectMapper.Map<List<{实体名}>, List<{实体名}Dto>>(list));
    }

    [Authorize({模块名}Permissions.{实体名}s.Create)]
    public async Task<{实体名}Dto> CreateAsync(Create{实体名}Dto input)
    {
        var entity = await _{实体名小写}Manager.CreateAsync(/* input 参数 */);
        await _{实体名小写}Repository.InsertAsync(entity);
        return ObjectMapper.Map<{实体名}, {实体名}Dto>(entity);
    }

    [Authorize({模块名}Permissions.{实体名}s.Update)]
    public async Task<{实体名}Dto> UpdateAsync(Guid id, Update{实体名}Dto input)
    {
        var entity = await _{实体名小写}Repository.GetAsync(id);
        // 更新属性
        return ObjectMapper.Map<{实体名}, {实体名}Dto>(entity);
    }

    [Authorize({模块名}Permissions.{实体名}s.Delete)]
    public async Task DeleteAsync(Guid id)
    {
        await _{实体名小写}Repository.DeleteAsync(id);
    }
}
```

### 7. EF Core 仓储实现

```csharp
// 文件路径: src/Linkyou.System.EntityFrameworkCore/{模块名}/Ef{实体名}Repository.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading;
using System.Threading.Tasks;
using Linkyou.System.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace Linkyou.System.{模块名};

/// <summary>
/// {实体名} EF Core 仓储实现
/// </summary>
public class Ef{实体名}Repository :
    EfCoreRepository<LinkyouSystemDbContext, {实体名}, Guid>,
    I{实体名}Repository
{
    public Ef{实体名}Repository(
        IDbContextProvider<LinkyouSystemDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }

    public async Task<{实体名}?> FindByNameAsync(
        string name,
        CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();
        return await dbSet
            .Where(x => x.Name == name)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<List<{实体名}>> GetListAsync(
        string? filter = null,
        int skipCount = 0,
        int maxResultCount = 10,
        string sorting = nameof({实体名}.CreationTime),
        CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();
        return await dbSet
            .WhereIf(!filter.IsNullOrWhiteSpace(),
                x => x.Name.Contains(filter!))
            .OrderBy(sorting)
            .Skip(skipCount)
            .Take(maxResultCount)
            .ToListAsync(cancellationToken);
    }

    public async Task<long> GetCountAsync(
        string? filter = null,
        CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();
        return await dbSet
            .WhereIf(!filter.IsNullOrWhiteSpace(),
                x => x.Name.Contains(filter!))
            .LongCountAsync(cancellationToken);
    }
}
```

### 8. HTTP API 控制器

```csharp
// 文件路径: src/Linkyou.System.HttpApi/{模块名}/{实体名}Controller.cs
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using Volo.Abp.Application.Dtos;

namespace Linkyou.System.{模块名};

/// <summary>
/// {实体名} HTTP API 控制器
/// </summary>
[RemoteService]
[Area("app")]
[ControllerName("{实体名}")]
[Route("api/app/{实体名小写复数}")]
public class {实体名}Controller : LinkyouSystemController, I{实体名}AppService
{
    private readonly I{实体名}AppService _{实体名小写}AppService;

    public {实体名}Controller(I{实体名}AppService {实体名小写}AppService)
    {
        _{实体名小写}AppService = {实体名小写}AppService;
    }

    [HttpGet("{id}")]
    public Task<{实体名}Dto> GetAsync(Guid id)
        => _{实体名小写}AppService.GetAsync(id);

    [HttpGet]
    public Task<PagedResultDto<{实体名}Dto>> GetListAsync(Get{实体名}ListInput input)
        => _{实体名小写}AppService.GetListAsync(input);

    [HttpPost]
    public Task<{实体名}Dto> CreateAsync(Create{实体名}Dto input)
        => _{实体名小写}AppService.CreateAsync(input);

    [HttpPut("{id}")]
    public Task<{实体名}Dto> UpdateAsync(Guid id, Update{实体名}Dto input)
        => _{实体名小写}AppService.UpdateAsync(id, input);

    [HttpDelete("{id}")]
    public Task DeleteAsync(Guid id)
        => _{实体名小写}AppService.DeleteAsync(id);
}
```

## AutoMapper 配置规范

```csharp
// 在 Application 层的 LinkyouSystemApplicationAutoMapperProfile.cs 中添加
CreateMap<{实体名}, {实体名}Dto>();
```

## DbContext 配置规范

在 `LinkyouSystemDbContext.cs` 中添加：
```csharp
public DbSet<{实体名}> {实体名}s { get; set; }
```

在 `LinkyouSystemDbContextModelCreatingExtensions.cs` 中添加：
```csharp
builder.Entity<{实体名}>(b =>
{
    b.ToTable(LinkyouSystemConsts.DbTablePrefix + "{实体名}s",
        LinkyouSystemConsts.DbSchema);
    b.ConfigureByConvention(); // ABP 约定配置（软删除、审计字段等）
    b.Property(x => x.Name)
        .IsRequired()
        .HasMaxLength({实体名}Consts.MaxNameLength);
    b.HasIndex(x => x.Name);
});
```

## 常量定义规范

```csharp
// 文件路径: src/Linkyou.System.Domain.Shared/{模块名}/{实体名}Consts.cs
namespace Linkyou.System.{模块名};

/// <summary>
/// {实体名} 常量定义（Domain.Shared层，前后端共用）
/// </summary>
public static class {实体名}Consts
{
    public const int MaxNameLength = 128;
    public const int MaxDescriptionLength = 1024;
}
```

## 本地化资源规范

在 `src/Linkyou.System.Domain.Shared/Localization/` 下的 `zh-Hans.json` 和 `en.json` 中添加对应的翻译键。

## 依赖注入注册规范

仓储实现通过 `LinkyouSystemEntityFrameworkCoreModule` 中的 `AddDefaultRepositories` 自动注册。
领域服务、应用服务通过 ABP 的依赖注入约定自动注册（继承了 `DomainService`、`ApplicationService` 等基类）。

## 生成步骤

接收到代码生成请求后，按以下顺序生成：
1. `{实体名}Consts.cs`（Domain.Shared）
2. `{实体名}.cs`（Domain - 实体）
3. `I{实体名}Repository.cs`（Domain - 仓储接口）
4. `{实体名}Manager.cs`（Domain - 领域服务）
5. `{实体名}Dto.cs` / `Create{实体名}Dto.cs` / `Update{实体名}Dto.cs` / `Get{实体名}ListInput.cs`（Application.Contracts）
6. `{模块名}Permissions.cs` + `{模块名}PermissionDefinitionProvider.cs`（Application.Contracts）
7. `I{实体名}AppService.cs`（Application.Contracts）
8. `{实体名}AppService.cs`（Application）
9. `Ef{实体名}Repository.cs`（EntityFrameworkCore）
10. DbContext 和 ModelCreating 更新说明
11. `{实体名}Controller.cs`（HttpApi）
12. AutoMapper 配置更新说明
13. 本地化资源更新说明

每个文件都要完整输出，不能省略。
