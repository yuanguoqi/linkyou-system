<script setup lang="ts">
import { ref, onMounted, watch } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { tenantApi } from '@/api/modules/tenants'
import type { TenantDto } from '@/api/modules/tenants'
import { usePagination } from '@/composables/usePagination'
import { formatDateTime } from '@/utils/date'
import TenantDialog from './components/TenantDialog.vue'

// ── State ──────────────────────────────────────────────
const loading = ref(false)
const tableData = ref<TenantDto[]>([])
const totalCount = ref(0)
const searchFilter = ref('')

const dialogVisible = ref(false)
const dialogMode = ref<'create' | 'edit'>('create')
const editingRow = ref<TenantDto | null>(null)

const {
  pagination,
  currentPage,
  pageSize,
  handlePageChange,
  handleSizeChange,
  resetPage,
  getSkipCount,
} = usePagination()

// ── Data Fetching ──────────────────────────────────────
async function fetchData() {
  loading.value = true
  try {
    const { data } = await tenantApi.getList({
      filter: searchFilter.value || undefined,
      skipCount: getSkipCount(),
      maxResultCount: pageSize.value,
    })
    tableData.value = data.items
    totalCount.value = data.totalCount
  } catch {
    // interceptor handles error toast
  } finally {
    loading.value = false
  }
}

// ── Search ─────────────────────────────────────────────
function handleSearch() {
  resetPage()
}

function handleReset() {
  searchFilter.value = ''
  resetPage()
}

// ── CRUD Actions ───────────────────────────────────────
function handleCreate() {
  dialogMode.value = 'create'
  editingRow.value = null
  dialogVisible.value = true
}

function handleEdit(row: TenantDto) {
  dialogMode.value = 'edit'
  editingRow.value = { ...row }
  dialogVisible.value = true
}

async function handleDelete(row: TenantDto) {
  try {
    await ElMessageBox.confirm(
      `确定要删除租户「${row.name}」吗？此操作不可恢复。`,
      '确认删除',
      { type: 'warning', confirmButtonText: '删除', cancelButtonText: '取消' },
    )
    await tenantApi.delete(row.id)
    ElMessage.success('删除成功')
    fetchData()
  } catch {
    // user cancelled or error
  }
}

function handleDialogSuccess() {
  dialogVisible.value = false
  fetchData()
}

// ── Lifecycle ──────────────────────────────────────────
watch([currentPage, pageSize], fetchData)
onMounted(fetchData)
</script>

<template>
  <div class="page-container">
    <!-- Search Panel -->
    <div class="panel">
      <div class="panel-header">
        <span class="panel-title">租户管理</span>
      </div>
      <div class="search-bar">
        <el-input
          v-model="searchFilter"
          placeholder="搜索租户名称..."
          clearable
          class="search-input"
          @keyup.enter="handleSearch"
          @clear="handleSearch"
        >
          <template #prefix>
            <el-icon><Search /></el-icon>
          </template>
        </el-input>
        <div class="search-actions">
          <button class="btn btn-primary" @click="handleSearch">
            <el-icon :size="14"><Search /></el-icon>
            搜索
          </button>
          <button class="btn btn-ghost" @click="handleReset">
            <el-icon :size="14"><RefreshRight /></el-icon>
            重置
          </button>
        </div>
      </div>
    </div>

    <!-- Data Panel -->
    <div class="panel">
      <div class="panel-header">
        <span class="panel-title">租户列表</span>
        <button class="btn btn-primary" @click="handleCreate">
          <el-icon :size="14"><Plus /></el-icon>
          新建租户
        </button>
      </div>

      <el-table
        :data="tableData"
        v-loading="loading"
        class="dark-table"
        row-key="id"
        stripe
      >
        <el-table-column
          prop="name"
          label="租户名称"
          min-width="200"
          show-overflow-tooltip
        />
        <el-table-column
          prop="creationTime"
          label="创建时间"
          width="200"
        >
          <template #default="{ row }">
            {{ formatDateTime(row.creationTime) }}
          </template>
        </el-table-column>
        <el-table-column
          label="操作"
          width="160"
          fixed="right"
        >
          <template #default="{ row }">
            <div class="action-buttons">
              <button class="btn-action btn-action-edit" @click="handleEdit(row as TenantDto)">
                <el-icon :size="13"><Edit /></el-icon>
                编辑
              </button>
              <button class="btn-action btn-action-delete" @click="handleDelete(row as TenantDto)">
                <el-icon :size="13"><Delete /></el-icon>
                删除
              </button>
            </div>
          </template>
        </el-table-column>
      </el-table>

      <div class="pagination-wrap">
        <el-pagination
          v-bind="pagination"
          :total="totalCount"
          @current-change="handlePageChange"
          @size-change="handleSizeChange"
        />
      </div>
    </div>

    <!-- Dialog -->
    <TenantDialog
      v-model:visible="dialogVisible"
      :mode="dialogMode"
      :data="editingRow"
      @success="handleDialogSuccess"
    />
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

