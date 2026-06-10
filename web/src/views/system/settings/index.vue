<script setup lang="ts">
import { ref, reactive, computed, onMounted } from 'vue'
import { ElMessage } from 'element-plus'
import { Refresh } from '@element-plus/icons-vue'
import { useI18n } from 'vue-i18n'
import { settingApi } from '@/api/modules/settings'
import type { SettingDto, UpdateSettingDto } from '@/api/modules/settings'

const { t, te, locale } = useI18n()

// ── State ──────────────────────────────────────────────
const loading = ref(false)
const saving = ref(false)
const settingsList = ref<SettingDto[]>([])
const settingValues = reactive<Record<string, string>>({})

// ── Default values for specific settings ───────────────
const DEFAULT_VALUES: Record<string, string> = {
  'Abp.Localization.DefaultLanguage': 'zh-Hans',
  'Abp.Timing.Timezone': 'Asia/Shanghai',
  'Abp.Identity.SignIn.RequireConfirmedEmail': 'False',
  'Abp.Identity.SignIn.RequireConfirmedPhoneNumber': 'False',
}

// ── Dropdown options (computed for i18n reactivity) ─────
const languageOptions = computed(() => [
  { label: t('settings.languageZhHans'), value: 'zh-Hans' },
  { label: t('settings.languageEn'), value: 'en' },
])

const verificationOptions = computed(() => [
  { label: t('settings.disabled'), value: 'False' },
  { label: t('settings.enabled'), value: 'True' },
])

const timezoneOptions = [
  { label: '(UTC+08:00) 北京/上海/香港', value: 'Asia/Shanghai' },
  { label: '(UTC+09:00) 东京/首尔', value: 'Asia/Tokyo' },
  { label: '(UTC+07:00) 曼谷/河内', value: 'Asia/Bangkok' },
  { label: '(UTC+05:30) 新德里/孟买', value: 'Asia/Kolkata' },
  { label: '(UTC+00:00) 伦敦', value: 'Europe/London' },
  { label: '(UTC+01:00) 巴黎/柏林', value: 'Europe/Berlin' },
  { label: '(UTC-05:00) 纽约', value: 'America/New_York' },
  { label: '(UTC-08:00) 洛杉矶', value: 'America/Los_Angeles' },
  { label: '(UTC+03:00) 莫斯科', value: 'Europe/Moscow' },
  { label: '(UTC+10:00) 悉尼', value: 'Australia/Sydney' },
]

// ── Hidden categories (entire module excluded) ──────────
const HIDDEN_CATEGORIES = new Set([
  'Abp.Mailing',
])

// ── Hidden individual settings ──────────────────────────
const HIDDEN_SETTINGS = new Set([
  'Abp.Timing.TimeZone',
  'Abp.Identity.Password.RequiredUniqueChars',
])

// ── Computed: Group settings by category ───────────────
interface SettingGroup {
  category: string
  items: SettingDto[]
}

const settingGroups = computed<SettingGroup[]>(() => {
  const groups = new Map<string, SettingDto[]>()

  settingsList.value.forEach(setting => {
    if (HIDDEN_SETTINGS.has(setting.name)) return
    const parts = setting.name.split('.')
    const category = parts.length > 1 ? parts.slice(0, -1).join('.') : 'General'
    if (HIDDEN_CATEGORIES.has(category)) return

    if (!groups.has(category)) {
      groups.set(category, [])
    }
    groups.get(category)!.push(setting)
  })

  // Sort groups: Localization first, then Timing, then others
  const CATEGORY_ORDER: Record<string, number> = {
    'Abp.Localization': 0,
    'Abp.Timing': 1,
  }

  return Array.from(groups.entries())
    .map(([category, items]) => ({ category, items }))
    .sort((a, b) => {
      const orderA = CATEGORY_ORDER[a.category] ?? 99
      const orderB = CATEGORY_ORDER[b.category] ?? 99
      return orderA - orderB || a.category.localeCompare(b.category)
    })
})

