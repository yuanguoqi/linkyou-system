# 标准列表页模板

适用于 CRUD 管理页面，包含搜索栏、操作按钮、数据表格、分页。

## 完整模板

```vue
<script setup lang="ts">
import { ref, reactive, onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
// import { xxxApi } from '@/api/modules/xxx'
// import type { XxxDto } from '@/api/modules/xxx'
// import XxxDialog from './components/XxxDialog.vue'

// ---- 类型（替换为实际 DTO）----
interface ItemDto {
  id: string
  name: string
  creationTime: string
}

// ---- 查询参数 ----
const queryParams = reactive({
  filter: '',
  skipCount: 0,
  maxResultCount: 10,
  sorting: 'creationTime desc',
})

const currentPage = ref(1)
const pageSize = ref(10)

// ---- 数据 ----
const tableData = ref<ItemDto[]>([])
const total = ref(0)
const tableLoading = ref(false)

// ---- 弹窗控制 ----
const dialogVisible = ref(false)
const editId = ref<string | undefined>()

// ---- 方法 ----
async function fetchList() {
  tableLoading.value = true
  try {
    queryParams.skipCount = (currentPage.value - 1) * pageSize.value
    queryParams.maxResultCount = pageSize.value
    // const res = await xxxApi.getList(queryParams)
    // tableData.value = res.data.items
    // total.value = res.data.totalCount
  } finally {
    tableLoading.value = false
  }
}

function handleSearch() {
  currentPage.value = 1
  fetchList()
}

function handleReset() {
  queryParams.filter = ''
  currentPage.value = 1
  fetchList()
}

function handleCreate() {
  editId.value = undefined
  dialogVisible.value = true
}

function handleEdit(row: ItemDto) {
  editId.value = row.id
  dialogVisible.value = true
}

async function handleDelete(row: ItemDto) {
  await ElMessageBox.confirm(
    `确定要删除「${row.name}」吗？此操作不可撤销。`,
    '删除确认',
    { type: 'warning', confirmButtonText: '确认删除', cancelButtonText: '取消' }
  )
  // await xxxApi.delete(row.id)
  ElMessage.success('删除成功')
  fetchList()
}

function handleSaved() {
  dialogVisible.value = false
  fetchList()
}

onMounted(fetchList)
</script>

<template>
  <div class="page-container">
    <!-- 搜索区域 -->
    <div class="search-panel">
      <div class="search-fields">
        <el-input
          v-model="queryParams.filter"
          placeholder="输入关键词搜索..."
          clearable
          class="search-input"
          @keyup.enter="handleSearch"
        >
          <template #prefix>
            <el-icon><Search /></el-icon>
          </template>
        </el-input>
        <!-- 更多筛选项可在此扩展 -->
      </div>
      <div class="search-actions">
        <el-button class="btn-ghost" @click="handleReset">重置</el-button>
        <el-button type="primary" class="btn-primary" @click="handleSearch">
          <el-icon><Search /></el-icon>搜索
        </el-button>
      </div>
    </div>

    <!-- 列表面板 -->
    <div class="panel">
      <div class="panel-header">
        <span class="panel-title font-display">XXX 管理</span>
        <div class="panel-actions">
          <el-button type="primary" class="btn-primary" @click="handleCreate">
            <el-icon><Plus /></el-icon>新增
          </el-button>
        </div>
      </div>

      <div class="panel-body">
        <el-table
          v-loading="tableLoading"
          :data="tableData"
          class="data-table"
        >
          <el-table-column prop="name" label="名称" min-width="180" />
          <el-table-column
            prop="creationTime"
            label="创建时间"
            width="180"
            :formatter="(_, __, val) => val ? new Date(val).toLocaleString('zh-CN') : '—'"
          />
          <el-table-column label="操作" width="160" fixed="right">
            <template #default="{ row }">
              <div class="row-actions">
                <button class="action-btn" @click="handleEdit(row)">编辑</button>
                <button class="action-btn danger" @click="handleDelete(row)">删除</button>
              </div>
            </template>
          </el-table-column>
        </el-table>

        <el-pagination
          v-model:current-page="currentPage"
          v-model:page-size="pageSize"
          :total="total"
          :page-sizes="[10, 20, 50, 100]"
          layout="total, sizes, prev, pager, next, jumper"
          @current-change="fetchList"
          @size-change="() => { currentPage = 1; fetchList() }"
        />
      </div>
    </div>

    <!-- 弹窗 -->
    <!-- <XxxDialog v-model:visible="dialogVisible" :edit-id="editId" @saved="handleSaved" /> -->
  </div>
</template>

<style scoped lang="scss">
.page-container {
  padding: 16px;
  display: flex;
  flex-direction: column;
  gap: 12px;
  min-height: 100%;
}

// 搜索区
.search-panel {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
  padding: 14px 16px;
  background: var(--bg-card);
  border: 1px solid var(--border-default);
  border-radius: 12px;
  transition: background 0.3s, border-color 0.3s;

  .search-fields {
    display: flex;
    align-items: center;
    gap: 12px;
    flex: 1;
  }

  .search-input { width: 280px; }

  .search-actions {
    display: flex;
    gap: 8px;
    flex-shrink: 0;
  }
}

// 面板
.panel {
  background: var(--bg-card);
  border: 1px solid var(--border-default);
  border-radius: 12px;
  overflow: hidden;
  transition: background 0.3s, border-color 0.3s;

  .panel-header {
    display: flex;
    align-items: center;
    justify-content: space-between;
    padding: 14px 16px;
    border-bottom: 1px solid var(--border-default);

    .panel-title {
      font-family: 'Plus Jakarta Sans', 'Inter', sans-serif;
      font-size: 13.5px;
      font-weight: 600;
      color: var(--text-secondary);
      letter-spacing: -0.2px;
    }
  }

  .panel-body { padding: 16px; }
}

// 表格覆盖
.data-table {
  :deep(th.el-table__cell) {
    background: rgba(255, 255, 255, 0.02);
    color: var(--text-secondary);
    font-weight: 500;
    font-size: 12px;
    border-bottom: 1px solid var(--border-default);
  }

  :deep(td.el-table__cell) {
    color: var(--text-primary);
    border-bottom: 1px solid var(--border-default);
    font-size: 13px;
  }

  :deep(tr:hover > td) {
    background: var(--bg-hover) !important;
  }

  :deep(.el-table__body-wrapper) { background: transparent; }
  :deep(.el-table__empty-block) { background: transparent; color: var(--text-disabled); }
}

// 行操作按钮
.row-actions {
  display: flex;
  gap: 4px;
}

.action-btn {
  padding: 3px 10px;
  font-size: 12px;
  border-radius: 6px;
  border: 1px solid var(--border-primary);
  background: var(--primary-dim);
  color: var(--primary-pale);
  cursor: pointer;
  transition: all 0.15s;

  &:hover {
    background: rgba(99, 102, 241, 0.18);
    border-color: var(--border-active);
  }

  &.danger {
    border-color: rgba(248, 113, 113, 0.2);
    background: var(--danger-dim);
    color: #fca5a5;

    &:hover {
      background: rgba(248, 113, 113, 0.18);
      border-color: rgba(248, 113, 113, 0.4);
    }
  }
}

// 幽灵按钮
.btn-ghost {
  background: transparent !important;
  border-color: var(--border-subtle) !important;
  color: var(--text-secondary) !important;
  font-size: 13px;

  &:hover {
    background: var(--primary-dim) !important;
    border-color: var(--border-primary) !important;
    color: var(--primary-pale) !important;
  }
}

// 主要按钮
.btn-primary {
  background: linear-gradient(135deg, #4f46e5, #6366f1) !important;
  border: none !important;
  border-radius: 8px !important;
  box-shadow: 0 4px 12px rgba(79, 70, 229, 0.25) !important;
  font-weight: 500 !important;
  font-size: 13px;

  &:hover {
    transform: translateY(-1px);
    box-shadow: 0 6px 16px rgba(79, 70, 229, 0.35) !important;
  }
}

// 分页覆盖
:deep(.el-pagination) {
  justify-content: flex-end;
  margin-top: 12px;

  .el-pager li {
    background: var(--bg-input);
    border: 1px solid var(--border-default);
    color: var(--text-muted);
    border-radius: 6px;
    font-family: 'JetBrains Mono', monospace;
    font-size: 12px;

    &.is-active { background: var(--primary); border-color: var(--primary); color: #fff; }
    &:hover:not(.is-active) { background: var(--primary-dim); color: var(--primary-pale); }
  }

  button {
    background: var(--bg-input);
    border: 1px solid var(--border-default);
    color: var(--text-muted);
    border-radius: 6px;
    &:hover { color: var(--primary-pale); }
  }

  .el-pagination__total,
  .el-pagination__sizes,
  .el-pagination__jump { color: var(--text-muted); }
}

// 搜索输入框覆盖
:deep(.el-input) {
  .el-input__wrapper {
    background: var(--bg-input);
    box-shadow: 0 0 0 1px var(--border-subtle) inset;
    border-radius: 8px;

    &:hover { box-shadow: 0 0 0 1px rgba(99, 102, 241, 0.25) inset; }
    &.is-focus { box-shadow: 0 0 0 1px var(--primary) inset, 0 0 0 3px rgba(99, 102, 241, 0.08); }
  }

  .el-input__inner { color: var(--text-primary); }
  .el-input__inner::placeholder { color: var(--text-muted); }
  .el-icon { color: var(--text-muted); }
}
</style>
```
