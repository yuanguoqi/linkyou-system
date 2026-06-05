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

项目采用**深色科技感**风格，核心理念：
- 克制的黑色基调搭配紫蓝主色，营造专业感而不压抑
- 发光/辉光效果作为视觉焦点，而非装饰堆砌
- 圆角和间距保持一致节奏，形成秩序感
- 交互反馈微妙而及时（hover、focus 状态清晰）

在写任何新组件之前，先阅读本文件了解规范，再参考 `references/` 目录中的模板代码。

---

## 颜色系统

### 背景层次
```scss
// 页面背景（最底层）
$bg-page:    #0f1117;
// 卡片/面板背景（第二层）
$bg-card:    #13151f;
// 输入框/内嵌区域（第三层）
$bg-input:   rgba(255, 255, 255, 0.04);
// 悬停高亮
$bg-hover:   rgba(99, 102, 241, 0.08);
```

### 主题色
```scss
$primary:        #6366f1;   // 主色，用于主要按钮、激活状态
$primary-light:  #818cf8;   // 主色浅色，用于悬停
$primary-pale:   #a5b4fc;   // 主色更浅，用于文字高亮、激活文字
$primary-glow:   rgba(99, 102, 241, 0.3);   // 发光阴影
$secondary:      #8b5cf6;   // 辅助色（渐变配色）
```

### 语义色
```scss
$success:  #34d399;
$warning:  #fbbf24;
$danger:   #f87171;
$info:     #94a3b8;
```

### 文字层次
```scss
$text-primary:   #e2e8f0;   // 主要内容文字
$text-secondary: #94a3b8;   // 次要文字、描述
$text-muted:     #64748b;   // 辅助文字、占位符
$text-disabled:  #334155;   // 禁用状态
```

### 边框
```scss
$border-default:  rgba(255, 255, 255, 0.06);   // 默认边框（几乎不可见）
$border-subtle:   rgba(255, 255, 255, 0.08);   // 略微可见
$border-primary:  rgba(99, 102, 241, 0.2);     // 主色边框（悬停/聚焦）
$border-active:   rgba(99, 102, 241, 0.4);     // 激活边框
```

---

## 间距规范

遵循 4px 基础单位，常用值：
- **xs**: 4px — 图标与文字间距、小标签内边距
- **sm**: 8px — 列表项间距、小组件内边距
- **md**: 12px — 标准间距
- **lg**: 16px — 卡片内边距、组件间距
- **xl**: 20-24px — 区块间距、页面内边距
- **2xl**: 32px — 大区块分割

---

## 圆角规范

```scss
$radius-sm:   6px;    // 小元素（tab、badge、小按钮）
$radius-md:   8px;    // 普通按钮、输入框、小卡片
$radius-lg:   10px;   // 弹窗输入框、中等组件
$radius-xl:   14px;   // 卡片、面板
$radius-2xl:  16-24px; // 主卡片、登录面板
$radius-full: 9999px; // 胶囊形标签
```

---

## 字体规范

```scss
// 字号
$text-xs:   11px;   // 辅助标签、角标
$text-sm:   12px;   // 次要说明
$text-base: 13-14px; // 正文
$text-lg:   15-16px; // 小标题
$text-xl:   20-22px; // 标题
$text-2xl:  26-28px; // 大标题/数字

// 字重
$font-normal:  400;
$font-medium:  500;  // 导航菜单、按钮、表头
$font-semibold: 600; // 卡片标题、面板标题
$font-bold:    700;  // 数据数字、Logo
```

---

## 动效规范

```scss
// 过渡时长
$duration-fast:   0.15s;  // 微小变化（颜色、透明度）
$duration-normal: 0.2s;   // 标准交互反馈
$duration-slow:   0.28s;  // 布局变化（侧栏折叠）

// 缓动函数
$ease-standard: cubic-bezier(0.4, 0, 0.2, 1);  // 标准（Material）
$ease-enter:    cubic-bezier(0.16, 1, 0.3, 1);  // 入场（弹性）
$ease-smooth:   ease;                            // 通用平滑
```

---

## 组件模板速查

以下是常用组件模板的位置，需要时直接读取对应文件：

| 组件类型 | 文件 | 说明 |
|---------|------|------|
| 标准列表页（搜索+表格+分页） | `references/page-list.md` | 最常用，CRUD 列表页完整模板 |
| 新增/编辑弹窗 | `references/dialog-form.md` | 含表单校验、loading 状态 |
| 统计卡片组 | `references/stat-cards.md` | 仪表盘数字卡片 |
| 通用面板/卡片 | `references/panel.md` | 通用内容容器 |
| 页面内容容器 | 见下方「布局规范」| 每个页面的顶层 wrapper |

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
  padding: 20px;
  display: flex;
  flex-direction: column;
  gap: 16px;
  min-height: 100%;
}
</style>
```

### 卡片/面板容器

避免使用 `el-card`（Element Plus 默认是白色），改用自定义深色面板：

```vue
<div class="panel">
  <div class="panel-header">
    <span class="panel-title">标题</span>
    <div class="panel-actions"><!-- 操作按钮 --></div>
  </div>
  <div class="panel-body">
    <!-- 内容 -->
  </div>
