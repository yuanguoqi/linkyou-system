<script setup lang="ts">
import { ref, reactive, onMounted } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { ElMessage } from 'element-plus'
import type { FormInstance, FormRules } from 'element-plus'
import { User, Lock, Message, RefreshRight, Sunny, Moon, Warning, Close, OfficeBuilding } from '@element-plus/icons-vue'
import { useAuthStore } from '@/stores/auth'
import { useThemeStore } from '@/stores/theme'
import { authApi } from '@/api/modules/auth'
import type { TenantLookupDto } from '@/api/modules/auth'
import CaptchaCanvas from './components/CaptchaCanvas.vue'

const router = useRouter()
const route = useRoute()
const authStore = useAuthStore()
const themeStore = useThemeStore()

// 租户列表
const tenantList = ref<TenantLookupDto[]>([])
const selectedTenantId = ref<string | null>(null)
const tenantLoading = ref(false)

// 切换面板：login | register
const activePanel = ref<'login' | 'register'>('login')
const formRef = ref<FormInstance>()
const registerFormRef = ref<FormInstance>()
const loading = ref(false)

// 验证码
const loginCaptchaRef = ref<InstanceType<typeof CaptchaCanvas>>()
const registerCaptchaRef = ref<InstanceType<typeof CaptchaCanvas>>()
const loginCaptchaCode = ref('')
const registerCaptchaCode = ref('')

// 登录表单
const loginForm = reactive({
  userNameOrEmailAddress: '',
  password: '',
  rememberMe: false as boolean,
  captcha: '',
})

// 登录错误提示
const loginError = ref('')

// 注册表单
const registerForm = reactive({
  userName: '',
  email: '',
  password: '',
  confirmPassword: '',
  captcha: '',
})

// 登录规则
const loginRules: FormRules = {
  userNameOrEmailAddress: [
    { required: true, message: '请输入用户名或邮箱', trigger: 'blur' },
  ],
  password: [
    { required: true, message: '请输入密码', trigger: 'blur' },
    { min: 6, message: '密码至少 6 位', trigger: 'blur' },
  ],
  captcha: [
    { required: true, message: '请输入验证码', trigger: 'blur' },
    {
      validator: (_: unknown, value: string, cb: (e?: Error) => void) => {
        if (value.toLowerCase() !== loginCaptchaCode.value.toLowerCase()) {
          cb(new Error('验证码错误'))
        } else {
          cb()
        }
      },
      trigger: 'blur',
    },
  ],
}

// 注册规则
const registerRules: FormRules = {
  userName: [
    { required: true, message: '请输入用户名', trigger: 'blur' },
    { min: 3, max: 20, message: '用户名长度 3-20 个字符', trigger: 'blur' },
  ],
  email: [
    { required: true, message: '请输入邮箱', trigger: 'blur' },
    { type: 'email', message: '请输入有效的邮箱地址', trigger: 'blur' },
  ],
  password: [
    { required: true, message: '请输入密码', trigger: 'blur' },
    { min: 6, message: '密码至少 6 位', trigger: 'blur' },
  ],
  confirmPassword: [
    { required: true, message: '请确认密码', trigger: 'blur' },
    {
      validator: (_: unknown, value: string, cb: (e?: Error) => void) => {
        if (value !== registerForm.password) cb(new Error('两次密码不一致'))
        else cb()
      },
      trigger: 'blur',
    },
  ],
  captcha: [
    { required: true, message: '请输入验证码', trigger: 'blur' },
    {
      validator: (_: unknown, value: string, cb: (e?: Error) => void) => {
        if (value.toLowerCase() !== registerCaptchaCode.value.toLowerCase()) {
          cb(new Error('验证码错误'))
        } else {
          cb()
        }
      },
      trigger: 'blur',
    },
  ],
}

