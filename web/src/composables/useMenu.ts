import { ref, onMounted } from 'vue'
import { menuApi } from '@/api/modules/menus'
import type { UserMenuDto } from '@/api/modules/menus'

/**
 * 动态菜单组合函数
 * 从后端获取当前用户有权访问的菜单（树形结构）
 */
export function useMenu() {
  const menuItems = ref<UserMenuDto[]>([])
  const loading = ref(false)

  async function loadMenus() {
    loading.value = true
    try {
      const res = await menuApi.getUserMenus()
      menuItems.value = res.data
    } catch {
      menuItems.value = []
    } finally {
      loading.value = false
    }
  }

  onMounted(loadMenus)

  return { menuItems, loading, reload: loadMenus }
}
