import axios from 'axios'
import type { AxiosInstance, AxiosResponse, InternalAxiosRequestConfig } from 'axios'
import { ElMessage, ElMessageBox } from 'element-plus'
import { useAuthStore } from '@/stores/auth'
import router from '@/router'
import type { AbpErrorResponse } from '@/types/common'
import i18n from '@/locales'

const { t } = i18n.global

// 创建 axios 实例
const http: AxiosInstance = axios.create({
  // 基础 URL，开发时由 vite proxy 转发，生产时指向真实域名
  baseURL: '/api',
  timeout: 30000,
  headers: {
    'Content-Type': 'application/json',
  },
})

// ==================== 请求拦截器 ====================
http.interceptors.request.use(
  (config: InternalAxiosRequestConfig) => {
    const authStore = useAuthStore()

    // 附加 JWT Token
    if (authStore.accessToken) {
      config.headers.Authorization = `Bearer ${authStore.accessToken}`
    }

    // ABP 多租户：从 store 读取当前租户并附加到请求头
    if (authStore.currentTenantId) {
      config.headers['__tenant'] = authStore.currentTenantId
    }

    // 附加语言标头（后端本地化）
    const locale = localStorage.getItem('locale') || 'zh-Hans'
    config.headers['Accept-Language'] = locale

    return config
  },
  (error) => Promise.reject(error),
)

// ==================== 响应拦截器 ====================
http.interceptors.response.use(
  (response: AxiosResponse) => response,
  async (error) => {
    const { response, config } = error

    // 标记了 skipErrorHandler 的请求（如登录接口），由调用方自行处理错误
    if (config?._skipErrorHandler) {
      return Promise.reject(error)
    }

    if (!response) {
      // 网络错误或超时
      ElMessage.error(t('api.networkError'))
      return Promise.reject(error)
    }

    const { status, data } = response
    const abpError = (data as AbpErrorResponse)?.error

    switch (status) {
      case 400:
        // 检查是否是租户不存在错误
        if (abpError?.code?.includes('MultiTenancy')) {
          // 清除无效的租户信息并跳转登录
          const authStore = useAuthStore()
          authStore.setTenant(null)
          localStorage.removeItem('tenant_id')
          document.cookie = '__tenant=; expires=Thu, 01 Jan 1970 00:00:00 UTC; path=/;'
          router.push('/login')
          ElMessage.error(t('api.tenantNotFound'))
        } else {
          ElMessage.error(abpError?.message || t('api.badRequest'))
        }
        break

      case 401:
        // 未认证，尝试刷新令牌（登录页不会触发此分支，因为登录接口用 skipErrorHandler）
        await handleUnauthorized()
        break

      case 403:
        ElMessage.error(t('api.forbidden'))
        router.push('/403')
        break

      case 404:
        ElMessage.error(abpError?.message || t('api.notFound'))
        break

      case 500:
        // 检查是否是租户不存在错误（ABP 可能返回 500）
        if (abpError?.code?.includes('MultiTenancy')) {
          const authStore2 = useAuthStore()
          authStore2.setTenant(null)
          localStorage.removeItem('tenant_id')
          document.cookie = '__tenant=; expires=Thu, 01 Jan 1970 00:00:00 UTC; path=/;'
          router.push('/login')
          ElMessage.error(t('api.tenantNotFound'))
        } else {
          ElMessage.error(abpError?.message || t('api.serverError'))
        }
        break

      default:
        ElMessage.error(abpError?.message || t('api.requestFailed', { status }))
    }

    return Promise.reject(error)
  },
)

// 处理 401 - 尝试刷新令牌，失败则跳转登录
let isRefreshing = false
let pendingRequests: Array<(token: string) => void> = []

async function handleUnauthorized() {
  const authStore = useAuthStore()

  if (isRefreshing) {
    // 等待刷新完成后重试
    return new Promise<void>((resolve) => {
      pendingRequests.push(() => resolve())
    })
  }

  isRefreshing = true

  try {
    await authStore.refreshToken()
    pendingRequests.forEach((cb) => cb(authStore.accessToken!))
  } catch {
    // 刷新失败，清除状态并跳转登录
    authStore.logout()
    ElMessageBox.alert(t('api.sessionExpired'), t('common.hint'), {
      confirmButtonText: t('api.relogin'),
      callback: () => router.push('/login'),
    })
  } finally {
    isRefreshing = false
    pendingRequests = []
  }
}

export { http }
export default http
