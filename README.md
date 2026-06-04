# Linkyou.System 领佑通用管理系统

基于 **ABP vNext 10.4** + **.NET 10** + **Vue 3.4** 的企业级管理系统。

## 技术栈

| 层次 | 技术 |
|------|------|
| 后端框架 | ABP vNext 10.4 |
| 运行时 | .NET 10.0 |
| 数据库 | PostgreSQL 16 |
| ORM | Entity Framework Core 10.0 |
| 认证 | JWT Bearer + 刷新令牌 |
| 前端框架 | Vue 3.4 + Vite 5 |
| UI 组件库 | Element Plus 2.7 |
| 状态管理 | Pinia 2.1 |
| 容器化 | Docker + Docker Compose |

## 项目结构

```
linkyou-system/
├── src/
│   ├── Linkyou.System.Domain.Shared/       # 共享常量、枚举、错误码、本地化
│   ├── Linkyou.System.Domain/              # 领域实体、领域服务、仓储接口
│   ├── Linkyou.System.Application.Contracts/ # DTO、应用服务接口、权限定义
│   ├── Linkyou.System.Application/         # 应用服务实现、AutoMapper
│   ├── Linkyou.System.EntityFrameworkCore/ # DbContext、EF Core 迁移、仓储实现
│   ├── Linkyou.System.HttpApi/             # REST API 控制器
│   ├── Linkyou.System.HttpApi.Client/      # HTTP 动态代理客户端
│   └── Linkyou.System.Host/               # 应用入口、中间件配置
├── web/                                    # Vue 3 前端（待生成）
├── docker-compose.yml
└── Linkyou.System.sln
```

## 快速开始

### 环境要求

- .NET 10 SDK
- Node.js 20+
- Docker Desktop（可选，用于本地数据库）
- PostgreSQL 16（或使用 Docker）

### 1. 启动数据库（Docker 方式）

```bash
# 仅启动 PostgreSQL 和 pgAdmin
docker compose --profile dev up db pgadmin -d
```

数据库连接信息：
- Host: `localhost:5432`
- Database: `LinkyouSystem`
- User: `postgres` / Password: `postgres`

pgAdmin 访问地址：`http://localhost:5050`（admin@linkyou.com / admin123）

### 2. 配置连接字符串

编辑 `src/Linkyou.System.Host/appsettings.Development.json`：

```json
{
  "ConnectionStrings": {
    "Default": "Host=localhost;Port=5432;Database=LinkyouSystem;Username=postgres;Password=postgres"
  }
}
```

### 3. 执行数据库迁移

```bash
cd src/Linkyou.System.EntityFrameworkCore

# 创建初始迁移（首次执行）
dotnet ef migrations add InitialCreate --startup-project ../Linkyou.System.Host

# 应用迁移到数据库
dotnet ef database update --startup-project ../Linkyou.System.Host
```

### 4. 启动后端 API

```bash
cd src/Linkyou.System.Host
dotnet run
```

API 地址：`http://localhost:5000`
Swagger 文档：`http://localhost:5000/swagger`

### 5. 启动前端（待生成）

```bash
cd web
npm install
npm run dev
```

前端地址：`http://localhost:5173`

---

## Docker 完整部署

```bash
# 构建并启动所有服务
docker compose up -d --build

# 查看服务状态
docker compose ps

# 查看 API 日志
docker compose logs -f api
```

服务端口：
- API：`http://localhost:5000`
- PostgreSQL：`localhost:5432`
- pgAdmin（dev profile）：`http://localhost:5050`

---

## 开发指南

### 新增业务模块

使用 `abp-module-generator` skill 一键生成前后端完整模块：

> "帮我做一个商品管理模块，包含名称、价格、库存字段"

### 执行数据库迁移

```bash
# 每次修改实体后，在 EntityFrameworkCore 目录下执行：
dotnet ef migrations add <MigrationName> --startup-project ../Linkyou.System.Host

# 应用到数据库：
dotnet ef database update --startup-project ../Linkyou.System.Host
```

### 多租户

系统默认启用多租户，前端请求时通过请求头传递租户标识：

```
__tenant: TenantName
```

### 默认管理员账号

首次启动后，系统自动创建默认管理员：
- 邮箱：`admin@linkyou.com`
- 密码：`Admin@123456`

---

## 项目依赖层次

```
Host
 └── HttpApi
      └── Application.Contracts
           └── Domain.Shared
 └── Application
      └── Domain
           └── Domain.Shared
 └── EntityFrameworkCore
      └── Domain
```
