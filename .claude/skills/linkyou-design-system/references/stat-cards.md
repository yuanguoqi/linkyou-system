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
  glow: string         // 发光色，如 'rgba(99,102,241,0.25)'
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
        <el-icon :size="20"><component :is="stat.icon" /></el-icon>
      </div>
      <div class="stat-info">
        <div class="stat-value tabular">{{ stat.value }}</div>
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
  border-radius: 12px;
  padding: 16px;
  background: var(--bg-card);
  border: 1px solid var(--border-default);
  overflow: hidden;
  transition: all 0.25s cubic-bezier(0.4, 0, 0.2, 1), background 0.3s;
  cursor: default;

  &:hover {
    transform: translateY(-1px);
    border-color: var(--border-primary);
    box-shadow: var(--shadow-card);
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

  .stat-icon {
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
  }

  .stat-title {
    font-size: 12px;
    color: var(--text-muted);
  }

  .stat-footer {
    position: relative;
    display: flex;
    align-items: center;
    justify-content: space-between;
    padding-top: 10px;
    border-top: 1px solid var(--border-default);
  }

  .stat-trend {
    display: inline-flex;
    align-items: center;
    gap: 3px;
    font-size: 11px;
    font-weight: 500;

    &.up   { color: #34d399; }
    &.down { color: #f87171; }
  }

  .stat-period {
    font-size: 10.5px;
    color: var(--text-disabled);
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
    glow: 'rgba(99,102,241,0.25)',
    trend: '+8.2%',
    trendUp: true,
  },
  {
    title: '今日订单',
    value: '328',
    icon: 'ShoppingCart',
    color: '#06b6d4',
    glow: 'rgba(6,182,212,0.25)',
    trend: '+12.5%',
    trendUp: true,
  },
  {
    title: '活跃租户',
    value: '56',
    icon: 'OfficeBuilding',
    color: '#8b5cf6',
    glow: 'rgba(139,92,246,0.25)',
    trend: '-2.1%',
    trendUp: false,
  },
  {
    title: '系统告警',
    value: '3',
    icon: 'Warning',
    color: '#fbbf24',
    glow: 'rgba(251,191,36,0.25)',
  },
])
</script>

<template>
  <el-row :gutter="12">
    <el-col v-for="stat in stats" :key="stat.title" :xs="24" :sm="12" :lg="6">
      <StatCard :stat="stat" />
    </el-col>
  </el-row>
</template>
```

## 推荐图标颜色搭配

| 场景 | color | glow |
|------|-------|------|
| 用户/主要指标 | `#6366f1` | `rgba(99,102,241,0.25)` |
| 数据/流量 | `#06b6d4` | `rgba(6,182,212,0.25)` |
| 收入/金额 | `#34d399` | `rgba(52,211,153,0.25)` |
| 租户/组织 | `#8b5cf6` | `rgba(139,92,246,0.25)` |
| 警告/待处理 | `#fbbf24` | `rgba(251,191,36,0.25)` |
| 错误/异常 | `#f87171` | `rgba(248,113,113,0.25)` |
