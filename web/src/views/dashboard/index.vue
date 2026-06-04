<script setup lang="ts">
// 仪表盘首页
import { ref } from 'vue'
import { useAuthStore } from '@/stores/auth'

const authStore = useAuthStore()

// 统计卡片数据（实际项目中从 API 获取）
const stats = ref([
  { title: '用户总数', value: '-', icon: 'User', color: '#409eff' },
  { title: '角色总数', value: '-', icon: 'UserFilled', color: '#67c23a' },
  { title: '租户总数', value: '-', icon: 'OfficeBuilding', color: '#e6a23c' },
  { title: '今日登录', value: '-', icon: 'DataLine', color: '#f56c6c' },
])
</script>

<template>
  <div class="dashboard">
    <!-- 欢迎横幅 -->
    <el-card class="welcome-card" shadow="never">
      <div class="welcome-content">
        <div>
          <h2>欢迎回来，{{ authStore.userDisplayName }} 👋</h2>
          <p>今天是个好日子，祝工作顺利！</p>
        </div>
      </div>
    </el-card>

    <!-- 统计卡片 -->
    <el-row :gutter="16" class="stat-row">
      <el-col
        v-for="stat in stats"
        :key="stat.title"
        :xs="24" :sm="12" :lg="6"
      >
        <el-card shadow="never" class="stat-card">
          <div class="stat-content">
            <div class="stat-info">
              <div class="stat-title">{{ stat.title }}</div>
              <div class="stat-value">{{ stat.value }}</div>
            </div>
            <div class="stat-icon" :style="{ background: stat.color + '1a', color: stat.color }">
              <el-icon :size="28"><component :is="stat.icon" /></el-icon>
            </div>
          </div>
        </el-card>
      </el-col>
    </el-row>
  </div>
</template>

<style scoped lang="scss">
.dashboard {
  padding: 16px;
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.welcome-card {
  background: linear-gradient(135deg, #1a237e, #1565c0);
  border: none;
  :deep(.el-card__body) { padding: 24px; }

  .welcome-content {
    h2 { color: #fff; font-size: 20px; margin: 0 0 6px; }
    p  { color: rgba(255,255,255,.7); margin: 0; font-size: 14px; }
  }
}

.stat-row { margin: 0 !important; }

.stat-card {
  :deep(.el-card__body) { padding: 20px; }

  .stat-content {
    display: flex;
    justify-content: space-between;
    align-items: center;
  }

  .stat-title {
    font-size: 13px;
    color: #909399;
    margin-bottom: 8px;
  }

  .stat-value {
    font-size: 28px;
    font-weight: 700;
    color: #303133;
  }

  .stat-icon {
    width: 56px;
    height: 56px;
    border-radius: 12px;
    display: flex;
    align-items: center;
    justify-content: center;
  }
}
</style>
