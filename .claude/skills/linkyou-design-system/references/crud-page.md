# 通用 CRUD 页面模板

使用 `useCrud` composable 快速构建标准列表页，封装了分页、搜索、新增、编辑、删除的完整流程。

## useCrud Composable

```typescript
import { useCrud } from '@/composables/useCrud'

const {
  // 状态
  loading,          // 加载状态
  tableData,        // 表格数据
  totalCount,       // 总条数
  searchFilter,     // 搜索关键词
  currentPage,      // 当前页
  pageSize,         // 每页条数
  dialogVisible,    // 弹窗显示
  dialogMode,       // 'create' | 'edit'
  editingRow,       // 当前编辑行

  // 方法
  fetchData,        // 手动刷新数据
  handleSearch,     // 搜索
  handleReset,      // 重置
  handleCreate,     // 新增
  handleEdit,       // 编辑
  handleDelete,     // 删除
  handleDialogSuccess, // 弹窗成功回调
  handlePageChange, // 页码变化
  handleSizeChange, // 每页条数变化
} = useCrud<EntityDto, CreateDto, UpdateDto>({
  fetchList: api.getList,    // 必填：列表 API
  delete: api.delete,        // 可选：删除 API
  getRowName: (row) => row.name,  // 删除确认消息中的名称
  defaultSorting: 'creationTime desc',
  defaultPageSize: 10,
})
```

## 标准列表页模板

```vue
<script setup lang="ts">
import { xxxApi } from '@/api/modules/xxx'
import type { XxxDto } from '@/api/modules/xxx'
import { useCrud } from '@/composables/useCrud'
import { formatDateTime } from '@/utils/date'
import XxxDialog from './components/XxxDialog.vue'

const {
  loading, tableData, totalCount, searchFilter,
  currentPage, pageSize,
  dialogVisible, dialogMode, editingRow,
  handleSearch, handleReset,
  handleCreate, handleEdit, handleDelete, handleDialogSuccess,
  handlePageChange, handleSizeChange,
} = useCrud<XxxDto, CreateXxxDto>({
  fetchList: xxxApi.getList,
  delete: xxxApi.delete,
  getRowName: (row) => row.name,
})
</script>

<template>
  <div class="page-container">
    <!-- 搜索区 -->
    <div class="panel">
      <div class="panel-header">
        <span class="panel-title font-display">XXX 管理</span>
      </div>
      <div class="search-bar">
        <el-input
          v-model="searchFilter"
          placeholder="搜索..."
          clearable
          class="search-input"
          @keyup.enter="handleSearch"
          @clear="handleSearch"
        >
          <template #prefix><el-icon><Search /></el-icon></template>
        </el-input>
        <div class="search-actions">
          <button class="btn btn-primary" @click="handleSearch">
            <el-icon :size="14"><Search /></el-icon>搜索
          </button>
          <button class="btn btn-ghost" @click="handleReset">
            <el-icon :size="14"><RefreshRight /></el-icon>重置
          </button>
        </div>
      </div>
    </div>

    <!-- 数据区 -->
    <div class="panel">
      <div class="panel-header">
        <span class="panel-title">XXX 列表</span>
        <button class="btn btn-primary" @click="handleCreate">
          <el-icon :size="14"><Plus /></el-icon>新增
        </button>
      </div>

      <el-table :data="tableData" v-loading="loading" class="dark-table" row-key="id">
        <el-table-column prop="name" label="名称" min-width="200" />
        <el-table-column prop="creationTime" label="创建时间" width="200">
          <template #default="{ row }">{{ formatDateTime(row.creationTime) }}</template>
        </el-table-column>
        <el-table-column label="操作" width="160" fixed="right">
          <template #default="{ row }">
            <div class="action-buttons">
              <button class="btn-action btn-action-edit" @click="handleEdit(row as XxxDto)">
                <el-icon :size="13"><Edit /></el-icon>编辑
              </button>
              <button class="btn-action btn-action-delete" @click="handleDelete(row as XxxDto)">
                <el-icon :size="13"><Delete /></el-icon>删除
              </button>
            </div>
          </template>
        </el-table-column>
      </el-table>

      <div class="pagination-wrap">
        <el-pagination
          :current-page="currentPage"
          :page-size="pageSize"
          :page-sizes="[10, 20, 50]"
          :total="totalCount"
          layout="total, sizes, prev, pager, next"
          @current-change="handlePageChange"
          @size-change="handleSizeChange"
        />
      </div>
    </div>

    <!-- 弹窗 -->
    <XxxDialog
      v-model:visible="dialogVisible"
      :mode="dialogMode"
      :data="editingRow"
      @success="handleDialogSuccess"
    />
  </div>
</template>

<style scoped lang="scss">
// 样式直接从 tenants/index.vue 复制，包含：
// .page-container, .panel, .panel-header, .panel-title
// .search-bar, .search-input, .search-actions
// .btn, .btn-primary, .btn-ghost
// .action-buttons, .btn-action, .btn-action-edit, .btn-action-delete
// .pagination-wrap
// :deep(.dark-table), :deep(.el-pagination), :deep(.el-input__wrapper)
</style>
```

