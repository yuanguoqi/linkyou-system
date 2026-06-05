---
name: abp-module-generator
description: |
  ABP vNext 完整业务模块生成器，用于一次性生成前后端完整CRUD业务模块。当用户说
  "生成完整模块"、"创建业务模块"、"全栈CRUD"、"前后端一起生成"、"生成完整功能"、
  "新增业务功能"、"做一个XX管理模块"时，优先使用此skill（而不是分别调用
  abp-backend-scaffolder 和 vue3-frontend-scaffolder）。此skill会协调后端和前端的
  代码生成，确保命名约定、API路径、权限定义完全一致，并自动处理多租户、审计日志、
  国际化等横切关注点。一站式生成：不要让用户分两步走。
---

# ABP 完整业务模块生成器

## 使用场景

当用户想要创建一个完整的业务功能模块时使用，例如：
- "帮我做一个商品管理模块"
- "创建用户通知功能（前后端都要）"
- "生成部门管理的完整代码"

## 生成流程

```
1. 分析需求 → 确定实体名称、字段、业务规则
2. 生成后端代码（遵循 abp-backend-scaffolder 规范）
3. 生成前端代码（遵循 vue3-frontend-scaffolder + linkyou-design-system 规范）
4. 输出数据库迁移命令
5. 输出集成清单（需要手动修改的文件列表）
```

## 信息收集

在生成前，先确认以下信息（如果用户没有提供）：

| 项目 | 说明 | 示例 |
|------|------|------|
| 模块名称 | C# 命名空间中的模块名 | `Products` |
| 实体名称 | 主实体的类名（单数） | `Product` |
| 实体字段 | 属性名称和类型 | `Name(string)`, `Price(decimal)` |
| 是否多租户 | 实体是否区分租户 | 是（默认）/ 否 |
| 是否软删除 | 是否需要软删除功能 | 是（默认）|
| 是否审计 | 是否记录完整审计信息 | 是（默认）|

## 命名约定（前后端必须统一）

| 位置 | 格式 | 示例（实体=Product，模块=Products） |
|------|------|------|
| C# 命名空间 | `Linkyou.System.{模块名}` | `Linkyou.System.Products` |
| C# 类名 | PascalCase | `Product`, `ProductAppService` |
| API路由 | `/api/app/{实体小写复数}` | `/api/app/products` |
| 前端API路径 | `web/src/api/modules/{模块小写}.ts` | `web/src/api/modules/products.ts` |
| Vue组件路径 | `web/src/views/{模块小写}/` | `web/src/views/products/` |
| Pinia Store | `use{实体名}Store` | `useProductStore` |
| 路由name | `{实体名}List` | `ProductList` |
| 权限常量 | `{模块名}.{实体复数}` | `Products.Products` |

## 完整输出结构

生成一个模块时，输出如下文件（每个文件完整代码，不省略）：

### 后端文件清单

```
src/Linkyou.System.Domain.Shared/
└── {模块名}/
    ├── {实体名}Consts.cs
    └── {实体名}ErrorCodes.cs         ← 新增：结构化错误码

src/Linkyou.System.Domain/
└── {模块名}/
    ├── {实体名}.cs                   ← private set 保护封装
    ├── I{实体名}Repository.cs
    └── {实体名}Manager.cs            ← 含唯一性校验

src/Linkyou.System.Application.Contracts/
└── {模块名}/
    ├── {实体名}Dto.cs
    ├── Create{实体名}Dto.cs
    ├── Update{实体名}Dto.cs
    ├── Get{实体名}ListInput.cs
    ├── I{实体名}AppService.cs
    ├── {模块名}Permissions.cs
    └── {模块名}PermissionDefinitionProvider.cs

src/Linkyou.System.Application/
└── {模块名}/
    └── {实体名}AppService.cs

src/Linkyou.System.EntityFrameworkCore/
└── {模块名}/
    └── Ef{实体名}Repository.cs       ← AsNoTracking() 只读查询

src/Linkyou.System.HttpApi/
└── {模块名}/
    └── {实体名}Controller.cs
```

### 前端文件清单

```
web/src/
├── api/modules/{模块小写}.ts
├── views/{模块小写}/
│   ├── index.vue                     ← 深色风格面板，不用 el-card
│   └── components/
│       └── {实体名}Dialog.vue        ← 深色弹窗覆盖
└── router/modules/{模块小写}.ts
```

