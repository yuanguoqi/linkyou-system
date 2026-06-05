<script setup lang="ts">
import { ref } from 'vue'
import { useAuthStore } from '@/stores/auth'

const authStore = useAuthStore()

const stats = ref([
  { title: '用户总数', value: '—', icon: 'User', color: '#6366f1', glow: 'rgba(99,102,241,0.25)' },
  { title: '角色总数', value: '—', icon: 'UserFilled', color: '#06b6d4', glow: 'rgba(6,182,212,0.25)' },
  { title: '租户总数', value: '—', icon: 'OfficeBuilding', color: '#8b5cf6', glow: 'rgba(139,92,246,0.25)' },
  { title: '今日登录', value: '—', icon: 'DataLine', color: '#34d399', glow: 'rgba(52,211,153,0.25)' },
])

const now = new Date()
const greeting = now.getHours() < 12 ? '早上好' : now.getHours() < 18 ? '下午好' : '晚上好'
</script>

<template>
  <div class="dashboard">
    <!-- 欢迎横幅 -->
    <div class="welcome-card">
      <div class="welcome-bg" />
      <div class="welcome-gradient" />
      <div class="welcome-grid" />
      <div class="noise-overlay" />
      <div class="welcome-content">
        <div class="welcome-text">
          <div class="greeting-tag">
            <span class="greeting-dot" />
            {{ greeting }}，欢迎回来
          </div>
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
    <el-row :gutter="12" class="stat-row">
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
              <el-icon :size="20"><component :is="stat.icon" /></el-icon>
            </div>
            <div class="stat-info">
              <div class="stat-value tabular">{{ stat.value }}</div>
              <div class="stat-title">{{ stat.title }}</div>
            </div>
          </div>
          <div class="stat-footer">
            <span class="stat-trend">
              <el-icon :size="11"><ArrowUp /></el-icon>
              暂无数据
            </span>
          </div>
        </div>
      </el-col>
    </el-row>

    <!-- 下方内容区 -->
    <el-row :gutter="12">
      <el-col :xs="24" :lg="16">
        <div class="panel">
          <div class="panel-header">
            <span class="panel-title font-display">系统概览</span>
            <span class="panel-badge">实时</span>
          </div>
          <div class="panel-empty">
            <el-icon :size="36" class="empty-icon"><DataLine /></el-icon>
            <p>暂无图表数据</p>
          </div>
        </div>
      </el-col>
      <el-col :xs="24" :lg="8">
        <div class="panel">
          <div class="panel-header">
            <span class="panel-title font-display">最近活动</span>
          </div>
          <div class="panel-empty">
            <el-icon :size="36" class="empty-icon"><Bell /></el-icon>
            <p>暂无活动记录</p>
          </div>
        </div>
      </el-col>
    </el-row>
  </div>
</template>

<style scoped lang="scss">
.dashboard {
  padding: 16px;
  display: flex;
  flex-direction: column;
  gap: 12px;
  min-height: 100%;
}