## 弹窗组件模板

```vue
<script setup lang="ts">
import { ref, watch, computed } from 'vue'
import { ElMessage } from 'element-plus'
import type { FormInstance, FormRules } from 'element-plus'
import { xxxApi } from '@/api/modules/xxx'

interface Props {
  visible: boolean
  mode: 'create' | 'edit'
  data: XxxDto | null
}

const props = defineProps<Props>()
const emit = defineEmits<{
  'update:visible': [value: boolean]
  success: []
}>()

const formRef = ref<FormInstance>()
const submitting = ref(false)
const isCreate = computed(() => props.mode === 'create')

const form = reactive({ name: '' })

const rules: FormRules = {
  name: [{ required: true, message: '请输入名称', trigger: 'blur' }],
}

watch(() => props.visible, (val) => {
  if (val) {
    formRef.value?.clearValidate()
    if (props.mode === 'edit' && props.data) {
      form.name = props.data.name
    } else {
      form.name = ''
    }
  }
})

function handleClose() { emit('update:visible', false) }

async function handleSubmit() {
  await formRef.value?.validate()
  submitting.value = true
  try {
    if (isCreate.value) {
      await xxxApi.create(form)
      ElMessage.success('创建成功')
    } else {
      await xxxApi.update(props.data!.id, { ...form, concurrencyStamp: props.data!.concurrencyStamp })
      ElMessage.success('更新成功')
    }
    emit('success')
  } finally {
    submitting.value = false
  }
}
</script>

<template>
  <el-dialog
    :model-value="visible"
    :title="isCreate ? '新增 XXX' : '编辑 XXX'"
    width="480px"
    :close-on-click-modal="false"
    @close="handleClose"
  >
    <el-form ref="formRef" :model="form" :rules="rules" label-position="top">
      <el-form-item label="名称" prop="name">
        <el-input v-model="form.name" placeholder="请输入名称" />
      </el-form-item>
    </el-form>

    <template #footer>
      <div class="dialog-footer">
        <button class="btn btn-ghost" @click="handleClose">取消</button>
        <button class="btn btn-primary" :disabled="submitting" @click="handleSubmit">
          {{ isCreate ? '创建' : '保存' }}
        </button>
      </div>
    </template>
  </el-dialog>
</template>
```

## API 模块模板

```typescript
import { http } from '../index'
import type { PagedResultDto } from '@/types/common'

export interface XxxDto {
  id: string
  name: string
  creationTime: string
  concurrencyStamp?: string
}

export interface CreateXxxDto { name: string }
export interface UpdateXxxDto extends CreateXxxDto { concurrencyStamp?: string }

const BASE_URL = '/xxx/xxx'

export const xxxApi = {
  getList: (params: { filter?: string; skipCount?: number; maxResultCount?: number; sorting?: string }) =>
    http.get<PagedResultDto<XxxDto>>(BASE_URL, { params }),
  create: (data: CreateXxxDto) => http.post<XxxDto>(BASE_URL, data),
  update: (id: string, data: UpdateXxxDto) => http.put<XxxDto>(`${BASE_URL}/${id}`, data),
  delete: (id: string) => http.delete(`${BASE_URL}/${id}`),
}
```