// ── Apply default language from setting ─────────────────
function applyDefaultLanguage() {
  const langKey = 'Abp.Localization.DefaultLanguage'
  const lang = settingValues[langKey]
  if (lang && (lang === 'zh-Hans' || lang === 'en') && lang !== locale.value) {
    locale.value = lang as 'zh-Hans' | 'en'
    localStorage.setItem('locale', lang)
  }
}

// ── Data Fetching ──────────────────────────────────────
async function fetchSettings() {
  loading.value = true
  try {
    const res = await settingApi.get({})
    const data = Array.isArray(res.data) ? res.data : (res.data as any)?.items ?? []
    settingsList.value = data

    // 先用后端返回值填充
    data.forEach((setting: SettingDto) => {
      const v = setting.value ?? DEFAULT_VALUES[setting.name] ?? ''
      settingValues[setting.name] = v
    })

    // 后端未返回的设置项：补默认值 + 补入 settingsList 以显示在页面上
    const existingNames = new Set(data.map((s: SettingDto) => s.name))
    for (const [key, defaultVal] of Object.entries(DEFAULT_VALUES)) {
      if (!settingValues[key]) {
        settingValues[key] = defaultVal
      }
      if (!existingNames.has(key)) {
        const parts = key.split('.')
        const displayName = parts[parts.length - 1]!
        settingsList.value.push({
          name: key,
          value: defaultVal,
          displayName,
          description: key,
          isEncrypted: false,
        })
      }
    }

    // 根据「默认语言」设置项的值切换界面语言
    applyDefaultLanguage()
  } catch (err: unknown) {
    const axiosErr = err as { response?: { data?: { error?: { message?: string } } } }
    ElMessage.error(axiosErr?.response?.data?.error?.message || t('settings.fetchFailed'))
  } finally {
    loading.value = false
  }
}

// ── Save ───────────────────────────────────────────────
async function handleSave() {
  saving.value = true
  try {
    const payload: UpdateSettingDto[] = Object.entries(settingValues)
      .filter(([name]) => {
        if (HIDDEN_SETTINGS.has(name)) return false
        const parts = name.split('.')
        const cat = parts.length > 1 ? parts.slice(0, -1).join('.') : 'General'
        return !HIDDEN_CATEGORIES.has(cat)
      })
      .map(([name, value]) => ({
        name,
        value,
      }))

    await settingApi.update(payload)

    applyDefaultLanguage()

    ElMessage.success(t('settings.saveSuccess'))
  } catch (err: unknown) {
    const axiosErr = err as { response?: { data?: { error?: { message?: string } } } }
    ElMessage.error(axiosErr?.response?.data?.error?.message || t('settings.saveFailed'))
  } finally {
    saving.value = false
  }
}

function handleRefresh() {
  fetchSettings()
}

// ── I18n: setting name → translation key ────────────────
const SETTING_LABEL_MAP: Record<string, string> = {
  // Localization
  'Abp.Localization.DefaultLanguage': 'settings.defaultLanguage',
  // Timing
  'Abp.Timing.Timezone': 'settings.timezone',
  // SignIn
  'Abp.Identity.SignIn.RequireConfirmedEmail': 'settings.requireConfirmedEmail',
  'Abp.Identity.SignIn.RequireConfirmedPhoneNumber': 'settings.requireConfirmedPhone',
  // Password
  'Abp.Identity.Password.RequiredLength': 'settings.passwordRequiredLength',
  'Abp.Identity.Password.RequireNonAlphanumeric': 'settings.passwordRequireNonAlphanumeric',
  'Abp.Identity.Password.RequireLowercase': 'settings.passwordRequireLowercase',
  'Abp.Identity.Password.RequireUppercase': 'settings.passwordRequireUppercase',
  'Abp.Identity.Password.RequireDigit': 'settings.passwordRequireDigit',
  // Account
  'Abp.Account.IsSelfRegistrationEnabled': 'settings.selfRegistration',
  // Lockout
  'Abp.Identity.Lockout.IsEnabled': 'settings.userLockoutEnabled',
  'Abp.Identity.Lockout.MaxFailedAccessAttempts': 'settings.maxFailedLoginAttempts',
  'Abp.Identity.Lockout.DefaultLockoutTimeSpanSeconds': 'settings.lockoutDurationSeconds',
  'Abp.Identity.UserLockOut.IsEnabled': 'settings.userLockoutEnabled',
  'Abp.Identity.UserLockOut.MaxFailedAccessAttempts': 'settings.maxFailedLoginAttempts',
  'Abp.Identity.UserLockOut.DefaultLockoutTimeSpanSeconds': 'settings.lockoutDurationSeconds',
  // OrganizationUnit
  'Abp.Identity.OrganizationUnit.MaxUserCount': 'settings.maxUserCountPerOrgUnit',
}

