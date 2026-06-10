<script setup lang="ts">
import { ref, reactive, onMounted } from 'vue'
import { useI18n } from 'vue-i18n'
import { ElMessage, ElMessageBox } from 'element-plus'
import { identityUserApi } from '@/api/modules/identity'
import type { IdentityUserDto } from '@/api/modules/identity'
import { formatDateTime } from '@/utils/date'
import UserDialog from './components/UserDialog.vue'

const { t } = useI18n()

// ── State ────────────────────────────────────────────
const loading = ref(false)
const tableData = ref<IdentityUserDto[]>([])
const totalCount = ref(0)
const userDialogRef = ref<InstanceType<typeof UserDialog>>()

const searchParams = reactive({
  keyword: '',
})

const pagination = reactive({
  currentPage: 1,
  pageSize: 10,
})

// ── Methods ──────────────────────────────────────────
function getSkipCount(): number {
  return (pagination.currentPage - 1) * pagination.pageSize
}

async function fetchUsers() {
  loading.value = true
  try {
    const res = await identityUserApi.getList({
      filter: searchParams.keyword || undefined,
      skipCount: getSkipCount(),
      maxResultCount: pagination.pageSize,
    })
    tableData.value = res.data.items
    totalCount.value = res.data.totalCount
  } catch {
    ElMessage.error(t('user.fetchFailed'))
  } finally {
    loading.value = false
  }
}

function handleSearch() {
  pagination.currentPage = 1
  fetchUsers()
}

function handleReset() {
  searchParams.keyword = ''
  pagination.currentPage = 1
  fetchUsers()
}

function handlePageChange(page: number) {
  pagination.currentPage = page
  fetchUsers()
}

function handleSizeChange(size: number) {
  pagination.pageSize = size
  pagination.currentPage = 1
  fetchUsers()
}

function handleCreate() {
  userDialogRef.value?.open()
}

function handleEdit(row: IdentityUserDto) {
  userDialogRef.value?.open(row)
}

function handleDelete(row: IdentityUserDto) {
  ElMessageBox.confirm(
    t('user.deleteConfirmMessage', { userName: row.userName }),
    t('user.deleteConfirmTitle'),
    {
      confirmButtonText: t('common.confirmDelete'),
      cancelButtonText: t('common.cancel'),
      type: 'warning',
    },
  ).then(async () => {
    try {
      await identityUserApi.delete(row.id)
      ElMessage.success(t('user.deleteSuccess'))
      fetchUsers()
    } catch {
      ElMessage.error(t('user.deleteFailed'))
    }
  }).catch(() => {
    // user cancelled
  })
}

function handleDialogSuccess() {
  fetchUsers()
}

function getDisplayName(row: IdentityUserDto): string {
  return row.name || row.userName || '-'
}

// ── Lifecycle ────────────────────────────────────────
onMounted(() => {
  fetchUsers()
})
</script>

