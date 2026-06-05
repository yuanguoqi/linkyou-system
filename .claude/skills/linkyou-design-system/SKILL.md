---
name: linkyou-design-system
description: |
  Linkyou.System 项目前端设计规范与组件生成器。当用户需要创建新的Vue页面、组件、表单、
  表格、弹窗、统计卡片、搜索栏等UI元素时，必须使用此skill确保风格一致。
  触发场景：
  - "创建新页面"、"写一个列表页"、"做一个管理页面"
  - "封装组件"、"写一个弹窗"、"做一个表单"
  - "设计风格怎么写"、"参考设计规范"、"保持风格一致"
  - "统计卡片"、"搜索栏"、"数据表格"
  - 任何需要输出 .vue 文件的场景，都应先查阅此skill
  只要涉及前端UI开发，都应主动调用此skill，不要依赖通用默认风格。
---

# Linkyou.System 前端设计规范

## 设计哲学

项目采用**「精炼宇宙靛蓝」**设计方向，灵感源自 Linear 的极致简约、Vercel 的鲜明对比、Stripe Dashboard 的优雅数据呈现：

- **深邃空间感** — 深蓝黑基调营造层次丰富的视觉纵深，而非平面纯黑
- **靛蓝主色调** — 精致克制的 #6366f1，用于关键交互元素，不过度渲染
- **精密玻璃态** — 适度的 backdrop-filter 模糊，保持通透感而不失可读性
- **几何和谐** — 统一的圆角节奏（8px / 10px / 12px / 14px），精确的间距系统
- **层次阴影** — 多层叠加的 box-shadow，营造真实的空间层级
- **克制动效** — 0.2s 的 cubic-bezier(0.4, 0, 0.2, 1) 缓动，目的明确的交互反馈

在写任何新组件之前，先阅读本文件了解规范，再参考 `references/` 目录中的模板代码。

---

## 字体系统

### 字体家族
```scss
// 标题 / 数字 — 几何感现代显示字体
$font-display: 'Plus Jakarta Sans', 'Inter', sans-serif;

// 正文 — 精致易读的万能字体
$font-body: 'Inter', -apple-system, BlinkMacSystemFont, 'PingFang SC', sans-serif;

// 数字 / 代码 — 精确等宽字体
$font-mono: 'JetBrains Mono', 'Inter', monospace;
```

### 字号规范
```scss
$text-xs:   11px;     // 辅助标签、角标
$text-sm:   12px;     // 次要说明
$text-base: 13px;     // 正文（核心字号）
$text-lg:   14px;     // 小标题、面板标题
$text-xl:   15px;     // 弹窗标题
$text-2xl:  20px;     // 页面标题
$text-3xl:  24px;     // 大标题、欢迎语
$text-hero: 96px;     // 错误页码
```

### 字重
```scss
$font-normal:   400;   // 正文
$font-medium:   500;   // 导航菜单、按钮、标签
$font-semibold: 600;   // 卡片标题、面板标题、表单标签
$font-bold:     700;   // 数据数字、Logo
$font-black:    800;   // 错误页码等特殊场景
```

### 字间距
```scss
$tracking-tight:  -0.5px;   // 大数字、标题
$tracking-normal: -0.2px;   // 正文标题
$tracking-base:    0;       // 正文
$tracking-wide:    1.5px;   // 英文副标题（大写场景）
```

---

## 颜色系统

### 背景层次 — 深邃空间感
```scss
// 页面背景（最底层）— 近乎纯黑的深蓝黑
$bg-page:    #080a0f;

// 卡片/面板背景（第二层）
$bg-card:    #0e1118;

// 悬浮层/弹窗内嵌（第三层）
$bg-elevated: #141722;

// 输入框/内嵌区域
$bg-input:   rgba(255, 255, 255, 0.03);

// 悬停高亮
$bg-hover:   rgba(99, 102, 241, 0.06);

// 激活状态
$bg-active:  rgba(99, 102, 241, 0.1);
```

