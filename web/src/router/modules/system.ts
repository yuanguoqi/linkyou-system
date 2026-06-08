import type { RouteRecordRaw } from 'vue-router'

const systemRoutes: RouteRecordRaw[] = [
  {
    path: '/system',
    meta: { title: 'route.system', icon: 'Setting' },
    children: [
      {
        path: 'users',
        name: 'UserList',
        component: () => import('@/views/system/users/index.vue'),
        meta: { title: 'route.user', icon: 'User' },
      },
      {
        path: 'roles',
        name: 'RoleList',
        component: () => import('@/views/system/roles/index.vue'),
        meta: { title: 'route.role', icon: 'UserFilled' },
      },
      {
        path: 'tenants',
        name: 'TenantList',
        component: () => import('@/views/system/tenants/index.vue'),
        meta: { title: 'route.tenant', icon: 'OfficeBuilding' },
      },
      {
        path: 'audit-logs',
        name: 'AuditLogList',
        component: () => import('@/views/system/audit-logs/index.vue'),
        meta: { title: 'route.auditLog', icon: 'Document' },
      },
      {
        path: 'menus',
        name: 'MenuList',
        component: () => import('@/views/system/menus/index.vue'),
        meta: { title: 'route.menu', icon: 'Menu' },
      },
      {
        path: 'settings',
        name: 'SystemSettings',
        component: () => import('@/views/system/settings/index.vue'),
        meta: { title: 'route.settings', icon: 'Tools' },
      },
    ],
  },
]

export default systemRoutes