function getSettingLabel(name: string, fallback: string): string {
  if (SETTING_LABEL_MAP[name]) return t(SETTING_LABEL_MAP[name])
  // 尝试用设置名最后一段 + "settings." 前缀查 i18n
  const shortKey = 'settings.' + name.split('.').pop()!
  if (te(shortKey)) return t(shortKey)
  return fallback
}

// ── Category i18n ────────────────────────────────────────
const CATEGORY_LABEL_MAP: Record<string, string> = {
  'Abp.Localization': 'settings.catLocalization',
  'Abp.Timing': 'settings.catTiming',
  'Abp.Identity': 'settings.catIdentity',
  'Abp.Account': 'settings.catAccount',
  'Abp.SettingManagement': 'settings.catSettingManagement',
  'Abp.TenantManagement': 'settings.catTenantManagement',
  'Abp.FeatureManagement': 'settings.catFeatureManagement',
  'Abp.PermissionManagement': 'settings.catPermissionManagement',
  'Abp.Identity.Password': 'settings.catPassword',
  'Abp.Identity.SignIn': 'settings.catSignIn',
  'Abp.Identity.Lockout': 'settings.catLockout',
  'Abp.Identity.UserLockOut': 'settings.catLockout',
  'Abp.Identity.OrganizationUnit': 'settings.catOrganizationUnit',
}

// ── Helpers ────────────────────────────────────────────
function formatCategoryName(category: string): string {
  if (CATEGORY_LABEL_MAP[category]) return t(CATEGORY_LABEL_MAP[category])
  const parts = category.split('.')
  return parts[parts.length - 1]
}

function getCategoryDescription(category: string): string {
  const descriptions: Record<string, string> = {
    'Abp.Identity': t('settings.descIdentity'),
    'Abp.Account': t('settings.descAccount'),
    'Abp.SettingManagement': t('settings.descSettingManagement'),
    'Abp.TenantManagement': t('settings.descTenantManagement'),
  }
  return descriptions[category] || `${formatCategoryName(category)} ${t('settings.descGeneric')}`
}

function isPasswordSetting(name: string): boolean {
  const lower = name.toLowerCase()
  return lower.includes('password') || lower.includes('secret') || lower.includes('key')
}

function isNumberSetting(name: string): boolean {
  const lower = name.toLowerCase()
  return lower.includes('port') || lower.includes('timeout') || lower.includes('count') || lower.includes('size') || lower.includes('length') || lower.includes('period')
}

function isBooleanSetting(name: string): boolean {
  const lower = name.toLowerCase()
  return lower.includes('enable') || lower.includes('is') || lower.includes('use') || lower.includes('require')
}

function getBoolValue(name: string): boolean {
  return settingValues[name]?.toLowerCase() === 'true'
}

function setBoolValue(name: string, val: boolean) {
  settingValues[name] = val ? 'True' : 'False'
}