### 主题色 — 精致靛蓝
```scss
$primary:        #6366f1;   // 主色，用于主要按钮、激活状态
$primary-light:  #818cf8;   // 主色浅色，用于悬停
$primary-pale:   #a5b4fc;   // 主色更浅，用于文字高亮、激活文字
$primary-dim:    rgba(99, 102, 241, 0.12);  // 淡化背景
$primary-glow:   rgba(99, 102, 241, 0.25);  // 发光阴影
$secondary:      #8b5cf6;   // 辅助色（渐变配色）
```

### 语义色 — 克制低饱和
```scss
$success:  #34d399;
$success-dim: rgba(52, 211, 153, 0.1);
$warning:  #fbbf24;
$warning-dim: rgba(251, 191, 36, 0.1);
$danger:   #f87171;
$danger-dim: rgba(248, 113, 113, 0.1);
$info:     #64748b;
```

### 文字层次 — 精确亮度梯度
```scss
$text-primary:   #e8ecf4;   // 主要内容文字（近白，带蓝灰感）
$text-secondary: #7b8aa8;   // 次要文字、描述
$text-muted:     #3d4a63;   // 辅助文字、占位符
$text-disabled:  #252f42;   // 禁用状态
```

### 边框 — 极细层次
```scss
$border-default:  rgba(255, 255, 255, 0.04);   // 默认（几乎不可见）
$border-subtle:   rgba(255, 255, 255, 0.07);   // 略微可见
$border-medium:   rgba(255, 255, 255, 0.1);    // 中等边框
$border-primary:  rgba(99, 102, 241, 0.25);    // 主色边框（悬停/聚焦）
$border-active:   rgba(99, 102, 241, 0.5);     // 激活边框
```

---

## 间距规范

遵循 4px 基础单位，常用值：
- **xs**: 3-4px — 图标与文字间距、小标签内边距
- **sm**: 6-8px — 列表项间距、小组件内边距
- **md**: 10-12px — 标准间距
- **lg**: 14-16px — 卡片内边距、组件间距
- **xl**: 20-24px — 区块间距、页面内边距
- **2xl**: 28-32px — 大区块分割

---

## 圆角规范

```scss
$radius-sm:   6px;    // 小元素（tab、badge）
$radius-md:   8px;    // 普通按钮、输入框、图标容器
$radius-lg:   10px;   // 弹窗输入框、操作按钮
$radius-xl:   12px;   // 卡片、面板
$radius-2xl:  14px;   // 主卡片、仪表盘面板
$radius-3xl:  16-20px; // 弹窗、登录面板
$radius-full: 9999px; // 胶囊形标签
```

---

## 动效规范

```scss
// 过渡时长
$duration-fast:   0.15s;  // 微小变化（颜色、透明度）
$duration-normal: 0.2s;   // 标准交互反馈
$duration-slow:   0.28s;  // 布局变化（侧栏折叠）

// 缓动函数
$ease-standard: cubic-bezier(0.4, 0, 0.2, 1);  // 标准（所有交互）
$ease-enter:    cubic-bezier(0.16, 1, 0.3, 1);  // 入场（弹性）
$ease-smooth:   ease;                            // 通用平滑
```

---

## 阴影规范

```scss
// 深色主题阴影
$shadow-xs:     0 1px 2px rgba(0, 0, 0, 0.4);
$shadow-sm:     0 2px 8px rgba(0, 0, 0, 0.4);
$shadow-card:   0 4px 24px rgba(0, 0, 0, 0.45), 0 0 0 1px rgba(255, 255, 255, 0.03);
$shadow-dialog: 0 32px 80px rgba(0, 0, 0, 0.65), 0 0 0 1px rgba(255, 255, 255, 0.05);
```

---

## 组件模板速查

| 组件类型 | 文件 | 说明 |
|---------|------|------|
| 标准列表页（搜索+表格+分页） | `references/page-list.md` | 最常用，CRUD 列表页完整模板 |
| 新增/编辑弹窗 | `references/dialog-form.md` | 含表单校验、loading 状态 |
| 统计卡片组 | `references/stat-cards.md` | 仪表盘数字卡片 |

---

## 布局规范

### 页面根容器

