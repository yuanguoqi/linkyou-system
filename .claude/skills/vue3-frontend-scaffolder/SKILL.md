---
name: vue3-frontend-scaffolder
description: |
  Vue 3.4+ 前端代码生成器，专为 Linkyou.System 项目设计。当用户需要创建前端页面、
  组件、store、API调用、路由配置时，必须使用此skill。触发场景包括：
  "创建页面"、"新建组件"、"添加路由"、"创建store"、"生成前端代码"、"写Vue组件"、
  "添加列表页"、"创建表单"、"生成CRUD页面"、"前端脚手架"、"写接口调用"等。
  只要用户提到Vue前端开发的任何需求，都应主动使用此skill。
---

# Vue 3.4+ 前端代码生成器

## 项目上下文

- **前端框架**: Vue 3.4+（Composition API + `<script setup>` 语法）
- **构建工具**: Vite 5.0+
- **UI组件库**: Element Plus 2.7+
- **状态管理**: Pinia 2.1+
- **路由**: Vue Router 4.3+
- **HTTP客户端**: Axios 1.7+
- **前端目录**: `src/Linkyou.System.Web/` 或 `web/`

## 项目目录结构

```
src/
├── api/                    # API接口定义（按模块分文件）
│   ├── modules/            # 各业务模块API
│   └── index.ts            # axios实例和拦截器
├── components/             # 全局公共组件
├── composables/            # 组合式函数（useXxx）
├── layouts/                # 布局组件
├── router/                 # 路由配置
│   ├── index.ts
│   └── modules/            # 按模块拆分路由
├── stores/                 # Pinia状态管理
│   ├── auth.ts             # 认证状态
│   └── modules/            # 各业务模块store
├── types/                  # TypeScript类型定义
├── utils/                  # 工具函数
├── views/                  # 页面组件（按模块分目录）
│   └── {模块名}/
│       ├── index.vue       # 列表页
│       ├── detail.vue      # 详情页（可选）
│       └── components/     # 页面私有组件
└── locales/                # i18n本地化文件
    ├── zh-Hans.ts
    └── en.ts
```

## 代码生成规范

### 1. API 接口定义

```typescript
// 文件路径: src/api/modules/{模块名}.ts
import { http } from '../index'
import type { PagedResultDto } from '@/types/common'

// DTO 类型定义（与后端对应）
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
  // 其他必填字段
}

export interface Update{实体名}Dto extends Create{实体名}Dto {
  // 更新特有字段
}

export interface Get{实体名}ListInput {
  filter?: string
  skipCount?: number
  maxResultCount?: number
  sorting?: string
}

// API 方法
const BASE_URL = '/api/app/{实体名小写复数}'

export const {实体名小写}Api = {
  /** 获取单条记录 */
  get: (id: string) =>
    http.get<{实体名}Dto>(`${BASE_URL}/${id}`),

  /** 获取分页列表 */
  getList: (params: Get{实体名}ListInput) =>
    http.get<PagedResultDto<{实体名}Dto>>(BASE_URL, { params }),

  /** 创建 */
  create: (data: Create{实体名}Dto) =>
    http.post<{实体名}Dto>(BASE_URL, data),

  /** 更新 */
  update: (id: string, data: Update{实体名}Dto) =>
    http.put<{实体名}Dto>(`${BASE_URL}/${id}`, data),

  /** 删除 */
  delete: (id: string) =>
    http.delete(`${BASE_URL}/${id}`)
}
```

### 2. Pinia Store

```typescript
// 文件路径: src/stores/modules/{模块名小写}.ts
import { defineStore } from 'pinia'
import { ref } from 'vue'
import { {实体名小写}Api } from '@/api/modules/{模块名小写}'
import type {
  {实体名}Dto,
  Get{实体名}ListInput
} from '@/api/modules/{模块名小写}'

export const use{实体名}Store = defineStore('{实体名小写}', () => {
  // 状态
  const list = ref<{实体名}Dto[]>([])
  const total = ref(0)
  const loading = ref(false)
  const currentItem = ref<{实体名}Dto | null>(null)

  // 获取列表
  async function fetchList(params: Get{实体名}ListInput) {
    loading.value = true
    try {
      const res = await {实体名小写}Api.getList(params)
      list.value = res.data.items
      total.value = res.data.totalCount
    } finally {
      loading.value = false
    }
  }

  // 获取单条
  async function fetchById(id: string) {
    const res = await {实体名小写}Api.get(id)
    currentItem.value = res.data
    return res.data
  }

  // 重置状态
  function reset() {
    list.value = []
    total.value = 0
    currentItem.value = null
  }

  return { list, total, loading, currentItem, fetchList, fetchById, reset }
})
```

