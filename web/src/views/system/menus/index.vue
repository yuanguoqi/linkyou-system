<script setup lang="ts">
import { ref, reactive, onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { Plus, Search, Refresh, Edit, Delete } from '@element-plus/icons-vue'
import { menuApi } from '@/api/modules/menus'
import type { MenuDto, GetMenuListInput } from '@/api/modules/menus'
import { formatDateTime } from '@/utils/date'
import MenuDialog from './components/MenuDialog.vue'

// ── Extended type for tree ─────────────────────────────
interface MenuTreeNode extends MenuDto {
  children?: MenuTreeNode[]
}

// ── State ──────────────────────────────────────────────
const loading = ref(false)
const menuList = ref<MenuTreeNode[]>([])
const allMenus = ref<MenuDto[]>([])

// Search
const searchForm = reactive<GetMenuListInput>({
  filter: '',
})

// Dialog
const dialogVisible = ref(false)
const editData = ref<MenuDto | null>(null)

// ── Lifecycle ──────────────────────────────────────────
onMounted(() => {
  fetchMenuList()
})

// ── Methods ────────────────────────────────────────────
async function fetchMenuList() {
  loading.value = true
  try {
    const res = await menuApi.getList({
      filter: searchForm.filter || undefined,
      skipCount: 0,
      maxResultCount: 1000,
    })
    menuList.value = buildTree(res.data.items)
    allMenus.value = res.data.items
  } catch (err: unknown) {
    const axiosErr = err as { response?: { data?: { error?: { message?: string } } } }
    ElMessage.error(axiosErr?.response?.data?.error?.message || '获取菜单列表失败')
  } finally {
    loading.value = false
  }
}

function buildTree(items: MenuDto[]): MenuTreeNode[] {
  const map = new Map<string, MenuTreeNode>()
  const roots: MenuTreeNode[] = []

  items.forEach(item => {
    map.set(item.id, { ...item, children: [] })
  })

  items.forEach(item => {
    const node = map.get(item.id)!
    if (item.parentId && map.has(item.parentId)) {
      map.get(item.parentId)!.children!.push(node)
    } else {
      roots.push(node)
    }
  })

  return roots
}

function handleSearch() {
  fetchMenuList()
}

function handleReset() {
  searchForm.filter = ''
  fetchMenuList()
}

function handleCreate() {
  editData.value = null
  dialogVisible.value = true
}

function handleEdit(row: MenuDto) {
  editData.value = { ...row }
  dialogVisible.value = true
}

async function handleDelete(row: MenuDto) {
  try {
    await ElMessageBox.confirm(
      `确定要删除菜单「${row.name}」吗？此操作不可恢复。`,
      '删除确认',
      {
        confirmButtonText: '确定删除',
        cancelButtonText: '取消',
        type: 'warning',
      },
    )

    await menuApi.delete(row.id)
    ElMessage.success('菜单删除成功')
    fetchMenuList()
  } catch (err) {
    if (err !== 'cancel') {
      const axiosErr = err as { response?: { data?: { error?: { message?: string } } } }
      ElMessage.error(axiosErr?.response?.data?.error?.message || '删除失败')
    }
  }
}

function handleDialogSaved() {
  fetchMenuList()
}

</script>

<template>
  <div class="page-container">
    <!-- Search Panel -->
    <div class="panel search-panel">
      <div class="search-row">
        <el-input
          v-model="searchForm.filter"
          placeholder="搜索菜单名称..."
          :prefix-icon="Search"
          clearable
          class="search-input"
          @keyup.enter="handleSearch"
        />
        <div class="search-actions">
          <el-button class="btn-ghost" :icon="Refresh" @click="handleReset">重置</el-button>
          <el-button class="btn-primary" :icon="Search" @click="handleSearch">搜索</el-button>
        </div>
      </div>
    </div>

    <!-- Data Table Panel -->
    <div class="panel table-panel">
      <div class="panel-header">
        <span class="panel-title font-display">菜单列表</span>
        <el-button class="btn-primary btn-sm" :icon="Plus" @click="handleCreate">
          新增菜单
        </el-button>
      </div>

      <el-table
        :data="menuList"
        v-loading="loading"
        row-key="id"
        default-expand-all
        :tree-props="{ children: 'children' }"
        class="menu-table"
      >
        <el-table-column prop="name" label="菜单名称" min-width="160" show-overflow-tooltip>
          <template #default="{ row }">
            <span class="cell-name">{{ row.name }}</span>
          </template>
        </el-table-column>

        <el-table-column prop="path" label="路由路径" min-width="180" show-overflow-tooltip>
          <template #default="{ row }">
            <span class="cell-path">{{ row.path || '—' }}</span>
          </template>
        </el-table-column>

        <el-table-column prop="icon" label="图标" width="100" align="center">
          <template #default="{ row }">
            <span class="cell-icon">{{ row.icon || '—' }}</span>
          </template>
        </el-table-column>

        <el-table-column prop="sort" label="排序" width="80" align="center">
          <template #default="{ row }">
            <span class="cell-sort tabular">{{ row.sort }}</span>
          </template>
        </el-table-column>

        <el-table-column prop="isVisible" label="是否可见" width="100" align="center">
          <template #default="{ row }">
            <el-tag
              :type="row.isVisible ? 'success' : 'info'"
              size="small"
              effect="dark"
              class="visibility-tag"
            >
              {{ row.isVisible ? '可见' : '隐藏' }}
            </el-tag>
          </template>
        </el-table-column>

        <el-table-column prop="permission" label="权限标识" min-width="160" show-overflow-tooltip>
          <template #default="{ row }">
            <span class="cell-permission">{{ row.permission || '—' }}</span>
          </template>
        </el-table-column>

        <el-table-column prop="creationTime" label="创建时间" width="160" align="center">
          <template #default="{ row }">
            <span class="cell-time tabular">{{ formatDateTime(row.creationTime) }}</span>
          </template>
        </el-table-column>

        <el-table-column label="操作" width="140" align="center" fixed="right">
          <template #default="{ row }">
            <div class="action-buttons">
              <el-tooltip content="编辑" placement="top">
                <el-button
                  class="action-btn action-edit"
                  :icon="Edit"
                  circle
                  @click="handleEdit(row as MenuDto)"
                />
              </el-tooltip>
              <el-tooltip content="删除" placement="top">
                <el-button
                  class="action-btn action-delete"
                  :icon="Delete"
                  circle
                  @click="handleDelete(row as MenuDto)"
                />
              </el-tooltip>
            </div>
          </template>
        </el-table-column>
      </el-table>
    </div>

    <!-- Dialog -->
    <MenuDialog
      v-model:visible="dialogVisible"
      :edit-data="editData"
      :menu-tree="allMenus"
      @saved="handleDialogSaved"
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

// ─── Panel ─────────────────────────────────────────────
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

// ─── Search Panel ──────────────────────────────────────
.search-panel {
  padding: 14px 16px;
}

.search-row {
  display: flex;
  align-items: center;
  gap: 10px;
}

.search-input {
  max-width: 320px;

  :deep(.el-input__wrapper) {
    background: var(--input-bg) !important;
    border: 1px solid var(--input-border) !important;
    border-radius: 8px !important;
    box-shadow: none !important;
    transition: border-color 0.2s, box-shadow 0.2s;

    &:hover {
      border-color: rgba(99, 102, 241, 0.3) !important;
    }

    &.is-focus {
      border-color: rgba(99, 102, 241, 0.6) !important;
      box-shadow: 0 0 0 3px rgba(99, 102, 241, 0.1) !important;
    }

    .el-input__inner {
      color: var(--text-primary) !important;
      font-size: 13px;

      &::placeholder {
        color: var(--text-muted) !important;
      }
    }

    .el-input__prefix .el-icon {
      color: var(--text-muted) !important;
    }
  }
}

.search-actions {
  display: flex;
  gap: 8px;
  margin-left: auto;
}

// ─── Buttons ───────────────────────────────────────────
.btn-ghost {
  background: transparent !important;
  border: 1px solid var(--border-default) !important;
  color: var(--text-secondary) !important;
  border-radius: 8px !important;
  font-size: 13px;
  height: 36px;
  padding: 0 14px;
  transition: all 0.2s;

  &:hover {
    border-color: var(--border-medium) !important;
    color: var(--text-primary) !important;
    background: var(--bg-hover) !important;
  }
}

.btn-primary {
  background: linear-gradient(135deg, #4f46e5, #6366f1) !important;
  border: none !important;
  color: #fff !important;
  border-radius: 8px !important;
  font-size: 13px;
  font-weight: 500;
  height: 36px;
  padding: 0 14px;
  box-shadow: 0 4px 12px rgba(79, 70, 229, 0.3);
  transition: all 0.2s;

  &:hover {
    transform: translateY(-1px);
    box-shadow: 0 6px 16px rgba(79, 70, 229, 0.4);
  }

  &:active {
    transform: translateY(0);
  }
}

.btn-sm {
  height: 32px;
  padding: 0 12px;
  font-size: 12.5px;
}

// ─── Table ─────────────────────────────────────────────
.menu-table {
  --el-table-border-color: var(--border-default);
  --el-table-header-bg-color: transparent;
  --el-table-tr-bg-color: transparent;
  --el-table-expanded-cell-bg-color: transparent;

  :deep(.el-table__header-wrapper) {
    th.el-table__cell {
      background: var(--bg-elevated) !important;
      border-bottom: 1px solid var(--border-default) !important;
      color: var(--text-muted) !important;
      font-size: 12px;
      font-weight: 500;
      letter-spacing: 0.3px;
      text-transform: uppercase;
      padding: 10px 0;
    }
  }

  :deep(.el-table__body-wrapper) {
    .el-table__row {
      transition: background 0.15s;

      &:hover > td.el-table__cell {
        background: var(--bg-hover) !important;
      }

      td.el-table__cell {
        border-bottom: 1px solid var(--border-default) !important;
        padding: 10px 0;
        color: var(--text-primary);
        font-size: 13px;
      }
    }
  }

  // Expand icon
  :deep(.el-table__expand-icon) {
    color: var(--text-muted) !important;
    transition: color 0.2s;

    &:hover {
      color: var(--primary) !important;
    }
  }

  // Empty state
  :deep(.el-table__empty-block) {
    .el-table__empty-text {
      color: var(--text-muted);
      font-size: 13px;
    }
  }
}

.cell-name {
  font-weight: 500;
  color: var(--text-primary);
}

.cell-path {
  font-family: 'JetBrains Mono', 'Inter', monospace;
  font-size: 12px;
  color: var(--text-secondary);
}

.cell-icon {
  font-size: 12px;
  color: var(--text-muted);
}

.cell-sort {
  font-size: 12px;
  color: var(--text-secondary);
}

.cell-permission {
  font-family: 'JetBrains Mono', 'Inter', monospace;
  font-size: 11.5px;
  color: var(--primary-pale);
  background: var(--primary-dim);
  padding: 2px 8px;
  border-radius: 4px;
}

.cell-time {
  font-size: 12px;
  color: var(--text-muted);
}

.visibility-tag {
  border-radius: 6px;
  font-size: 11px;
  font-weight: 500;
  padding: 0 8px;
  height: 22px;
  line-height: 22px;

  &.el-tag--dark.el-tag--success {
    --el-tag-bg-color: rgba(52, 211, 153, 0.12);
    --el-tag-border-color: rgba(52, 211, 153, 0.2);
    --el-tag-text-color: #34d399;
  }

  &.el-tag--dark.el-tag--info {
    --el-tag-bg-color: rgba(100, 116, 139, 0.12);
    --el-tag-border-color: rgba(100, 116, 139, 0.2);
    --el-tag-text-color: #94a3b8;
  }
}

// ─── Action Buttons ────────────────────────────────────
.action-buttons {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 6px;
}

.action-btn {
  width: 30px !important;
  height: 30px !important;
  border: 1px solid var(--border-default) !important;
  background: transparent !important;
  transition: all 0.2s;

  :deep(.el-icon) {
    font-size: 14px;
  }
}

.action-edit {
  :deep(.el-icon) {
    color: var(--text-muted) !important;
  }

  &:hover {
    border-color: rgba(99, 102, 241, 0.4) !important;
    background: rgba(99, 102, 241, 0.08) !important;

    :deep(.el-icon) {
      color: #6366f1 !important;
    }
  }
}

.action-delete {
  :deep(.el-icon) {
    color: var(--text-muted) !important;
  }

  &:hover {
    border-color: rgba(248, 113, 113, 0.4) !important;
    background: rgba(248, 113, 113, 0.08) !important;

    :deep(.el-icon) {
      color: #f87171 !important;
    }
  }
}

// ─── Responsive ────────────────────────────────────────
@media (max-width: 768px) {
  .search-row {
    flex-direction: column;
    align-items: stretch;
  }

  .search-input {
    max-width: 100%;
  }

  .search-actions {
    margin-left: 0;
    justify-content: flex-end;
  }
}
</style>
