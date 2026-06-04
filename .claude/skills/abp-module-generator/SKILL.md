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

接收请求后按以下顺序执行：

```
1. 分析需求 → 确定实体名称、字段、业务规则
2. 生成后端代码（调用 abp-backend-scaffolder 规范）
3. 生成前端代码（调用 vue3-frontend-scaffolder 规范）
4. 生成数据库迁移说明
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
| Vue组件路径 | `src/views/{模块小写}/` | `src/views/products/` |
| Pinia Store | `use{实体名}Store` | `useProductStore` |
| 路由name | `{实体名}List` | `ProductList` |
| 权限常量 | `{模块名}.{实体复数}` | `Products.Products` |

## 完整输出结构

生成一个模块时，输出如下文件（每个文件完整代码，不省略）：

### 后端文件清单

```
src/Linkyou.System.Domain.Shared/
└── {模块名}/
    └── {实体名}Consts.cs

src/Linkyou.System.Domain/
└── {模块名}/
    ├── {实体名}.cs
    ├── I{实体名}Repository.cs
    └── {实体名}Manager.cs

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
    └── Ef{实体名}Repository.cs

src/Linkyou.System.HttpApi/
└── {模块名}/
    └── {实体名}Controller.cs
```

### 前端文件清单

```
src/Linkyou.System.Web/src/
├── api/modules/{模块小写}.ts
├── stores/modules/{模块小写}.ts（业务复杂时生成）
├── views/{模块小写}/
│   ├── index.vue
│   └── components/
│       └── {实体名}Dialog.vue
└── router/modules/{模块小写}.ts
```

### 需要手动更新的文件

生成完所有文件后，输出一个"集成清单"，告知用户需要手动修改哪些现有文件：

```
## 集成清单 - 请手动完成以下步骤

### 后端集成

1. **LinkyouSystemDbContext.cs** - 添加 DbSet：
   public DbSet<{实体名}> {实体名}s { get; set; }

2. **LinkyouSystemDbContextModelCreatingExtensions.cs** - 添加实体配置：
   （提供完整的 builder.Entity<{实体名}> 配置代码）

3. **LinkyouSystemApplicationAutoMapperProfile.cs** - 添加映射：
   CreateMap<{实体名}, {实体名}Dto>();

4. **LinkyouSystemEntityFrameworkCoreModule.cs** - 注册仓储（如未使用通用仓储）：
   context.Services.AddTransient<I{实体名}Repository, Ef{实体名}Repository>();

5. **Localization/zh-Hans.json** - 添加中文翻译键
6. **Localization/en.json** - 添加英文翻译键

### 前端集成

7. **src/router/index.ts** - 导入并注册新路由模块：
   import {模块小写}Routes from './modules/{模块小写}'
   // 在 routes 数组中展开：...{模块小写}Routes

8. **src/layouts/MainLayout.vue 或菜单配置** - 添加菜单项
```

## 多租户实现规范

当实体需要支持多租户时：

```csharp
// 实体实现 IMultiTenant
public class {实体名} : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }
    // ...
}
```

EF Core 配置中 ABP 的 `ConfigureByConvention()` 会自动处理租户过滤器，无需额外代码。

## 审计日志规范

- 需要完整审计（创建人、创建时间、修改人、修改时间、删除人、删除时间）：继承 `FullAuditedAggregateRoot<Guid>`
- 只需创建和修改审计：继承 `AuditedAggregateRoot<Guid>`
- 只需创建审计：继承 `CreationAuditedAggregateRoot<Guid>`

## 本地化资源规范

每个新模块必须在本地化文件中添加对应的翻译：

```json
// zh-Hans.json 中新增（示例）
"{实体名}Management": "产品管理",
"Permission:{实体名}s": "产品管理",
"Permission:{实体名}s.Create": "创建产品",
"Permission:{实体名}s.Update": "编辑产品",
"Permission:{实体名}s.Delete": "删除产品"
```

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
| 外键关联 | `Guid` | `HasIndex` | `string` | `el-select（远程搜索）` |

## 权限验证集成

前端路由配置中的 `meta.permission` 字段值必须与后端权限常量完全对应：

```typescript
// 前端路由
meta: { permission: 'Products.Products' }

// 后端权限常量
public const string Default = "Products.Products";
```

前端路由守卫会根据 `meta.permission` 自动检查用户是否拥有对应权限。

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

**生成所有文件，然后输出集成清单。**
