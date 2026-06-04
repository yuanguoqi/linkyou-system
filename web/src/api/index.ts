import axios from 'axios'
import type { AxiosInstance, AxiosRequestConfig, AxiosResponse, InternalAxiosRequestConfig } from 'axios'
import { ElMessage, ElMessageBox } from 'element-plus'
import { useAuthStore } from '@/stores/auth'
import router from '@/router'
import type { AbpErrorResponse } from '@/types/common'

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
    const { response } = error

    if (!response) {
      // 网络错误或超时
      ElMessage.error('网络连接异常，请检查网络设置')
      return Promise.reject(error)
    }

    const { status, data } = response
    const abpError = (data as AbpErrorResponse)?.error

    switch (status) {
      case 400:
        // 业务校验失败，显示后端返回的错误消息
        ElMessage.error(abpError?.message || '请求参数错误')
        break

      case 401:
        // 未认证，尝试刷新令牌
        await handleUnauthorized()
        break

      case 403:
        // 无权限
        ElMessage.error('您没有权限执行此操作')
        router.push('/403')
        break

      case 404:
        ElMessage.error(abpError?.message || '请求的资源不存在')
        break

      case 500:
        ElMessage.error(abpError?.message || '服务器内部错误，请联系管理员')
        break

      default:
        ElMessage.error(abpError?.message || `请求失败（${status}）`)
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
    ElMessageBox.alert('登录已过期，请重新登录', '提示', {
      confirmButtonText: '重新登录',
      callback: () => router.push('/login'),
    })
  } finally {
    isRefreshing = false
    pendingRequests = []
  }
}

export { http }
export default http