function isTimezoneSetting(name: string): boolean {
  return name === 'Abp.Timing.Timezone'
}

function isLanguageSetting(name: string): boolean {
  return name === 'Abp.Localization.DefaultLanguage'
}

// 保留为空，所有 bool 设置现在统一走 isBooleanSetting → el-switch
function isBoolDropdownSetting(_name: string): boolean {
  return false
}

// ── Lifecycle ──────────────────────────────────────────
onMounted(() => {
  fetchSettings()
})
</script>

<template>
  <div class="page-container">
    <!-- Header Panel -->
    <div class="panel header-panel">
      <div class="panel-header">
        <div class="header-left">
          <span class="panel-title font-display">{{ t('settings.title') }}</span>
          <span class="panel-subtitle">{{ t('settings.subtitle') }}</span>
        </div>
        <div class="header-actions">
          <button class="btn-ghost" :disabled="loading" @click="handleRefresh">
            <el-icon :size="14"><Refresh /></el-icon>
            {{ t('common.refresh') }}
          </button>
          <button class="btn-primary" :disabled="saving" @click="handleSave">
            {{ saving ? t('settings.saving') : t('settings.save') }}
          </button>
        </div>
      </div>
    </div>

    <!-- Loading State -->
    <div v-if="loading && settingsList.length === 0" class="panel loading-panel">
      <div class="loading-content">
        <el-icon class="loading-icon" :size="28"><Refresh /></el-icon>
        <span>{{ t('settings.loading') }}</span>
      </div>
    </div>

    <!-- Empty State -->
    <div v-else-if="!loading && settingsList.length === 0" class="panel empty-panel">
      <div class="empty-content">
        <el-icon :size="36" class="empty-icon"><Refresh /></el-icon>
        <p>{{ t('settings.noData') }}</p>
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
          <span class="group-badge">{{ group.items.length }} {{ t('settings.items') }}</span>
        </div>

        <div class="group-body">
          <div
            v-for="setting in group.items"
            :key="setting.name"
            class="setting-item"
          >
            <div class="setting-info">
              <div class="setting-label">{{ getSettingLabel(setting.name, setting.displayName || setting.name) }}</div>
              <div v-if="setting.description" class="setting-desc">{{ setting.description }}</div>
              <div class="setting-key">{{ setting.name }}</div>
            </div>

            <div class="setting-control">
              <!-- Language dropdown -->
              <el-select
                v-if="isLanguageSetting(setting.name)"
                :model-value="settingValues[setting.name]"
                @update:model-value="(val: string | number | boolean) => { settingValues[setting.name] = String(val) }"
                class="setting-select"
              >
                <el-option
                  v-for="opt in languageOptions"
                  :key="opt.value"
                  :label="opt.label"
                  :value="opt.value"
                />
              </el-select>

              <!-- Bool dropdown (verification + password requires) -->
              <el-select
                v-else-if="isBoolDropdownSetting(setting.name)"
                :model-value="settingValues[setting.name]"
                @update:model-value="(val: string | number | boolean) => { settingValues[setting.name] = String(val) }"
                class="setting-select"
              >
                <el-option
                  v-for="opt in verificationOptions"
                  :key="opt.value"
                  :label="opt.label"
                  :value="opt.value"
                />
              </el-select>

              <!-- Timezone dropdown -->
              <el-select
                v-else-if="isTimezoneSetting(setting.name)"
                :model-value="settingValues[setting.name]"
                @update:model-value="(val: string | number | boolean) => { settingValues[setting.name] = String(val) }"
                filterable
                class="setting-select"
                :placeholder="t('settings.timezonePlaceholder')"
              >
                <el-option
                  v-for="opt in timezoneOptions"
                  :key="opt.value"
                  :label="opt.label"
                  :value="opt.value"
                />
              </el-select>

              <!-- Boolean switch -->
              <el-switch
                v-else-if="isBooleanSetting(setting.name) && (settingValues[setting.name] === 'True' || settingValues[setting.name] === 'False' || settingValues[setting.name] === 'true' || settingValues[setting.name] === 'false')"
                :model-value="getBoolValue(setting.name)"
                @update:model-value="(val: string | number | boolean) => setBoolValue(setting.name, !!val)"
                :active-text="t('common.enabled')"
                :inactive-text="t('common.disabled')"
              />

              <!-- Number input -->
              <el-input
                v-else-if="isNumberSetting(setting.name)"
                v-model="settingValues[setting.name]"
                type="number"
                :placeholder="t('settings.numberPlaceholder')"
                class="setting-input"
              />

              <!-- Password input -->
              <el-input
                v-else-if="isPasswordSetting(setting.name)"
                v-model="settingValues[setting.name]"
                type="password"
                show-password
                :placeholder="t('settings.textPlaceholder')"
                class="setting-input"
              />

              <!-- Default text input -->
              <el-input
                v-else
                v-model="settingValues[setting.name]"
                :placeholder="t('settings.valuePlaceholder')"
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

