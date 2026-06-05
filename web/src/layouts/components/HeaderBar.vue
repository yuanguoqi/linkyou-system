<script setup lang="ts">
import { useAuthStore } from '@/stores/auth'
import { useAppStore } from '@/stores/app'
import { useThemeStore } from '@/stores/theme'
import { useRouter } from 'vue-router'
import { ElMessageBox } from 'element-plus'

const authStore = useAuthStore()
const appStore = useAppStore()
const themeStore = useThemeStore()
const router = useRouter()

async function handleCommand(cmd: string) {
  if (cmd === 'logout') {
    await ElMessageBox.confirm('确定要退出登录吗？', '退出确认', {
      confirmButtonText: '确定退出',
      cancelButtonText: '取消',
      type: 'warning',
    })
    await authStore.logout()
    router.push('/login')
  } else if (cmd === 'profile') {
    router.push('/profile')
  } else if (cmd === 'password') {
    router.push('/change-password')
  }
}
</script>

<template>
  <div class="header-bar">
    <!-- 左侧：折叠按钮 + 面包屑 -->
    <div class="header-left">
      <button class="action-btn" @click="appStore.toggleSidebar()">
        <el-icon :size="16">
          <Fold v-if="!appStore.sidebarCollapsed" />
          <Expand v-else />
        </el-icon>
      </button>

      <el-breadcrumb separator="/" class="breadcrumb">
        <el-breadcrumb-item
          v-for="item in appStore.breadcrumbs"
          :key="item.path || item.title"
          :to="item.path"
        >
          {{ item.title }}
        </el-breadcrumb-item>
      </el-breadcrumb>
    </div>

    <!-- 右侧操作区 -->
    <div class="header-right">
      <!-- 语言切换 -->
      <el-tooltip content="切换语言" placement="bottom">
        <el-dropdown @command="appStore.setLocale">
          <button class="action-btn">
            <el-icon :size="15"><Grid /></el-icon>
          </button>
          <template #dropdown>
            <el-dropdown-menu>
              <el-dropdown-item command="zh-Hans">简体中文</el-dropdown-item>
              <el-dropdown-item command="en">English</el-dropdown-item>
            </el-dropdown-menu>
          </template>
        </el-dropdown>
      </el-tooltip>

      <!-- 主题切换 -->
      <el-tooltip :content="themeStore.isDark ? '切换为浅色' : '切换为深色'" placement="bottom">
        <button class="action-btn theme-btn" @click="themeStore.toggleDark()">
          <transition name="theme-icon" mode="out-in">
            <el-icon v-if="themeStore.isDark" :size="15" key="sun"><Sunny /></el-icon>
            <el-icon v-else :size="15" key="moon"><Moon /></el-icon>
          </transition>
        </button>
      </el-tooltip>

      <div class="divider" />

      <!-- 用户信息下拉 -->
      <el-dropdown trigger="click" @command="handleCommand">
        <div class="user-info">
          <div class="user-avatar">
            {{ authStore.userDisplayName.charAt(0).toUpperCase() }}
          </div>
          <div class="user-meta">
            <span class="user-name">{{ authStore.userDisplayName }}</span>
            <span class="user-role">管理员</span>
          </div>
          <el-icon class="arrow-icon" :size="10"><ArrowDown /></el-icon>
        </div>
        <template #dropdown>
          <el-dropdown-menu>
            <el-dropdown-item command="profile">
              <el-icon><User /></el-icon>
              <span>个人信息</span>
            </el-dropdown-item>
            <el-dropdown-item command="password">
              <el-icon><Lock /></el-icon>
              <span>修改密码</span>
            </el-dropdown-item>
            <el-dropdown-item divided command="logout">
              <el-icon><SwitchButton /></el-icon>
              <span>退出登录</span>
            </el-dropdown-item>
          </el-dropdown-menu>
        </template>
      </el-dropdown>
    </div>
  </div>
</template>

<style scoped lang="scss">
.header-bar {
  height: 100%;
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 0 16px;
}

.header-left {
  display: flex;
  align-items: center;
  gap: 10px;

  .breadcrumb {
    :deep(.el-breadcrumb__item .el-breadcrumb__inner) {
      color: var(--breadcrumb-color);
      font-size: 12.5px;
      transition: color 0.2s;
    }
    :deep(.el-breadcrumb__item:last-child .el-breadcrumb__inner) {
      color: var(--breadcrumb-active);
      font-weight: 500;
    }
    :deep(.el-breadcrumb__separator) {
      color: var(--breadcrumb-separator);
    }
  }
}

.header-right {
  display: flex;
  align-items: center;
  gap: 4px;

  .divider {
    width: 1px;
    height: 16px;
    background: var(--border-subtle);
    margin: 0 6px;
  }
}

// Action buttons
.action-btn {
  width: 32px;
  height: 32px;
  display: flex;
  align-items: center;
  justify-content: center;
  background: transparent;
  border: 1px solid transparent;
  border-radius: 8px;
  cursor: pointer;
  color: var(--text-muted);
  transition: all 0.2s cubic-bezier(0.4, 0, 0.2, 1);

  &:hover {
    background: var(--bg-hover);
    border-color: var(--border-primary);
    color: var(--primary-pale);
  }
}

// Theme button icon transition
.theme-icon-enter-active,
.theme-icon-leave-active {
  transition: all 0.2s cubic-bezier(0.4, 0, 0.2, 1);
}
.theme-icon-enter-from {
  opacity: 0;
  transform: rotate(-90deg) scale(0.6);
}
.theme-icon-leave-to {
  opacity: 0;
  transform: rotate(90deg) scale(0.6);
}

// User info
.user-info {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 4px 10px 4px 4px;
  border-radius: 10px;
  cursor: pointer;
  border: 1px solid transparent;
  transition: all 0.25s cubic-bezier(0.4, 0, 0.2, 1);

  &:hover {
    background: var(--bg-hover);
    border-color: var(--border-primary);
  }

  .user-avatar {
    width: 30px;
    height: 30px;
    border-radius: 8px;
    background: var(--avatar-gradient);
    display: flex;
    align-items: center;
    justify-content: center;
    font-family: 'Plus Jakarta Sans', 'Inter', sans-serif;
    font-size: 12.5px;
    font-weight: 700;
    color: #fff;
    box-shadow: 0 0 8px var(--primary-glow);
    flex-shrink: 0;
    transition: all 0.25s cubic-bezier(0.4, 0, 0.2, 1);

    .user-info:hover & {
      transform: scale(1.08);
      box-shadow: 0 0 12px var(--primary-glow),
                  0 0 24px rgba(99, 102, 241, 0.15);
    }
  }

  .user-meta {
    display: flex;
    flex-direction: column;
    gap: 0;

    .user-name {
      font-size: 12.5px;
      font-weight: 500;
      color: var(--text-primary);
      line-height: 1.4;
      transition: color 0.2s;
    }

    .user-role {
      font-size: 11px;
      color: var(--text-muted);
      line-height: 1.3;
      transition: color 0.2s;
    }
  }

  .arrow-icon {
    color: var(--text-muted);
    margin-left: 2px;
    transition: transform 0.2s;

    .user-info:hover & {
      transform: translateY(1px);
    }
  }
}
</style>
