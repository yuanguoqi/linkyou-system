<script setup lang="ts">
import { ref, computed, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import { useAppStore } from '@/stores/app'
import SidebarMenu from './components/SidebarMenu.vue'
import HeaderBar from './components/HeaderBar.vue'
import TabsNav from './components/TabsNav.vue'

const route = useRoute()
const router = useRouter()
const authStore = useAuthStore()
const appStore = useAppStore()

const collapsed = computed(() => appStore.sidebarCollapsed)
</script>

<template>
  <el-container class="layout-wrapper">
    <!-- 左侧菜单 -->
    <el-aside :width="collapsed ? '64px' : '220px'" class="layout-aside">
      <div class="logo-area">
        <img src="@/assets/logo.svg" alt="logo" class="logo-img" />
        <span v-show="!collapsed" class="logo-text">领佑管理系统</span>
      </div>
      <SidebarMenu :collapsed="collapsed" />
    </el-aside>

    <el-container class="layout-main">
      <!-- 顶部栏 -->
      <el-header class="layout-header">
        <HeaderBar />
      </el-header>

      <!-- 标签页导航 -->
      <TabsNav />

      <!-- 页面内容 -->
      <el-main class="layout-content">
        <router-view v-slot="{ Component }">
          <transition name="fade" mode="out-in">
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
}

.layout-aside {
  background-color: var(--sidebar-bg);
  transition: width 0.3s ease;
  overflow: hidden;
  box-shadow: 2px 0 6px rgba(0, 0, 0, 0.08);

  .logo-area {
    height: 60px;
    display: flex;
    align-items: center;
    padding: 0 16px;
    gap: 10px;
    border-bottom: 1px solid var(--sidebar-border);
    overflow: hidden;
    white-space: nowrap;

    .logo-img {
      width: 32px;
      height: 32px;
      flex-shrink: 0;
    }

    .logo-text {
      font-size: 16px;
      font-weight: 600;
      color: #fff;
    }
  }
}

.layout-header {
  height: 60px;
  padding: 0;
  background: #fff;
  border-bottom: 1px solid #e8e8e8;
  box-shadow: 0 1px 4px rgba(0, 0, 0, 0.06);
}

.layout-content {
  background: #f0f2f5;
  overflow-y: auto;
  padding: 0;
}

.fade-enter-active,
.fade-leave-active {
  transition: opacity 0.2s ease;
}

.fade-enter-from,
.fade-leave-to {
  opacity: 0;
}
</style>