<template>
  <div class="page-container">
    <!-- Search Panel -->
    <div class="panel">
      <div class="panel-header">
        <span class="panel-title font-display">{{ t('user.title') }}</span>
        <button class="btn-primary" @click="handleCreate">
          <el-icon :size="14"><Plus /></el-icon>
          {{ t('user.createUser') }}
        </button>
      </div>
      <div class="panel-body">
        <div class="search-bar">
          <el-input
            v-model="searchParams.keyword"
            :placeholder="t('user.searchPlaceholder')"
            clearable
            class="search-input"
            @keyup.enter="handleSearch"
          />
          <button class="btn-primary btn-sm" @click="handleSearch">
            <el-icon :size="13"><Search /></el-icon>
            {{ t('common.search') }}
          </button>
          <button class="btn-ghost btn-sm" @click="handleReset">
            <el-icon :size="13"><RefreshRight /></el-icon>
            {{ t('common.reset') }}
          </button>
        </div>
      </div>
    </div>

    <!-- Table Panel -->
    <div class="panel">
      <div class="table-wrap">
        <el-table
          :data="tableData"
          v-loading="loading"
          stripe
          style="width: 100%"
        >
          <el-table-column
            prop="userName"
            :label="t('user.username')"
            min-width="140"
            show-overflow-tooltip
          />
          <el-table-column
            :label="t('user.nickname')"
            min-width="140"
            show-overflow-tooltip
          >
            <template #default="{ row }">
              {{ getDisplayName(row as IdentityUserDto) }}
            </template>
          </el-table-column>
          <el-table-column
            prop="email"
            :label="t('user.email')"
            min-width="200"
            show-overflow-tooltip
          />
          <el-table-column
            :label="t('user.role')"
            min-width="160"
          >
            <template #default="{ row }">
              <div class="role-tags">
                <el-tag
                  v-for="role in (row as IdentityUserDto).roles"
                  :key="role.roleId"
                  size="small"
                  effect="plain"
                  round
                  class="role-tag"
                >
                  {{ role.roleName }}
                </el-tag>
                <span v-if="!(row as IdentityUserDto).roles?.length" class="no-role">-</span>
              </div>
            </template>
          </el-table-column>
          <el-table-column
            prop="isActive"
            :label="t('common.status')"
            width="100"
            align="center"
          >
            <template #default="{ row }">
              <el-tag
                :type="row.isActive ? 'success' : 'danger'"
                size="small"
                effect="dark"
                round
              >
                {{ row.isActive ? t('common.enabled') : t('common.disabled') }}
              </el-tag>
            </template>
          </el-table-column>
          <el-table-column
            prop="creationTime"
            :label="t('common.createTime')"
            width="170"
            align="center"
          >
            <template #default="{ row }">
              {{ formatDateTime(row.creationTime) }}
            </template>
          </el-table-column>
          <el-table-column
            :label="t('common.operation')"
            width="140"
            align="center"
            fixed="right"
          >
            <template #default="{ row }">
              <div class="action-links">
                <button class="action-link" @click="handleEdit(row as IdentityUserDto)">
                  <el-icon :size="13"><Edit /></el-icon>
                  {{ t('common.edit') }}
                </button>
                <span class="action-divider" />
                <button class="action-link action-link--danger" @click="handleDelete(row as IdentityUserDto)">
                  <el-icon :size="13"><Delete /></el-icon>
                  {{ t('common.delete') }}
                </button>
              </div>
            </template>
          </el-table-column>
        </el-table>
      </div>

      <!-- Pagination -->
      <div class="pagination-wrap">
        <el-pagination
          v-model:current-page="pagination.currentPage"
          v-model:page-size="pagination.pageSize"
          :total="totalCount"
          :page-sizes="[10, 20, 50, 100]"
          layout="total, sizes, prev, pager, next, jumper"
          background
          @current-change="handlePageChange"
          @size-change="handleSizeChange"
        />
      </div>
    </div>

    <!-- User Dialog -->
    <UserDialog ref="userDialogRef" @success="handleDialogSuccess" />
  </div>
</template>

<style scoped lang="scss">
// ── Page Container ─────────────────────────────────────
.page-container {
  padding: 20px;
  display: flex;
  flex-direction: column;
  gap: 16px;
  min-height: 100%;
}

