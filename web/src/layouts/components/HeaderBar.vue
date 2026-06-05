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
        <el-icon :size="18">
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
            <el-icon :size="16"><Grid /></el-icon>
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
          <el-icon :size="16">
            <Sunny v-if="themeStore.isDark" />
            <Moon v-else />
          </el-icon>
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
          <el-icon class="arrow-icon" :size="12"><ArrowDown /></el-icon>
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
  padding: 0 20px;
}

.header-left {
  display: flex;
  align-items: center;
  gap: 12px;

  .breadcrumb {
    :deep(.el-breadcrumb__item .el-breadcrumb__inner) {
      color: var(--breadcrumb-color);
      font-size: 13px;
      transition: color 0.3s;
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
  gap: 6px;

  .divider {
    width: 1px;
    height: 20px;
    background: var(--border-subtle);
    margin: 0 4px;
  }
}

// 通用操作按钮（折叠、语言、主题）
.action-btn {
  width: 34px;
  height: 34px;
  display: flex;
  align-items: center;
  justify-content: center;
  background: var(--bg-input);
  border: 1px solid var(--border-subtle);
  border-radius: 8px;
  cursor: pointer;
  color: var(--text-secondary);
  transition: all 0.2s;

  &:hover {
    background: var(--bg-hover);
    border-color: var(--border-active);
    color: var(--primary-pale);
  }
}

// 主题按钮 hover 带旋转动效
.theme-btn:hover {
  :deep(.el-icon) {
    transform: rotate(20deg);
    transition: transform 0.3s;
  }
}

// 用户信息
.user-info {
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 6px 12px 6px 6px;
  border-radius: 10px;
  cursor: pointer;
  border: 1px solid transparent;
  transition: all 0.2s;

  &:hover {
    background: var(--bg-hover);
    border-color: var(--border-primary);
  }

  .user-avatar {
    width: 34px;
    height: 34px;
    border-radius: 8px;
    background: var(--avatar-gradient);
    display: flex;
    align-items: center;
    justify-content: center;
    font-size: 14px;
    font-weight: 700;
    color: #fff;
    box-shadow: 0 0 12px var(--primary-glow);
    flex-shrink: 0;
  }

  .user-meta {
    display: flex;
    flex-direction: column;
    gap: 1px;

    .user-name {
      font-size: 13px;
      font-weight: 500;
      color: var(--text-primary);
      line-height: 1.3;
      transition: color 0.3s;
    }

    .user-role {
      font-size: 11px;
      color: var(--text-muted);
      line-height: 1.3;
      transition: color 0.3s;
    }
  }

  .arrow-icon {
    color: var(--text-muted);
    margin-left: 2px;
  }
}
</style>
