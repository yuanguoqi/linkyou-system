<script setup lang="ts">
import { ref } from 'vue'
import { useAuthStore } from '@/stores/auth'

const authStore = useAuthStore()

const stats = ref([
  { title: '用户总数', value: '—', icon: 'User', color: '#6366f1', glow: 'rgba(99,102,241,0.3)' },
  { title: '角色总数', value: '—', icon: 'UserFilled', color: '#22d3ee', glow: 'rgba(34,211,238,0.3)' },
  { title: '租户总数', value: '—', icon: 'OfficeBuilding', color: '#a78bfa', glow: 'rgba(167,139,250,0.3)' },
  { title: '今日登录', value: '—', icon: 'DataLine', color: '#34d399', glow: 'rgba(52,211,153,0.3)' },
])

const now = new Date()
const greeting = now.getHours() < 12 ? '早上好' : now.getHours() < 18 ? '下午好' : '晚上好'
</script>

<template>
  <div class="dashboard">
    <!-- 欢迎横幅 -->
    <div class="welcome-card">
      <div class="welcome-bg" />
      <div class="welcome-content">
        <div class="welcome-text">
          <div class="greeting-tag">{{ greeting }}，欢迎回来</div>
          <h2 class="user-name">{{ authStore.userDisplayName }}</h2>
          <p class="sub-text">系统运行正常，今天也是高效工作的一天</p>
        </div>
        <div class="welcome-illustration">
          <div class="orbit-ring ring-1" />
          <div class="orbit-ring ring-2" />
          <div class="orbit-ring ring-3" />
          <div class="center-dot" />
        </div>
      </div>
    </div>

    <!-- 统计卡片 -->
    <el-row :gutter="16" class="stat-row">
      <el-col
        v-for="stat in stats"
        :key="stat.title"
        :xs="24" :sm="12" :lg="6"
      >
        <div class="stat-card">
          <div
            class="stat-card-bg"
            :style="{ background: `radial-gradient(circle at top right, ${stat.glow}, transparent 60%)` }"
          />
          <div class="stat-content">
            <div
              class="stat-icon-wrap"
              :style="{ '--icon-color': stat.color, '--icon-glow': stat.glow }"
            >
              <el-icon :size="22"><component :is="stat.icon" /></el-icon>
            </div>
            <div class="stat-info">
              <div class="stat-value">{{ stat.value }}</div>
              <div class="stat-title">{{ stat.title }}</div>
            </div>
          </div>
          <div class="stat-footer">
            <span class="stat-trend">
              <el-icon :size="12"><ArrowUp /></el-icon>
              暂无数据
            </span>
          </div>
        </div>
      </el-col>
    </el-row>

    <!-- 下方内容区 -->
    <el-row :gutter="16">
      <el-col :xs="24" :lg="16">
        <div class="panel">
          <div class="panel-header">
            <span class="panel-title">系统概览</span>
            <span class="panel-badge">实时</span>
          </div>
          <div class="panel-empty">
            <el-icon :size="40" class="empty-icon"><DataLine /></el-icon>
            <p>暂无图表数据</p>
          </div>
        </div>
      </el-col>
      <el-col :xs="24" :lg="8">
        <div class="panel">
          <div class="panel-header">
            <span class="panel-title">最近活动</span>
          </div>
          <div class="panel-empty">
            <el-icon :size="40" class="empty-icon"><Bell /></el-icon>
            <p>暂无活动记录</p>
          </div>
        </div>
      </el-col>
    </el-row>
  </div>
</template>

<style scoped lang="scss">
.dashboard {
  padding: 20px;
  display: flex;
  flex-direction: column;
  gap: 16px;
  min-height: 100%;
}

