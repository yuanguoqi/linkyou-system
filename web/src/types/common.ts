// ABP 后端通用响应类型定义

/** 分页结果 */
export interface PagedResultDto<T> {
  items: T[]
  totalCount: number
}

/** ABP 错误响应结构 */
export interface AbpErrorResponse {
  error: {
    code: string
    message: string
    details?: string
    validationErrors?: ValidationError[]
    data?: Record<string, unknown>
  }
}

/** 字段验证错误 */
export interface ValidationError {
  message: string
  members: string[]
}

/** 分页查询基类 */
export interface PagedAndSortedResultRequestDto {
  skipCount?: number
  maxResultCount?: number
  sorting?: string
}

/** 审计字段基类 */
export interface FullAuditedEntityDto {
  id: string
  creationTime: string
  creatorId: string | null
  lastModificationTime: string | null
  lastModifierId: string | null
  isDeleted: boolean
  deletionTime: string | null
  deleterId: string | null
}

/** 通用键值对（用于下拉框等） */
export interface LookupDto<TKey = string> {
  id: TKey
  displayName: string
}