// 持久化记住密码
onMounted(() => {
  const saved = localStorage.getItem('remember_me')
  if (saved) {
    try {
      const parsed = JSON.parse(saved)
      loginForm.userNameOrEmailAddress = parsed.userNameOrEmailAddress || ''
      loginForm.password = parsed.password || ''
      loginForm.rememberMe = true
    } catch {
      localStorage.removeItem('remember_me')
    }
  }
  // 加载租户列表
  loadTenants()
})

// 加载租户列表
async function loadTenants() {
  tenantLoading.value = true
  try {
    // 先清除旧的租户 ID，防止请求头携带无效租户
    const savedTenantId = localStorage.getItem('tenant_id')
    authStore.setTenant(null)

    const { data } = await authApi.getTenants()
    tenantList.value = data

    // 恢复上次选择的租户（需验证该租户仍在列表中）
    if (savedTenantId && tenantList.value.some(t => t.id === savedTenantId)) {
      selectedTenantId.value = savedTenantId
      authStore.setTenant(savedTenantId)
    } else if (tenantList.value.length > 0) {
      // 默认选中第一个租户
      selectedTenantId.value = tenantList.value[0].id
      authStore.setTenant(tenantList.value[0].id)
    }
  } catch {
    // 租户列表获取失败，清除无效的租户 ID
    authStore.setTenant(null)
    localStorage.removeItem('tenant_id')
  } finally {
    tenantLoading.value = false
  }
}

// 切换租户
function handleTenantChange(tenantId: string | null) {
  selectedTenantId.value = tenantId
  authStore.setTenant(tenantId)
}

function handleLoginCaptchaCode(code: string) {
  loginCaptchaCode.value = code
}

function handleRegisterCaptchaCode(code: string) {
  registerCaptchaCode.value = code
}

function refreshLoginCaptcha() {
  loginCaptchaRef.value?.generate()
  loginForm.captcha = ''
}

function refreshRegisterCaptcha() {
  registerCaptchaRef.value?.generate()
  registerForm.captcha = ''
}

function switchPanel(panel: 'login' | 'register') {
  activePanel.value = panel
  if (panel === 'login') {
    refreshLoginCaptcha()
  } else {
    refreshRegisterCaptcha()
  }
  formRef.value?.clearValidate()
  registerFormRef.value?.clearValidate()
}

async function handleLogin() {
  loginError.value = ''
  try {
    await formRef.value?.validate()
  } catch {
    return
  }
  loading.value = true
  try {
    await authStore.login({
      userNameOrEmailAddress: loginForm.userNameOrEmailAddress,
      password: loginForm.password,
      rememberMe: loginForm.rememberMe,
    })
    if (loginForm.rememberMe) {
      localStorage.setItem('remember_me', JSON.stringify({
        userNameOrEmailAddress: loginForm.userNameOrEmailAddress,
        password: loginForm.password,
      }))
    } else {
      localStorage.removeItem('remember_me')
    }
    const redirect = (route.query.redirect as string) || '/'
    router.push(redirect)
  } catch (err: unknown) {
    const axiosErr = err as { response?: { data?: { error?: { message?: string } } } }
    loginError.value =
      axiosErr?.response?.data?.error?.message ||
      '登录失败，请检查用户名和密码'
    refreshLoginCaptcha()
  } finally {
    loading.value = false
  }
}

async function handleRegister() {
  await registerFormRef.value?.validate()
  loading.value = true
  try {
    ElMessage.info('注册功能即将开放，请联系管理员')
  } finally {
    loading.value = false
    refreshRegisterCaptcha()
  }
}

</script>