每个业务页面都需要 `.page-container` 作为根元素：

```vue
<template>
  <div class="page-container">
    <!-- 搜索区 -->
    <!-- 内容区 -->
  </div>
</template>

<style scoped lang="scss">
.page-container {
  padding: 16px;
  display: flex;
  flex-direction: column;
  gap: 12px;
  min-height: 100%;
}
</style>
```

### 卡片/面板容器

避免使用 `el-card`（Element Plus 默认是白色），改用自定义深色面板：

```vue
<div class="panel">
  <div class="panel-header">
    <span class="panel-title font-display">标题</span>
    <div class="panel-actions"><!-- 操作按钮 --></div>
  </div>
  <div class="panel-body">
    <!-- 内容 -->
  </div>
</div>
```

```scss
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

    .panel-title {
      font-family: 'Plus Jakarta Sans', 'Inter', sans-serif;
      font-size: 13.5px;
      font-weight: 600;
      color: var(--text-secondary);
      letter-spacing: -0.2px;
    }
  }

  .panel-body {
    padding: 16px;
  }
}
```

---

## Element Plus 深色覆盖

由于 Element Plus 默认是浅色主题，需要在组件的 `<style scoped>` 中对常用组件进行覆盖：

### Table 深色覆盖
```scss
:deep(.el-table) {
  background: transparent;
  color: var(--text-primary);

  th.el-table__cell {
    background: rgba(255, 255, 255, 0.02);
    color: var(--text-secondary);
    font-weight: 500;
    font-size: 12px;
    border-bottom: 1px solid var(--border-default);
  }

  td.el-table__cell {
    border-bottom: 1px solid var(--border-default);
    font-size: 13px;
  }

  tr:hover > td {
    background: var(--bg-hover) !important;
  }

  .el-table__empty-block {
    background: transparent;
    color: var(--text-disabled);
  }
}
```

### 分页深色覆盖
```scss
:deep(.el-pagination) {
  --el-pagination-bg-color: transparent;
  --el-pagination-text-color: var(--text-muted);
  --el-pagination-border-radius: 6px;
  justify-content: flex-end;
  margin-top: 12px;

  .el-pager li {
    background: var(--bg-input);
    border: 1px solid var(--border-default);
    color: var(--text-muted);
    border-radius: 6px;
    font-family: 'JetBrains Mono', monospace;
    font-size: 12px;

    &.is-active {
      background: var(--primary);
      border-color: var(--primary);
      color: #fff;
    }

    &:hover:not(.is-active) {
      background: var(--primary-dim);
      color: var(--primary-pale);
    }
  }

  button {
    background: var(--bg-input);
    border: 1px solid var(--border-default);
    color: var(--text-muted);
    border-radius: 6px;
    &:hover { color: var(--primary-pale); }
    &:disabled { opacity: 0.3; }
  }
}
```

### Input 深色覆盖
```scss
:deep(.el-input) {
  --el-input-bg-color: var(--bg-input);
  --el-input-border-color: var(--border-subtle);
  --el-input-text-color: var(--text-primary);
  --el-input-placeholder-color: var(--text-muted);
  --el-input-focus-border-color: var(--primary);

  .el-input__wrapper {
    border-radius: 8px;
    box-shadow: 0 0 0 1px var(--border-subtle) inset;
    transition: box-shadow 0.2s;

    &:hover { box-shadow: 0 0 0 1px rgba(99, 102, 241, 0.25) inset; }
    &.is-focus { box-shadow: 0 0 0 1px var(--primary) inset, 0 0 0 3px rgba(99, 102, 241, 0.08); }
  }
}
```

### Select 深色覆盖
```scss
:deep(.el-select) {
  --el-select-input-color: var(--text-primary);

  .el-select__wrapper {
    background: var(--bg-input);
    border-color: var(--border-subtle);
    border-radius: 8px;
    box-shadow: 0 0 0 1px var(--border-subtle) inset;

    &:hover { box-shadow: 0 0 0 1px rgba(99, 102, 241, 0.25) inset; }
  }
}
```

