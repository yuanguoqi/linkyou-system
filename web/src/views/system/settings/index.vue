<script setup lang="ts">
import { ref, reactive, computed, onMounted } from 'vue'
import { ElMessage } from 'element-plus'
import { Refresh } from '@element-plus/icons-vue'
import { settingApi } from '@/api/modules/settings'
import type { SettingDto, UpdateSettingDto } from '@/api/modules/settings'

// ── State ──────────────────────────────────────────────
const loading = ref(false)
const saving = ref(false)
const settingsList = ref<SettingDto[]>([])
const settingValues = reactive<Record<string, string>>({})

// ── Lifecycle ──────────────────────────────────────────
onMounted(() => {
  fetchSettings()
})

// ── Computed: Group settings by category ───────────────
interface SettingGroup {
  category: string
  items: SettingDto[]
}

const settingGroups = computed<SettingGroup[]>(() => {
  const groups = new Map<string, SettingDto[]>()

  settingsList.value.forEach(setting => {
    // Extract category from setting name (e.g., "Abp.Mailing.SmtpHost" -> "Abp.Mailing")
    const parts = setting.name.split('.')
    const category = parts.length > 1 ? parts.slice(0, -1).join('.') : 'General'

    if (!groups.has(category)) {
      groups.set(category, [])
    }
    groups.get(category)!.push(setting)
  })

  return Array.from(groups.entries()).map(([category, items]) => ({
    category,
    items,
  }))
})

// ── Methods ────────────────────────────────────────────
async function fetchSettings() {
  loading.value = true
  try {
    const res = await settingApi.get({})
    settingsList.value = res.data

    // Populate reactive values
    res.data.forEach((setting: SettingDto) => {
      settingValues[setting.name] = setting.value ?? ''
    })
  } catch (err: unknown) {
    const axiosErr = err as { response?: { data?: { error?: { message?: string } } } }
    ElMessage.error(axiosErr?.response?.data?.error?.message || '获取系统设置失败')
  } finally {
    loading.value = false
  }
}

async function handleSave() {
  saving.value = true
  try {
    const payload: UpdateSettingDto[] = Object.entries(settingValues).map(([name, value]) => ({
      name,
      value,
    }))

    await settingApi.update(payload)
    ElMessage.success('系统设置保存成功')
  } catch (err: unknown) {
    const axiosErr = err as { response?: { data?: { error?: { message?: string } } } }
    ElMessage.error(axiosErr?.response?.data?.error?.message || '保存系统设置失败')
  } finally {
    saving.value = false
  }
}

function handleRefresh() {
  fetchSettings()
}

// ── Helpers ────────────────────────────────────────────
function formatCategoryName(category: string): string {
  // "Abp.Mailing" -> "Mailing", "Abp" -> "Abp"
  const parts = category.split('.')
  return parts[parts.length - 1]
}

function getCategoryDescription(category: string): string {
  const descriptions: Record<string, string> = {
    'Abp.Mailing': '邮件发送相关配置',
    'Abp.Security': '安全策略配置',
    'Abp.Account': '账户相关配置',
    'Abp.Identity': '身份认证配置',
    'Abp.SettingManagement': '设置管理配置',
    'Abp.TenantManagement': '租户管理配置',
  }
  return descriptions[category] || `${formatCategoryName(category)} 相关配置`
}

function isPasswordSetting(name: string): boolean {
  const lower = name.toLowerCase()
  return lower.includes('password') || lower.includes('secret') || lower.includes('key')
}

function isNumberSetting(name: string): boolean {
  const lower = name.toLowerCase()
  return lower.includes('port') || lower.includes('timeout') || lower.includes('count') || lower.includes('size')
}

function isBooleanSetting(name: string): boolean {
  const lower = name.toLowerCase()
  return lower.includes('enable') || lower.includes('is') || lower.includes('use') || lower === 'true' || lower === 'false'
}

function getBooleanValue(name: string): boolean {
  return settingValues[name]?.toLowerCase() === 'true'
}

function setBooleanValue(name: string, val: boolean) {
  settingValues[name] = val ? 'True' : 'False'
}
</script>

