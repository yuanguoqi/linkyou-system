---
name: vue3-frontend-scaffolder
description: |
  Vue 3.4+ 前端代码生成器，专为 Linkyou.System 项目设计。当用户需要创建前端页面、
  组件、store、API调用、路由配置时，必须使用此skill。触发场景包括：
  "创建页面"、"新建组件"、"添加路由"、"创建store"、"生成前端代码"、"写Vue组件"、
  "添加列表页"、"创建表单"、"生成CRUD页面"、"前端脚手架"、"写接口调用"等。
  只要用户提到Vue前端开发的任何需求，都应主动使用此skill。
  生成代码必须遵循 linkyou-design-system 深色科技感风格规范。
---

# Vue 3.4+ 前端代码生成器

## 项目上下文

- **前端框架**: Vue 3.4+（Composition API + `<script setup>` 语法）
- **构建工具**: Vite 5.0+
- **UI组件库**: Element Plus 2.7+
- **状态管理**: Pinia 2.1+
- **路由**: Vue Router 4.3+
- **HTTP客户端**: Axios 1.7+
- **前端目录**: `web/src/`（实际路径，不是 `src/Linkyou.System.Web/`）
- **样式**: SCSS + CSS 变量驱动主题（深色默认，`html.light` 浅色覆盖）

## 项目目录结构

```
web/src/
├── api/
│   ├── modules/            # 各业务模块 API（如 products.ts）
│   └── index.ts            # axios 实例和拦截器
├── components/             # 全局公共组件
├── composables/            # 组合式函数（useXxx）
├── layouts/                # 布局组件
├── router/
│   ├── index.ts
│   └── modules/            # 按模块拆分路由
├── stores/
│   ├── auth.ts
│   ├── app.ts
│   ├── theme.ts
│   └── modules/            # 各业务模块 store
├── types/                  # TypeScript 类型定义
├── utils/
├── views/
│   └── {模块名}/
│       ├── index.vue
│       └── components/
└── assets/styles/
    └── index.scss          # 全局 CSS 变量（深色/浅色主题）
```

## 风格规范（必须遵守）

生成的所有 `.vue` 文件必须遵循以下规则，与项目深色科技感设计保持一致。

**禁止使用 `el-card`**，改用自定义 `.panel` 结构。
**Element Plus 组件必须用 `:deep()` 覆盖**为深色样式。
**所有颜色使用 CSS 变量**，不要硬编码颜色值。

### 页面根容器

```vue
<div class="page-container">
  <!-- 内容 -->
</div>

<style scoped lang="scss">
.page-container {
  padding: 20px;
  display: flex;
  flex-direction: column;
  gap: 16px;
  min-height: 100%;
}
</style>
```

### 面板/卡片容器（替代 el-card）