// ─── Welcome Banner ────────────────────────────────────
.welcome-card {
  --wc-bg-from: #141722;
  --wc-bg-mid: #10131c;
  --wc-bg-to: #0c0e14;
  --wc-border: rgba(99, 102, 241, 0.12);
  --wc-greeting-bg: rgba(99, 102, 241, 0.1);
  --wc-greeting-border: rgba(99, 102, 241, 0.15);
  --wc-grid-opacity: 0.03;
  --wc-glow-1: rgba(99, 102, 241, 0.1);
  --wc-glow-2: rgba(139, 92, 246, 0.06);

  .layout-wrapper.light & {
    --wc-bg-from: #eef0f8;
    --wc-bg-mid: #e8eaf4;
    --wc-bg-to: #e2e5f0;
    --wc-border: rgba(99, 102, 241, 0.15);
    --wc-greeting-bg: rgba(99, 102, 241, 0.08);
    --wc-greeting-border: rgba(99, 102, 241, 0.18);
    --wc-grid-opacity: 0.04;
    --wc-glow-1: rgba(99, 102, 241, 0.08);
    --wc-glow-2: rgba(139, 92, 246, 0.05);
  }

  position: relative;
  border-radius: 14px;
  padding: 28px 32px;
  background: linear-gradient(135deg, var(--wc-bg-from) 0%, var(--wc-bg-mid) 50%, var(--wc-bg-to) 100%);
  border: 1px solid var(--wc-border);
  overflow: hidden;
  transition: background 0.3s, border-color 0.3s;

  .welcome-bg {
    position: absolute;
    inset: 0;
    background:
      radial-gradient(ellipse at 15% 50%, var(--wc-glow-1) 0%, transparent 50%),
      radial-gradient(ellipse at 85% 20%, var(--wc-glow-2) 0%, transparent 40%);
    pointer-events: none;
    transition: background 0.3s;
  }

  // Animated gradient overlay
  .welcome-gradient {
    position: absolute;
    inset: 0;
    pointer-events: none;
    background: linear-gradient(
      135deg,
      rgba(99, 102, 241, 0.06) 0%,
      transparent 30%,
      rgba(139, 92, 246, 0.04) 50%,
      transparent 70%,
      rgba(99, 102, 241, 0.05) 100%
    );
    background-size: 200% 200%;
    animation: gradient-shift 8s ease-in-out infinite;
  }

  @keyframes gradient-shift {
    0%, 100% { background-position: 0% 50%; }
    50% { background-position: 100% 50%; }
  }

  .welcome-grid {
    position: absolute;
    inset: 0;
    background-image:
      linear-gradient(rgba(99, 102, 241, var(--wc-grid-opacity)) 1px, transparent 1px),
      linear-gradient(90deg, rgba(99, 102, 241, var(--wc-grid-opacity)) 1px, transparent 1px);
    background-size: 32px 32px;
    pointer-events: none;
    mask-image: radial-gradient(ellipse at 70% 50%, black 0%, transparent 70%);
    -webkit-mask-image: radial-gradient(ellipse at 70% 50%, black 0%, transparent 70%);
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
      gap: 6px;
      padding: 4px 12px 4px 8px;
      background: var(--wc-greeting-bg);
      border: 1px solid var(--wc-greeting-border);
      border-radius: 20px;
      font-size: 12px;
      color: var(--primary-pale);
      margin-bottom: 12px;
      transition: background 0.3s, border-color 0.3s;

      .greeting-dot {
        width: 5px;
        height: 5px;
        border-radius: 50%;
        background: var(--success);
        box-shadow: 0 0 6px rgba(52, 211, 153, 0.5);
      }
    }

    .user-name {
      font-family: 'Plus Jakarta Sans', 'Inter', sans-serif;
      font-size: 24px;
      font-weight: 700;
      color: var(--text-primary);
      margin: 0 0 6px;
      letter-spacing: -0.5px;
    }

    .sub-text {
      font-size: 13px;
      color: var(--text-secondary);
      margin: 0;
    }
  }

  .welcome-illustration {
    --orbit-1: rgba(99,102,241,0.2);
    --orbit-2: rgba(139,92,246,0.15);
    --orbit-3: rgba(167,139,250,0.1);
    --center-glow: rgba(99, 102, 241, 0.6);

    .layout-wrapper.light & {
      --orbit-1: rgba(99,102,241,0.25);
      --orbit-2: rgba(139,92,246,0.2);
      --orbit-3: rgba(167,139,250,0.15);
      --center-glow: rgba(99, 102, 241, 0.4);
    }

    position: relative;
    width: 90px;
    height: 90px;
    display: flex;
    align-items: center;
    justify-content: center;

    @media (max-width: 640px) { display: none; }

    .orbit-ring {
      position: absolute;
      border-radius: 50%;
      border: 1px solid;
      animation: orbit-spin linear infinite;

      &.ring-1 { width: 80px; height: 80px; border-color: var(--orbit-1); animation-duration: 10s; }
      &.ring-2 { width: 56px; height: 56px; border-color: var(--orbit-2); animation-duration: 7s; animation-direction: reverse; }
      &.ring-3 { width: 32px; height: 32px; border-color: var(--orbit-3); animation-duration: 4s; }
    }

    .center-dot {
      width: 10px;
      height: 10px;
      border-radius: 50%;
      background: linear-gradient(135deg, #6366f1, #8b5cf6);
      box-shadow: 0 0 12px var(--center-glow);
    }
  }
}

