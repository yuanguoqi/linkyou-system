<script setup lang="ts">
import { ref, reactive, computed } from 'vue'
import { useI18n } from 'vue-i18n'
import { useAuthStore } from '@/stores/auth'
import { useAppStore } from '@/stores/app'
import { useThemeStore } from '@/stores/theme'
import { useRouter } from 'vue-router'
import { ElMessage, ElMessageBox } from 'element-plus'
import type { FormInstance, FormRules } from 'element-plus'
import { authApi } from '@/api/modules/auth'
import { settingApi } from '@/api/modules/settings'

const { t } = useI18n()
const authStore = useAuthStore()
const appStore = useAppStore()
const themeStore = useThemeStore()
const router = useRouter()

// ── 修改密码弹窗 ──────────────────────────────────────
const passwordVisible = ref(false)
const passwordSubmitting = ref(false)
const passwordFormKey = ref(0)
const passwordFormRef = ref<FormInstance>()
const passwordForm = reactive({
  currentPassword: '',
  newPassword: '',
  confirmPassword: '',
})

// 从系统设置动态获取的密码策略
const passwordPolicy = reactive({
  requiredLength: 6,
  requireUppercase: false,
  requireLowercase: false,
  requireDigit: false,
  requireNonAlphanumeric: false,
})

const passwordHint = computed(() => {
  const rules: string[] = [`至少 ${passwordPolicy.requiredLength} 位`]
  if (passwordPolicy.requireUppercase) rules.push('大写字母')
  if (passwordPolicy.requireLowercase) rules.push('小写字母')
  if (passwordPolicy.requireDigit) rules.push('数字')
  if (passwordPolicy.requireNonAlphanumeric) rules.push('特殊字符')
  return '要求：' + rules.join('、')
})

const passwordRules = computed<FormRules>(() => {
  const rules: any[] = [
    { required: true, message: '请输入新密码', trigger: 'blur' },
    { min: passwordPolicy.requiredLength, message: `密码至少 ${passwordPolicy.requiredLength} 位`, trigger: 'blur' },
  ]
  if (passwordPolicy.requireUppercase) rules.push({ pattern: /[A-Z]/, message: '需包含至少一位大写字母', trigger: 'blur' })
  if (passwordPolicy.requireLowercase) rules.push({ pattern: /[a-z]/, message: '需包含至少一位小写字母', trigger: 'blur' })
  if (passwordPolicy.requireDigit) rules.push({ pattern: /[0-9]/, message: '需包含至少一位数字', trigger: 'blur' })
  if (passwordPolicy.requireNonAlphanumeric) rules.push({ pattern: /[^a-zA-Z0-9]/, message: '需包含至少一位特殊字符', trigger: 'blur' })
  return {
    currentPassword: [{ required: true, message: '请输入当前密码', trigger: 'blur' }],
    newPassword: rules,
    confirmPassword: [
      { required: true, message: '请确认新密码', trigger: 'blur' },
      { validator: (_r: any, v: string, cb: any) => cb(v !== passwordForm.newPassword ? new Error('两次密码不一致') : undefined), trigger: 'blur' },
    ],
  }
})

async function fetchPasswordPolicy() {
  try {
    const res = await settingApi.get({})
    const data = Array.isArray(res.data) ? res.data : (res.data as any)?.items ?? []
    const map: Record<string, string> = {}
    data.forEach((s: any) => { map[s.name] = s.value ?? '' })
    const getBool = (k: string) => (map[k] || 'False').toLowerCase() === 'true'
    const getNum = (k: string) => parseInt(map[k]) || 6
    passwordPolicy.requiredLength = getNum('Abp.Identity.Password.RequiredLength')
    passwordPolicy.requireUppercase = getBool('Abp.Identity.Password.RequireUppercase')
    passwordPolicy.requireLowercase = getBool('Abp.Identity.Password.RequireLowercase')
    passwordPolicy.requireDigit = getBool('Abp.Identity.Password.RequireDigit')
    passwordPolicy.requireNonAlphanumeric = getBool('Abp.Identity.Password.RequireNonAlphanumeric')
    passwordFormKey.value++ // 强制 el-form 重渲染以应用最新 rules
  } catch { /* 获取失败用默认值 */ }
}

async function openPasswordDialog() {
  passwordForm.currentPassword = ''
  passwordForm.newPassword = ''
  passwordForm.confirmPassword = ''
  passwordVisible.value = true
  await fetchPasswordPolicy()
  // form 因 :key 变化已重建，clearValidate 针对新实例
  passwordFormRef.value?.clearValidate()
}

async function handleChangePassword() {
  try { await passwordFormRef.value?.validate() } catch { return }
  passwordSubmitting.value = true
  try {
    await authApi.changePassword({
      currentPassword: passwordForm.currentPassword,
      newPassword: passwordForm.newPassword,
    })
    ElMessage.success('密码修改成功')
    passwordVisible.value = false
  } catch (err: any) {
    const msg = err?.response?.data?.error?.message || '密码修改失败，请确保新密码包含大写字母、小写字母、数字和特殊字符'
    ElMessage.error(msg)
  } finally {
    passwordSubmitting.value = false
  }
}

