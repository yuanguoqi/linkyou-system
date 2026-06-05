import { http } from '../index'
import type { PagedResultDto } from '@/types/common'

// ── Audit Log DTOs ─────────────────────────────────────
export interface AuditLogDto {
  id: string
  applicationName: string
  userId: string | null
  userName: string | null
  tenantId: string | null
  tenantName: string | null
  executionTime: string
  executionDuration: number
  clientIpAddress: string
  clientName: string | null
  browserInfo: string | null
  httpMethod: string | null
  url: string | null
  exceptions: string | null
  comments: string | null
  httpStatusCode: number | null
}

export interface GetAuditLogListInput {
  filter?: string
  startTime?: string
  endTime?: string
  httpMethod?: string
  url?: string
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