// ── Panel ──────────────────────────────────────────────
.panel {
  background: var(--bg-card);
  border: 1px solid var(--border-default);
  border-radius: 14px;
  overflow: hidden;
  transition: background 0.3s, border-color 0.3s;

  .panel-header {
    display: flex;
    align-items: center;
    justify-content: space-between;
    padding: 16px 20px;
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

  .panel-body {
    padding: 14px 16px;
  }
}

// ── Search Bar ─────────────────────────────────────────
.search-bar {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 16px 20px;

  .search-input {
    width: 320px;
    max-width: 100%;
  }
}

// ── Buttons ────────────────────────────────────────────
.btn-primary {
  display: inline-flex;
  align-items: center;
  gap: 5px;
  padding: 7px 16px;
  font-size: 12.5px;
  font-weight: 600;
  color: #fff;
  background: linear-gradient(135deg, #4f46e5, #6366f1);
  border: none;
  border-radius: 8px;
  cursor: pointer;
  transition: transform 0.2s, box-shadow 0.2s;
  box-shadow: 0 4px 14px rgba(79, 70, 229, 0.3);
  letter-spacing: 0.3px;

  &:hover {
    transform: translateY(-1px);
    box-shadow: 0 6px 20px rgba(79, 70, 229, 0.4);
  }

  &:active {
    transform: translateY(0);
  }
}

.btn-ghost {
  display: inline-flex;
  align-items: center;
  gap: 5px;
  padding: 7px 16px;
  font-size: 12.5px;
  font-weight: 500;
  color: var(--text-secondary);
  background: transparent;
  border: 1px solid var(--border-subtle);
  border-radius: 8px;
  cursor: pointer;
  transition: all 0.2s cubic-bezier(0.4, 0, 0.2, 1);

  &:hover {
    color: var(--text-primary);
    border-color: var(--border-medium);
    background: var(--bg-hover);
  }
}

.btn-sm {
  padding: 6px 14px;
  font-size: 12px;
}

// ── Table Override ─────────────────────────────────────
.table-wrap {
  :deep(.el-table) {
    --el-table-bg-color: transparent;
    --el-table-tr-bg-color: transparent;
    --el-table-header-bg-color: rgba(99, 102, 241, 0.04);
    --el-table-row-hover-bg-color: var(--bg-hover);
    --el-table-border-color: var(--border-default);
    --el-table-text-color: var(--text-primary);
    --el-table-header-text-color: var(--text-secondary);
    --el-table-current-row-bg-color: var(--bg-active);

    font-size: 12.5px;
    background: transparent;

    .el-table__header th {
      font-family: 'Plus Jakarta Sans', 'Inter', sans-serif;
      font-size: 11.5px;
      font-weight: 600;
      text-transform: uppercase;
      letter-spacing: 0.5px;
      color: var(--text-secondary);
      background: rgba(99, 102, 241, 0.04);
      border-bottom: 1px solid var(--border-default);
    }

    .el-table__row td {
      border-bottom: 1px solid var(--border-default);
      transition: background 0.15s;
    }

    // Stripe rows
    .el-table__row--striped td {
      background: rgba(255, 255, 255, 0.01);
    }

    // Remove default borders
    &::before,
    &::after {
      display: none;
    }

    .el-table__inner-wrapper::before {
      display: none;
    }

    // Empty state
    .el-table__empty-block {
      background: transparent;
    }
    .el-table__empty-text {
      color: var(--text-muted);
      font-size: 13px;
    }
  }
}

// ── Pagination Override ────────────────────────────────
.pagination-wrap {
  display: flex;
  justify-content: flex-end;
  padding: 16px 20px;
  padding: 12px 16px;
  border-top: 1px solid var(--border-default);

  :deep(.el-pagination) {
    --el-pagination-bg-color: transparent;
    --el-pagination-text-color: var(--text-secondary);
    --el-pagination-button-bg-color: var(--bg-input);
    --el-pagination-hover-color: var(--primary);
    --el-pagination-button-disabled-bg-color: transparent;

    font-size: 12.5px;

    .el-pagination__total,
    .el-pagination__jump {
      color: var(--text-secondary);
      font-size: 12px;
    }

    .el-pager li {
      background: var(--bg-input);
      color: var(--text-secondary);
      border: 1px solid var(--border-default);
      border-radius: 6px;
      min-width: 30px;
      height: 28px;
      line-height: 28px;
      font-size: 12px;
      transition: all 0.2s;
      margin: 0 2px;

      &:hover {
        color: var(--primary);
        border-color: var(--border-primary);
      }

      &.is-active {
        background: linear-gradient(135deg, #4f46e5, #6366f1);
        color: #fff;
        border-color: transparent;
        box-shadow: 0 2px 8px rgba(99, 102, 241, 0.3);
      }
    }

    .btn-prev,
    .btn-next {
      background: var(--bg-input) !important;
      border: 1px solid var(--border-default);
      border-radius: 6px;
      color: var(--text-secondary);
      min-width: 28px;
      height: 28px;

      &:hover:not(:disabled) {
        color: var(--primary);
        border-color: var(--border-primary);
      }
    }

    .el-pagination__sizes {
      .el-select {
        .el-input__wrapper {
          background: var(--bg-input);
          border: 1px solid var(--border-default);
          border-radius: 6px;
          box-shadow: none !important;
          height: 28px;

          &:hover {
            border-color: var(--border-primary);
          }
        }
        .el-input__inner {
          color: var(--text-secondary);
          font-size: 12px;
        }
      }
    }

    .el-pagination__jump {
      .el-input__wrapper {
        background: var(--bg-input);
        border: 1px solid var(--border-default);
        border-radius: 6px;
        box-shadow: none !important;
        height: 28px;

        &:hover {
          border-color: var(--border-primary);
        }
      }
      .el-input__inner {
        color: var(--text-secondary);
        font-size: 12px;
      }
    }
  }
}

// ── Input Override ─────────────────────────────────────
:deep(.el-input) {
  .el-input__wrapper {
    background: var(--bg-input) !important;
    border: 1px solid var(--border-default) !important;
    border-radius: 8px !important;
    box-shadow: none !important;
    transition: border-color 0.25s cubic-bezier(0.4, 0, 0.2, 1),
                box-shadow 0.25s cubic-bezier(0.4, 0, 0.2, 1);

    &:hover {
      border-color: rgba(99, 102, 241, 0.3) !important;
    }

    &.is-focus {
      border-color: rgba(99, 102, 241, 0.6) !important;
      box-shadow: 0 0 0 3px rgba(99, 102, 241, 0.1),
                  0 0 16px rgba(99, 102, 241, 0.06) !important;
    }

    .el-input__inner {
      color: var(--text-primary);
      font-size: 13px;

      &::placeholder {
        color: var(--text-muted);
      }
    }

    .el-input__prefix .el-icon,
    .el-input__suffix .el-icon {
      color: var(--text-muted);
    }
  }
}

// ── Action Links ───────────────────────────────────────
.action-links {
  display: inline-flex;
  align-items: center;
  gap: 6px;
}

.action-link {
  display: inline-flex;
  align-items: center;
  gap: 3px;
  padding: 3px 6px;
  font-size: 12px;
  font-weight: 500;
  color: var(--primary-light);
  background: transparent;
  border: none;
  border-radius: 4px;
  cursor: pointer;
  transition: all 0.15s;

  &:hover {
    background: var(--bg-hover);
    color: var(--primary);
  }

  &--danger {
    color: var(--danger);

    &:hover {
      background: var(--danger-dim);
      color: #fca5a5;
    }
  }
}

.action-divider {
  width: 1px;
  height: 12px;
  background: var(--border-default);
}

// ── Role Tags ──────────────────────────────────────────
.role-tags {
  display: flex;
  flex-wrap: wrap;
  gap: 4px;
}

.role-tag {
  font-size: 11px;
  font-weight: 500;
  background: var(--primary-dim) !important;
  color: var(--primary-pale) !important;
  border: 1px solid rgba(99, 102, 241, 0.15) !important;
  border-radius: 12px !important;
  padding: 0 8px !important;
  height: 22px !important;
  line-height: 22px !important;
}

.no-role {
  color: var(--text-muted);
  font-size: 12px;
}

// ── Tag Override ───────────────────────────────────────
:deep(.el-tag) {
  &.el-tag--success {
    --el-tag-bg-color: var(--success-dim);
    --el-tag-text-color: var(--success);
    --el-tag-border-color: transparent;
  }

  &.el-tag--danger {
    --el-tag-bg-color: var(--danger-dim);
    --el-tag-text-color: var(--danger);
    --el-tag-border-color: transparent;
  }
}
</style>