### Button 深色样式
```scss
// 主要按钮（渐变靛蓝）
.btn-primary {
  background: linear-gradient(135deg, #4f46e5, #6366f1);
  border: none;
  color: #fff;
  border-radius: 8px;
  box-shadow: 0 4px 12px rgba(79, 70, 229, 0.25);
  font-weight: 500;
  font-size: 13px;
  transition: all 0.2s cubic-bezier(0.4, 0, 0.2, 1);

  &:hover {
    transform: translateY(-1px);
    box-shadow: 0 6px 16px rgba(79, 70, 229, 0.35);
  }
}

// 幽灵按钮（边框）
.btn-ghost {
  background: transparent;
  border: 1px solid var(--border-subtle);
  color: var(--text-secondary);
  border-radius: 8px;
  font-size: 13px;
  transition: all 0.2s;

  &:hover {
    background: var(--primary-dim);
    border-color: var(--border-primary);
    color: var(--primary-pale);
  }
}
```

---

## 视觉效果工具库

### 噪点纹理（Noise/Grain）
用于增加画面深度和质感，适用于大面积背景区域：
```html
<div class="noise-overlay" />
```
CSS 类 `.noise-overlay` 已在全局样式中定义，使用 SVG feTurbulence 滤镜生成噪点。

### 动画渐变边框
用于卡片、登录面板等需要视觉焦点的容器：
```scss
.card-glow {
  position: absolute;
  inset: -1px;
  border-radius: inherit;
  z-index: -1;
  background: conic-gradient(from 0deg, ...);
  animation: glow-rotate 8s linear infinite;
  opacity: 0.7;
}
```

### 交错入场动画
用于表单元素的级联出现效果：
```html
<div class="stagger-item" style="--i:0">...</div>
<div class="stagger-item" style="--i:1">...</div>
```
```scss
.stagger-item {
  animation: stagger-in 0.4s cubic-bezier(0.16, 1, 0.3, 1) both;
  animation-delay: calc(0.08s * var(--i, 0) + 0.3s);
}
```

### 几何网格图案
用于装饰面板的背景纹理：
```scss
.card-left-mesh {
  &::before {
    background-image:
      linear-gradient(rgba(99, 102, 241, 0.04) 1px, transparent 1px),
      linear-gradient(90deg, rgba(99, 102, 241, 0.04) 1px, transparent 1px);
    background-size: 24px 24px;
    mask-image: linear-gradient(180deg, transparent 0%, black 30%, black 70%, transparent 100%);
  }
}
```

### 动画渐变背景
用于欢迎横幅等需要视觉动感的区域：
```scss
.welcome-gradient {
  background: linear-gradient(135deg, ...);
  background-size: 200% 200%;
  animation: gradient-shift 8s ease-in-out infinite;
}
```

---

## 设计原则清单

1. **字体一致性** — 标题用 Plus Jakarta Sans（`font-display`），数字用 JetBrains Mono（`tabular`），正文用 Inter
2. **不用 el-card** — 改用自定义 `.panel` 结构保持深色风格
3. **覆盖 Element Plus** — 所有 EP 组件都要用 `:deep()` 覆盖为深色样式
4. **统一 SCSS 结构** — 每个组件底部都有 `<style scoped lang="scss">`，使用 `:deep()` 而非全局样式
5. **hover 状态必须** — 任何可点击元素都要有 hover 反馈（颜色变化 + 可选 translateY）
6. **发光效果克制** — 只在主要交互元素（主按钮、激活菜单、用户头像）上使用 box-shadow 发光
7. **圆角节奏** — 遵循 8/10/12/14px 的圆角节奏，不混用
8. **间距精确** — 使用 4px 倍数的间距系统，保持视觉秩序
9. **动效统一** — 所有交互使用 `cubic-bezier(0.4, 0, 0.2, 1)` 缓动，时长 0.2s
10. **CSS 变量优先** — 所有颜色、阴影、边框使用 CSS 变量，确保主题切换一致性
11. **噪点纹理** — 在大面积深色背景上使用 `.noise-overlay` 增加质感
12. **交错动画** — 表单元素使用 `stagger-item` 实现级联入场效果
