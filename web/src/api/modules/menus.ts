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

export interface UserMenuDto {
  id: string
  name: string
  path: string
  icon: string | null
  parentId: string | null
  sort: number
  children: UserMenuDto[]
}

export interface MenuRolePermissionDto {
  menuId: string
  roleName: string
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
const BASE_URL = '/app/menus'

export const menuApi = {
  get: (id: string) =>
    http.get<MenuDto>(`${BASE_URL}/${id}`),

  getList: (params: GetMenuListInput) =>
    http.get<PagedResultDto<MenuDto>>(BASE_URL, { params }),

  /** 获取当前用户的菜单（根据角色权限过滤，树形结构） */
  getUserMenus: () =>
    http.get<UserMenuDto[]>(`${BASE_URL}/user-menus`),

  /** 获取指定菜单的角色权限列表 */
  getMenuRolePermissions: (menuId: string) =>
    http.get<MenuRolePermissionDto[]>(`${BASE_URL}/${menuId}/role-permissions`),

  /** 设置菜单的角色权限（覆盖写入） */
  setMenuRolePermissions: (menuId: string, roleNames: string[]) =>
    http.put(`${BASE_URL}/${menuId}/role-permissions`, roleNames),

  create: (data: CreateMenuDto) =>
    http.post<MenuDto>(BASE_URL, data),

  update: (id: string, data: UpdateMenuDto) =>
    http.put<MenuDto>(`${BASE_URL}/${id}`, data),

  delete: (id: string) =>
    http.delete(`${BASE_URL}/${id}`),
}
