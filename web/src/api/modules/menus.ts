import { http } from '../index'
import type { PagedResultDto } from '@/types/common'

// ── Menu DTOs ──────────────────────────────────────────
export interface MenuDto {
  id: string
  name: string
  path: string
  icon: string
  parentId: string | null
  sort: number
  isVisible: boolean
  permission: string
  creationTime: string
}

export interface CreateMenuDto {
  name: string
  path: string
  icon?: string
  parentId?: string
  sort?: number
  isVisible?: boolean
  permission?: string
}

export interface UpdateMenuDto extends CreateMenuDto {}

export interface GetMenuListInput {
  filter?: string
  skipCount?: number
  maxResultCount?: number
  sorting?: string
}

// ── API ────────────────────────────────────────────────
const BASE_URL = '/api/app/menus'

export const menuApi = {
  get: (id: string) =>
    http.get<MenuDto>(`${BASE_URL}/${id}`),

  getList: (params: GetMenuListInput) =>
    http.get<PagedResultDto<MenuDto>>(BASE_URL, { params }),

  getTree: () =>
    http.get<MenuDto[]>(`${BASE_URL}/tree`),

  create: (data: CreateMenuDto) =>
    http.post<MenuDto>(BASE_URL, data),

  update: (id: string, data: UpdateMenuDto) =>
    http.put<MenuDto>(`${BASE_URL}/${id}`, data),

  delete: (id: string) =>
    http.delete(`${BASE_URL}/${id}`),
}