// ── Panel ──────────────────────────────────────────────
.panel {
  background: var(--bg-card);
  border: 1px solid var(--border-default);
  border-radius: 12px;
  overflow: hidden;
  transition: background 0.3s, border-color 0.3s;
}

.panel-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 14px 16px;
  border-bottom: 1px solid var(--border-default);
  transition: border-color 0.3s;
}

.panel-title {
  font-family: 'Plus Jakarta Sans', 'Inter', sans-serif;
  font-size: 13.5px;
  font-weight: 600;
  color: var(--text-secondary);
  letter-spacing: -0.2px;
  transition: color 0.3s;
}

// ── Search Bar ─────────────────────────────────────────
.search-bar {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 14px 16px;
  flex-wrap: wrap;
}

.search-input {
  max-width: 280px;
}

.search-actions {
  display: flex;
  gap: 8px;
}

// ── Buttons ────────────────────────────────────────────
.btn {
  display: inline-flex;
  align-items: center;
  gap: 5px;
  padding: 7px 14px;
  border-radius: 8px;
  font-size: 12.5px;
  font-weight: 500;
  cursor: pointer;
  border: none;
  transition: all 0.2s cubic-bezier(0.4, 0, 0.2, 1);
  white-space: nowrap;
}

.btn-primary {
  background: linear-gradient(135deg, #6366f1, #818cf8);
  color: #fff;
  box-shadow: 0 2px 8px rgba(99, 102, 241, 0.3);

  &:hover {
    box-shadow: 0 4px 16px rgba(99, 102, 241, 0.45);
    transform: translateY(-1px);
  }
}

.btn-ghost {
  background: transparent;
  color: var(--text-secondary);
  border: 1px solid var(--border-subtle);

  &:hover {
    background: var(--bg-hover);
    color: var(--text-primary);
    border-color: var(--border-primary);
  }
}

// ── Action Buttons ─────────────────────────────────────
.action-buttons {
  display: flex;
  gap: 6px;
}

.btn-action {
  display: inline-flex;
  align-items: center;
  gap: 3px;
  padding: 4px 10px;
  border-radius: 6px;
  font-size: 12px;
  font-weight: 500;
  cursor: pointer;
  border: none;
  transition: all 0.2s cubic-bezier(0.4, 0, 0.2, 1);
}

.btn-action-edit {
  background: rgba(99, 102, 241, 0.08);
  color: var(--primary-light);

  &:hover {
    background: rgba(99, 102, 241, 0.15);
  }
}

.btn-action-delete {
  background: rgba(248, 113, 113, 0.08);
  color: var(--danger);

  &:hover {
    background: rgba(248, 113, 113, 0.15);
  }
}

// ── Pagination ─────────────────────────────────────────
.pagination-wrap {
  display: flex;
  justify-content: flex-end;
  padding: 14px 16px;
  border-top: 1px solid var(--border-default);
}

// ── Deep overrides: Table ──────────────────────────────
:deep(.dark-table) {
  --el-table-bg-color: transparent;
  --el-table-tr-bg-color: transparent;
  --el-table-header-bg-color: var(--bg-elevated);
  --el-table-header-text-color: var(--text-secondary);
  --el-table-text-color: var(--text-primary);
  --el-table-border-color: var(--border-default);
  --el-table-row-hover-bg-color: var(--bg-hover);
  --el-table-current-row-bg-color: var(--bg-active);
  --el-table-expanded-cell-bg-color: transparent;

  font-size: 13px;

  .el-table__header th {
    font-weight: 600;
    font-size: 12px;
    text-transform: uppercase;
    letter-spacing: 0.3px;
  }
}

// ── Deep overrides: Pagination ─────────────────────────
:deep(.el-pagination) {
  --el-pagination-bg-color: transparent;
  --el-pagination-text-color: var(--text-secondary);
  --el-pagination-button-bg-color: var(--bg-input);
  --el-pagination-button-color: var(--text-secondary);
  --el-pagination-hover-color: var(--primary);

  .el-pagination__total {
    color: var(--text-muted);
  }

  .el-pager li {
    background: var(--bg-input);
    color: var(--text-secondary);
    border-radius: 6px;
    border: 1px solid transparent;
    transition: all 0.2s;

    &:hover {
      color: var(--primary-light);
      border-color: var(--border-primary);
    }

    &.is-active {
      background: linear-gradient(135deg, #6366f1, #818cf8);
      color: #fff;
    }
  }

  .btn-prev,
  .btn-next {
    background: var(--bg-input);
    color: var(--text-secondary);
    border-radius: 6px;

    &:hover {
      color: var(--primary-light);
    }
  }
}

// ── Deep overrides: Input ──────────────────────────────
:deep(.el-input__wrapper) {
  background: var(--input-bg);
  border: 1px solid var(--input-border);
  border-radius: 8px;
  box-shadow: none;
  transition: border-color 0.2s;

  &:hover {
    border-color: var(--border-medium);
  }

  &.is-focus {
    border-color: var(--primary);
    box-shadow: 0 0 0 2px var(--primary-dim);
  }

  .el-input__inner {
    color: var(--input-text);
    font-size: 13px;

    &::placeholder {
      color: var(--input-placeholder);
    }
  }
}
</style>
