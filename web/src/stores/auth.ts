import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { authApi } from '@/api/modules/auth'
import type { CurrentUser, LoginRequest } from '@/types/auth'

/** 认证状态管理 */
export const useAuthStore = defineStore('auth', () => {
  // ==================== 状态 ====================
  const accessToken = ref<string | null>(localStorage.getItem('access_token'))
  const refreshTokenValue = ref<string | null>(localStorage.getItem('refresh_token'))
  const currentUser = ref<CurrentUser | null>(null)
  const currentTenantId = ref<string | null>(localStorage.getItem('tenant_id'))

  // ==================== 计算属性 ====================
  const isAuthenticated = computed(() => !!accessToken.value)

  const userDisplayName = computed(() => {
    if (!currentUser.value) return ''
    const { name, surName, userName } = currentUser.value
    return name || surName ? `${name ?? ''} ${surName ?? ''}`.trim() : userName
  })

  /** 检查是否拥有某个权限 */
  function hasPermission(permission: string): boolean {
    return currentUser.value?.permissions?.includes(permission) ?? false
  }

  /** 检查是否拥有某个角色 */
  function hasRole(role: string): boolean {
    return currentUser.value?.roles?.includes(role) ?? false
  }

  // ==================== 操作 ====================

  /** 登录 */
  async function login(data: LoginRequest) {
    const res = await authApi.login(data)
    const { accessToken: token, refreshToken } = res.data

    // 持久化到 localStorage
    accessToken.value = token
    refreshTokenValue.value = refreshToken
    localStorage.setItem('access_token', token)
    localStorage.setItem('refresh_token', refreshToken)

    // 获取当前用户信息
    await fetchCurrentUser()
  }

  /** 刷新令牌 */
  async function refreshToken() {
    if (!refreshTokenValue.value) throw new Error('无刷新令牌')

    const res = await authApi.refreshToken({ refreshToken: refreshTokenValue.value })
    const { accessToken: token, refreshToken: newRefreshToken } = res.data

    accessToken.value = token
    refreshTokenValue.value = newRefreshToken
    localStorage.setItem('access_token', token)
    localStorage.setItem('refresh_token', newRefreshToken)
  }

  /** 获取当前用户信息 */
  async function fetchCurrentUser() {
    const res = await authApi.getCurrentUser()
    currentUser.value = res.data
  }

  /** 切换租户 */
  function setTenant(tenantId: string | null) {
    currentTenantId.value = tenantId
    if (tenantId) {
      localStorage.setItem('tenant_id', tenantId)
    } else {
      localStorage.removeItem('tenant_id')
    }
  }

  /** 退出登录，清除所有状态 */
  async function logout() {
    try {
      await authApi.logout()
    } catch {
      // 忽略退出接口错误，本地清除即可
    } finally {
      accessToken.value = null
      refreshTokenValue.value = null
      currentUser.value = null
      currentTenantId.value = null
      localStorage.removeItem('access_token')
      localStorage.removeItem('refresh_token')
      localStorage.removeItem('tenant_id')
    }
  }

  return {
    accessToken,
    currentUser,
    currentTenantId,
    isAuthenticated,
    userDisplayName,
    hasPermission,
    hasRole,
    login,
    refreshToken,
    fetchCurrentUser,
    setTenant,
    logout,
  }
})