```vue
<div class="panel">
  <div class="panel-header">
    <span class="panel-title">标题</span>
    <div class="panel-actions"><!-- 操作按钮 --></div>
  </div>
  <div class="panel-body"><!-- 内容 --></div>
</div>

<style scoped lang="scss">
.panel {
  background: var(--bg-card);
  border: 1px solid var(--border-default);
  border-radius: 14px;
  overflow: hidden;

  .panel-header {
    display: flex;
    align-items: center;
    justify-content: space-between;
    padding: 16px 20px;
    border-bottom: 1px solid var(--border-subtle);

    .panel-title {
      font-size: 14px;
      font-weight: 600;
      color: var(--text-primary);
    }
  }

  .panel-body { padding: 20px; }
}
</style>
```

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
  creatorId: string | null
  lastModificationTime: string | null
  isDeleted: boolean
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
    http.delete(`${BASE_URL}/${id}`)
}
```

### 2. 列表页面组件（深色风格）

```vue
<!-- 文件路径: web/src/views/{模块名小写}/index.vue -->
<script setup lang="ts">
import { ref, reactive, onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { {实体名小写}Api } from '@/api/modules/{模块名小写}'
import type { {实体名}Dto } from '@/api/modules/{模块名小写}'
import {实体名}Dialog from './components/{实体名}Dialog.vue'

const queryParams = reactive({
  filter: '',
  skipCount: 0,
  maxResultCount: 10,
  sorting: 'creationTime desc'
})
const currentPage = ref(1)
const pageSize = ref(10)
const tableData = ref<{实体名}Dto[]>([])
const total = ref(0)
const tableLoading = ref(false)
const dialogVisible = ref(false)
const editId = ref<string | undefined>()

async function fetchList() {
  tableLoading.value = true
  try {
    queryParams.skipCount = (currentPage.value - 1) * pageSize.value
    queryParams.maxResultCount = pageSize.value
    const res = await {实体名小写}Api.getList(queryParams)
    tableData.value = res.data.items
    total.value = res.data.totalCount
  } finally {
    tableLoading.value = false
  }
}

function handleSearch() { currentPage.value = 1; fetchList() }
function handleReset() { queryParams.filter = ''; currentPage.value = 1; fetchList() }
function handleCreate() { editId.value = undefined; dialogVisible.value = true }
function handleEdit(row: {实体名}Dto) { editId.value = row.id; dialogVisible.value = true }

async function handleDelete(row: {实体名}Dto) {
  await ElMessageBox.confirm(`确定要删除 "${row.name}" 吗？此操作不可撤销。`, '删除确认', { type: 'warning' })
  await {实体名小写}Api.delete(row.id)
  ElMessage.success('删除成功')
  fetchList()
}

function handleSaved() { dialogVisible.value = false; fetchList() }
function handlePageChange(page: number) { currentPage.value = page; fetchList() }
function handleSizeChange(size: number) { pageSize.value = size; currentPage.value = 1; fetchList() }

onMounted(fetchList)
</script>

<template>
  <div class="page-container">
    <!-- 搜索区域 -->
    <div class="panel search-panel">
      <div class="panel-body">
        <div class="search-row">
          <el-input
            v-model="queryParams.filter"
            placeholder="搜索关键词..."
            clearable
            class="search-input"
            @keyup.enter="handleSearch"
          >
            <template #prefix>
              <el-icon><Search /></el-icon>
            </template>
          </el-input>
          <el-button class="btn-primary" @click="handleSearch">查询</el-button>
          <el-button class="btn-ghost" @click="handleReset">重置</el-button>
        </div>
      </div>
    </div>

    <!-- 数据面板 -->
    <div class="panel">
      <div class="panel-header">
        <span class="panel-title">{实体名}管理</span>
        <el-button class="btn-primary" @click="handleCreate">
          <el-icon><Plus /></el-icon>
          新增
        </el-button>
      </div>
      <div class="panel-body">
        <el-table v-loading="tableLoading" :data="tableData">
          <el-table-column prop="name" label="名称" min-width="150" />
          <el-table-column
            prop="creationTime"
            label="创建时间"
            width="180"
            :formatter="(_, __, val) => val ? new Date(val).toLocaleString('zh-CN') : '-'"
          />
          <el-table-column label="操作" width="160" fixed="right">
            <template #default="{ row }">
              <el-button type="primary" link @click="handleEdit(row)">编辑</el-button>
              <el-button type="danger" link @click="handleDelete(row)">删除</el-button>
            </template>
          </el-table-column>
        </el-table>

        <el-pagination
          v-model:current-page="currentPage"
          v-model:page-size="pageSize"
          :total="total"
          :page-sizes="[10, 20, 50, 100]"
          layout="total, sizes, prev, pager, next, jumper"
          @current-change="handlePageChange"
          @size-change="handleSizeChange"
        />
      </div>
    </div>

    <{实体名}Dialog v-model:visible="dialogVisible" :edit-id="editId" @saved="handleSaved" />
  </div>
</template>

<style scoped lang="scss">
.page-container {
  padding: 20px;
  display: flex;
  flex-direction: column;
  gap: 16px;
  min-height: 100%;
}

.panel {
  background: var(--bg-card);
  border: 1px solid var(--border-default);
  border-radius: 14px;
  overflow: hidden;

  .panel-header {
    display: flex;
    align-items: center;
    justify-content: space-between;
    padding: 16px 20px;
    border-bottom: 1px solid var(--border-subtle);

    .panel-title { font-size: 14px; font-weight: 600; color: var(--text-primary); }
  }

  .panel-body { padding: 20px; }
}

.search-row {
  display: flex;
  gap: 10px;
  align-items: center;
  .search-input { width: 280px; }
}

.btn-primary {
  background: linear-gradient(135deg, #6366f1, #818cf8);
  border: none;
  color: #fff;
  border-radius: 8px;
  box-shadow: 0 4px 12px rgba(99, 102, 241, 0.3);
  transition: all 0.2s;
  &:hover { transform: translateY(-1px); box-shadow: 0 6px 18px rgba(99, 102, 241, 0.4); }
}

.btn-ghost {
  background: var(--bg-input);
  border: 1px solid var(--border-subtle);
  color: var(--text-secondary);
  border-radius: 8px;
  &:hover { background: rgba(99, 102, 241, 0.08); border-color: rgba(99, 102, 241, 0.3); color: #a5b4fc; }
}

:deep(.el-table) {
  background: transparent;
  color: var(--text-primary);

  th.el-table__cell {
    background: rgba(255, 255, 255, 0.03);
    color: var(--text-secondary);
    font-weight: 500;
    border-bottom: 1px solid var(--border-default);
  }

  td.el-table__cell { border-bottom: 1px solid rgba(255, 255, 255, 0.04); }
  tr:hover > td { background: rgba(99, 102, 241, 0.05) !important; }
  .el-table__empty-block { background: transparent; color: var(--text-muted); }
}

:deep(.el-input) {
  --el-input-bg-color: var(--bg-input);
  --el-input-border-color: var(--border-subtle);
  --el-input-text-color: var(--text-primary);
  --el-input-placeholder-color: var(--text-muted);
  --el-input-focus-border-color: #6366f1;

  .el-input__wrapper {
    border-radius: 8px;
    box-shadow: 0 0 0 1px var(--border-subtle) inset;
    &:hover { box-shadow: 0 0 0 1px rgba(99, 102, 241, 0.3) inset; }
    &.is-focus { box-shadow: 0 0 0 1px #6366f1 inset, 0 0 0 3px rgba(99, 102, 241, 0.12); }
  }
}

:deep(.el-pagination) {
  justify-content: flex-end;
  margin-top: 16px;
  --el-pagination-bg-color: transparent;
  --el-pagination-text-color: var(--text-muted);

  .el-pager li {
    background: var(--bg-input);
    border: 1px solid var(--border-subtle);
    color: var(--text-muted);
    border-radius: 6px;
    &.is-active { background: #6366f1; border-color: #6366f1; color: #fff; }
    &:hover:not(.is-active) { background: rgba(99, 102, 241, 0.1); color: #a5b4fc; }
  }

  button {
    background: var(--bg-input);
    border: 1px solid var(--border-subtle);
    color: var(--text-muted);
    border-radius: 6px;
    &:hover { color: #a5b4fc; }
    &:disabled { opacity: 0.3; }
  }
}
</style>
```

### 3. 新增/编辑弹窗组件（深色风格）

```vue
<!-- 文件路径: web/src/views/{模块名小写}/components/{实体名}Dialog.vue -->
<script setup lang="ts">
import { ref, watch } from 'vue'
import { ElMessage } from 'element-plus'
import type { FormInstance, FormRules } from 'element-plus'
import { {实体名小写}Api } from '@/api/modules/{模块名小写}'
import type { Create{实体名}Dto } from '@/api/modules/{模块名小写}'

const props = defineProps<{ visible: boolean; editId?: string }>()
const emit = defineEmits<{ 'update:visible': [value: boolean]; saved: [] }>()

const formRef = ref<FormInstance>()
const submitting = ref(false)
const form = ref<Create{实体名}Dto>({ name: '' })

const rules: FormRules = {
  name: [
    { required: true, message: '请输入名称', trigger: 'blur' },
    { max: 128, message: '名称不能超过128个字符', trigger: 'blur' }
  ]
}

watch(() => props.visible, async (val) => {
  if (!val) return
  if (props.editId) {
    const res = await {实体名小写}Api.get(props.editId)
    form.value = { name: res.data.name }
  } else {
    form.value = { name: '' }
  }
})

async function handleSubmit() {
  await formRef.value?.validate()
  submitting.value = true
  try {
    if (props.editId) {
      await {实体名小写}Api.update(props.editId, form.value)
      ElMessage.success('更新成功')
    } else {
      await {实体名小写}Api.create(form.value)
      ElMessage.success('创建成功')
    }
    emit('saved')
  } finally {
    submitting.value = false
  }
}

function handleClose() { emit('update:visible', false) }
</script>

<template>
  <el-dialog
    :model-value="visible"
    :title="editId ? '编辑{实体名}' : '新增{实体名}'"
    width="520px"
    :close-on-click-modal="false"
    @update:model-value="handleClose"
  >
    <el-form ref="formRef" :model="form" :rules="rules" label-width="80px">
      <el-form-item label="名称" prop="name">
        <el-input v-model="form.name" placeholder="请输入名称" maxlength="128" show-word-limit />
      </el-form-item>
    </el-form>

    <template #footer>
      <el-button class="btn-ghost" @click="handleClose">取消</el-button>
      <el-button class="btn-primary" :loading="submitting" @click="handleSubmit">确定</el-button>
    </template>
  </el-dialog>
</template>

<style scoped lang="scss">
:deep(.el-dialog) {
  background: #1a1d2e;
  border: 1px solid rgba(255, 255, 255, 0.08);
  border-radius: 16px;

  .el-dialog__header { border-bottom: 1px solid rgba(255, 255, 255, 0.06); padding: 20px 24px; }
  .el-dialog__title { color: #e2e8f0; font-weight: 600; font-size: 16px; }
  .el-dialog__body { padding: 24px; }
  .el-dialog__footer { padding: 16px 24px; border-top: 1px solid rgba(255, 255, 255, 0.06); }
}

:deep(.el-form-item__label) { color: #94a3b8; }

:deep(.el-input) {
  --el-input-bg-color: rgba(255, 255, 255, 0.04);
  --el-input-border-color: rgba(255, 255, 255, 0.08);
  --el-input-text-color: #e2e8f0;
  --el-input-placeholder-color: #475569;
  --el-input-focus-border-color: #6366f1;

  .el-input__wrapper {
    border-radius: 10px;
    box-shadow: 0 0 0 1px rgba(255, 255, 255, 0.08) inset;
    &:hover { box-shadow: 0 0 0 1px rgba(99, 102, 241, 0.3) inset; }
    &.is-focus { box-shadow: 0 0 0 1px #6366f1 inset, 0 0 0 3px rgba(99, 102, 241, 0.12); }
  }
}

.btn-primary {
  background: linear-gradient(135deg, #6366f1, #818cf8);
  border: none;
  color: #fff;
  border-radius: 8px;
  box-shadow: 0 4px 12px rgba(99, 102, 241, 0.3);
  &:hover { transform: translateY(-1px); box-shadow: 0 6px 18px rgba(99, 102, 241, 0.4); }
}

.btn-ghost {
  background: rgba(255, 255, 255, 0.04);
  border: 1px solid rgba(255, 255, 255, 0.08);
  color: #94a3b8;
  border-radius: 8px;
  &:hover { background: rgba(99, 102, 241, 0.08); border-color: rgba(99, 102, 241, 0.3); color: #a5b4fc; }
}
</style>
```

### 4. 路由配置

```typescript
// 文件路径: web/src/router/modules/{模块名小写}.ts
import type { RouteRecordRaw } from 'vue-router'

const {模块名小写}Routes: RouteRecordRaw[] = [
  {
    path: '/{模块名小写}',
    meta: { title: '{模块名}管理', icon: 'Folder' },
    children: [
      {
        path: '{实体名小写复数}',
        name: '{实体名}List',
        component: () => import('@/views/{模块名小写}/index.vue'),
        meta: {
          title: '{实体名}管理',
          permission: '{模块名}.{实体名}s'
        }
      }
    ]
  }
]

export default {模块名小写}Routes
```

### 5. 公共类型定义

```typescript
// 文件路径: web/src/types/common.ts
export interface PagedResultDto<T> {
  items: T[]
  totalCount: number
}

export interface AbpErrorResponse {
  error: {
    code: string
    message: string
    details?: string
    data?: Record<string, unknown>
  }
}
```

## 前端安全规范

1. **不在前端存储敏感数据**：token 存于 `localStorage`，不放入 Vuex/Pinia 全局 state（以防 XSS 序列化泄露）。
2. **XSS 防护**：模板绑定使用 `:` 或 `{{ }}`，绝不使用 `v-html` 渲染用户输入内容。
3. **API 错误统一处理**：axios 响应拦截器统一捕获 4xx/5xx，不在组件内重复弹错。
4. **权限控制**：路由守卫检查 `meta.permission`，无权限时跳转 403 页面，不依赖后端隐藏入口。
5. **Loading 状态**：所有异步操作（getList、create、update、delete）加 loading 变量，防止重复提交。

## 生成步骤

接收到前端代码生成请求后，按以下顺序输出（每个文件完整代码，不可省略）：

1. `web/src/types/common.ts`（如不存在则生成）
2. `web/src/api/modules/{模块名小写}.ts`（API + DTO 类型）
3. `web/src/stores/modules/{模块名小写}.ts`（Pinia Store，业务复杂时生成）
4. `web/src/views/{模块名小写}/index.vue`（列表页，深色风格）
5. `web/src/views/{模块名小写}/components/{实体名}Dialog.vue`（表单弹窗）
6. `web/src/router/modules/{模块名小写}.ts`（路由配置）
7. 路由注册说明（在 `web/src/router/index.ts` 中引入的代码片段）
