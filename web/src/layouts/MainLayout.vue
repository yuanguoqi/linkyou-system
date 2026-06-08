<script setup lang="ts">
import { computed } from 'vue'
import { useI18n } from 'vue-i18n'
import { useAppStore } from '@/stores/app'
import { useThemeStore } from '@/stores/theme'
import SidebarMenu from './components/SidebarMenu.vue'
import HeaderBar from './components/HeaderBar.vue'
import TabsNav from './components/TabsNav.vue'

const { t } = useI18n()
const appStore = useAppStore()
const themeStore = useThemeStore()
const collapsed = computed(() => appStore.sidebarCollapsed)
</script>

<template>
  <el-container class="layout-wrapper" :class="{ light: !themeStore.isDark }">
    <!-- 左侧导航 -->
    <el-aside :width="collapsed ? '60px' : '220px'" class="layout-aside">
      <div class="logo-area" :class="{ collapsed }">
        <div class="logo-mark">
          <svg viewBox="0 0 28 28" fill="none" xmlns="http://www.w3.org/2000/svg">
            <defs>
              <linearGradient id="logoGrad" x1="0" y1="0" x2="28" y2="28" gradientUnits="userSpaceOnUse">
                <stop stop-color="#6366f1"/>
                <stop offset="1" stop-color="#818cf8"/>
              </linearGradient>
            </defs>
            <rect width="28" height="28" rx="7" fill="url(#logoGrad)"/>
            <path d="M7 14L12 9L17 14L12 19Z" fill="white" opacity="0.95"/>
            <path d="M13 14L18 9L23 14L18 19Z" fill="white" opacity="0.4"/>
          </svg>
        </div>
        <transition name="logo-slide">
          <span v-show="!collapsed" class="logo-text">{{ t('app.title') }}</span>
        </transition>
      </div>
      <SidebarMenu :collapsed="collapsed" />
    </el-aside>

    <el-container class="layout-main">
      <el-header class="layout-header" :style="{ height: 'var(--header-height)' }">
        <HeaderBar />
      </el-header>
      <TabsNav />
      <el-main class="layout-content">
        <router-view v-slot="{ Component }">
          <transition name="page-enter" mode="out-in">
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
}

// ─── Sidebar ───────────────────────────────────────────
.layout-aside {
  background: var(--sidebar-gradient);
  border-right: 1px solid var(--sidebar-border);
  box-shadow: var(--shadow-sidebar);
  transition: width 0.3s cubic-bezier(0.4, 0, 0.2, 1);
  overflow: hidden;
  display: flex;
  flex-direction: column;
  position: relative;
  z-index: 10;

  // Subtle inner glow on right edge
  &::after {
    content: '';
    position: absolute;
    top: 0;
    right: 0;
    bottom: 0;
    width: 1px;
    background: linear-gradient(180deg, rgba(99, 102, 241, 0.08) 0%, transparent 50%, rgba(99, 102, 241, 0.04) 100%);
    pointer-events: none;
  }
}

// Logo Area
.logo-area {
  height: 52px;
  display: flex;
  align-items: center;
  padding: 0 14px;
  gap: 10px;
  border-bottom: 1px solid var(--border-default);
  overflow: hidden;
  white-space: nowrap;
  flex-shrink: 0;
  transition: padding 0.28s cubic-bezier(0.4, 0, 0.2, 1);

  &.collapsed {
    padding: 0;
    justify-content: center;
  }

  .logo-mark {
    width: 28px;
    height: 28px;
    flex-shrink: 0;
    border-radius: 7px;
    overflow: hidden;
    transition: transform 0.3s cubic-bezier(0.4, 0, 0.2, 1);

    &:hover { transform: scale(1.05); }
  }

  .logo-text {
    font-family: 'Plus Jakarta Sans', 'Inter', sans-serif;
    font-size: 13.5px;
    font-weight: 700;
    letter-spacing: -0.3px;
    background: var(--logo-text-gradient);
    -webkit-background-clip: text;
    -webkit-text-fill-color: transparent;
    background-clip: text;
  }
}

// ─── Header ────────────────────────────────────────────
.layout-header {
  height: var(--header-height) !important;
  padding: 0;
  background: var(--header-bg);
  border-bottom: 1px solid var(--header-border);
  backdrop-filter: blur(20px) saturate(180%);
  -webkit-backdrop-filter: blur(20px) saturate(180%);
  box-shadow: var(--shadow-header);
  flex-shrink: 0;
  position: relative;
  z-index: 9;

  // Animated subtle top border accent
  &::after {
    content: '';
    position: absolute;
    bottom: 0;
    left: 0;
    right: 0;
    height: 1px;
    background: linear-gradient(
      90deg,
      transparent 0%,
      rgba(99, 102, 241, 0.15) 20%,
      rgba(139, 92, 246, 0.1) 50%,
      rgba(99, 102, 241, 0.15) 80%,
      transparent 100%
    );
    animation: header-shimmer 6s ease-in-out infinite;
  }
}

@keyframes header-shimmer {
  0%, 100% { opacity: 0.3; }
  50% { opacity: 0.8; }
}

// ─── Content ───────────────────────────────────────────
.layout-content {
  background: var(--content-bg);
  overflow-y: auto;
  padding: 0;

  &::-webkit-scrollbar { width: 4px; }
  &::-webkit-scrollbar-track { background: transparent; }
  &::-webkit-scrollbar-thumb {
    background: var(--scrollbar-thumb);
    border-radius: 8px;
    &:hover { background: var(--scrollbar-thumb-hover); }
  }
}

// ─── Transitions ───────────────────────────────────────
.logo-slide-enter-active,
.logo-slide-leave-active {
  transition: opacity 0.2s ease, transform 0.2s ease;
}
.logo-slide-enter-from,
.logo-slide-leave-to {
  opacity: 0;
  transform: translateX(-6px);
}

.page-enter-enter-active {
  transition: opacity 0.2s cubic-bezier(0.4, 0, 0.2, 1),
              transform 0.2s cubic-bezier(0.4, 0, 0.2, 1);
}
.page-enter-leave-active {
  transition: opacity 0.15s ease;
}
.page-enter-enter-from {
  opacity: 0;
  transform: translateY(4px);
}
.page-enter-leave-to {
  opacity: 0;
}
</style>
