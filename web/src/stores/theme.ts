import { defineStore } from 'pinia'
import { ref, watchEffect } from 'vue'

/** 主题（深色/亮色）状态管理 */
export const useThemeStore = defineStore('theme', () => {
  const isDark = ref(localStorage.getItem('theme') === 'dark')

  function toggleDark() {
    isDark.value = !isDark.value
    localStorage.setItem('theme', isDark.value ? 'dark' : 'light')
  }

  // 同步到 html 标签（供全局样式使用）
  watchEffect(() => {
    document.documentElement.classList.toggle('dark', isDark.value)
  })

  return { isDark, toggleDark }
})