// ── Panel ─────────────────────────────────────────────
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

// ── Header ────────────────────────────────────────────
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

// ── Buttons ───────────────────────────────────────────
.btn-primary {
  display: inline-flex;
  align-items: center;
  gap: 5px;
  padding: 7px 16px;
  font-size: 12.5px;
  font-weight: 600;
  color: #fff;
  background: linear-gradient(135deg, #4f46e5, #6366f1);
  border: none;
  border-radius: 8px;
  cursor: pointer;
  transition: transform 0.2s, box-shadow 0.2s, opacity 0.2s;
  box-shadow: 0 4px 12px rgba(79, 70, 229, 0.3);
  letter-spacing: 0.3px;

  &:hover:not(:disabled) {
    transform: translateY(-1px);
    box-shadow: 0 6px 16px rgba(79, 70, 229, 0.4);
  }

  &:disabled {
    opacity: 0.6;
    cursor: not-allowed;
  }
}

.btn-ghost {
  display: inline-flex;
  align-items: center;
  gap: 5px;
  padding: 7px 14px;
  font-size: 12.5px;
  font-weight: 500;
  color: var(--text-secondary);
  background: transparent;
  border: 1px solid var(--border-subtle);
  border-radius: 8px;
  cursor: pointer;
  transition: all 0.2s;

  &:hover:not(:disabled) {
    color: var(--text-primary);
    border-color: var(--border-medium);
    background: var(--bg-hover);
  }

  &:disabled {
    opacity: 0.5;
    cursor: not-allowed;
  }
}

// ── Loading / Empty States ────────────────────────────
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

// ── Settings Group ────────────────────────────────────
.settings-group {
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

// ── Setting Item ──────────────────────────────────────
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
  width: 300px;
}

// ── Input ─────────────────────────────────────────────
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

// ── Select ────────────────────────────────────────────
.setting-select {
  width: 100%;

  :deep(.el-select__wrapper) {
    background: var(--input-bg) !important;
    border: 1px solid var(--input-border) !important;
    border-radius: 8px !important;
    box-shadow: none !important;

    &:hover {
      border-color: rgba(99, 102, 241, 0.3) !important;
    }

    &.is-focus {
      border-color: rgba(99, 102, 241, 0.6) !important;
      box-shadow: 0 0 0 3px rgba(99, 102, 241, 0.1) !important;
    }

    .el-select__placeholder,
    .el-select__selected-item {
      color: var(--text-primary) !important;
      font-size: 13px;
    }
  }
}

// ── Switch ────────────────────────────────────────────
:deep(.el-switch) {
  --el-switch-on-color: #6366f1;
  --el-switch-off-color: var(--border-medium);

  .el-switch__label {
    color: var(--text-muted);
    font-size: 12px;
  }
}

// ── Responsive ────────────────────────────────────────
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
