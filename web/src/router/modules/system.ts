import type { RouteRecordRaw } from 'vue-router'

const systemRoutes: RouteRecordRaw[] = [
  {
    path: '/system',
    meta: { title: '系统管理', icon: 'Setting' },
    children: [
      {
        path: 'users',
        name: 'UserList',
        component: () => import('@/views/system/users/index.vue'),
        meta: { title: '用户管理', icon: 'User' },
      },
      {
        path: 'roles',
        name: 'RoleList',
        component: () => import('@/views/system/roles/index.vue'),
        meta: { title: '角色管理', icon: 'UserFilled' },
      },
      {
        path: 'tenants',
        name: 'TenantList',
        component: () => import('@/views/system/tenants/index.vue'),
        meta: { title: '租户管理', icon: 'OfficeBuilding' },
      },
      {
        path: 'audit-logs',
        name: 'AuditLogList',
        component: () => import('@/views/system/audit-logs/index.vue'),
        meta: { title: '审计日志', icon: 'Document' },
      },
      {
        path: 'menus',
        name: 'MenuList',
        component: () => import('@/views/system/menus/index.vue'),
        meta: { title: '菜单管理', icon: 'Menu' },
      },
      {
        path: 'settings',
        name: 'SystemSettings',
        component: () => import('@/views/system/settings/index.vue'),
        meta: { title: '系统设置', icon: 'Tools' },
      },
    ],
  },
]

export default systemRoutes