<template>
  <div class="login-page" :class="{ dark: themeStore.isDark }">
    <!-- 背景效果 -->
    <div class="bg-grid" />
    <div class="bg-orb orb-1" />
    <div class="bg-orb orb-2" />
    <div class="bg-orb orb-3" />
    <div class="noise-overlay" />

    <!-- 主题切换 -->
    <button class="theme-toggle" @click="themeStore.toggleDark">
      <transition name="theme-icon" mode="out-in">
        <el-icon v-if="themeStore.isDark" :size="18" key="sun"><Sunny /></el-icon>
        <el-icon v-else :size="18" key="moon"><Moon /></el-icon>
      </transition>
    </button>

    <!-- 登录卡片 -->
    <div class="login-card">
      <!-- 左侧装饰面板 -->
      <div class="card-left">
        <div class="card-left-mesh" />
        <div class="brand">
          <div class="brand-icon">
            <svg viewBox="0 0 48 48" fill="none" xmlns="http://www.w3.org/2000/svg">
              <defs>
                <linearGradient id="brandGrad" x1="0" y1="0" x2="48" y2="48" gradientUnits="userSpaceOnUse">
                  <stop stop-color="#6366f1"/>
                  <stop offset="1" stop-color="#818cf8"/>
                </linearGradient>
              </defs>
              <path d="M24 4L44 14V34L24 44L4 34V14L24 4Z" stroke="url(#brandGrad)" stroke-width="2" fill="none"/>
              <path d="M24 12L36 18V30L24 36L12 30V18L24 12Z" fill="url(#brandGrad)" opacity="0.2"/>
              <circle cx="24" cy="24" r="5" fill="url(#brandGrad)"/>
            </svg>
          </div>
          <h1 class="brand-name font-display">领佑系统</h1>
          <p class="brand-desc">Enterprise Management Platform</p>
        </div>
        <div class="feature-list">
          <div class="feature-item"><span class="dot" />多租户企业级架构</div>
          <div class="feature-item"><span class="dot" />细粒度权限管理</div>
          <div class="feature-item"><span class="dot" />实时审计日志</div>
          <div class="feature-item"><span class="dot" />高性能 API 接口</div>
        </div>
        <div class="left-footer">Powered by ABP vNext 10.4</div>
      </div>

      <!-- 右侧表单面板 -->
      <div class="card-right">
        <!-- Tab 切换 -->
        <div class="panel-tabs">
          <button
            class="tab-btn"
            :class="{ active: activePanel === 'login' }"
            @click="switchPanel('login')"
          >登录</button>
          <button
            class="tab-btn"
            :class="{ active: activePanel === 'register' }"
            @click="switchPanel('register')"
          >注册</button>
          <div class="tab-indicator" :class="{ right: activePanel === 'register' }" />
        </div>

        <!-- 登录表单 -->
        <Transition name="slide-fade" mode="out-in">
          <div v-if="activePanel === 'login'" key="login" class="form-panel">
            <p class="form-welcome">欢迎回来，请登录您的账户</p>
            <!-- 登录错误横幅 -->
            <div v-if="loginError" class="login-error-banner">
              <el-icon class="error-icon"><Warning /></el-icon>
              <span>{{ loginError }}</span>
              <el-icon class="close-icon" @click="loginError = ''"><Close /></el-icon>
            </div>
            <el-form ref="formRef" :model="loginForm" :rules="loginRules" size="large">
              <!-- 租户选择 -->
              <el-form-item class="stagger-item" style="--i:0">
                <el-select
                  v-model="selectedTenantId"
                  placeholder="选择租户（默认宿主）"
                  clearable
                  filterable
                  :loading="tenantLoading"
                  class="glass-input tenant-select"
                  @change="handleTenantChange"
                >
                  <el-option
                    v-for="tenant in tenantList"
                    :key="tenant.id"
                    :label="tenant.name"
                    :value="tenant.id"
                  >
                    <div class="tenant-option">
                      <el-icon :size="14"><OfficeBuilding /></el-icon>
                      <span>{{ tenant.name }}</span>
                    </div>
                  </el-option>
                  <template #prefix>
                    <el-icon><OfficeBuilding /></el-icon>
                  </template>
                </el-select>
              </el-form-item>
              <el-form-item prop="userNameOrEmailAddress" class="stagger-item" style="--i:1">
                <el-input
                  v-model="loginForm.userNameOrEmailAddress"
                  placeholder="用户名 / 邮箱"
                  :prefix-icon="User"
                  clearable
                  autocomplete="username"
                  class="glass-input"
                />
              </el-form-item>
              <el-form-item prop="password" class="stagger-item" style="--i:2">
                <el-input
                  v-model="loginForm.password"
                  type="password"
                  placeholder="密码"
                  :prefix-icon="Lock"
                  show-password
                  autocomplete="current-password"
                  class="glass-input"
                  @keyup.enter="handleLogin"
                />
              </el-form-item>
              <el-form-item prop="captcha" class="stagger-item" style="--i:3">
                <div class="captcha-row">
                  <el-input
                    v-model="loginForm.captcha"
                    placeholder="验证码"
                    class="glass-input captcha-input"
                    maxlength="4"
                    @keyup.enter="handleLogin"
                  />
                  <div class="captcha-img" @click="refreshLoginCaptcha" title="点击刷新">
                    <CaptchaCanvas ref="loginCaptchaRef" @code="handleLoginCaptchaCode" />
                    <el-icon class="refresh-icon"><RefreshRight /></el-icon>
                  </div>
                </div>
              </el-form-item>
              <div class="form-options stagger-item" style="--i:4">
                <el-checkbox v-model="loginForm.rememberMe" class="remember-check">
                  记住密码
                </el-checkbox>
                <a href="javascript:;" class="forgot-link">忘记密码？</a>
              </div>
              <el-form-item class="stagger-item" style="--i:5">
                <el-button
                  type="primary"
                  class="submit-btn"
                  :loading="loading"
                  @click="handleLogin"
                >
                  <span v-if="!loading">立即登录</span>
                  <span v-else>登录中...</span>
                </el-button>
              </el-form-item>
            </el-form>
          </div>

          <!-- 注册表单 -->
          <div v-else key="register" class="form-panel">
            <p class="form-welcome">创建您的账户，开始使用</p>
            <el-form ref="registerFormRef" :model="registerForm" :rules="registerRules" size="large">
              <el-form-item prop="userName">
                <el-input
                  v-model="registerForm.userName"
                  placeholder="用户名"
                  :prefix-icon="User"
                  clearable
                  class="glass-input"
                />
              </el-form-item>
              <el-form-item prop="email">
                <el-input
                  v-model="registerForm.email"
                  placeholder="邮箱地址"
                  :prefix-icon="Message"
                  clearable
                  class="glass-input"
                />
              </el-form-item>
              <el-form-item prop="password">
                <el-input
                  v-model="registerForm.password"
                  type="password"
                  placeholder="设置密码"
                  :prefix-icon="Lock"
                  show-password
                  class="glass-input"
                />
              </el-form-item>
              <el-form-item prop="confirmPassword">
                <el-input
                  v-model="registerForm.confirmPassword"
                  type="password"
                  placeholder="确认密码"
                  :prefix-icon="Lock"
                  show-password
                  class="glass-input"
                />
              </el-form-item>
              <el-form-item prop="captcha">
                <div class="captcha-row">
                  <el-input
                    v-model="registerForm.captcha"
                    placeholder="验证码"
                    class="glass-input captcha-input"
                    maxlength="4"
                  />
                  <div class="captcha-img" @click="refreshRegisterCaptcha" title="点击刷新">
                    <CaptchaCanvas ref="registerCaptchaRef" @code="handleRegisterCaptchaCode" />
                    <el-icon class="refresh-icon"><RefreshRight /></el-icon>
                  </div>
                </div>
              </el-form-item>
              <el-form-item>
                <el-button
                  type="primary"
                  class="submit-btn"
                  :loading="loading"
                  @click="handleRegister"
                >
                  <span v-if="!loading">注册账户</span>
                  <span v-else>注册中...</span>
                </el-button>
              </el-form-item>
            </el-form>
          </div>
        </Transition>
      </div>
    </div>
  </div>
