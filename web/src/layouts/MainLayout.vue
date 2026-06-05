<script setup lang="ts">
import { computed } from 'vue'
import { useAppStore } from '@/stores/app'
import { useThemeStore } from '@/stores/theme'
import SidebarMenu from './components/SidebarMenu.vue'
import HeaderBar from './components/HeaderBar.vue'
import TabsNav from './components/TabsNav.vue'

const appStore = useAppStore()
const themeStore = useThemeStore()
const collapsed = computed(() => appStore.sidebarCollapsed)
</script>

<template>
  <el-container class="layout-wrapper" :class="{ light: !themeStore.isDark }">
    <!-- 左侧菜单 -->
    <el-aside :width="collapsed ? '64px' : '220px'" class="layout-aside">
      <div class="logo-area" :class="{ collapsed }">
        <div class="logo-icon">
          <svg viewBox="0 0 32 32" fill="none" xmlns="http://www.w3.org/2000/svg">
            <rect width="32" height="32" rx="8" fill="url(#logoGrad)"/>
            <path d="M8 16L14 10L20 16L14 22Z" fill="white" opacity="0.9"/>
            <path d="M14 16L20 10L26 16L20 22Z" fill="white" opacity="0.5"/>
            <defs>
              <linearGradient id="logoGrad" x1="0" y1="0" x2="32" y2="32">
                <stop offset="0%" stop-color="#6366f1"/>
                <stop offset="100%" stop-color="#8b5cf6"/>
              </linearGradient>
            </defs>
          </svg>
        </div>
        <transition name="logo-text">
          <span v-show="!collapsed" class="logo-text">领佑管理系统</span>
        </transition>
      </div>
      <SidebarMenu :collapsed="collapsed" />
    </el-aside>

    <el-container class="layout-main">
      <el-header class="layout-header">
        <HeaderBar />
      </el-header>
      <TabsNav />
      <el-main class="layout-content">
        <router-view v-slot="{ Component }">
          <transition name="page-fade" mode="out-in">
            <keep-alive>
              <component :is="Component" />
            </keep-alive>
          </transition>
        </router-view>
      </el-main>
    </el-container>
  </el-container>
</template>

<style scoped lang="scss">
.layout-wrapper {
  height: 100vh;
  overflow: hidden;
  background: var(--bg-page);
  transition: background 0.3s;
}

.layout-aside {
  background: var(--sidebar-gradient);
  transition: width 0.28s cubic-bezier(0.4, 0, 0.2, 1), background 0.3s;
  overflow: hidden;
  display: flex;
  flex-direction: column;
  border-right: 1px solid var(--sidebar-border);
  box-shadow: var(--shadow-sidebar);
  position: relative;

  &::before {
    content: '';
    position: absolute;
    top: -60px;
    left: -60px;
    width: 200px;
    height: 200px;
    background: radial-gradient(circle, rgba(99, 102, 241, 0.1) 0%, transparent 70%);
    pointer-events: none;
  }

  .logo-area {
    height: 64px;
    display: flex;
    align-items: center;
    padding: 0 16px;
    gap: 12px;
    border-bottom: 1px solid var(--border-default);
    overflow: hidden;
    white-space: nowrap;
    flex-shrink: 0;
    transition: padding 0.28s;

    &.collapsed {
      padding: 0;
      justify-content: center;
    }

    .logo-icon {
      width: 32px;
      height: 32px;
      flex-shrink: 0;
      filter: drop-shadow(0 0 8px rgba(99, 102, 241, 0.5));
    }

    .logo-text {
      font-size: 15px;
      font-weight: 700;
      background: var(--logo-text-gradient);
      -webkit-background-clip: text;
      -webkit-text-fill-color: transparent;
      background-clip: text;
      letter-spacing: 0.5px;
    }
  }
}

.layout-header {
  height: 60px;
  padding: 0;
  background: var(--header-bg);
  border-bottom: 1px solid var(--header-border);
  backdrop-filter: blur(12px);
  box-shadow: var(--shadow-header);
  flex-shrink: 0;
  transition: background 0.3s, border-color 0.3s;
}

.layout-content {
  background: var(--content-bg);
  overflow-y: auto;
  padding: 0;
  transition: background 0.3s;

  &::-webkit-scrollbar { width: 6px; }
  &::-webkit-scrollbar-track { background: transparent; }
  &::-webkit-scrollbar-thumb {
    background: var(--scrollbar-thumb);
    border-radius: 3px;
    &:hover { background: var(--scrollbar-thumb-hover); }
  }
}

.logo-text-enter-active,
.logo-text-leave-active {
  transition: opacity 0.2s ease, transform 0.2s ease;
}
.logo-text-enter-from,
.logo-text-leave-to {
  opacity: 0;
  transform: translateX(-8px);
}

.page-fade-enter-active,
.page-fade-leave-active {
  transition: opacity 0.18s ease, transform 0.18s ease;
}
.page-fade-enter-from,
.page-fade-leave-to {
  opacity: 0;
  transform: translateY(4px);
}
</style>
