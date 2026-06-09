import { http } from '../index'
import type { PagedResultDto } from '@/types/common'

export interface AuditLogDto {
  id: string
  userId: string | null
  userName: string | null
  tenantId: string | null
  applicationName: string | null
  clientIpAddress: string | null
  httpMethod: string | null
  url: string | null
  httpStatusCode: number | null
  executionDuration: number
  executionTime: string
  exceptions: string | null
  browserInfo: string | null
}

export interface GetAuditLogListInput {
  url?: string
  userName?: string
  httpMethod?: string
  startTime?: string
  endTime?: string
  skipCount?: number
  maxResultCount?: number
  sorting?: string
}

const BASE_URL = '/audit-logging/audit-logs'

export const auditLogApi = {
  get: (id: string) =>
    http.get<AuditLogDto>(`${BASE_URL}/${id}`),

  getList: (params: GetAuditLogListInput) =>
    http.get<PagedResultDto<AuditLogDto>>(BASE_URL, { params }),
}