<template>
  <div class="page-container">
    <!-- Header Panel -->
    <div class="panel header-panel">
      <div class="panel-header">
        <div class="header-left">
          <span class="panel-title font-display">系统设置</span>
          <span class="panel-subtitle">管理系统全局配置参数</span>
        </div>
        <div class="header-actions">
          <el-button class="btn-ghost" :icon="Refresh" :loading="loading" @click="handleRefresh">
            刷新
          </el-button>
          <el-button class="btn-primary" :loading="saving" @click="handleSave">
            保存设置
          </el-button>
        </div>
      </div>
    </div>

    <!-- Loading State -->
    <div v-if="loading && settingsList.length === 0" class="panel loading-panel">
      <div class="loading-content">
        <el-icon class="loading-icon" :size="32"><Refresh /></el-icon>
        <span>加载设置中...</span>
      </div>
    </div>

    <!-- Empty State -->
    <div v-else-if="!loading && settingsList.length === 0" class="panel empty-panel">
      <div class="empty-content">
        <el-icon :size="36" class="empty-icon"><Refresh /></el-icon>
        <p>暂无系统设置</p>
      </div>
    </div>

    <!-- Settings Groups -->
    <template v-else>
      <div
        v-for="group in settingGroups"
        :key="group.category"
        class="panel settings-group"
      >
        <div class="group-header">
          <div class="group-info">
            <span class="group-name font-display">{{ formatCategoryName(group.category) }}</span>
            <span class="group-desc">{{ getCategoryDescription(group.category) }}</span>
          </div>
          <span class="group-badge">{{ group.items.length }} 项</span>
        </div>

        <div class="group-body">
          <div
            v-for="setting in group.items"
            :key="setting.name"
            class="setting-item"
          >
            <div class="setting-info">
              <div class="setting-label">{{ setting.displayName || setting.name }}</div>
              <div v-if="setting.description" class="setting-desc">{{ setting.description }}</div>
              <div class="setting-key">{{ setting.name }}</div>
            </div>

            <div class="setting-control">
              <!-- Boolean switch -->
              <el-switch
                v-if="isBooleanSetting(setting.name) && (settingValues[setting.name] === 'True' || settingValues[setting.name] === 'False' || settingValues[setting.name] === 'true' || settingValues[setting.name] === 'false')"
                :model-value="getBooleanValue(setting.name)"
                @update:model-value="(val: string | number | boolean) => setBooleanValue(setting.name, !!val)"
                active-text="启用"
                inactive-text="禁用"
              />

              <!-- Number input -->
              <el-input
                v-else-if="isNumberSetting(setting.name)"
                v-model="settingValues[setting.name]"
                type="number"
                placeholder="请输入数值"
                class="setting-input"
              />

              <!-- Password input -->
              <el-input
                v-else-if="isPasswordSetting(setting.name)"
                v-model="settingValues[setting.name]"
                type="password"
                show-password
                placeholder="请输入"
                class="setting-input"
              />

              <!-- Default text input -->
              <el-input
                v-else
                v-model="settingValues[setting.name]"
                placeholder="请输入设置值"
                class="setting-input"
              />
            </div>
          </div>
        </div>
      </div>
    </template>
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

// ─── Panel ─────────────────────────────────────────────
.panel {
  background: var(--bg-card);
  border: 1px solid var(--border-default);
  border-radius: 12px;
  overflow: hidden;
  transition: background 0.3s, border-color 0.3s;
}

.panel-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 14px 16px;
  border-bottom: 1px solid var(--border-default);
  transition: border-color 0.3s;
}

.panel-title {
  font-family: 'Plus Jakarta Sans', 'Inter', sans-serif;
  font-size: 13.5px;
  font-weight: 600;
  color: var(--text-secondary);
  letter-spacing: -0.2px;
  transition: color 0.3s;
}

// ─── Header Panel ──────────────────────────────────────
.header-panel {
  .header-left {
    display: flex;
    align-items: baseline;
    gap: 10px;
  }

  .panel-subtitle {
    font-size: 12px;
    color: var(--text-muted);
  }

  .header-actions {
    display: flex;
    gap: 8px;
  }
}

// ─── Buttons ───────────────────────────────────────────
.btn-ghost {
  background: transparent !important;
  border: 1px solid var(--border-default) !important;
  color: var(--text-secondary) !important;
  border-radius: 8px !important;
  font-size: 13px;
  height: 36px;
  padding: 0 14px;
  transition: all 0.2s;

  &:hover {
    border-color: var(--border-medium) !important;
    color: var(--text-primary) !important;
    background: var(--bg-hover) !important;
  }
}

