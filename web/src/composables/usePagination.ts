// 分页组合式函数，封装通用分页逻辑
import { ref, reactive } from 'vue'

export interface PaginationOptions {
  defaultPageSize?: number
}

/**
 * 通用分页 composable
 * 用法: const { pagination, handlePageChange, handleSizeChange, resetPage } = usePagination()
 */
export function usePagination(options: PaginationOptions = {}) {
  const { defaultPageSize = 10 } = options

  const currentPage = ref(1)
  const pageSize = ref(defaultPageSize)

  const pagination = reactive({
    currentPage,
    pageSize,
    pageSizes: [10, 20, 50, 100],
    layout: 'total, sizes, prev, pager, next, jumper',
  })

  function handlePageChange(page: number) {
    currentPage.value = page
  }

  function handleSizeChange(size: number) {
    pageSize.value = size
    currentPage.value = 1
  }

  function resetPage() {
    currentPage.value = 1
  }

  /** 计算 skipCount（ABP 分页偏移量） */
  function getSkipCount(): number {
    return (currentPage.value - 1) * pageSize.value
  }

  return {
    currentPage,
    pageSize,
    pagination,
    handlePageChange,
    handleSizeChange,
    resetPage,
    getSkipCount,
  }
}
