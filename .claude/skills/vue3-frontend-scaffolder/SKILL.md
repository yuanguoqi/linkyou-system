---
name: vue3-frontend-scaffolder
description: |
  Vue 3.4+ 前端代码生成器，专为 Linkyou.System 项目设计。当用户需要创建前端页面、
  组件、store、API调用、路由配置时，必须使用此skill。触发场景包括：
  "创建页面"、"新建组件"、"添加路由"、"创建store"、"生成前端代码"、"写Vue组件"、
  "添加列表页"、"创建表单"、"生成CRUD页面"、"前端脚手架"、"写接口调用"等。
  只要用户提到Vue前端开发的任何需求，都应主动使用此skill。
  生成代码必须遵循 linkyou-design-system 设计规范。
---

# Vue 3.4+ 前端代码生成器

## 项目上下文

- **前端框架**: Vue 3.4+（Composition API + `<script setup>` 语法）
- **构建工具**: Vite 5.0+
- **UI组件库**: Element Plus 2.7+
- **状态管理**: Pinia 2.1+
- **路由**: Vue Router 4.3+
- **HTTP客户端**: Axios 1.7+
- **前端目录**: `web/src/`
- **样式**: SCSS + CSS 变量驱动主题（深色默认，`html.light` 浅色覆盖）

## 项目目录结构

```
web/src/
├── api/
│   ├── modules/            # 各业务模块 API（如 menus.ts）
│   └── index.ts            # axios 实例和拦截器
├── assets/
│   ├── logo.svg
│   └── styles/
│       ├── variables.scss  # SCSS 变量
│       └── index.scss      # 全局样式 + CSS 变量
├── composables/            # 组合式函数（useMenu, usePagination, usePermission）
├── layouts/
│   ├── MainLayout.vue      # 主布局
│   └── components/         # SidebarMenu, HeaderBar, TabsNav
├── locales/                # i18n 翻译文件
├── router/
│   ├── index.ts            # 路由守卫 + 权限检查
│   └── modules/            # 按模块拆分路由
├── stores/
│   ├── auth.ts             # 认证状态（token, user, permissions）
│   ├── app.ts              # 应用状态（sidebar, tabs, breadcrumbs）
│   └── theme.ts            # 主题切换（dark/light）
├── types/                  # TypeScript 类型定义
├── utils/
│   ├── common.ts           # 通用工具（storage, debounce）
│   └── date.ts             # 日期格式化（formatDateTime, formatDate）
└── views/
    ├── dashboard/          # 仪表盘
    ├── login/              # 登录页
    ├── error/              # 403/404 错误页
    └── system/             # 系统管理模块
        ├── users/          # 用户管理
        ├── roles/          # 角色管理
        ├── tenants/        # 租户管理
        ├── audit-logs/     # 审计日志
        ├── menus/          # 菜单管理
        └── settings/       # 系统设置
```

## 已有前端模块参考

| 模块 | API文件 | 页面 | 说明 |
|------|---------|------|------|
| 认证 | `api/modules/auth.ts` | `views/login/` | 登录、刷新令牌、用户信息 |
| 用户管理 | `api/modules/identity.ts` | `views/system/users/` | 调用 ABP Identity API |
| 角色管理 | `api/modules/identity.ts` | `views/system/roles/` | 调用 ABP Identity API |
| 租户管理 | `api/modules/tenants.ts` | `views/system/tenants/` | 调用 ABP TenantManagement API |
| 审计日志 | `api/modules/audit-logs.ts` | `views/system/audit-logs/` | 调用 ABP AuditLogging API |
| 菜单管理 | `api/modules/menus.ts` | `views/system/menus/` | 自定义 API + 角色权限 |
| 系统设置 | `api/modules/settings.ts` | `views/system/settings/` | 调用 ABP SettingManagement API |

## 动态菜单系统

项目使用动态菜单，根据用户角色权限过滤显示：

- **菜单配置**: `composables/useMenu.ts` — 调用 `GET /api/app/menus/user-menus`
- **后端返回**: 树形结构 `UserMenuDto[]`，已根据角色权限过滤
- **侧边栏**: `layouts/components/SidebarMenu.vue` — 渲染动态菜单

