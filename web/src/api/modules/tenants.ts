import { http } from '../index'
import type { PagedResultDto } from '@/types/common'

// ── Tenant DTOs ────────────────────────────────────────
export interface TenantDto {
  id: string
  name: string
  concurrencyStamp: string
  creationTime: string
}

export interface CreateTenantDto {
  name: string
  adminEmailAddress: string
  adminPassword: string
}

export interface UpdateTenantDto {
  name: string
  concurrencyStamp: string
}

export interface GetTenantListInput {
  filter?: string
  skipCount?: number
  maxResultCount?: number
  sorting?: string
}

// ── API ────────────────────────────────────────────────
const BASE_URL = '/api/multi-tenancy/tenants'

export const tenantApi = {
  get: (id: string) =>
    http.get<TenantDto>(`${BASE_URL}/${id}`),

  getList: (params: GetTenantListInput) =>
    http.get<PagedResultDto<TenantDto>>(BASE_URL, { params }),

  create: (data: CreateTenantDto) =>
    http.post<TenantDto>(BASE_URL, data),

  update: (id: string, data: UpdateTenantDto) =>
    http.put<TenantDto>(`${BASE_URL}/${id}`, data),

  delete: (id: string) =>
    http.delete(`${BASE_URL}/${id}`),
}