</template>

<style scoped lang="scss">
// ── CSS Variables ──────────────────────────────────────
.login-page {
  --bg-from: #080a0f;
  --bg-to: #0c1020;
  --orb-1: rgba(99, 102, 241, 0.25);
  --orb-2: rgba(79, 70, 229, 0.18);
  --orb-3: rgba(139, 92, 246, 0.12);
  --card-bg: rgba(16, 19, 28, 0.8);
  --card-border: rgba(255, 255, 255, 0.06);
  --input-bg: rgba(255, 255, 255, 0.04);
  --input-border: rgba(255, 255, 255, 0.08);
  --input-focus: rgba(99, 102, 241, 0.6);
  --text-primary: #e8ecf4;
  --text-secondary: rgba(123, 138, 168, 0.8);
  --tab-inactive: rgba(123, 138, 168, 0.5);
  --tab-active: #e8ecf4;
  --btn-from: #4f46e5;
  --btn-to: #6366f1;
  --left-bg: rgba(99, 102, 241, 0.04);
  --shadow: 0 40px 100px rgba(0, 0, 0, 0.6), 0 0 0 1px rgba(255, 255, 255, 0.04);

  // Light mode
  &:not(.dark) {
    --bg-from: #f0f2fa;
    --bg-to: #e8ecf8;
    --orb-1: rgba(99, 102, 241, 0.12);
    --orb-2: rgba(79, 70, 229, 0.08);
    --orb-3: rgba(139, 92, 246, 0.06);
    --card-bg: rgba(255, 255, 255, 0.75);
    --card-border: rgba(0, 0, 0, 0.06);
    --input-bg: rgba(0, 0, 0, 0.025);
    --input-border: rgba(0, 0, 0, 0.08);
    --input-focus: rgba(99, 102, 241, 0.4);
    --text-primary: #0c0e14;
    --text-secondary: #5a6478;
    --tab-inactive: #9ca3b4;
    --tab-active: #0c0e14;
    --left-bg: rgba(99, 102, 241, 0.03);
    --shadow: 0 32px 80px rgba(0, 0, 0, 0.08), 0 0 0 1px rgba(0, 0, 0, 0.04);
  }
}

