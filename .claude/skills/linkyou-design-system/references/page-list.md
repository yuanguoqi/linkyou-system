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
        <el-button type="primary" @click="handleSearch">
          <el-icon><Search /></el-icon>搜索
        </el-button>
      </div>
    </div>

    <!-- 列表面板 -->
    <div class="panel">
      <div class="panel-header">
        <span class="panel-title">XXX 管理</span>
        <div class="panel-actions">
          <el-button type="primary" @click="handleCreate">
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
  padding: 20px;
  display: flex;
  flex-direction: column;
  gap: 16px;
  min-height: 100%;
}

// 搜索区
.search-panel {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
  padding: 16px 20px;
  background: #13151f;
  border: 1px solid rgba(255, 255, 255, 0.06);
  border-radius: 12px;

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
  background: #13151f;
  border: 1px solid rgba(255, 255, 255, 0.06);
  border-radius: 14px;
  overflow: hidden;

  .panel-header {
    display: flex;
    align-items: center;
    justify-content: space-between;
    padding: 16px 20px;
    border-bottom: 1px solid rgba(255, 255, 255, 0.05);

    .panel-title {
      font-size: 14px;
      font-weight: 600;
      color: #cbd5e1;
    }
  }

  .panel-body { padding: 20px; }
}

// 表格覆盖
.data-table {
  :deep(th.el-table__cell) {
    background: rgba(255, 255, 255, 0.03);
    color: #94a3b8;
    font-weight: 500;
    border-bottom: 1px solid rgba(255, 255, 255, 0.06);
  }

  :deep(td.el-table__cell) {
    color: #cbd5e1;
    border-bottom: 1px solid rgba(255, 255, 255, 0.04);
  }

  :deep(tr:hover > td) {
    background: rgba(99, 102, 241, 0.05) !important;
  }

  :deep(.el-table__body-wrapper) { background: transparent; }
  :deep(.el-table__empty-block) { background: transparent; color: #475569; }
}

// 行操作按钮
.row-actions {
  display: flex;
  gap: 4px;
}

.action-btn {
  padding: 3px 10px;
  font-size: 12px;
  border-radius: 5px;
  border: 1px solid rgba(99, 102, 241, 0.25);
  background: rgba(99, 102, 241, 0.08);
  color: #a5b4fc;
  cursor: pointer;
  transition: all 0.15s;

  &:hover {
    background: rgba(99, 102, 241, 0.18);
    border-color: rgba(99, 102, 241, 0.5);
  }

  &.danger {
    border-color: rgba(248, 113, 113, 0.25);
    background: rgba(248, 113, 113, 0.08);
    color: #fca5a5;

    &:hover {
      background: rgba(248, 113, 113, 0.18);
      border-color: rgba(248, 113, 113, 0.5);
    }
  }
}

// 幽灵按钮
.btn-ghost {
  background: rgba(255, 255, 255, 0.04) !important;
  border-color: rgba(255, 255, 255, 0.08) !important;
  color: #94a3b8 !important;

  &:hover {
    background: rgba(99, 102, 241, 0.08) !important;
    border-color: rgba(99, 102, 241, 0.3) !important;
    color: #a5b4fc !important;
  }
}

// 分页覆盖
:deep(.el-pagination) {
  justify-content: flex-end;
  margin-top: 16px;

  .el-pager li {
    background: rgba(255, 255, 255, 0.04);
    border: 1px solid rgba(255, 255, 255, 0.06);
    color: #64748b;
    border-radius: 6px;

    &.is-active { background: #6366f1; border-color: #6366f1; color: #fff; }
    &:hover:not(.is-active) { background: rgba(99, 102, 241, 0.1); color: #a5b4fc; }
  }

  button {
    background: rgba(255, 255, 255, 0.04);
    border: 1px solid rgba(255, 255, 255, 0.06);
    color: #64748b;
    border-radius: 6px;
    &:hover { color: #a5b4fc; }
  }

  .el-pagination__total,
  .el-pagination__sizes,
  .el-pagination__jump { color: #64748b; }
}

// 搜索输入框覆盖
:deep(.el-input) {
  .el-input__wrapper {
    background: rgba(255, 255, 255, 0.04);
    box-shadow: 0 0 0 1px rgba(255, 255, 255, 0.08) inset;
    border-radius: 8px;

    &:hover { box-shadow: 0 0 0 1px rgba(99, 102, 241, 0.3) inset; }
    &.is-focus { box-shadow: 0 0 0 1px #6366f1 inset, 0 0 0 3px rgba(99, 102, 241, 0.12); }
  }

  .el-input__inner { color: #e2e8f0; }
  .el-input__inner::placeholder { color: #475569; }
  .el-icon { color: #64748b; }
}
</style>
```
