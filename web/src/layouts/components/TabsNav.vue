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
          <span class="tab-dot" />
          <span class="tab-title">{{ tab.title }}</span>
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
  background: var(--tab-bg);
  border-bottom: 1px solid var(--tab-border);
  padding: 6px 12px 0;
  height: 38px;
  flex-shrink: 0;
  transition: background 0.3s, border-color 0.3s;
}

.tabs-inner {
  display: flex;
  gap: 4px;
  align-items: flex-end;
  height: 100%;
}

.tab-item {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  padding: 4px 12px;
  font-size: 12.5px;
  border-radius: 6px 6px 0 0;
  border: 1px solid var(--tab-item-border);
  border-bottom: none;
  cursor: pointer;
  color: var(--text-muted);
  background: var(--tab-item-bg);
  white-space: nowrap;
  transition: all 0.2s;
  position: relative;

  .tab-dot {
    width: 6px;
    height: 6px;
    border-radius: 50%;
    background: var(--tab-dot);
    flex-shrink: 0;
    transition: background 0.2s;
  }

  .tab-title {
    font-weight: 500;
    transition: color 0.2s;
  }

  &:hover {
    color: var(--text-secondary);
    background: var(--bg-hover);
    border-color: var(--border-primary);

    .tab-dot { background: var(--primary-light); }
  }

  &.active {
    color: var(--primary-pale);
    background: var(--tab-active-bg);
    border-color: var(--tab-active-border);

    .tab-dot {
      background: var(--tab-dot-active);
      box-shadow: 0 0 6px var(--primary-glow);
    }

    &::after {
      content: '';
      position: absolute;
      bottom: -1px;
      left: 0;
      right: 0;
      height: 1px;
      background: var(--tab-bg);
    }
  }

  .tab-close {
    font-size: 11px;
    border-radius: 3px;
    color: var(--text-disabled);
    transition: all 0.15s;
    margin-left: 2px;

    &:hover {
      background: rgba(248, 113, 113, 0.2);
      color: #f87171;
    }
  }
}
</style>