@keyframes orbit-spin {
  from { transform: rotate(0deg); }
  to   { transform: rotate(360deg); }
}

// ─── Stat Cards ────────────────────────────────────────
.stat-row { margin: 0 !important; }

.stat-card {
  position: relative;
  border-radius: 12px;
  padding: 16px;
  background: var(--bg-card);
  border: 1px solid var(--border-default);
  overflow: hidden;
  transition: all 0.25s cubic-bezier(0.4, 0, 0.2, 1), background 0.3s;
  cursor: default;

  &:hover {
    transform: translateY(-2px);
    border-color: var(--border-primary);
    box-shadow: var(--shadow-card),
                0 0 0 1px var(--border-primary),
                0 0 16px var(--icon-glow, var(--primary-glow));
  }

  .stat-card-bg {
    position: absolute;
    inset: 0;
    pointer-events: none;
    opacity: 0.6;
    transition: opacity 0.3s;

    .stat-card:hover & { opacity: 1; }
  }

  .stat-content {
    position: relative;
    display: flex;
    align-items: center;
    gap: 12px;
    margin-bottom: 12px;
  }

  .stat-icon-wrap {
    width: 40px;
    height: 40px;
    border-radius: 10px;
    display: flex;
    align-items: center;
    justify-content: center;
    background: color-mix(in srgb, var(--icon-color) 10%, transparent);
    border: 1px solid color-mix(in srgb, var(--icon-color) 15%, transparent);
    color: var(--icon-color);
    box-shadow: 0 0 12px var(--icon-glow);
    flex-shrink: 0;
    transition: transform 0.25s;

    .stat-card:hover & { transform: scale(1.05); }
  }

  .stat-value {
    font-family: 'JetBrains Mono', 'Plus Jakarta Sans', monospace;
    font-size: 24px;
    font-weight: 600;
    color: var(--text-primary);
    line-height: 1;
    margin-bottom: 2px;
    letter-spacing: -0.5px;
    transition: color 0.3s;
  }

  .stat-title {
    font-size: 12px;
    color: var(--text-muted);
    transition: color 0.3s;
  }

  .stat-footer {
    position: relative;
    padding-top: 10px;
    border-top: 1px solid var(--border-default);
    transition: border-color 0.3s;
  }

  .stat-trend {
    display: inline-flex;
    align-items: center;
    gap: 3px;
    font-size: 11px;
    color: var(--text-disabled);
    transition: color 0.3s;
  }
}

// ─── Panels ────────────────────────────────────────────
.panel {
  background: var(--bg-card);
  border: 1px solid var(--border-default);
  border-radius: 12px;
  overflow: hidden;
  transition: background 0.3s, border-color 0.3s;

  .panel-header {
    display: flex;
    align-items: center;
    justify-content: space-between;
    padding: 14px 16px;
    border-bottom: 1px solid var(--border-default);
    transition: border-color 0.3s;

    .panel-title {
      font-family: 'Plus Jakarta Sans', 'Inter', sans-serif;
      font-size: 13.5px;
      font-weight: 600;
      color: var(--text-secondary);
      letter-spacing: -0.2px;
      transition: color 0.3s;
    }

    .panel-badge {
      font-size: 10.5px;
      font-weight: 500;
      padding: 2px 8px;
      background: var(--success-dim);
      color: var(--success);
      border-radius: 20px;
      border: 1px solid rgba(52, 211, 153, 0.15);
    }
  }

  .panel-empty {
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    padding: 40px 16px;
    gap: 10px;
    font-size: 12.5px;
    color: var(--text-disabled);
    transition: color 0.3s;

    p { margin: 0; }

    .empty-icon { color: var(--border-subtle); }
  }
}
</style>
