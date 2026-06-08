<script setup lang="ts">
import { computed } from 'vue'
import { useI18n } from 'vue-i18n'
import { useRoute } from 'vue-router'
import { useMenu } from '@/composables/useMenu'

const { t } = useI18n()

defineProps<{
  collapsed: boolean
}>()

const route = useRoute()
const { menuItems } = useMenu()

const activeMenu = computed(() => route.path)

// 菜单名称 → i18n key 映射
const menuNameMap: Record<string, string> = {
  '仪表盘': 'route.dashboard',
  '系统管理': 'route.system',
  '用户管理': 'route.user',
  '角色管理': 'route.role',
  '租户管理': 'route.tenant',
  '审计日志': 'route.auditLog',
  '菜单管理': 'route.menu',
  '系统设置': 'route.settings',
}

/** 解析菜单名称（支持 i18n） */
function resolveMenuName(name: string): string {
  const key = menuNameMap[name]
  return key ? t(key) : name
}
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
    <template v-for="(item, idx) in menuItems" :key="item.id">
      <!-- Section divider between groups -->
      <div v-if="idx > 0" class="menu-divider" />
      <el-sub-menu v-if="item.children && item.children.length > 0" :index="item.id">
        <template #title>
          <el-icon class="menu-icon"><component :is="item.icon" /></el-icon>
          <span class="menu-title">{{ resolveMenuName(item.name) }}</span>
        </template>
        <el-menu-item
          v-for="child in item.children"
          :key="child.id"
          :index="child.path"
        >
          <el-icon class="menu-icon"><component :is="child.icon" /></el-icon>
          <template #title>
            <span class="menu-title">{{ resolveMenuName(child.name) }}</span>
          </template>
        </el-menu-item>
      </el-sub-menu>

      <el-menu-item v-else :index="item.path">
        <el-icon class="menu-icon"><component :is="item.icon" /></el-icon>
        <template #title>
          <span class="menu-title">{{ resolveMenuName(item.name) }}</span>
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
    height: 42px;
    line-height: 42px;
    margin: 2px 8px;
    border-radius: 8px;
    padding-left: 12px !important;
    transition: all 0.2s cubic-bezier(0.4, 0, 0.2, 1);
    color: var(--menu-text);

    .menu-icon {
      font-size: 17px;
      margin-right: 10px;
      transition: color 0.2s;
    }

    .menu-title {
      font-size: 13.5px;
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
