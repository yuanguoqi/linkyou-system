<script setup lang="ts">
import { computed } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import { usePermission } from '@/composables/usePermission'

const props = defineProps<{
  collapsed: boolean
}>()

const router = useRouter()
const route = useRoute()
const { hasPermission } = usePermission()

// 菜单列表（静态配置，可扩展为动态路由）
const menuItems = [
  {
    path: '/dashboard',
    title: '仪表盘',
    icon: 'Odometer',
    permission: '',
  },
  {
    title: '系统管理',
    icon: 'Setting',
    permission: '',
    children: [
      { path: '/system/users', title: '用户管理', icon: 'User', permission: 'AbpIdentity.Users' },
      { path: '/system/roles', title: '角色管理', icon: 'UserFilled', permission: 'AbpIdentity.Roles' },
      { path: '/system/tenants', title: '租户管理', icon: 'OfficeBuilding', permission: 'AbpTenantManagement.Tenants' },
      { path: '/system/audit-logs', title: '审计日志', icon: 'Document', permission: 'AuditLogging.AuditLogs' },
      { path: '/system/settings', title: '系统设置', icon: 'Tools', permission: 'SettingManagement.Emailing' },
    ],
  },
]

const activeMenu = computed(() => route.path)
</script>

<template>
  <el-menu
    :default-active="activeMenu"
    :collapse="collapsed"
    :collapse-transition="false"
    background-color="transparent"
    text-color="#bfcbd9"
    active-text-color="#409eff"
    router
    class="sidebar-menu"
  >
    <template v-for="item in menuItems" :key="item.path || item.title">
      <!-- 有子菜单 -->
      <el-sub-menu
        v-if="item.children"
        :index="item.title"
      >
        <template #title>
          <el-icon><component :is="item.icon" /></el-icon>
          <span>{{ item.title }}</span>
        </template>
        <el-menu-item
          v-for="child in item.children"
          :key="child.path"
          :index="child.path"
        >
          <el-icon><component :is="child.icon" /></el-icon>
          <span>{{ child.title }}</span>
        </el-menu-item>
      </el-sub-menu>

      <!-- 无子菜单 -->
      <el-menu-item
        v-else
        :index="item.path"
      >
        <el-icon><component :is="item.icon" /></el-icon>
        <template #title>{{ item.title }}</template>
      </el-menu-item>
    </template>
  </el-menu>
</template>

<style scoped lang="scss">
.sidebar-menu {
  border-right: none;
  flex: 1;
  overflow-y: auto;
  overflow-x: hidden;

  :deep(.el-menu-item),
  :deep(.el-sub-menu__title) {
    &:hover {
      background-color: rgba(255, 255, 255, 0.05) !important;
    }
    &.is-active {
      background-color: rgba(64, 158, 255, 0.15) !important;
    }
  }
}
</style>