// 欢迎横幅
.welcome-card {
  position: relative;
  border-radius: 16px;
  padding: 32px 36px;
  background: linear-gradient(135deg, #1e1b4b 0%, #1a1035 50%, #0f172a 100%);
  border: 1px solid rgba(99, 102, 241, 0.2);
  overflow: hidden;

  // 浅色模式下调整欢迎横幅
  :root:not(.dark) & {
    background: linear-gradient(135deg, #ede9fe 0%, #ddd6fe 50%, #c7d2fe 100%);
    border-color: rgba(99, 102, 241, 0.25);
  }

  .welcome-bg {
    position: absolute;
    inset: 0;
    background:
      radial-gradient(ellipse at 20% 50%, rgba(99, 102, 241, 0.15) 0%, transparent 50%),
      radial-gradient(ellipse at 80% 20%, rgba(139, 92, 246, 0.1) 0%, transparent 40%);
  }

  .welcome-content {
    position: relative;
    display: flex;
    justify-content: space-between;
    align-items: center;
  }

  .welcome-text {
    .greeting-tag {
      display: inline-flex;
      align-items: center;
      padding: 4px 12px;
      background: rgba(99, 102, 241, 0.2);
      border: 1px solid rgba(99, 102, 241, 0.3);
      border-radius: 20px;
      font-size: 12px;
      color: #a5b4fc;
      margin-bottom: 12px;
    }

    .user-name {
      font-size: 26px;
      font-weight: 700;
      color: #f1f5f9;
      margin: 0 0 8px;
      letter-spacing: -0.5px;
    }

    .sub-text {
      font-size: 13px;
      color: rgba(255, 255, 255, 0.55);
      margin: 0;
    }
  }

  .welcome-illustration {
    position: relative;
    width: 100px;
    height: 100px;
    display: flex;
    align-items: center;
    justify-content: center;

    @media (max-width: 640px) { display: none; }

    .orbit-ring {
      position: absolute;
      border-radius: 50%;
      border: 1px solid;
      animation: orbit-spin linear infinite;

      &.ring-1 { width: 90px; height: 90px; border-color: rgba(99,102,241,0.3); animation-duration: 8s; }
      &.ring-2 { width: 65px; height: 65px; border-color: rgba(139,92,246,0.25); animation-duration: 5s; animation-direction: reverse; }
      &.ring-3 { width: 40px; height: 40px; border-color: rgba(167,139,250,0.2); animation-duration: 3s; }
    }

    .center-dot {
      width: 14px;
      height: 14px;
      border-radius: 50%;
      background: linear-gradient(135deg, #6366f1, #8b5cf6);
      box-shadow: 0 0 16px rgba(99, 102, 241, 0.7);
    }
  }
}

@keyframes orbit-spin {
  from { transform: rotate(0deg); }
  to   { transform: rotate(360deg); }
}

// 统计卡片
.stat-row { margin: 0 !important; }

.stat-card {
  position: relative;
  border-radius: 14px;
  padding: 20px;
  background: var(--bg-card);
  border: 1px solid var(--border-default);
  overflow: hidden;
  transition: all 0.25s, background 0.3s;
  cursor: default;

  &:hover {
    transform: translateY(-2px);
    border-color: var(--border-primary);
    box-shadow: var(--shadow-card);
  }

  .stat-card-bg {
    position: absolute;
    inset: 0;
    pointer-events: none;
    opacity: 0.7;
  }

  .stat-content {
    position: relative;
    display: flex;
    align-items: center;
    gap: 16px;
    margin-bottom: 16px;
  }

  .stat-icon-wrap {
    width: 48px;
    height: 48px;
    border-radius: 12px;
    display: flex;
    align-items: center;
    justify-content: center;
    background: color-mix(in srgb, var(--icon-color) 12%, transparent);
    border: 1px solid color-mix(in srgb, var(--icon-color) 20%, transparent);
    color: var(--icon-color);
    box-shadow: 0 0 16px var(--icon-glow);
    flex-shrink: 0;
  }

  .stat-value {
    font-size: 28px;
    font-weight: 700;
    color: var(--text-primary);
    line-height: 1;
    margin-bottom: 4px;
    letter-spacing: -1px;
    transition: color 0.3s;
  }

  .stat-title {
    font-size: 12.5px;
    color: var(--text-muted);
    transition: color 0.3s;
  }

  .stat-footer {
    position: relative;
    padding-top: 12px;
    border-top: 1px solid var(--border-default);
    transition: border-color 0.3s;
  }

  .stat-trend {
    display: inline-flex;
    align-items: center;
    gap: 4px;
    font-size: 12px;
    color: var(--text-disabled);
    transition: color 0.3s;
  }
}

// 面板
.panel {
  background: var(--bg-card);
  border: 1px solid var(--border-default);
  border-radius: 14px;
  overflow: hidden;
  transition: background 0.3s, border-color 0.3s;

  .panel-header {
    display: flex;
    align-items: center;
    justify-content: space-between;
    padding: 16px 20px;
    border-bottom: 1px solid var(--border-default);
    transition: border-color 0.3s;

    .panel-title {
      font-size: 14px;
      font-weight: 600;
      color: var(--text-secondary);
      transition: color 0.3s;
    }

    .panel-badge {
      font-size: 11px;
      padding: 2px 8px;
      background: rgba(52, 211, 153, 0.12);
      color: #34d399;
      border-radius: 20px;
      border: 1px solid rgba(52, 211, 153, 0.2);
    }
  }

  .panel-empty {
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    padding: 48px 20px;
    gap: 12px;
    font-size: 13px;
    color: var(--text-disabled);
    transition: color 0.3s;

    p { margin: 0; }

    .empty-icon { color: var(--border-subtle); }
  }
}
</style>