### 3. 列表页面组件

```vue
<!-- 文件路径: src/views/{模块名小写}/index.vue -->
<script setup lang="ts">
import { ref, reactive, onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { {实体名小写}Api } from '@/api/modules/{模块名小写}'
import type { {实体名}Dto } from '@/api/modules/{模块名小写}'
import {实体名}Dialog from './components/{实体名}Dialog.vue'

// 查询参数
const queryParams = reactive({
  filter: '',
  skipCount: 0,
  maxResultCount: 10,
  sorting: 'creationTime desc'
})

// 分页
const currentPage = ref(1)
const pageSize = ref(10)

// 数据
const tableData = ref<{实体名}Dto[]>([])
const total = ref(0)
const tableLoading = ref(false)

// 弹窗控制
const dialogVisible = ref(false)
const editId = ref<string | undefined>()

/** 查询列表 */
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

/** 查询按钮 */
function handleSearch() {
  currentPage.value = 1
  fetchList()
}

/** 重置查询 */
function handleReset() {
  queryParams.filter = ''
  currentPage.value = 1
  fetchList()
}

/** 新增 */
function handleCreate() {
  editId.value = undefined
  dialogVisible.value = true
}

/** 编辑 */
function handleEdit(row: {实体名}Dto) {
  editId.value = row.id
  dialogVisible.value = true
}

/** 删除 */
async function handleDelete(row: {实体名}Dto) {
  await ElMessageBox.confirm(
    `确定要删除 "${row.name}" 吗？此操作不可撤销。`,
    '删除确认',
    { type: 'warning' }
  )
  await {实体名小写}Api.delete(row.id)
  ElMessage.success('删除成功')
  fetchList()
}

/** 弹窗保存成功回调 */
function handleSaved() {
  dialogVisible.value = false
  fetchList()
}

/** 分页变化 */
function handlePageChange(page: number) {
  currentPage.value = page
  fetchList()
}

function handleSizeChange(size: number) {
  pageSize.value = size
  currentPage.value = 1
  fetchList()
}

onMounted(fetchList)
</script>

<template>
  <div class="page-container">
    <!-- 查询区域 -->
    <el-card class="search-card" shadow="never">
      <el-form :model="queryParams" inline>
        <el-form-item label="关键词">
          <el-input
            v-model="queryParams.filter"
            placeholder="请输入关键词"
            clearable
            @keyup.enter="handleSearch"
          />
        </el-form-item>
        <el-form-item>
          <el-button type="primary" @click="handleSearch">查询</el-button>
          <el-button @click="handleReset">重置</el-button>
        </el-form-item>
      </el-form>
    </el-card>

    <!-- 列表区域 -->
    <el-card class="table-card" shadow="never">
      <template #header>
        <div class="card-header">
          <span>{实体名}管理</span>
          <el-button type="primary" @click="handleCreate">新增</el-button>
        </div>
      </template>

      <el-table
        v-loading="tableLoading"
        :data="tableData"
        border
        stripe
      >
        <el-table-column prop="name" label="名称" min-width="150" />
        <el-table-column
          prop="creationTime"
          label="创建时间"
          width="180"
          :formatter="(_, __, val) => val ? new Date(val).toLocaleString('zh-CN') : '-'"
        />
        <el-table-column label="操作" width="180" fixed="right">
          <template #default="{ row }">
            <el-button
              type="primary"
              link
              @click="handleEdit(row)"
            >
              编辑
            </el-button>
            <el-button
              type="danger"
              link
              @click="handleDelete(row)"
            >
              删除
            </el-button>
          </template>
        </el-table-column>
      </el-table>

      <!-- 分页 -->
      <el-pagination
        v-model:current-page="currentPage"
        v-model:page-size="pageSize"
        :total="total"
        :page-sizes="[10, 20, 50, 100]"
        layout="total, sizes, prev, pager, next, jumper"
        class="mt-4"
        @current-change="handlePageChange"
        @size-change="handleSizeChange"
      />
    </el-card>

    <!-- 新增/编辑弹窗 -->
    <{实体名}Dialog
      v-model:visible="dialogVisible"
      :edit-id="editId"
      @saved="handleSaved"
    />
  </div>
</template>

<style scoped>
.page-container {
  padding: 16px;
  display: flex;
  flex-direction: column;
  gap: 16px;
}
.search-card :deep(.el-card__body) {
  padding-bottom: 0;
}
.card-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
}
.mt-4 {
  margin-top: 16px;
  justify-content: flex-end;
}
</style>
```

