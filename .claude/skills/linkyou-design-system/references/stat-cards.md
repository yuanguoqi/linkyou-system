# 统计卡片模板

适用于仪表盘或概览页面的数据指标展示。

## 单个卡片模板

```vue
<script setup lang="ts">
interface StatItem {
  title: string
  value: string | number
  icon: string
  color: string        // 图标主色，如 '#6366f1'
  glow: string         // 发光色，如 'rgba(99,102,241,0.3)'
  trend?: string       // 趋势文字，如 '+12.5%'
  trendUp?: boolean    // true=上涨绿色, false=下跌红色
}

defineProps<{ stat: StatItem }>()
</script>

<template>
  <div class="stat-card">
    <div
      class="stat-card-bg"
      :style="{ background: `radial-gradient(circle at top right, ${stat.glow}, transparent 60%)` }"
    />
    <div class="stat-content">
      <div
        class="stat-icon"
        :style="{ '--icon-color': stat.color, '--icon-glow': stat.glow }"
      >
        <el-icon :size="22"><component :is="stat.icon" /></el-icon>
      </div>
      <div class="stat-info">
        <div class="stat-value">{{ stat.value }}</div>
        <div class="stat-title">{{ stat.title }}</div>
      </div>
    </div>
    <div v-if="stat.trend" class="stat-footer">
      <span :class="['stat-trend', stat.trendUp ? 'up' : 'down']">
        <el-icon :size="11"><component :is="stat.trendUp ? 'ArrowUp' : 'ArrowDown'" /></el-icon>
        {{ stat.trend }}
      </span>
      <span class="stat-period">较上月</span>
    </div>
  </div>
</template>

<style scoped lang="scss">
.stat-card {
  position: relative;
  border-radius: 14px;
  padding: 20px;
  background: #13151f;
  border: 1px solid rgba(255, 255, 255, 0.06);
  overflow: hidden;
  transition: all 0.25s;
  cursor: default;

  &:hover {
    transform: translateY(-2px);
    border-color: rgba(99, 102, 241, 0.2);
    box-shadow: 0 8px 30px rgba(0, 0, 0, 0.3);
  }

  .stat-card-bg {
    position: absolute;
    inset: 0;
    pointer-events: none;
  }

  .stat-content {
    position: relative;
    display: flex;
    align-items: center;
    gap: 16px;
    margin-bottom: 16px;
  }

  .stat-icon {
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
    color: #f1f5f9;
    line-height: 1;
    margin-bottom: 4px;
    letter-spacing: -1px;
  }

  .stat-title {
    font-size: 12.5px;
    color: #64748b;
  }

  .stat-footer {
    position: relative;
    display: flex;
    align-items: center;
    justify-content: space-between;
    padding-top: 12px;
    border-top: 1px solid rgba(255, 255, 255, 0.05);
  }

  .stat-trend {
    display: inline-flex;
    align-items: center;
    gap: 3px;
    font-size: 12px;
    font-weight: 500;

    &.up   { color: #34d399; }
    &.down { color: #f87171; }
  }

  .stat-period {
    font-size: 11px;
    color: #475569;
  }
}
</style>
```

## 四列卡片组使用示例

```vue
<script setup lang="ts">
const stats = ref([
  {
    title: '用户总数',
    value: '1,234',
    icon: 'User',
    color: '#6366f1',
    glow: 'rgba(99,102,241,0.3)',
    trend: '+8.2%',
    trendUp: true,
  },
  {
    title: '今日订单',
    value: '328',
    icon: 'ShoppingCart',
    color: '#22d3ee',
    glow: 'rgba(34,211,238,0.3)',
    trend: '+12.5%',
    trendUp: true,
  },
  {
    title: '活跃租户',
    value: '56',
    icon: 'OfficeBuilding',
    color: '#a78bfa',
    glow: 'rgba(167,139,250,0.3)',
    trend: '-2.1%',
    trendUp: false,
  },
  {
    title: '系统告警',
    value: '3',
    icon: 'Warning',
    color: '#fbbf24',
    glow: 'rgba(251,191,36,0.3)',
  },
])
</script>

<template>
  <el-row :gutter="16">
    <el-col v-for="stat in stats" :key="stat.title" :xs="24" :sm="12" :lg="6">
      <StatCard :stat="stat" />
    </el-col>
  </el-row>
</template>
```

## 推荐图标颜色搭配

| 场景 | color | glow |
|------|-------|------|
| 用户/主要指标 | `#6366f1` | `rgba(99,102,241,0.3)` |
| 数据/流量 | `#22d3ee` | `rgba(34,211,238,0.3)` |
| 收入/金额 | `#34d399` | `rgba(52,211,153,0.3)` |
| 租户/组织 | `#a78bfa` | `rgba(167,139,250,0.3)` |
| 警告/待处理 | `#fbbf24` | `rgba(251,191,36,0.3)` |
| 错误/异常 | `#f87171` | `rgba(248,113,113,0.3)` |