// ── Page Container ─────────────────────────────────────
.login-page {
  min-height: 100vh;
  background: linear-gradient(135deg, var(--bg-from) 0%, var(--bg-to) 100%);
  display: flex;
  align-items: center;
  justify-content: center;
  overflow: hidden;
  position: relative;
  transition: background 0.4s;
}

// ── Background Effects ─────────────────────────────────
.bg-grid {
  position: absolute;
  inset: 0;
  background-image:
    linear-gradient(rgba(99, 102, 241, 0.03) 1px, transparent 1px),
    linear-gradient(90deg, rgba(99, 102, 241, 0.03) 1px, transparent 1px);
  background-size: 48px 48px;
  pointer-events: none;
}

.bg-orb {
  position: absolute;
  border-radius: 50%;
  filter: blur(100px);
  pointer-events: none;
  animation: float 10s ease-in-out infinite;
}
.orb-1 { width: 500px; height: 500px; top: -180px; left: -120px; background: var(--orb-1); animation-delay: 0s; }
.orb-2 { width: 400px; height: 400px; bottom: -120px; right: -80px; background: var(--orb-2); animation-delay: -4s; }
.orb-3 { width: 350px; height: 350px; top: 50%; left: 50%; transform: translate(-50%, -50%); background: var(--orb-3); animation-delay: -7s; }

@keyframes float {
  0%, 100% { transform: translateY(0) scale(1); }
  50% { transform: translateY(-20px) scale(1.03); }
}
.orb-3 { animation-name: float-center; }
@keyframes float-center {
  0%, 100% { transform: translate(-50%, -50%) scale(1); }
  50% { transform: translate(-50%, calc(-50% - 15px)) scale(1.05); }
}