</div>
```

```scss
.panel {
  background: #13151f;
  border: 1px solid rgba(255, 255, 255, 0.06);
  border-radius: 14px;
  overflow: hidden;

  .panel-header {
    display: flex;
    align-items: center;
    justify-content: space-between;
    padding: 16px 20px;
    border-bottom: 1px solid rgba(255, 255, 255, 0.05);

    .panel-title {
      font-size: 14px;
      font-weight: 600;
      color: #cbd5e1;
    }
  }

  .panel-body {
    padding: 20px;
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
  color: #cbd5e1;

  th.el-table__cell {
    background: rgba(255, 255, 255, 0.03);
    color: #94a3b8;
    font-weight: 500;
    border-bottom: 1px solid rgba(255, 255, 255, 0.06);
  }

  td.el-table__cell {
    border-bottom: 1px solid rgba(255, 255, 255, 0.04);
  }

  tr:hover > td {
    background: rgba(99, 102, 241, 0.05) !important;
  }

  .el-table__empty-block {
    background: transparent;
    color: #475569;
  }
}
```

### 分页深色覆盖
```scss
:deep(.el-pagination) {
  --el-pagination-bg-color: transparent;
  --el-pagination-text-color: #64748b;
  --el-pagination-border-radius: 6px;
  justify-content: flex-end;
  margin-top: 16px;

  .el-pager li {
    background: rgba(255, 255, 255, 0.04);
    border: 1px solid rgba(255, 255, 255, 0.06);
    color: #64748b;
    border-radius: 6px;

    &.is-active {
      background: #6366f1;
      border-color: #6366f1;
      color: #fff;
    }

    &:hover:not(.is-active) {
      background: rgba(99, 102, 241, 0.1);
      color: #a5b4fc;
    }
  }

  button {
    background: rgba(255, 255, 255, 0.04);
    border: 1px solid rgba(255, 255, 255, 0.06);
    color: #64748b;
    border-radius: 6px;
    &:hover { color: #a5b4fc; }
    &:disabled { opacity: 0.3; }
  }
}
```

### Input 深色覆盖
```scss
:deep(.el-input) {
  --el-input-bg-color: rgba(255, 255, 255, 0.04);
  --el-input-border-color: rgba(255, 255, 255, 0.08);
  --el-input-text-color: #e2e8f0;
  --el-input-placeholder-color: #475569;
  --el-input-focus-border-color: #6366f1;

  .el-input__wrapper {
    border-radius: 8px;
    box-shadow: 0 0 0 1px rgba(255, 255, 255, 0.08) inset;

    &:hover { box-shadow: 0 0 0 1px rgba(99, 102, 241, 0.3) inset; }
    &.is-focus { box-shadow: 0 0 0 1px #6366f1 inset, 0 0 0 3px rgba(99, 102, 241, 0.12); }
  }
}
```

### Select 深色覆盖
```scss
:deep(.el-select) {
  --el-select-input-color: #e2e8f0;

  .el-select__wrapper {
    background: rgba(255, 255, 255, 0.04);
    border-color: rgba(255, 255, 255, 0.08);
    border-radius: 8px;
    box-shadow: 0 0 0 1px rgba(255, 255, 255, 0.08) inset;

    &:hover { box-shadow: 0 0 0 1px rgba(99, 102, 241, 0.3) inset; }
  }
}
```

### Button 深色样式
```scss
// 主要按钮（渐变紫）
.btn-primary {
  background: linear-gradient(135deg, #6366f1, #818cf8);
  border: none;
  color: #fff;
  border-radius: 8px;
  box-shadow: 0 4px 12px rgba(99, 102, 241, 0.3);
  transition: all 0.2s;

  &:hover {
    transform: translateY(-1px);
    box-shadow: 0 6px 18px rgba(99, 102, 241, 0.4);
  }
}

// 幽灵按钮（边框）
.btn-ghost {
  background: rgba(255, 255, 255, 0.04);
  border: 1px solid rgba(255, 255, 255, 0.08);
  color: #94a3b8;
  border-radius: 8px;

  &:hover {
    background: rgba(99, 102, 241, 0.08);
    border-color: rgba(99, 102, 241, 0.3);
    color: #a5b4fc;
  }
}
```

---

## 开发流程规范

1. **读模板先行**：创建新页面前，先读 `references/page-list.md`（列表页）或 `references/dialog-form.md`（弹窗）
2. **不用 el-card**：改用自定义 `.panel` 结构保持深色风格
3. **覆盖 Element Plus**：所有 EP 组件都要用 `:deep()` 覆盖为深色样式
4. **统一 SCSS 结构**：每个组件底部都有 `<style scoped lang="scss">`，使用 `:deep()` 而非全局样式
5. **hover 状态必须**：任何可点击元素都要有 hover 反馈（颜色变化 + 可选 translateY）
6. **发光效果克制**：只在主要交互元素（主按钮、激活菜单、用户头像）上使用 box-shadow 发光，不要滥用
