<script setup lang="ts">
import { computed } from 'vue'
import { useRoute } from 'vue-router'

defineProps<{
  collapsed: boolean
}>()

const route = useRoute()

const menuItems = [
  {
    path: '/dashboard',
    title: '仪表盘',
    icon: 'Odometer',
  },
  {
    title: '系统管理',
    icon: 'Setting',
    children: [
      { path: '/system/users', title: '用户管理', icon: 'User' },
      { path: '/system/roles', title: '角色管理', icon: 'UserFilled' },
      { path: '/system/tenants', title: '租户管理', icon: 'OfficeBuilding' },
      { path: '/system/audit-logs', title: '审计日志', icon: 'Document' },
      { path: '/system/settings', title: '系统设置', icon: 'Tools' },
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
    :text-color="'var(--menu-text)'"
    :active-text-color="'var(--menu-text-active)'"
    router
    class="sidebar-menu"
  >
    <template v-for="(item, idx) in menuItems" :key="item.path || item.title">
      <!-- Section divider between groups -->
      <div v-if="idx > 0" class="menu-divider" />
      <el-sub-menu v-if="item.children" :index="item.title">
        <template #title>
          <el-icon class="menu-icon"><component :is="item.icon" /></el-icon>
          <span class="menu-title">{{ item.title }}</span>
        </template>
        <el-menu-item
          v-for="child in item.children"
          :key="child.path"
          :index="child.path"
        >
          <el-icon class="menu-icon"><component :is="child.icon" /></el-icon>
          <template #title>
            <span class="menu-title">{{ child.title }}</span>
          </template>
        </el-menu-item>
      </el-sub-menu>

      <el-menu-item v-else :index="item.path">
        <el-icon class="menu-icon"><component :is="item.icon" /></el-icon>
        <template #title>
          <span class="menu-title">{{ item.title }}</span>
        </template>
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
  padding: 6px 0;

  // Section divider
  .menu-divider {
    height: 1px;
    margin: 4px 14px;
    background: linear-gradient(90deg, transparent 0%, var(--border-subtle) 50%, transparent 100%);
  }

  &::-webkit-scrollbar { width: 3px; }
  &::-webkit-scrollbar-track { background: transparent; }
  &::-webkit-scrollbar-thumb {
    background: var(--scrollbar-thumb);
    border-radius: 6px;
  }

  :deep(.el-menu-item),
  :deep(.el-sub-menu__title) {
    height: 40px;
    line-height: 40px;
    margin: 1px 6px;
    border-radius: 8px;
    padding-left: 10px !important;
    transition: all 0.2s cubic-bezier(0.4, 0, 0.2, 1);
    color: var(--menu-text);

    .menu-icon {
      font-size: 16px;
      margin-right: 10px;
      transition: color 0.2s;
    }

    .menu-title {
      font-size: 13px;
      font-weight: 500;
      letter-spacing: -0.1px;
    }

    &:hover {
      background: var(--menu-bg-hover) !important;
      color: var(--menu-text-hover) !important;
      .menu-icon { color: var(--menu-icon-hover) !important; }
    }
  }

  :deep(.el-menu-item.is-active) {
    background: var(--menu-bg-active) !important;
    color: var(--menu-text-active) !important;
    position: relative;

    &::before {
      content: '';
      position: absolute;
      left: 0;
      top: 50%;
      transform: translateY(-50%);
      width: 3px;
      height: 16px;
      background: var(--menu-indicator);
      border-radius: 0 3px 3px 0;
      box-shadow: 0 0 8px var(--primary-glow),
                  0 0 16px rgba(99, 102, 241, 0.15);
    }

    // Subtle glow around active item
    box-shadow: inset 0 0 20px rgba(99, 102, 241, 0.03);

    .menu-icon { color: var(--menu-text-active) !important; }
  }

  :deep(.el-sub-menu .el-menu) {
    background: transparent !important;
  }

  :deep(.el-sub-menu__title .el-sub-menu__icon-arrow) {
    color: var(--sub-menu-arrow);
    font-size: 11px;
  }

  :deep(.el-sub-menu .el-menu-item) {
    padding-left: 34px !important;
    height: 38px;
    line-height: 38px;

    &::before { display: none; }
    &.is-active {
      background: var(--menu-bg-hover) !important;
      &::before { display: block; }
    }
  }

  &.el-menu--collapse {
    :deep(.el-menu-item),
    :deep(.el-sub-menu__title) {
      padding-left: 0 !important;
      justify-content: center;
    }
  }
}
</style>
