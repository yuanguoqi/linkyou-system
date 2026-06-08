import { ref, watch, onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import i18n from '@/locales'

/**
 * 通用 CRUD 分页列表 DTO
 */
export interface PagedResult<T> {
  items: T[]
  totalCount: number
}

/**
 * 通用 CRUD 查询参数
 */
export interface PagedQuery {
  filter?: string
  skipCount?: number
  maxResultCount?: number
  sorting?: string
  [key: string]: unknown
}

/**
 * useCrud 配置选项
 */
export interface UseCrudOptions<T, CreateDto, UpdateDto> {
  /** 获取列表数据 */
  fetchList: (params: PagedQuery) => Promise<{ data: PagedResult<T> }>
  /** 创建记录 */
  create?: (data: CreateDto) => Promise<unknown>
  /** 更新记录 */
  update?: (id: string, data: UpdateDto) => Promise<unknown>
  /** 删除记录 */
  delete?: (id: string) => Promise<unknown>
  /** 删除确认消息模板，{name} 会被替换为记录名称 */
  deleteMessage?: string | ((row: T) => string)
  /** 获取记录名称（用于删除确认） */
  getRowName?: (row: T) => string
  /** 默认排序 */
  defaultSorting?: string
  /** 默认每页条数 */
  defaultPageSize?: number
  /** 页面加载时自动获取数据 */
  immediate?: boolean
}

/**
 * 通用 CRUD 组合函数
 * 封装列表查询、分页、搜索、新增、编辑、删除的完整流程
 */
export function useCrud<T extends { id: string }, CreateDto, UpdateDto = CreateDto>(
  options: UseCrudOptions<T, CreateDto, UpdateDto>
) {
  const { t } = i18n.global

  const {
    fetchList,
    delete: deleteFn,
    deleteMessage = t('crud.deleteConfirm', { name: '{name}' }),
    getRowName = () => '',
    defaultSorting = 'creationTime desc',
    defaultPageSize = 10,
    immediate = true,
  } = options

  // ── 状态 ──────────────────────────────────────────────
  const loading = ref(false)
  const tableData = ref<T[]>([])
  const totalCount = ref(0)
  const searchFilter = ref('')

  // 分页
  const currentPage = ref(1)
  const pageSize = ref(defaultPageSize)

  // 弹窗
  const dialogVisible = ref(false)
  const dialogMode = ref<'create' | 'edit'>('create')
  const editingRow = ref<T | null>(null)

  // ── 计算 ──────────────────────────────────────────────
  function getSkipCount() {
    return (currentPage.value - 1) * pageSize.value
  }

  // ── 数据获取 ──────────────────────────────────────────
  async function fetchData() {
    loading.value = true
    try {
      const params: PagedQuery = {
        filter: searchFilter.value || undefined,
        skipCount: getSkipCount(),
        maxResultCount: pageSize.value,
        sorting: defaultSorting,
      }
      const { data } = await fetchList(params)
      tableData.value = data.items
      totalCount.value = data.totalCount
    } catch {
      // 拦截器已处理错误提示
    } finally {
      loading.value = false
    }
  }

  // ── 搜索 ──────────────────────────────────────────────
  function handleSearch() {
    currentPage.value = 1
    fetchData()
  }

  function handleReset() {
    searchFilter.value = ''
    currentPage.value = 1
    fetchData()
  }

  // ── 新增 ──────────────────────────────────────────────
  function handleCreate() {
    dialogMode.value = 'create'
    editingRow.value = null
    dialogVisible.value = true
  }

  // ── 编辑 ──────────────────────────────────────────────
  function handleEdit(row: T) {
    dialogMode.value = 'edit'
    editingRow.value = { ...row }
    dialogVisible.value = true
  }

  // ── 删除 ──────────────────────────────────────────────
  async function handleDelete(row: T) {
    if (!deleteFn) return

    const name = getRowName(row) || t('crud.thisRecord')
    const msg = typeof deleteMessage === 'function'
      ? deleteMessage(row)
      : deleteMessage.replace('{name}', name)

    try {
      await ElMessageBox.confirm(msg, t('crud.confirmDeleteTitle'), {
        type: 'warning',
        confirmButtonText: t('common.delete'),
        cancelButtonText: t('common.cancel'),
      })
      await deleteFn(row.id)
      ElMessage.success(t('common.deleteSuccess'))
      fetchData()
    } catch {
      // 用户取消或错误
    }
  }

  // ── 弹窗成功回调 ──────────────────────────────────────
  function handleDialogSuccess() {
    dialogVisible.value = false
    fetchData()
  }

  // ── 分页事件 ──────────────────────────────────────────
  function handlePageChange(page: number) {
    currentPage.value = page
    fetchData()
  }

  function handleSizeChange(size: number) {
    pageSize.value = size
    currentPage.value = 1
    fetchData()
  }

  // ── 监听分页变化 ──────────────────────────────────────
  watch([currentPage, pageSize], fetchData)

  // ── 自动加载 ──────────────────────────────────────────
  if (immediate) {
    onMounted(fetchData)
  }

  return {
    // 状态
    loading,
    tableData,
    totalCount,
    searchFilter,
    currentPage,
    pageSize,
    dialogVisible,
    dialogMode,
    editingRow,

    // 方法
    fetchData,
    handleSearch,
    handleReset,
    handleCreate,
    handleEdit,
    handleDelete,
    handleDialogSuccess,
    handlePageChange,
    handleSizeChange,
  }
}