## 代码生成规范

### 1. API 接口定义

```typescript
// 文件路径: web/src/api/modules/{模块名小写}.ts
import { http } from '../index'
import type { PagedResultDto } from '@/types/common'

export interface {实体名}Dto {
  id: string
  name: string
  creationTime: string
}

export interface Create{实体名}Dto {
  name: string
}

export interface Update{实体名}Dto extends Create{实体名}Dto {}

export interface Get{实体名}ListInput {
  filter?: string
  skipCount?: number
  maxResultCount?: number
  sorting?: string
}

const BASE_URL = '/api/app/{实体名小写复数}'

export const {实体名小写}Api = {
  get: (id: string) =>
    http.get<{实体名}Dto>(`${BASE_URL}/${id}`),

  getList: (params: Get{实体名}ListInput) =>
    http.get<PagedResultDto<{实体名}Dto>>(BASE_URL, { params }),

  create: (data: Create{实体名}Dto) =>
    http.post<{实体名}Dto>(BASE_URL, data),

  update: (id: string, data: Update{实体名}Dto) =>
    http.put<{实体名}Dto>(`${BASE_URL}/${id}`, data),

  delete: (id: string) =>
    http.delete(`${BASE_URL}/${id}`),
}
```

**重要**: API 返回的是 axios 响应，需要通过 `.data` 获取实际数据：
```typescript
const res = await xxxApi.getList(params)
tableData.value = res.data.items  // ← 使用 .data
```

### 2. 列表页面组件

参考 `views/system/users/index.vue` 或 `views/system/roles/index.vue` 的完整实现。

关键模式：
- 使用 `.page-container` 根容器
- 使用 `.panel` 替代 `el-card`
- 表格行类型需要断言：`@click="handleEdit(row as XxxDto)"`
- 分页使用 `usePagination` composable
- 日期格式化使用 `formatDateTime` from `@/utils/date`（不要内联实现）

### 3. 弹窗组件

参考 `views/system/users/components/UserDialog.vue`。

关键模式：
- `el-dialog` 使用深色主题覆盖
- `el-form` 使用深色输入框覆盖
- Props: `visible`, `editId`
- Emits: `update:visible`, `saved`

### 4. 路由配置

```typescript
// 文件路径: web/src/router/modules/{模块名小写}.ts
import type { RouteRecordRaw } from 'vue-router'

const {模块名小写}Routes: RouteRecordRaw[] = [
  {
    path: '/{模块名小写}',
    meta: { title: '{模块名}', icon: 'Setting' },
    children: [
      {
        path: '{实体名小写复数}',
        name: '{实体名}List',
        component: () => import('@/views/{模块名小写}/{实体名小写复数}/index.vue'),
        meta: {
          title: '{实体名}管理',
          icon: 'IconName',
          permission: '{模块名}.{实体名}s',
        },
      },
    ],
  },
]

export default {模块名小写}Routes
```

路由注册在 `router/index.ts` 中展平：
```typescript
...systemRoutes.flatMap(group =>
  (group.children || []).map(child => ({
    ...child,
    path: `${group.path}/${child.path}`.replace(/^\//, ''),
  }))
),
```

### 5. 菜单配置

如果新模块需要出现在侧边栏，需要在后端种子数据中添加菜单项：
- `LinkyouSystemDataSeederContributor.SeedMenusAsync()` 中添加菜单记录
- 为 `superadmin` 角色添加 `MenuRolePermission` 记录

## 样式规范

所有样式必须遵循 `linkyou-design-system` skill 中的规范：
- 使用 CSS 变量（`var(--bg-card)`, `var(--text-primary)` 等）
- 使用 `:deep()` 覆盖 Element Plus 样式
- 使用 `.btn-primary`、`.btn-ghost` 按钮样式
- 使用 `.panel`、`.panel-header`、`.panel-title` 容器样式

## 前端安全规范

1. Token 存于 `localStorage`，不放入 Pinia 全局 state
2. 模板绑定使用 `:` 或 `{{ }}`，不使用 `v-html`
3. axios 响应拦截器统一处理错误
4. 路由守卫检查 `meta.permission`
5. 所有异步操作加 loading 状态
