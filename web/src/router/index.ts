import { createRouter, createWebHistory } from 'vue-router'
import type { RouteRecordRaw } from 'vue-router'
import NProgress from 'nprogress'
import 'nprogress/nprogress.css'
import { useAuthStore } from '@/stores/auth'
import { useAppStore } from '@/stores/app'

// 不需要认证的白名单路由
const WHITE_LIST = ['/login', '/403', '/404']

// 静态路由（不需要权限）
const staticRoutes: RouteRecordRaw[] = [
  {
    path: '/login',
    name: 'Login',
    component: () => import('@/views/login/index.vue'),
    meta: { title: '登录', hidden: true },
  },
  {
    path: '/403',
    name: '403',
    component: () => import('@/views/error/403.vue'),
    meta: { title: '无权限', hidden: true },
  },
  {
    path: '/404',
    name: '404',
    component: () => import('@/views/error/404.vue'),
    meta: { title: '页面不存在', hidden: true },
  },
  // 主布局（需认证的页面都嵌套在此）
  {
    path: '/',
    component: () => import('@/layouts/MainLayout.vue'),
    redirect: '/dashboard',
    children: [
      {
        path: 'dashboard',
        name: 'Dashboard',
        component: () => import('@/views/dashboard/index.vue'),
        meta: { title: '仪表盘', icon: 'Odometer', affix: true },
      },
    ],
  },
  // 未匹配的路由重定向到 404
  {
    path: '/:pathMatch(.*)*',
    redirect: '/404',
  },
]

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: staticRoutes,
  scrollBehavior: () => ({ top: 0 }),
})

// ==================== 路由守卫 ====================
NProgress.configure({ showSpinner: false })

router.beforeEach(async (to, _from, next) => {
  NProgress.start()

  const authStore = useAuthStore()
  const appStore = useAppStore()

  // 设置页面标题
  if (to.meta?.title) {
    document.title = `${to.meta.title} - ${import.meta.env.VITE_APP_TITLE}`
  }

  // 白名单直接放行
  if (WHITE_LIST.includes(to.path)) {
    return next()
  }

  // 未登录跳转到登录页
  if (!authStore.isAuthenticated) {
    return next({ path: '/login', query: { redirect: to.fullPath } })
  }

  // 已登录但用户信息未加载，先获取
  if (!authStore.currentUser) {
    try {
      await authStore.fetchCurrentUser()
    } catch {
      authStore.logout()
      return next('/login')
    }
  }

  // 检查页面权限
  const requiredPermission = to.meta?.permission as string | undefined
  if (requiredPermission && !authStore.hasPermission(requiredPermission)) {
    return next('/403')
  }

  // 记录访问的 tab
  if (to.name && to.meta?.title) {
    appStore.addTab({
      name: to.name as string,
      path: to.fullPath,
      title: to.meta.title as string,
      affix: to.meta.affix as boolean | undefined,
    })
  }

  next()
})

router.afterEach(() => {
  NProgress.done()
})

export default router