// ── 用户菜单命令 ──────────────────────────────────────
async function handleCommand(cmd: string) {
  if (cmd === 'logout') {
    await ElMessageBox.confirm(t('header.logoutConfirm'), t('header.logoutTitle'), {
      confirmButtonText: t('header.confirmLogout'),
      cancelButtonText: t('common.cancel'),
      type: 'warning',
    })
    await authStore.logout()
    router.push('/login')
  } else if (cmd === 'profile') {
    router.push('/profile')
  } else if (cmd === 'password') {
    openPasswordDialog()
  }
}
</script>

<template>
  <div class="header-bar">
    <!-- 左侧 -->
    <div class="header-left">
      <button class="action-btn" @click="appStore.toggleSidebar()">
        <el-icon :size="16">
          <Fold v-if="!appStore.sidebarCollapsed" />
          <Expand v-else />
        </el-icon>
      </button>
      <el-breadcrumb separator="/" class="breadcrumb">
        <el-breadcrumb-item v-for="item in appStore.breadcrumbs" :key="item.path || item.title" :to="item.path">
          {{ item.title }}
        </el-breadcrumb-item>
      </el-breadcrumb>
    </div>

    <!-- 右侧 -->
    <div class="header-right">
      <el-tooltip :content="t('header.switchLanguage')" placement="bottom">
        <el-dropdown @command="appStore.setLocale">
          <button class="action-btn"><el-icon :size="15"><Grid /></el-icon></button>
          <template #dropdown>
            <el-dropdown-menu>
              <el-dropdown-item command="zh-Hans">{{ t('header.chinese') }}</el-dropdown-item>
              <el-dropdown-item command="en">English</el-dropdown-item>
            </el-dropdown-menu>
          </template>
        </el-dropdown>
      </el-tooltip>

      <el-tooltip :content="themeStore.isDark ? t('header.switchToLight') : t('header.switchToDark')" placement="bottom">
        <button class="action-btn theme-btn" @click="themeStore.toggleDark()">
          <transition name="theme-icon" mode="out-in">
            <el-icon v-if="themeStore.isDark" :size="15" key="sun"><Sunny /></el-icon>
            <el-icon v-else :size="15" key="moon"><Moon /></el-icon>
          </transition>
        </button>
      </el-tooltip>

      <div class="divider" />

      <el-dropdown trigger="click" @command="handleCommand">
        <div class="user-info">
          <div class="user-avatar">{{ authStore.userDisplayName.charAt(0).toUpperCase() }}</div>
          <div class="user-meta">
            <span class="user-name">{{ authStore.userDisplayName }}</span>
            <span class="user-role">{{ authStore.userRoleDisplay || t('header.admin') }}</span>
          </div>
          <el-icon class="arrow-icon" :size="10"><ArrowDown /></el-icon>
        </div>
        <template #dropdown>
          <el-dropdown-menu>
            <el-dropdown-item command="profile">
              <el-icon><User /></el-icon><span>{{ t('header.profile') }}</span>
            </el-dropdown-item>
            <el-dropdown-item @click="openPasswordDialog">
              <el-icon><Lock /></el-icon><span>{{ t('header.changePassword') }}</span>
            </el-dropdown-item>
            <el-dropdown-item divided command="logout">
              <el-icon><SwitchButton /></el-icon><span>{{ t('header.logout') }}</span>
            </el-dropdown-item>
          </el-dropdown-menu>
        </template>
      </el-dropdown>
    </div>

    <!-- 修改密码弹窗（append-to-body 确保不被父元素裁剪） -->
    <el-dialog v-model="passwordVisible" title="修改密码" width="440px" :close-on-click-modal="false" :append-to-body="true" destroy-on-close>
      <el-form ref="passwordFormRef" :key="passwordFormKey" :model="passwordForm" :rules="passwordRules" label-width="80px" label-position="left">
        <el-form-item label="当前密码" prop="currentPassword">
          <el-input v-model="passwordForm.currentPassword" type="password" show-password placeholder="请输入当前密码" />
        </el-form-item>
        <el-form-item label="新密码" prop="newPassword">
          <el-input v-model="passwordForm.newPassword" type="password" show-password placeholder="请输入新密码" />
          <div class="password-hint">{{ passwordHint }}</div>
        </el-form-item>
        <el-form-item label="确认密码" prop="confirmPassword">
          <el-input v-model="passwordForm.confirmPassword" type="password" show-password placeholder="请再次输入新密码" />
        </el-form-item>
      </el-form>
      <template #footer>
        <div class="dialog-footer">
          <button class="btn-ghost" @click="passwordVisible = false">取消</button>
          <button class="btn-primary" :disabled="passwordSubmitting" @click="handleChangePassword">
            {{ passwordSubmitting ? '提交中...' : '确定' }}
          </button>
        </div>
      </template>
    </el-dialog>
  </div>
