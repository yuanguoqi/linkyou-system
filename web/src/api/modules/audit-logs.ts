import { http } from '../index'
import type { PagedResultDto } from '@/types/common'

// ── Audit Log DTOs ─────────────────────────────────────
export interface AuditLogDto {
  id: string
  httpMethod: string
  url: string
  httpStatusCode: number
  executionDuration: number
  clientIpAddress: string | null
  userName: string | null
  exceptions: string | null
  creationTime: string
  creatorId: string | null
}

export interface GetAuditLogListInput {
  filter?: string
  httpMethod?: string
  startTime?: string
  endTime?: string
  skipCount?: number
  maxResultCount?: number
  sorting?: string
}

// ── API ────────────────────────────────────────────────
const BASE_URL = '/audit-logging/audit-logs'

export const auditLogApi = {
  get: (id: string) =>
    http.get<AuditLogDto>(`${BASE_URL}/${id}`),

  getList: (params: GetAuditLogListInput) =>
    http.get<PagedResultDto<AuditLogDto>>(BASE_URL, { params }),
}
