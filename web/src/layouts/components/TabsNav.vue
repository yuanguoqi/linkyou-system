<script setup lang="ts">
import { computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useAppStore } from '@/stores/app'

const route = useRoute()
const router = useRouter()
const appStore = useAppStore()

const tabs = computed(() => appStore.visitedTabs)
const activeTab = computed(() => route.fullPath)

function handleTabClick(path: string) {
  router.push(path)
}

function handleTabRemove(path: string) {
  const idx = tabs.value.findIndex((t) => t.path === path)
  appStore.removeTab(path)

  // 如果关闭的是当前激活 tab，跳转到前一个
  if (path === route.fullPath) {
    const nextTab = tabs.value[idx - 1] || tabs.value[idx]
    if (nextTab) router.push(nextTab.path)
  }
}
</script>

<template>
  <div class="tabs-nav">
    <el-scrollbar>
      <div class="tabs-inner">
        <span
          v-for="tab in tabs"
          :key="tab.path"
          :class="['tab-item', { active: tab.path === activeTab }]"
          @click="handleTabClick(tab.path)"
        >
          {{ tab.title }}
          <el-icon
            v-if="!tab.affix"
            class="tab-close"
            @click.stop="handleTabRemove(tab.path)"
          >
            <Close />
          </el-icon>
        </span>
      </div>
    </el-scrollbar>
  </div>
</template>

<style scoped lang="scss">
.tabs-nav {
  background: #fff;
  border-bottom: 1px solid #e8e8e8;
  padding: 4px 8px 0;
  height: 36px;
  flex-shrink: 0;
}

.tabs-inner {
  display: flex;
  gap: 4px;
}

.tab-item {
  display: inline-flex;
  align-items: center;
  gap: 4px;
  padding: 4px 10px;
  font-size: 13px;
  border-radius: 4px 4px 0 0;
  border: 1px solid #e8e8e8;
  border-bottom: none;
  cursor: pointer;
  color: #606266;
  background: #f5f7fa;
  white-space: nowrap;
  transition: all 0.2s;

  &:hover {
    color: #409eff;
    background: #ecf5ff;
  }

  &.active {
    color: #409eff;
    background: #fff;
    border-color: #e8e8e8;
  }

  .tab-close {
    font-size: 12px;
    border-radius: 50%;
    &:hover {
      background: #c0c4cc;
      color: #fff;
    }
  }
}
</style>
