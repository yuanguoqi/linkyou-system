<script setup lang="ts">
import { useAuthStore } from '@/stores/auth'
import { useAppStore } from '@/stores/app'
import { useRouter } from 'vue-router'
import { ElMessageBox } from 'element-plus'

const authStore = useAuthStore()
const appStore = useAppStore()
const router = useRouter()

async function handleLogout() {
  await ElMessageBox.confirm('确定要退出登录吗？', '提示', {
    confirmButtonText: '确定',
    cancelButtonText: '取消',
    type: 'warning',
  })
  await authStore.logout()
  router.push('/login')
}
</script>

<template>
  <div class="header-bar">
    <!-- 左侧：折叠按钮 + 面包屑 -->
    <div class="header-left">
      <el-icon
        class="collapse-btn"
        @click="appStore.toggleSidebar()"
      >
        <Fold v-if="!appStore.sidebarCollapsed" />
        <Expand v-else />
      </el-icon>

      <el-breadcrumb separator="/">
        <el-breadcrumb-item
          v-for="item in appStore.breadcrumbs"
          :key="item.path || item.title"
          :to="item.path"
        >
          {{ item.title }}
        </el-breadcrumb-item>
      </el-breadcrumb>
    </div>

    <!-- 右侧：语言切换 + 用户���息 -->
    <div class="header-right">
      <!-- 语言切换 -->
      <el-tooltip content="切换语言">
        <el-dropdown @command="appStore.setLocale">
          <el-icon class="action-icon"><Grid /></el-icon>
          <template #dropdown>
            <el-dropdown-menu>
              <el-dropdown-item command="zh-Hans">简体中文</el-dropdown-item>
              <el-dropdown-item command="en">English</el-dropdown-item>
            </el-dropdown-menu>
          </template>
        </el-dropdown>
      </el-tooltip>

      <!-- 用户头像下拉菜单 -->
      <el-dropdown trigger="click" @command="(cmd: string) => cmd === 'logout' && handleLogout()">
        <div class="user-info">
          <el-avatar :size="32" class="user-avatar">
            {{ authStore.userDisplayName.charAt(0).toUpperCase() }}
          </el-avatar>
          <span class="user-name">{{ authStore.userDisplayName }}</span>
          <el-icon><ArrowDown /></el-icon>
        </div>
        <template #dropdown>
          <el-dropdown-menu>
            <el-dropdown-item command="profile">
              <el-icon><User /></el-icon>个人信息
            </el-dropdown-item>
            <el-dropdown-item command="password">
              <el-icon><Lock /></el-icon>修改密码
            </el-dropdown-item>
            <el-dropdown-item divided command="logout">
              <el-icon><SwitchButton /></el-icon>退出登录
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
  gap: 16px;

  .collapse-btn {
    font-size: 20px;
    cursor: pointer;
    color: #606266;
    &:hover { color: #409eff; }
  }
}

.header-right {
  display: flex;
  align-items: center;
  gap: 16px;

  .action-icon {
    font-size: 18px;
    cursor: pointer;
    color: #606266;
    &:hover { color: #409eff; }
  }

  .user-info {
    display: flex;
    align-items: center;
    gap: 8px;
    cursor: pointer;
    padding: 4px 8px;
    border-radius: 4px;
    &:hover { background: #f5f7fa; }

    .user-name {
      font-size: 14px;
      color: #303133;
    }
  }
}
</style>
