import { defineStore } from 'pinia'
import { ref } from 'vue'
import type { MenuItem } from '@/types/router'

/** 应用全局状态（侧边栏折叠、面包屑、标签页等） */
export const useAppStore = defineStore('app', () => {
  // 侧边栏是否折叠
  const sidebarCollapsed = ref(false)

  // 当前语言
  const locale = ref(localStorage.getItem('locale') || 'zh-Hans')

  // 面包屑导航
  const breadcrumbs = ref<{ title: string; path?: string }[]>([])

  // 多标签页（tab）
  const visitedTabs = ref<{ name: string; path: string; title: string; affix?: boolean }[]>([])

  function toggleSidebar() {
    sidebarCollapsed.value = !sidebarCollapsed.value
  }

  function setLocale(lang: string) {
    locale.value = lang
    localStorage.setItem('locale', lang)
  }

  function setBreadcrumbs(items: { title: string; path?: string }[]) {
    breadcrumbs.value = items
  }

  function addTab(tab: { name: string; path: string; title: string; affix?: boolean }) {
    const exists = visitedTabs.value.some((t) => t.path === tab.path)
    if (!exists) {
      visitedTabs.value.push(tab)
    }
  }

  function removeTab(path: string) {
    const idx = visitedTabs.value.findIndex((t) => t.path === path)
    if (idx !== -1) {
      visitedTabs.value.splice(idx, 1)
    }
  }

  function closeOtherTabs(path: string) {
    visitedTabs.value = visitedTabs.value.filter((t) => t.path === path || t.affix)
  }

  function closeAllTabs() {
    visitedTabs.value = visitedTabs.value.filter((t) => t.affix)
  }

  return {
    sidebarCollapsed,
    locale,
    breadcrumbs,
    visitedTabs,
    toggleSidebar,
    setLocale,
    setBreadcrumbs,
    addTab,
    removeTab,
    closeOtherTabs,
    closeAllTabs,
  }
})
