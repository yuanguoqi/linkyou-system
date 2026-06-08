import { http } from '../index'
import type { PagedResultDto } from '@/types/common'

// ── User DTOs ──────────────────────────────────────────
export interface IdentityUserDto {
  id: string
  userName: string
  email: string
  name: string
  surname: string
  phoneNumber: string | null
  isActive: boolean
  lockoutEnd: string | null
  concurrencyStamp: string
  creationTime: string
  roles: IdentityUserRoleDto[]
}

export interface IdentityUserRoleDto {
  roleId: string
  roleName: string
}

export interface CreateIdentityUserDto {
  userName: string
  email: string
  name: string
  surname: string
  password: string
  phoneNumber?: string
  isActive?: boolean
  roleNames?: string[]
}

export interface UpdateIdentityUserDto extends CreateIdentityUserDto {
  concurrencyStamp: string
}

export interface GetIdentityUserListInput {
  filter?: string
  skipCount?: number
  maxResultCount?: number
  sorting?: string
}

// ── Role DTOs ──────────────────────────────────────────
export interface IdentityRoleDto {
  id: string
  name: string
  isDefault: boolean
  isStatic: boolean
  isPublic: boolean
  concurrencyStamp: string
  creationTime: string
}

export interface CreateIdentityRoleDto {
  name: string
  isDefault?: boolean
  isPublic?: boolean
}

export interface UpdateIdentityRoleDto extends CreateIdentityRoleDto {
  concurrencyStamp: string
}

export interface GetIdentityRoleListInput {
  filter?: string
  skipCount?: number
  maxResultCount?: number
  sorting?: string
}

// ── API ────────────────────────────────────────────────
const USER_BASE = '/identity/users'
const ROLE_BASE = '/identity/roles'

export const identityUserApi = {
  get: (id: string) =>
    http.get<IdentityUserDto>(`${USER_BASE}/${id}`),

  getList: (params: GetIdentityUserListInput) =>
    http.get<PagedResultDto<IdentityUserDto>>(USER_BASE, { params }),

  create: (data: CreateIdentityUserDto) =>
    http.post<IdentityUserDto>(USER_BASE, data),

  update: (id: string, data: UpdateIdentityUserDto) =>
    http.put<IdentityUserDto>(`${USER_BASE}/${id}`, data),

  delete: (id: string) =>
    http.delete(`${USER_BASE}/${id}`),
}

export const identityRoleApi = {
  get: (id: string) =>
    http.get<IdentityRoleDto>(`${ROLE_BASE}/${id}`),

  getList: (params: GetIdentityRoleListInput) =>
    http.get<PagedResultDto<IdentityRoleDto>>(ROLE_BASE, { params }),

  /** 获取所有角色（用于下拉选择） */
  getAllList: () =>
    http.get<PagedResultDto<IdentityRoleDto>>(ROLE_BASE, { params: { maxResultCount: 1000 } }),

  create: (data: CreateIdentityRoleDto) =>
    http.post<IdentityRoleDto>(ROLE_BASE, data),

  update: (id: string, data: UpdateIdentityRoleDto) =>
    http.put<IdentityRoleDto>(`${ROLE_BASE}/${id}`, data),

  delete: (id: string) =>
    http.delete(`${ROLE_BASE}/${id}`),
}
