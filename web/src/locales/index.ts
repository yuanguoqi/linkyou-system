import { createI18n } from 'vue-i18n'
import zhHans from './zh-Hans'
import en from './en'

const i18n = createI18n({
  legacy: false,         // 使用 Composition API 模式
  locale: localStorage.getItem('locale') || 'zh-Hans',
  fallbackLocale: 'zh-Hans',
  messages: {
    'zh-Hans': zhHans,
    en,
  },
})

export default i18n