// ── Theme Toggle ───────────────────────────────────────
.theme-toggle {
  position: fixed;
  top: 20px;
  right: 20px;
  width: 40px;
  height: 40px;
  border-radius: 10px;
  border: 1px solid var(--card-border);
  background: var(--card-bg);
  backdrop-filter: blur(16px);
  -webkit-backdrop-filter: blur(16px);
  color: var(--text-primary);
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: all 0.25s cubic-bezier(0.4, 0, 0.2, 1);
  z-index: 100;

  &:hover {
    border-color: rgba(99, 102, 241, 0.4);
    background: rgba(99, 102, 241, 0.1);
  }
}

.theme-icon-enter-active,
.theme-icon-leave-active {
  transition: all 0.2s cubic-bezier(0.4, 0, 0.2, 1);
}
.theme-icon-enter-from {
  opacity: 0;
  transform: rotate(-90deg) scale(0.6);
}
.theme-icon-leave-to {
  opacity: 0;
  transform: rotate(90deg) scale(0.6);
}

// ── Login Card ─────────────────────────────────────────
.login-card {
  position: relative;
  z-index: 10;
  width: 860px;
  min-height: 520px;
  display: flex;
  border-radius: 20px;
  border: 1px solid var(--card-border);
  background: var(--card-bg);
  backdrop-filter: blur(40px) saturate(180%);
  -webkit-backdrop-filter: blur(40px) saturate(180%);
  box-shadow: var(--shadow);
  overflow: hidden;
  animation: card-in 0.6s cubic-bezier(0.16, 1, 0.3, 1);
}
@keyframes card-in {
  from { opacity: 0; transform: translateY(24px) scale(0.98); }
  to   { opacity: 1; transform: translateY(0) scale(1); }
}

// ── Left Decorative Panel ──────────────────────────────
.card-left {
  width: 280px;
  flex-shrink: 0;
  padding: 40px 28px;
  background: var(--left-bg);
  border-right: 1px solid var(--card-border);
  display: flex;
  flex-direction: column;
  gap: 28px;
  position: relative;
  overflow: hidden;

  .brand {
    .brand-icon {
      width: 48px;
      height: 48px;
      margin-bottom: 14px;
      svg { width: 100%; height: 100%; }
    }
    .brand-name {
      font-size: 20px;
      font-weight: 700;
      color: var(--text-primary);
      margin: 0 0 4px;
      letter-spacing: -0.3px;
    }
    .brand-desc {
      font-size: 11px;
      color: var(--text-secondary);
      margin: 0;
      letter-spacing: 1.5px;
      text-transform: uppercase;
      font-weight: 500;
    }
  }

  .feature-list {
    display: flex;
    flex-direction: column;
    gap: 12px;
    .feature-item {
      display: flex;
      align-items: center;
      gap: 10px;
      font-size: 12.5px;
      color: var(--text-secondary);
      .dot {
        width: 5px;
        height: 5px;
        border-radius: 50%;
        background: #6366f1;
        flex-shrink: 0;
        box-shadow: 0 0 6px rgba(99, 102, 241, 0.5);
      }
    }
  }

  .left-footer {
    margin-top: auto;
    font-size: 10.5px;
    color: var(--text-secondary);
    opacity: 0.4;
  }
}

// Geometric mesh pattern in left panel
.card-left-mesh {
  position: absolute;
  inset: 0;
  pointer-events: none;
  opacity: 0.4;
  background:
    radial-gradient(circle at 20% 80%, rgba(99, 102, 241, 0.08) 0%, transparent 50%),
    radial-gradient(circle at 80% 20%, rgba(139, 92, 246, 0.06) 0%, transparent 50%);
  &::before {
    content: '';
    position: absolute;
    inset: 0;
    background-image:
      linear-gradient(rgba(99, 102, 241, 0.04) 1px, transparent 1px),
      linear-gradient(90deg, rgba(99, 102, 241, 0.04) 1px, transparent 1px);
    background-size: 24px 24px;
    mask-image: linear-gradient(180deg, transparent 0%, black 30%, black 70%, transparent 100%);
    -webkit-mask-image: linear-gradient(180deg, transparent 0%, black 30%, black 70%, transparent 100%);
  }
}

