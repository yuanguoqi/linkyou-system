<script setup lang="ts">
import { ref, reactive, onMounted, watch } from 'vue'
import { auditLogApi } from '@/api/modules/audit-logs'
import type { AuditLogDto, GetAuditLogListInput } from '@/api/modules/audit-logs'
import { usePagination } from '@/composables/usePagination'
import { formatDateTime } from '@/utils/date'


// ── State ──────────────────────────────────────────────
const loading = ref(false)
const tableData = ref<AuditLogDto[]>([])
const totalCount = ref(0)

const searchForm = reactive({
  keyword: '',
  httpMethod: '',
  dateRange: [] as string[],
})

const httpMethodOptions = [
  { label: 'GET', value: 'GET' },
  { label: 'POST', value: 'POST' },
  { label: 'PUT', value: 'PUT' },
  { label: 'DELETE', value: 'DELETE' },
  { label: 'PATCH', value: 'PATCH' },
]

const {
  pagination,
  currentPage,
  pageSize,
  handlePageChange,
  handleSizeChange,
  resetPage,
  getSkipCount,
} = usePagination()

// ── Helpers ────────────────────────────────────────────
function getMethodColor(method: string | null): string {
  switch (method?.toUpperCase()) {
    case 'GET':    return '#34d399'
    case 'POST':   return '#6366f1'
    case 'PUT':    return '#fbbf24'
    case 'DELETE': return '#f87171'
    case 'PATCH':  return '#f97316'
    default:       return '#64748b'
  }
}

function getStatusColor(code: number | null): string {
  if (!code) return '#64748b'
  if (code >= 200 && code < 300) return '#34d399'
  if (code >= 300 && code < 400) return '#6366f1'
  if (code >= 400 && code < 500) return '#fbbf24'
  if (code >= 500) return '#f87171'
  return '#64748b'
}

function truncateUrl(url: string | null, max = 60): string {
  if (!url) return '-'
  return url.length > max ? url.slice(0, max) + '...' : url
}

// ── Data Fetching ──────────────────────────────────────
async function fetchData() {
  loading.value = true
  try {
    const params: GetAuditLogListInput = {
      filter: searchForm.keyword || undefined,
      httpMethod: searchForm.httpMethod || undefined,
      skipCount: getSkipCount(),
      maxResultCount: pageSize.value,
    }

    if (searchForm.dateRange.length === 2) {
      params.startTime = searchForm.dateRange[0]
      params.endTime = searchForm.dateRange[1]
    }

    const { data } = await auditLogApi.getList(params)
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
  searchForm.keyword = ''
  searchForm.httpMethod = ''
  searchForm.dateRange = []
  resetPage()
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
        <span class="panel-title">审计日志</span>
      </div>
      <div class="search-bar">
        <el-input
          v-model="searchForm.keyword"
          placeholder="关键词搜索..."
          clearable
          class="search-input"
          @keyup.enter="handleSearch"
          @clear="handleSearch"
        >
          <template #prefix>
            <el-icon><Search /></el-icon>
          </template>
        </el-input>

        <el-select
          v-model="searchForm.httpMethod"
          placeholder="HTTP 方法"
          clearable
          class="search-select"
        >
          <el-option
            v-for="opt in httpMethodOptions"
            :key="opt.value"
            :label="opt.label"
            :value="opt.value"
          />
        </el-select>

        <el-date-picker
          v-model="searchForm.dateRange"
          type="daterange"
          range-separator="至"
          start-placeholder="开始时间"
          end-placeholder="结束时间"
          class="search-datepicker"
          value-format="YYYY-MM-DDTHH:mm:ss"
        />

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
        <span class="panel-title">日志列表</span>
      </div>

      <el-table
        :data="tableData"
        v-loading="loading"
        class="dark-table"
        row-key="id"
        stripe
      >
        <el-table-column
          prop="httpMethod"
          label="方法"
          width="100"
        >
          <template #default="{ row }">
            <span
              class="method-tag"
              :style="{ color: getMethodColor(row.httpMethod), background: getMethodColor(row.httpMethod) + '14', borderColor: getMethodColor(row.httpMethod) + '30' }"
            >
              {{ row.httpMethod || '-' }}
            </span>
          </template>
        </el-table-column>

        <el-table-column
          prop="url"
          label="请求地址"
          min-width="280"
          show-overflow-tooltip
        >
          <template #default="{ row }">
            <span class="url-text">{{ truncateUrl(row.url) }}</span>
          </template>
        </el-table-column>

        <el-table-column
          prop="userName"
          label="用户"
          width="130"
          show-overflow-tooltip
        >
          <template #default="{ row }">
            {{ row.userName || '-' }}
          </template>
        </el-table-column>

        <el-table-column
          prop="clientIpAddress"
          label="IP 地址"
          width="140"
        >
          <template #default="{ row }">
            <span class="mono-text">{{ row.clientIpAddress || '-' }}</span>
          </template>
        </el-table-column>

        <el-table-column
          prop="executionDuration"
          label="耗时"
          width="100"
        >
          <template #default="{ row }">
            <span class="mono-text duration-text">{{ row.executionDuration }} ms</span>
          </template>
        </el-table-column>

        <el-table-column
          prop="httpStatusCode"
          label="状态码"
          width="100"
        >
          <template #default="{ row }">
            <span
              class="status-tag"
              :style="{ color: getStatusColor(row.httpStatusCode), background: getStatusColor(row.httpStatusCode) + '14', borderColor: getStatusColor(row.httpStatusCode) + '30' }"
            >
              {{ row.httpStatusCode || '-' }}
            </span>
          </template>
        </el-table-column>

        <el-table-column
          prop="executionTime"
          label="执行时间"
          width="180"
        >
          <template #default="{ row }">
            {{ formatDateTime(row.executionTime) }}
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
  max-width: 220px;
}

.search-select {
  max-width: 140px;
}

.search-datepicker {
  max-width: 320px;
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

// ── Tags ───────────────────────────────────────────────
.method-tag,
.status-tag {
  display: inline-block;
  padding: 2px 8px;
  border-radius: 6px;
  font-family: 'JetBrains Mono', monospace;
  font-size: 11.5px;
  font-weight: 600;
  border: 1px solid;
  letter-spacing: 0.3px;
}

.url-text {
  font-family: 'JetBrains Mono', monospace;
  font-size: 12px;
  color: var(--text-secondary);
}

.mono-text {
  font-family: 'JetBrains Mono', monospace;
  font-size: 12.5px;
  color: var(--text-primary);
}

.duration-text {
  font-variant-numeric: tabular-nums;
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

// ── Deep overrides: Select ─────────────────────────────
:deep(.el-select) {
  .el-input__wrapper {
    background: var(--input-bg);
    border: 1px solid var(--input-border);
    border-radius: 8px;
    box-shadow: none;
  }
}

// ── Deep overrides: DatePicker ─────────────────────────
:deep(.el-date-editor) {
  .el-input__wrapper {
    background: var(--input-bg);
    border: 1px solid var(--input-border);
    border-radius: 8px;
    box-shadow: none;
  }
}
</style>