### 需要手动更新的文件

生成完所有文件后，输出"集成清单"：

```markdown
## 集成清单 - 请手动完成以下步骤

### 后端集成

1. **LinkyouSystemDbContext.cs** - 添加 DbSet：
   public DbSet<{实体名}> {实体名}s { get; set; }

2. **LinkyouSystemDbContextModelCreatingExtensions.cs** - 添加实体配置：
   builder.Entity<{实体名}>(b => {
     b.ToTable(LinkyouSystemConsts.DbTablePrefix + "{实体名}s", LinkyouSystemConsts.DbSchema);
     b.ConfigureByConvention();
     b.Property(x => x.Name).IsRequired().HasMaxLength({实体名}Consts.MaxNameLength);
     b.HasIndex(x => new { x.TenantId, x.Name }).IsUnique();
   });

3. **LinkyouSystemApplicationAutoMapperProfile.cs** - 添加映射：
   CreateMap<{实体名}, {实体名}Dto>();

4. **Localization/zh-Hans.json** - 添加中文翻译键（包含错误码消息）
5. **Localization/en.json** - 添加英文翻译键

6. **数据库迁移**（在项目根目录执行）：
   dotnet ef migrations add Add{实体名}s \
     --project src/Linkyou.System.EntityFrameworkCore \
     --startup-project src/Linkyou.System.Host
   dotnet ef database update \
     --project src/Linkyou.System.EntityFrameworkCore \
     --startup-project src/Linkyou.System.Host

### 前端集成

7. **web/src/router/index.ts** - 导入并注册新路由模块：
   import {模块小写}Routes from './modules/{模块小写}'
   // 在动态路由数组中展开：...{模块小写}Routes

8. **侧栏菜单配置** - 在菜单数据中添加对应菜单项（如有独立菜单配置文件）
```

## 多租户实现规范

```csharp
// 实体实现 IMultiTenant
public class {实体名} : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }
    // ...
}
```

EF Core 配置中 ABP 的 `ConfigureByConvention()` 自动处理租户过滤器。
唯一索引加入 `TenantId`：`HasIndex(x => new { x.TenantId, x.Name }).IsUnique()`

## 审计日志规范

| 需要 | 继承基类 |
|------|---------|
| 完整审计（创建/修改/删除） | `FullAuditedAggregateRoot<Guid>` |
| 创建和修改审计 | `AuditedAggregateRoot<Guid>` |
| 仅创建审计 | `CreationAuditedAggregateRoot<Guid>` |

## 实体字段类型映射

| 业务类型 | C# 类型 | EF Core 配置 | TypeScript 类型 | Element Plus 组件 |
|---------|---------|-------------|-----------------|-------------------|
| 短文本 | `string` | `HasMaxLength(128)` | `string` | `el-input` |
| 长文本 | `string` | `HasMaxLength(1024)` | `string` | `el-input type="textarea"` |
| 整数 | `int` | — | `number` | `el-input-number` |
| 金额/小数 | `decimal` | `HasColumnType("decimal(18,2)")` | `number` | `el-input-number :precision="2"` |
| 布尔值 | `bool` | — | `boolean` | `el-switch` |
| 日期时间 | `DateTime` | — | `string` | `el-date-picker` |
| 枚举 | `{枚举名}` | — | `number` | `el-select` |
| 外键关联 | `Guid` | `HasIndex` | `string` | `el-select`（远程搜索） |

## 本地化资源规范

```json
// zh-Hans.json 中新增（示例，Product 模块）
"Products": "商品管理",
"Permission:Products": "商品管理",
"Permission:Products.Create": "创建商品",
"Permission:Products.Update": "编辑商品",
"Permission:Products.Delete": "删除商品",
"Products:Product:010001": "名称 '{0}' 已存在，请使用不同的名称。",
"Products:Product:010002": "商品不存在。"
```

## 示例：生成商品管理模块

**用户输入**: "帮我生成一个商品管理模块，包含名称、价格、库存数量、是否上架字段"

**解析结果**:
- 模块名: `Products`
- 实体名: `Product`
- 字段:
  - `Name`: string, 必填, 最大128字符
  - `Price`: decimal(18,2), 必填
  - `StockCount`: int, 默认0
  - `IsPublished`: bool, 默认false

**生成所有文件（后端 + 前端），然后输出集成清单。**
