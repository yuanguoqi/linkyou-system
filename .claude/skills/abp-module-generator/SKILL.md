---
name: abp-module-generator
description: |
  ABP vNext 完整业务模块生成器，用于一次性生成前后端完整CRUD业务模块。当用户说
  "生成完整模块"、"创建业务模块"、"全栈CRUD"、"前后端一起生成"、"生成完整功能"、
  "新增业务功能"、"做一个XX管理模块"时，优先使用此skill（而不是分别调用
  abp-backend-scaffolder 和 vue3-frontend-scaffolder）。此skill会协调后端和前端的
  代码生成，确保命名约定、API路径、权限定义完全一致。一站式生成：不要让用户分两步走。
---

# ABP 完整业务模块生成器

## 生成流程

```
1. 分析需求 → 确定实体名称、字段、业务规则
2. 生成后端代码（遵循 abp-backend-scaffolder 规范）
3. 生成前端代码（遵循 vue3-frontend-scaffolder + linkyou-design-system 规范）
4. 输出数据库迁移命令
5. 输出集成清单（需要手动修改的文件列表）
```

## 信息收集

| 项目 | 说明 | 示例 |
|------|------|------|
| 模块名称 | C# 命名空间 | `Products` |
| 实体名称 | 主实体类名（单数） | `Product` |
| 实体字段 | 属性名和类型 | `Name(string)`, `Price(decimal)` |
| 是否多租户 | 实体是否区分租户 | 是（默认）/ 否 |
| 是否软删除 | 是否需要软删除 | 是（默认）|

## 命名约定（前后端统一）

| 位置 | 格式 | 示例（实体=Product） |
|------|------|------|
| C# 命名空间 | `Linkyou.System.{模块名}` | `Linkyou.System.Products` |
| API路由 | `/api/app/{实体小写复数}` | `/api/app/products` |
| 前端API | `web/src/api/modules/{模块小写}.ts` | `products.ts` |
| Vue页面 | `web/src/views/system/{模块小写}/` | `views/system/products/` |
| 权限常量 | `{模块名}.{实体复数}` | `Products.Products` |

## 输出结构

### 后端文件
```
Domain.Shared/{模块名}/  → {实体名}Consts.cs, {实体名}ErrorCodes.cs
Domain/{模块名}/         → {实体名}.cs, I{实体名}Repository.cs, {实体名}Manager.cs
Application.Contracts/{模块名}/ → DTO, 权限, 接口
Application/{模块名}/    → {实体名}AppService.cs
EFCore/{模块名}/         → Ef{实体名}Repository.cs
HttpApi/{模块名}/        → {实体名}Controller.cs
```

### 前端文件
```
web/src/api/modules/{模块小写}.ts
web/src/views/system/{模块小写}/index.vue
web/src/views/system/{模块小写}/components/{实体名}Dialog.vue
```

### 集成清单
生成后输出需要手动完成的步骤：
1. DbContext 添加 DbSet
2. DbContextModelCreatingExtensions 添加实体配置（**不加 Lys_ 前缀**）
3. AutoMapper 添加映射
4. 本地化资源添加翻译
5. 数据库迁移命令
6. 种子数据（如需菜单项）

## 关键规范

- 业务表名**不加** `Lys_` 前缀：`b.ToTable("{实体名}s", LinkyouSystemConsts.DbSchema)`
- 控制器基类使用 `LinkyouSystemController`（在 `Linkyou.System.Controllers` 命名空间）
- 控制器**不使用** `[ControllerName]` 属性
- 权限定义提供者需要 `using Linkyou.System.Localization;`
- API 返回 axios 响应，前端通过 `.data` 获取数据
- 表格行类型需要断言：`@click="handleEdit(row as XxxDto)"`