.btn-primary {
  background: linear-gradient(135deg, #4f46e5, #6366f1) !important;
  border: none !important;
  color: #fff !important;
  border-radius: 8px !important;
  font-size: 13px;
  font-weight: 500;
  height: 36px;
  padding: 0 14px;
  box-shadow: 0 4px 12px rgba(79, 70, 229, 0.3);
  transition: all 0.2s;

  &:hover {
    transform: translateY(-1px);
    box-shadow: 0 6px 16px rgba(79, 70, 229, 0.4);
  }

  &:active {
    transform: translateY(0);
  }
}

// ─── Loading / Empty States ────────────────────────────
.loading-panel,
.empty-panel {
  padding: 60px 16px;
}

.loading-content,
.empty-content {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 12px;
  font-size: 13px;
  color: var(--text-muted);
}

.loading-icon {
  color: var(--primary);
  animation: spin 1s linear infinite;
}

@keyframes spin {
  from { transform: rotate(0deg); }
  to { transform: rotate(360deg); }
}

.empty-icon {
  color: var(--border-subtle);
}

// ─── Settings Group ────────────────────────────────────
.settings-group {
  transition: background 0.3s, border-color 0.3s;

  &:hover {
    border-color: var(--border-subtle);
  }
}

.group-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 14px 16px;
  border-bottom: 1px solid var(--border-default);
  transition: border-color 0.3s;
}

.group-info {
  display: flex;
  align-items: baseline;
  gap: 10px;
}

.group-name {
  font-family: 'Plus Jakarta Sans', 'Inter', sans-serif;
  font-size: 14px;
  font-weight: 600;
  color: var(--text-primary);
  letter-spacing: -0.2px;
}

.group-desc {
  font-size: 12px;
  color: var(--text-muted);
}

.group-badge {
  font-size: 10.5px;
  font-weight: 500;
  padding: 2px 8px;
  background: var(--primary-dim);
  color: var(--primary-pale);
  border-radius: 20px;
  border: 1px solid rgba(99, 102, 241, 0.15);
}

.group-body {
  padding: 4px 0;
}

// ─── Setting Item ──────────────────────────────────────
.setting-item {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 24px;
  padding: 14px 16px;
  border-bottom: 1px solid var(--border-default);
  transition: background 0.15s, border-color 0.3s;

  &:last-child {
    border-bottom: none;
  }

  &:hover {
    background: var(--bg-hover);
  }
}

.setting-info {
  flex: 1;
  min-width: 0;
}

.setting-label {
  font-size: 13px;
  font-weight: 500;
  color: var(--text-primary);
  margin-bottom: 2px;
  transition: color 0.3s;
}

.setting-desc {
  font-size: 12px;
  color: var(--text-muted);
  margin-bottom: 4px;
  line-height: 1.4;
  transition: color 0.3s;
}

.setting-key {
  font-family: 'JetBrains Mono', 'Inter', monospace;
  font-size: 11px;
  color: var(--text-disabled);
  transition: color 0.3s;
}

.setting-control {
  flex-shrink: 0;
  width: 280px;
}

// ─── Input Overrides ───────────────────────────────────
.setting-input {
  :deep(.el-input__wrapper) {
    background: var(--input-bg) !important;
    border: 1px solid var(--input-border) !important;
    border-radius: 8px !important;
    box-shadow: none !important;
    transition: border-color 0.2s, box-shadow 0.2s;

    &:hover {
      border-color: rgba(99, 102, 241, 0.3) !important;
    }

    &.is-focus {
      border-color: rgba(99, 102, 241, 0.6) !important;
      box-shadow: 0 0 0 3px rgba(99, 102, 241, 0.1) !important;
    }

    .el-input__inner {
      color: var(--text-primary) !important;
      font-size: 13px;

      &::placeholder {
        color: var(--text-muted) !important;
      }
    }
  }
}

// ─── Switch Override ───────────────────────────────────
:deep(.el-switch) {
  --el-switch-on-color: #6366f1;
  --el-switch-off-color: var(--border-medium);

  .el-switch__label {
    color: var(--text-muted);
    font-size: 12px;
  }
}

// ─── Responsive ────────────────────────────────────────
@media (max-width: 768px) {
  .header-panel .panel-header {
    flex-direction: column;
    align-items: flex-start;
    gap: 12px;
  }

  .header-actions {
    width: 100%;
    justify-content: flex-end;
  }

  .setting-item {
    flex-direction: column;
    align-items: flex-start;
    gap: 10px;
  }

  .setting-control {
    width: 100%;
  }
}
</style>