</template>

<style scoped lang="scss">
.header-bar { height: 100%; display: flex; align-items: center; justify-content: space-between; padding: 0 16px; }
.header-left { display: flex; align-items: center; gap: 10px;
  .breadcrumb {
    :deep(.el-breadcrumb__item .el-breadcrumb__inner) { color: var(--breadcrumb-color); font-size: 12.5px; transition: color 0.2s; }
    :deep(.el-breadcrumb__item:last-child .el-breadcrumb__inner) { color: var(--breadcrumb-active); font-weight: 500; }
    :deep(.el-breadcrumb__separator) { color: var(--breadcrumb-separator); }
  }
}
.header-right { display: flex; align-items: center; gap: 4px;
  .divider { width: 1px; height: 16px; background: var(--border-subtle); margin: 0 6px; }
}
.action-btn {
  width: 32px; height: 32px; display: flex; align-items: center; justify-content: center;
  background: transparent; border: 1px solid transparent; border-radius: 8px; cursor: pointer; color: var(--text-muted);
  transition: all 0.2s cubic-bezier(0.4, 0, 0.2, 1);
  &:hover { background: var(--bg-hover); border-color: var(--border-primary); color: var(--primary-pale); }
}
.theme-icon-enter-active, .theme-icon-leave-active { transition: all 0.2s cubic-bezier(0.4, 0, 0.2, 1); }
.theme-icon-enter-from { opacity: 0; transform: rotate(-90deg) scale(0.6); }
.theme-icon-leave-to { opacity: 0; transform: rotate(90deg) scale(0.6); }
.user-info {
  display: flex; align-items: center; gap: 8px; padding: 4px 10px 4px 4px; border-radius: 10px; cursor: pointer;
  border: 1px solid transparent; transition: all 0.25s cubic-bezier(0.4, 0, 0.2, 1);
  &:hover { background: var(--bg-hover); border-color: var(--border-primary); }
  .user-avatar {
    width: 32px; height: 32px; border-radius: 8px; background: var(--avatar-gradient);
    display: flex; align-items: center; justify-content: center; font-family: 'Plus Jakarta Sans', 'Inter', sans-serif;
    font-size: 12.5px; font-weight: 700; color: #fff; box-shadow: 0 0 8px var(--primary-glow); flex-shrink: 0;
    transition: all 0.25s cubic-bezier(0.4, 0, 0.2, 1);
    .user-info:hover & { transform: scale(1.08); box-shadow: 0 0 12px var(--primary-glow), 0 0 24px rgba(99, 102, 241, 0.15); }
  }
  .user-meta { display: flex; flex-direction: column; gap: 0;
    .user-name { font-size: 12.5px; font-weight: 500; color: var(--text-primary); line-height: 1.4; transition: color 0.2s; }
    .user-role { font-size: 11px; color: var(--text-muted); line-height: 1.3; transition: color 0.2s; }
  }
  .arrow-icon { color: var(--text-muted); margin-left: 2px; transition: transform 0.2s;
    .user-info:hover & { transform: translateY(1px); }
  }
}
// 弹窗按钮
.dialog-footer { display: flex; justify-content: flex-end; gap: 10px; }
.btn-primary {
  display: inline-flex; align-items: center; gap: 5px; padding: 8px 20px; font-size: 12.5px; font-weight: 600; color: #fff;
  background: linear-gradient(135deg, #4f46e5, #6366f1); border: none; border-radius: 8px; cursor: pointer;
  transition: transform 0.2s, box-shadow 0.2s, opacity 0.2s; box-shadow: 0 4px 14px rgba(79, 70, 229, 0.3);
  &:hover:not(:disabled) { transform: translateY(-1px); box-shadow: 0 6px 20px rgba(79, 70, 229, 0.4); }
  &:disabled { opacity: 0.6; cursor: not-allowed; }
}
.password-hint { font-size: 11px; color: var(--text-muted); margin-top: 4px; line-height: 1.4; }
.btn-ghost {
  display: inline-flex; align-items: center; gap: 5px; padding: 8px 20px; font-size: 12.5px; font-weight: 500;
  color: var(--text-secondary); background: transparent; border: 1px solid var(--border-subtle); border-radius: 8px;
  cursor: pointer; transition: all 0.2s;
  &:hover { color: var(--text-primary); border-color: var(--border-medium); background: var(--bg-hover); }
}
:deep(.el-input__wrapper) {
  background: var(--input-bg) !important; border: 1px solid var(--border-subtle) !important; border-radius: 8px !important;
  box-shadow: none !important;
  &:hover { border-color: rgba(99, 102, 241, 0.3) !important; }
  &.is-focus { border-color: rgba(99, 102, 241, 0.6) !important; box-shadow: 0 0 0 3px rgba(99, 102, 241, 0.1) !important; }
  .el-input__inner { color: var(--text-primary); font-size: 13px; }
}
</style>
