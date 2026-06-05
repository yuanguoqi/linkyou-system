# 领佑通用管理系统 — 开发操作手册

## 技术栈

| 层级 | 技术 |
|------|------|
| 后端框架 | ABP vNext 10.4 / .NET 10.0 |
| 数据库 | PostgreSQL |
| ORM | Entity Framework Core 10.0 |
| 前端框架 | Vue 3.4+ / Vite 5.0+ |
| UI 组件库 | Element Plus 2.7+ |
| 状态管理 | Pinia 2.1+ |
| 路由 | Vue Router 4.3+ |

---

## 目录结构

```
linkyou-system/
├── src/
│   ├── Linkyou.System.Domain/                  # 领域层
│   ├── Linkyou.System.Domain.Shared/           # 领域共享层
│   ├── Linkyou.System.Application.Contracts/   # 应用契约层
│   ├── Linkyou.System.Application/             # 应用层
│   ├── Linkyou.System.EntityFrameworkCore/     # EF Core 层（迁移在此）
│   ├── Linkyou.System.HttpApi/                 # HTTP API 层
│   ├── Linkyou.System.HttpApi.Client/          # HTTP 客户端层
│   └── Linkyou.System.Host/                    # 宿主层（启动入口）
└── web/                                        # 前端项目
```

---

## 环境准备

### 前置要求

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download)
- [Node.js 20+](https://nodejs.org/)
- [PostgreSQL 14+](https://www.postgresql.org/)

### 数据库配置

连接字符串在 `src/Linkyou.System.Host/appsettings.json` 中配置：

```json
"ConnectionStrings": {
  "Default": "Host=<host>;Port=5432;Database=linkyou;Username=<user>;Password=<password>;Include Error Detail=true"
}
```

> **注意**：开发环境可在 `appsettings.Development.json` 中覆盖连接字符串，该文件不应提交到版本控制。

---

## 数据库迁移

所有迁移命令均在 EF Core 项目目录下执行，并指向 Host 作为启动项目。

### 创建新迁移

```bash
cd src/Linkyou.System.EntityFrameworkCore

dotnet ef migrations add <MigrationName> --startup-project ../Linkyou.System.Host
```

**示例：**

```bash
dotnet ef migrations add InitialCreate --startup-project ../Linkyou.System.Host
dotnet ef migrations add AddUserProfile --startup-project ../Linkyou.System.Host
```

### 应用迁移到数据库

```bash
cd src/Linkyou.System.EntityFrameworkCore

dotnet ef database update --startup-project ../Linkyou.System.Host
```

### 回滚迁移

```bash
# 回滚到指定迁移版本
dotnet ef database update <TargetMigrationName> --startup-project ../Linkyou.System.Host

# 回滚所有迁移（清空数据库结构）
dotnet ef database update 0 --startup-project ../Linkyou.System.Host
```

### 删除最后一次迁移（未应用时）

```bash
dotnet ef migrations remove --startup-project ../Linkyou.System.Host
```

### 查看迁移列表

```bash
dotnet ef migrations list --startup-project ../Linkyou.System.Host
```

---

## 后端启动

```bash
cd src/Linkyou.System.Host

dotnet run
```

启动成功后监听地址：

| 协议 | 地址 |
|------|------|
| HTTPS | https://localhost:11439 |
| HTTP | http://localhost:11440 |

### Swagger UI

启动后访问：[https://localhost:11439/swagger](https://localhost:11439/swagger)

### 生产环境发布

```bash
dotnet publish src/Linkyou.System.Host -c Release -o ./publish
```

---

## 前端启动

```bash
cd web

# 首次运行或依赖变更后安装依赖
npm install

# 开发模式启动（热更新）
npm run dev
```

前端默认运行在：[http://localhost:5173](http://localhost:5173)

### 其他前端命令

```bash
# 生产构建
npm run build

# 预览生产构建
npm run preview

# 代码格式化
npm run format

# ESLint 检查并自动修复
npm run lint
```

---

## 注意事项

### Npgsql DateTime 时区

Npgsql 8.x 默认要求写入 `timestamp with time zone` 的 `DateTime` 必须是 UTC，而 ABP 框架内部在部分场景使用本地时间（Local）。

项目已在 `src/Linkyou.System.Host/Program.cs` 顶部添加兼容开关：

```csharp
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
```

> **说明**：此开关让 Npgsql 以旧版行为处理时间类型，避免 `Cannot write DateTime with Kind=Local` 运行时错误。不要删除此行。

### CORS 跨域配置

前端开发地址需在 `appsettings.json` 的 `App.CorsOrigins` 中注册：

```json
"App": {
  "CorsOrigins": "http://localhost:5173,http://localhost:3000"
}
```

### JWT 签名密钥

`appsettings.json` 中的 `Jwt.SigningKey` 在生产环境必须替换为强随机密钥（建议 64 字符以上），不应使用默认值。

### 多租户

系统启用了多租户模式（`MultiTenancy.IsEnabled: true`）。通过请求头 `__tenant` 传递租户名称，不带此请求头则访问 Host 租户数据。

### EF Core 工具版本

若出现工具版本警告，可更新 EF Core 工具：

```bash
dotnet tool update --global dotnet-ef
```

---

## 常见问题

### 后端启动失败：表不存在

**错误**：`relation "AbpFeatureGroups" does not exist`

**原因**：数据库未建表，迁移尚未应用。

**解决**：

```bash
cd src/Linkyou.System.EntityFrameworkCore
dotnet ef database update --startup-project ../Linkyou.System.Host
```

### 前端 `import.meta.env` 类型报错

**解决**：确认 `web/tsconfig.json` 的 `compilerOptions.types` 包含 `"vite/client"`：

```json
{
  "compilerOptions": {
    "types": ["vite/client"]
  }
}
```

### dayjs `fromNow()` 返回英文

**原因**：未加载 `relativeTime` 插件或中文语言包。

**解决**：确认 `web/src/utils/date.ts` 中有以下代码：

```typescript
import relativeTime from 'dayjs/plugin/relativeTime'
import 'dayjs/locale/zh-cn'
dayjs.extend(relativeTime)
dayjs.locale('zh-cn')
```