// ── Right Form Panel ───────────────────────────────────
.card-right {
  flex: 1;
  padding: 40px 40px 36px;
  display: flex;
  flex-direction: column;
}

// ── Tab Switch ─────────────────────────────────────────
.panel-tabs {
  display: flex;
  margin-bottom: 24px;
  position: relative;
  width: fit-content;
  background: var(--input-bg);
  border-radius: 10px;
  padding: 3px;
  border: 1px solid var(--input-border);

  .tab-btn {
    position: relative;
    z-index: 2;
    padding: 7px 24px;
    border: none;
    background: transparent;
    border-radius: 7px;
    font-size: 13px;
    font-weight: 500;
    color: var(--tab-inactive);
    cursor: pointer;
    transition: color 0.25s;
    &.active { color: var(--tab-active); }
  }

  .tab-indicator {
    position: absolute;
    top: 3px;
    left: 3px;
    width: calc(50% - 3px);
    height: calc(100% - 6px);
    background: linear-gradient(135deg, #4f46e5, #6366f1);
    border-radius: 7px;
    transition: transform 0.3s cubic-bezier(0.4, 0, 0.2, 1);
    box-shadow: 0 4px 16px rgba(99, 102, 241, 0.3);
    &.right { transform: translateX(100%); }
  }
}

.form-welcome {
  font-size: 12.5px;
  color: var(--text-secondary);
  margin: 0 0 18px;
}

// ── Login Error Banner ─────────────────────────────────
.login-error-banner {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 10px 12px;
  margin-bottom: 14px;
  border-radius: 10px;
  background: rgba(248, 113, 113, 0.08);
  border: 1px solid rgba(248, 113, 113, 0.2);
  color: #fca5a5;
  font-size: 12.5px;
  .error-icon { flex-shrink: 0; color: #f87171; }
  span { flex: 1; }
  .close-icon {
    flex-shrink: 0;
    cursor: pointer;
    opacity: 0.5;
    transition: opacity 0.2s;
    &:hover { opacity: 1; }
  }
}

// ── Stagger Entrance Animations ────────────────────────
.stagger-item {
  animation: stagger-in 0.4s cubic-bezier(0.16, 1, 0.3, 1) both;
  animation-delay: calc(0.08s * var(--i, 0) + 0.3s);
}

@keyframes stagger-in {
  from {
    opacity: 0;
    transform: translateY(10px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

// ── Glass Input ────────────────────────────────────────
.glass-input {
  :deep(.el-input__wrapper) {
    background: var(--input-bg) !important;
    border: 1px solid var(--input-border) !important;
    border-radius: 10px !important;
    box-shadow: none !important;
    transition: border-color 0.25s cubic-bezier(0.4, 0, 0.2, 1),
                box-shadow 0.25s cubic-bezier(0.4, 0, 0.2, 1);
    &:hover { border-color: rgba(99, 102, 241, 0.3) !important; }
    &.is-focus {
      border-color: var(--input-focus) !important;
      box-shadow: 0 0 0 3px rgba(99, 102, 241, 0.1),
                  0 0 16px rgba(99, 102, 241, 0.06) !important;
    }
    .el-input__inner {
      color: var(--text-primary) !important;
      font-size: 13px;
      &::placeholder { color: var(--text-secondary) !important; }
    }
    .el-input__prefix-inner .el-icon { color: var(--text-secondary) !important; }
  }
}

// ── Tenant Select ──────────────────────────────────────
.tenant-select {
  width: 100%;

  :deep(.el-select__wrapper) {
    background: var(--input-bg) !important;
    border: 1px solid var(--input-border) !important;
    border-radius: 10px !important;
    box-shadow: none !important;
    min-height: 42px;
    transition: border-color 0.25s cubic-bezier(0.4, 0, 0.2, 1),
                box-shadow 0.25s cubic-bezier(0.4, 0, 0.2, 1);

    &:hover { border-color: rgba(99, 102, 241, 0.3) !important; }
    &.is-focus {
      border-color: var(--input-focus) !important;
      box-shadow: 0 0 0 3px rgba(99, 102, 241, 0.1),
                  0 0 16px rgba(99, 102, 241, 0.06) !important;
    }

    .el-select__selected-label {
      color: var(--text-primary) !important;
      font-size: 13px;
    }

    .el-select__placeholder {
      color: var(--text-secondary) !important;
      font-size: 13px;
    }

    .el-select__prefix .el-icon {
      color: var(--text-secondary) !important;
    }
  }
}

.tenant-option {
  display: flex;
  align-items: center;
  gap: 8px;
  font-size: 13px;
}

// ── Captcha Row ────────────────────────────────────────
.captcha-row {
  display: flex;
  gap: 10px;
  width: 100%;
  .captcha-input { flex: 1; }
  .captcha-img {
    position: relative;
    width: 120px;
    height: 40px;
    border-radius: 10px;
    overflow: hidden;
    cursor: pointer;
    flex-shrink: 0;
    border: 1px solid var(--input-border);
    transition: border-color 0.2s;
    &:hover {
      border-color: rgba(99, 102, 241, 0.4);
      .refresh-icon { opacity: 1; }
    }
    canvas { width: 100%; height: 100%; display: block; }
    .refresh-icon {
      position: absolute;
      inset: 0;
      display: flex;
      align-items: center;
      justify-content: center;
      background: rgba(0, 0, 0, 0.45);
      color: #fff;
      opacity: 0;
      transition: opacity 0.2s;
    }
  }
}

// ── Form Options ───────────────────────────────────────
.form-options {
  width: 100%;
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin: 2px 0 14px;
  position: relative;
  z-index: 1;
  .remember-check {
    cursor: pointer;
    :deep(.el-checkbox__label) {
      color: var(--text-secondary);
      font-size: 12.5px;
      user-select: none;
    }
    :deep(.el-checkbox__inner) {
      background: var(--input-bg);
      border-color: var(--input-border);
      transition: background 0.2s, border-color 0.2s;
    }
    :deep(.el-checkbox__input.is-checked .el-checkbox__inner) {
      background: #6366f1 !important;
      border-color: #6366f1 !important;
    }
    :deep(.el-checkbox__input.is-checked .el-checkbox__inner::after) {
      border-color: #fff !important;
    }
  }
  .forgot-link {
    font-size: 12.5px;
    color: #6366f1;
    text-decoration: none;
    transition: color 0.2s;
    &:hover { color: #818cf8; }
  }
}

// ── Submit Button ──────────────────────────────────────
.submit-btn {
  width: 100%;
  height: 42px;
  font-size: 13.5px;
  font-weight: 600;
  border-radius: 10px;
  border: none;
  background: linear-gradient(135deg, var(--btn-from), var(--btn-to)) !important;
  box-shadow: 0 6px 20px rgba(79, 70, 229, 0.3);
  letter-spacing: 0.5px;
  transition: transform 0.2s, box-shadow 0.2s;
  &:hover:not(:disabled) {
    transform: translateY(-1px);
    box-shadow: 0 8px 28px rgba(79, 70, 229, 0.4);
  }
  &:active { transform: translateY(0); }
}

// ── Panel Transition ───────────────────────────────────
.slide-fade-enter-active,
.slide-fade-leave-active { transition: all 0.2s ease; }
.slide-fade-enter-from { opacity: 0; transform: translateX(16px); }
.slide-fade-leave-to   { opacity: 0; transform: translateX(-16px); }

:deep(.el-form-item) { margin-bottom: 16px; }
:deep(.el-form-item__error) { color: #f87171; font-size: 11.5px; }

// ── Responsive ─────────────────────────────────────────
@media (max-width: 768px) {
  .login-card { width: calc(100vw - 32px); flex-direction: column; }
  .card-left { width: 100%; padding: 24px 20px; flex-direction: row; align-items: center; gap: 16px; }
  .feature-list, .left-footer { display: none; }
  .card-right { padding: 24px 20px; }
}
</style>
