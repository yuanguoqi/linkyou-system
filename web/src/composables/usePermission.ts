// 权限检查组合式函数
import { useAuthStore } from '@/stores/auth'

/**
 * 权限检查 composable
 * 用法: const { hasPermission, hasRole } = usePermission()
 */
export function usePermission() {
  const authStore = useAuthStore()

  /**
   * 检查是否拥有指定权限
   * @param permission 权限常量，如 'Products.Products'
   */
  function hasPermission(permission: string): boolean {
    return authStore.hasPermission(permission)
  }

  /**
   * 检查是否拥有指定角色
   */
  function hasRole(role: string): boolean {
    return authStore.hasRole(role)
  }

  /**
   * 检查是否拥有全部权限
   */
  function hasAllPermissions(...permissions: string[]): boolean {
    return permissions.every((p) => authStore.hasPermission(p))
  }

  /**
   * 检查是否拥有任一权限
   */
  function hasAnyPermission(...permissions: string[]): boolean {
    return permissions.some((p) => authStore.hasPermission(p))
  }

  return { hasPermission, hasRole, hasAllPermissions, hasAnyPermission }
}