### 4. 新增/编辑弹窗组件

```vue
<!-- 文件路径: src/views/{模块名小写}/components/{实体名}Dialog.vue -->
<script setup lang="ts">
import { ref, watch } from 'vue'
import { ElMessage } from 'element-plus'
import type { FormInstance, FormRules } from 'element-plus'
import { {实体名小写}Api } from '@/api/modules/{模块名小写}'
import type { Create{实体名}Dto } from '@/api/modules/{模块名小写}'

const props = defineProps<{
  visible: boolean
  editId?: string
}>()

const emit = defineEmits<{
  'update:visible': [value: boolean]
  saved: []
}>()

const formRef = ref<FormInstance>()
const submitting = ref(false)

const form = ref<Create{实体名}Dto>({
  name: ''
})

const rules: FormRules = {
  name: [
    { required: true, message: '请输入名称', trigger: 'blur' },
    { max: 128, message: '名称不能超过128个字符', trigger: 'blur' }
  ]
}

// 弹窗打开时初始化数据
watch(
  () => props.visible,
  async (val) => {
    if (!val) return
    if (props.editId) {
      const res = await {实体名小写}Api.get(props.editId)
      form.value = { name: res.data.name }
    } else {
      form.value = { name: '' }
    }
  }
)

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

function handleClose() {
  emit('update:visible', false)
}
</script>

<template>
  <el-dialog
    :model-value="visible"
    :title="editId ? '编辑{实体名}' : '新增{实体名}'"
    width="500px"
    :close-on-click-modal="false"
    @update:model-value="handleClose"
  >
    <el-form
      ref="formRef"
      :model="form"
      :rules="rules"
      label-width="80px"
    >
      <el-form-item label="名称" prop="name">
        <el-input
          v-model="form.name"
          placeholder="请输入名称"
          maxlength="128"
          show-word-limit
        />
      </el-form-item>
    </el-form>

    <template #footer>
      <el-button @click="handleClose">取消</el-button>
      <el-button
        type="primary"
        :loading="submitting"
        @click="handleSubmit"
      >
        确定
      </el-button>
    </template>
  </el-dialog>
</template>
```

### 5. 路由配置

```typescript
// 文件路径: src/router/modules/{模块名小写}.ts
import type { RouteRecordRaw } from 'vue-router'

const {模块名小写}Routes: RouteRecordRaw[] = [
  {
    path: '/{模块名小写}',
    component: () => import('@/layouts/MainLayout.vue'),
    meta: { title: '{模块名}管理', icon: 'Folder' },
    children: [
      {
        path: '{实体名小写复数}',
        name: '{实体名}List',
        component: () => import('@/views/{模块名小写}/index.vue'),
        meta: {
          title: '{实体名}管理',
          // 对应后端权限常量
          permission: '{模块名}.{实体名}s'
        }
      }
    ]
  }
]

export default {模块名小写}Routes
```

### 6. 公共类型定义

```typescript
// 文件路径: src/types/common.ts
/** ABP 分页结果 */
export interface PagedResultDto<T> {
  items: T[]
  totalCount: number
}

/** ABP 错误响应 */
export interface AbpErrorResponse {
  error: {
    code: string
    message: string
    details?: string
    data?: Record<string, unknown>
  }
}
```

## 生成步骤

接收到前端代码生成请求后，按以下顺序生成：
1. `src/types/` 下的 DTO 类型（如未存在通用类型则先生成）
2. `src/api/modules/{模块名小写}.ts`（API接口定义）
3. `src/stores/modules/{模块名小写}.ts`（Pinia Store，如业务复杂则生成）
4. `src/views/{模块名小写}/index.vue`（列表页）
5. `src/views/{模块名小写}/components/{实体名}Dialog.vue`（表单弹窗）
6. `src/router/modules/{模块名小写}.ts`（路由配置）
7. 路由注册说明（在 `src/router/index.ts` 中引入）

每个文件都要完整输出，不能省略代码。
