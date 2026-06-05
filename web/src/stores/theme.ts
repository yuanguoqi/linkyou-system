import { defineStore } from 'pinia'
import { ref, watchEffect } from 'vue'

/** 主题（深色/亮色）状态管理
 * 默认深色，CSS 变量在 :root 里定义深色值，html.light 覆盖为浅色值。
 */
export const useThemeStore = defineStore('theme', () => {
  // 默认深色，仅当 localStorage 明确存了 'light' 时才切浅色
  const isDark = ref(localStorage.getItem('theme') !== 'light')

  function toggleDark() {
    isDark.value = !isDark.value
    localStorage.setItem('theme', isDark.value ? 'dark' : 'light')
  }

  // 深色：移除 light class；浅色：添加 light class
  watchEffect(() => {
    document.documentElement.classList.toggle('light', !isDark.value)
  })

  return { isDark, toggleDark }
})
