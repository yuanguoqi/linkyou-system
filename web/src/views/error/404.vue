<script setup lang="ts">
import { useRouter } from 'vue-router'

const router = useRouter()
</script>

<template>
  <div class="error-page">
    <div class="error-bg" />
    <div class="error-content">
      <div class="error-code font-display">404</div>
      <div class="error-title font-display">页面不存在</div>
      <div class="error-desc">抱歉，您访问的页面不存在或已被删除</div>
      <el-button type="primary" class="back-btn" @click="router.push('/')">
        返回首页
      </el-button>
    </div>
  </div>
</template>

<style scoped lang="scss">
.error-page {
  min-height: 100vh;
  display: flex;
  align-items: center;
  justify-content: center;
  background: var(--bg-page);
  position: relative;
  overflow: hidden;
}

.error-bg {
  position: absolute;
  inset: 0;
  background:
    radial-gradient(ellipse at 50% 30%, rgba(99, 102, 241, 0.06) 0%, transparent 60%);
  pointer-events: none;
}

.error-content {
  text-align: center;
  position: relative;
  z-index: 1;
  animation: error-in 0.5s cubic-bezier(0.16, 1, 0.3, 1);
}

@keyframes error-in {
  from { opacity: 0; transform: translateY(16px); }
  to   { opacity: 1; transform: translateY(0); }
}

.error-code {
  font-family: 'JetBrains Mono', 'Plus Jakarta Sans', monospace;
  font-size: 96px;
  font-weight: 800;
  color: var(--text-disabled);
  line-height: 1;
  margin-bottom: 12px;
  letter-spacing: -4px;
  background: linear-gradient(180deg, var(--text-muted) 0%, var(--text-disabled) 100%);
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
  background-clip: text;
}

.error-title {
  font-family: 'Plus Jakarta Sans', 'Inter', sans-serif;
  font-size: 20px;
  font-weight: 600;
  color: var(--text-primary);
  margin-bottom: 6px;
  letter-spacing: -0.3px;
}

.error-desc {
  font-size: 13px;
  color: var(--text-secondary);
  margin-bottom: 24px;
}

.back-btn {
  height: 36px;
  padding: 0 20px;
  font-size: 13px;
  font-weight: 500;
  border-radius: 8px;
  background: linear-gradient(135deg, #4f46e5, #6366f1) !important;
  border: none;
  box-shadow: 0 4px 12px rgba(79, 70, 229, 0.3);
  transition: transform 0.2s, box-shadow 0.2s;

  &:hover {
    transform: translateY(-1px);
    box-shadow: 0 6px 16px rgba(79, 70, 229, 0.4);
  }
}
</style>
