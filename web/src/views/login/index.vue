<script setup lang="ts">
import { ref, reactive, onMounted } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { ElMessage } from 'element-plus'
import type { FormInstance, FormRules } from 'element-plus'
import { User, Lock, Message, RefreshRight, Sunny, Moon } from '@element-plus/icons-vue'
import { useAuthStore } from '@/stores/auth'
import { useThemeStore } from '@/stores/theme'
import type { LoginRequest } from '@/types/auth'
import CaptchaCanvas from './components/CaptchaCanvas.vue'

const router = useRouter()
const route = useRoute()
const authStore = useAuthStore()
const themeStore = useThemeStore()

// 切换面板：login | register
const activePanel = ref<'login' | 'register'>('login')
const formRef = ref<FormInstance>()
const registerFormRef = ref<FormInstance>()
const loading = ref(false)

// 验证码 - 登录和注册各自独立，避免面板切换时状态混乱
const loginCaptchaRef = ref<InstanceType<typeof CaptchaCanvas>>()
const registerCaptchaRef = ref<InstanceType<typeof CaptchaCanvas>>()
const loginCaptchaCode = ref('')
const registerCaptchaCode = ref('')

// 登录表单
const loginForm = reactive<LoginRequest & { captcha: string }>({
  userNameOrEmailAddress: '',
  password: '',
  rememberMe: false,
  captcha: '',
})

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

// 持久化记住密码 - 恢复用户名和密码
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
})

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
  // 切换面板时刷新对应验证码
  if (panel === 'login') {
    refreshLoginCaptcha()
  } else {
    refreshRegisterCaptcha()
  }
  formRef.value?.clearValidate()
  registerFormRef.value?.clearValidate()
}

async function handleLogin() {
  await formRef.value?.validate()
  loading.value = true
  try {
    await authStore.login({
      userNameOrEmailAddress: loginForm.userNameOrEmailAddress,
      password: loginForm.password,
      rememberMe: loginForm.rememberMe,
    })
    // 记住密码：同时保存用户名和密码（密码明文存储，适合内部系统）
    if (loginForm.rememberMe) {
      localStorage.setItem('remember_me', JSON.stringify({
        userNameOrEmailAddress: loginForm.userNameOrEmailAddress,
        password: loginForm.password,
      }))
    } else {
      localStorage.removeItem('remember_me')
    }
    ElMessage.success('登录成功，欢迎回来！')
    const redirect = (route.query.redirect as string) || '/'
    router.push(redirect)
  } catch {
    refreshLoginCaptcha()
  } finally {
    loading.value = false
  }
}

async function handleRegister() {
  await registerFormRef.value?.validate()
  loading.value = true
  try {
    // 注册接口预留，后续接入
    ElMessage.info('注册功能即将开放，请联系管理员')
  } finally {
    loading.value = false
    refreshRegisterCaptcha()
  }
}

</script>

<template>
  <div class="login-page" :class="{ dark: themeStore.isDark }">
    <!-- 动态背景粒子 -->
    <div class="bg-grid"></div>
    <div class="bg-orb orb-1"></div>
    <div class="bg-orb orb-2"></div>
    <div class="bg-orb orb-3"></div>

    <!-- 深色模式切换按钮 -->
    <button class="theme-toggle" @click="themeStore.toggleDark">
      <el-icon :size="20"><Sunny v-if="themeStore.isDark" /><Moon v-else /></el-icon>
    </button>

    <!-- 登录卡片 -->
    <div class="login-card">
      <!-- 左侧装饰面板 -->
      <div class="card-left">
        <div class="brand">
          <div class="brand-icon">
            <svg viewBox="0 0 48 48" fill="none" xmlns="http://www.w3.org/2000/svg">
              <path d="M24 4L44 14V34L24 44L4 34V14L24 4Z" stroke="currentColor" stroke-width="2" fill="none"/>
              <path d="M24 12L36 18V30L24 36L12 30V18L24 12Z" fill="currentColor" opacity="0.3"/>
              <circle cx="24" cy="24" r="6" fill="currentColor"/>
            </svg>
          </div>
          <h1 class="brand-name">领佑系统</h1>
          <p class="brand-desc">Enterprise Management Platform</p>
        </div>
        <div class="feature-list">
          <div class="feature-item"><span class="dot"></span>多租户企业级架构</div>
          <div class="feature-item"><span class="dot"></span>细粒度权限管理</div>
          <div class="feature-item"><span class="dot"></span>实时审计日志</div>
          <div class="feature-item"><span class="dot"></span>高性能 API 接口</div>
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
          <div class="tab-indicator" :class="{ right: activePanel === 'register' }"></div>
        </div>

        <!-- 登录表单 -->
        <Transition name="slide-fade" mode="out-in">
          <div v-if="activePanel === 'login'" key="login" class="form-panel">
            <p class="form-welcome">欢迎回来，请登录您的账户</p>
            <el-form ref="formRef" :model="loginForm" :rules="loginRules" size="large">
              <el-form-item prop="userNameOrEmailAddress">
                <el-input
                  v-model="loginForm.userNameOrEmailAddress"
                  placeholder="用户名 / 邮箱"
                  :prefix-icon="User"
                  clearable
                  autocomplete="username"
                  class="glass-input"
                />
              </el-form-item>
              <el-form-item prop="password">
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
              <el-form-item prop="captcha">
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
              <div class="form-options">
                <el-checkbox v-model="loginForm.rememberMe" class="remember-check">
                  记住密码
                </el-checkbox>
                <a href="javascript:;" class="forgot-link">忘记密码？</a>
              </div>
              <el-form-item>
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
// ===================== CSS 变量 =====================
.login-page {
  --bg-from: #0a0e1a;
  --bg-to: #0d1b2e;
  --orb-1: rgba(99, 102, 241, 0.35);
  --orb-2: rgba(14, 165, 233, 0.25);
  --orb-3: rgba(168, 85, 247, 0.2);
  --card-bg: rgba(255, 255, 255, 0.04);
  --card-border: rgba(255, 255, 255, 0.1);
  --input-bg: rgba(255, 255, 255, 0.07);
  --input-border: rgba(255, 255, 255, 0.12);
  --input-focus: rgba(99, 102, 241, 0.6);
  --text-primary: #f0f4ff;
  --text-secondary: rgba(200, 210, 240, 0.65);
  --tab-inactive: rgba(200, 210, 240, 0.45);
  --tab-active: #fff;
  --btn-from: #6366f1;
  --btn-to: #818cf8;
  --left-bg: rgba(99, 102, 241, 0.08);
  --shadow: 0 32px 80px rgba(0, 0, 0, 0.6);

  // 亮色模式覆盖
  &:not(.dark) {
    --bg-from: #e8eeff;
    --bg-to: #dde8f8;
    --orb-1: rgba(99, 102, 241, 0.18);
    --orb-2: rgba(14, 165, 233, 0.15);
    --orb-3: rgba(168, 85, 247, 0.12);
    --card-bg: rgba(255, 255, 255, 0.72);
    --card-border: rgba(180, 190, 240, 0.4);
    --input-bg: rgba(240, 244, 255, 0.8);
    --input-border: rgba(140, 155, 220, 0.35);
    --input-focus: rgba(99, 102, 241, 0.45);
    --text-primary: #1e2356;
    --text-secondary: #5a6490;
    --tab-inactive: #8b93c0;
    --tab-active: #1e2356;
    --left-bg: rgba(99, 102, 241, 0.06);
    --shadow: 0 32px 80px rgba(80, 100, 200, 0.15);
  }
}

// ===================== 页面容器 =====================
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

// ===================== 动态背景 =====================
.bg-grid {
  position: absolute;
  inset: 0;
  background-image:
    linear-gradient(rgba(99, 102, 241, 0.05) 1px, transparent 1px),
    linear-gradient(90deg, rgba(99, 102, 241, 0.05) 1px, transparent 1px);
  background-size: 48px 48px;
  pointer-events: none;
}

.bg-orb {
  position: absolute;
  border-radius: 50%;
  filter: blur(80px);
  pointer-events: none;
  animation: float 8s ease-in-out infinite;
}
.orb-1 { width: 600px; height: 600px; top: -200px; left: -150px; background: var(--orb-1); animation-delay: 0s; }
.orb-2 { width: 500px; height: 500px; bottom: -150px; right: -100px; background: var(--orb-2); animation-delay: -3s; }
.orb-3 { width: 400px; height: 400px; top: 50%; left: 50%; transform: translate(-50%, -50%); background: var(--orb-3); animation-delay: -6s; }

@keyframes float {
  0%, 100% { transform: translateY(0) scale(1); }
  50% { transform: translateY(-30px) scale(1.05); }
}
.orb-3 { animation-name: float-center; }
@keyframes float-center {
  0%, 100% { transform: translate(-50%, -50%) scale(1); }
  50% { transform: translate(-50%, calc(-50% - 20px)) scale(1.08); }
}

// ===================== 主题切换 =====================
.theme-toggle {
  position: fixed;
  top: 20px;
  right: 20px;
  width: 44px;
  height: 44px;
  border-radius: 50%;
  border: 1px solid var(--card-border);
  background: var(--card-bg);
  backdrop-filter: blur(12px);
  color: var(--text-primary);
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: all 0.3s;
  z-index: 100;
  &:hover {
    border-color: rgba(99, 102, 241, 0.6);
    background: rgba(99, 102, 241, 0.15);
    transform: rotate(20deg);
  }
}

// ===================== 登录卡片 =====================
.login-card {
  position: relative;
  z-index: 10;
  width: 880px;
  min-height: 540px;
  display: flex;
  border-radius: 24px;
  border: 1px solid var(--card-border);
  background: var(--card-bg);
  backdrop-filter: blur(24px);
  box-shadow: var(--shadow);
  overflow: hidden;
  animation: card-in 0.6s cubic-bezier(0.16, 1, 0.3, 1);
}
@keyframes card-in {
  from { opacity: 0; transform: translateY(30px) scale(0.97); }
  to   { opacity: 1; transform: translateY(0) scale(1); }
}

// ===================== 左侧装饰 =====================
.card-left {
  width: 300px;
  flex-shrink: 0;
  padding: 48px 36px;
  background: var(--left-bg);
  border-right: 1px solid var(--card-border);
  display: flex;
  flex-direction: column;
  gap: 32px;

  .brand {
    .brand-icon {
      width: 56px;
      height: 56px;
      color: #6366f1;
      margin-bottom: 16px;
      svg { width: 100%; height: 100%; }
    }
    .brand-name {
      font-size: 22px;
      font-weight: 700;
      color: var(--text-primary);
      margin: 0 0 6px;
      letter-spacing: 0.5px;
    }
    .brand-desc {
      font-size: 12px;
      color: var(--text-secondary);
      margin: 0;
      letter-spacing: 1px;
      text-transform: uppercase;
    }
  }

  .feature-list {
    display: flex;
    flex-direction: column;
    gap: 14px;
    .feature-item {
      display: flex;
      align-items: center;
      gap: 10px;
      font-size: 13px;
      color: var(--text-secondary);
      .dot {
        width: 6px;
        height: 6px;
        border-radius: 50%;
        background: linear-gradient(135deg, #6366f1, #818cf8);
        flex-shrink: 0;
        box-shadow: 0 0 8px rgba(99, 102, 241, 0.6);
      }
    }
  }

  .left-footer {
    margin-top: auto;
    font-size: 11px;
    color: var(--text-secondary);
    opacity: 0.5;
  }
}

// ===================== 右侧表单 =====================
.card-right {
  flex: 1;
  padding: 48px 48px 40px;
  display: flex;
  flex-direction: column;
}

// ===================== Tab 切换 =====================
.panel-tabs {
  display: flex;
  margin-bottom: 28px;
  position: relative;
  width: fit-content;
  background: var(--input-bg);
  border-radius: 10px;
  padding: 4px;
  border: 1px solid var(--input-border);

  .tab-btn {
    position: relative;
    z-index: 2;
    padding: 8px 28px;
    border: none;
    background: transparent;
    border-radius: 7px;
    font-size: 14px;
    font-weight: 500;
    color: var(--tab-inactive);
    cursor: pointer;
    transition: color 0.3s;
    &.active { color: var(--tab-active); }
  }

  .tab-indicator {
    position: absolute;
    top: 4px;
    left: 4px;
    width: calc(50% - 4px);
    height: calc(100% - 8px);
    background: linear-gradient(135deg, #6366f1, #818cf8);
    border-radius: 7px;
    transition: transform 0.3s cubic-bezier(0.4, 0, 0.2, 1);
    box-shadow: 0 4px 12px rgba(99, 102, 241, 0.4);
    &.right { transform: translateX(100%); }
  }
}

.form-welcome {
  font-size: 13px;
  color: var(--text-secondary);
  margin: 0 0 20px;
}

// ===================== 玻璃输入框 =====================
.glass-input {
  :deep(.el-input__wrapper) {
    background: var(--input-bg) !important;
    border: 1px solid var(--input-border) !important;
    border-radius: 10px !important;
    box-shadow: none !important;
    transition: border-color 0.25s, box-shadow 0.25s;
    &:hover { border-color: rgba(99, 102, 241, 0.4) !important; }
    &.is-focus {
      border-color: var(--input-focus) !important;
      box-shadow: 0 0 0 3px rgba(99, 102, 241, 0.12) !important;
    }
    .el-input__inner {
      color: var(--text-primary) !important;
      &::placeholder { color: var(--text-secondary) !important; }
    }
    .el-input__prefix-inner .el-icon { color: var(--text-secondary) !important; }
  }
}

// ===================== 验证码行 =====================
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
    transition: border-color 0.25s;
    &:hover {
      border-color: rgba(99, 102, 241, 0.5);
      .refresh-icon { opacity: 1; }
    }
    canvas { width: 100%; height: 100%; display: block; }
    .refresh-icon {
      position: absolute;
      inset: 0;
      display: flex;
      align-items: center;
      justify-content: center;
      background: rgba(0, 0, 0, 0.4);
      color: #fff;
      opacity: 0;
      transition: opacity 0.2s;
    }
  }
}

// ===================== 表单选项 =====================
.form-options {
  width: 100%;
  display: flex;
  justify-content: space-between;
  align-items: center;
  .remember-check {
    :deep(.el-checkbox__label) { color: var(--text-secondary); font-size: 13px; }
    :deep(.el-checkbox__inner) {
      background: var(--input-bg);
      border-color: var(--input-border);
    }
  }
  .forgot-link {
    font-size: 13px;
    color: #6366f1;
    text-decoration: none;
    transition: color 0.2s;
    &:hover { color: #818cf8; }
  }
}

// ===================== 提交按钮 =====================
.submit-btn {
  width: 100%;
  height: 46px;
  font-size: 15px;
  font-weight: 600;
  border-radius: 10px;
  border: none;
  background: linear-gradient(135deg, var(--btn-from), var(--btn-to)) !important;
  box-shadow: 0 8px 24px rgba(99, 102, 241, 0.35);
  letter-spacing: 1px;
  transition: transform 0.2s, box-shadow 0.2s;
  &:hover:not(:disabled) {
    transform: translateY(-2px);
    box-shadow: 0 12px 32px rgba(99, 102, 241, 0.5);
  }
  &:active { transform: translateY(0); }
}

// ===================== 面板切换动画 =====================
.slide-fade-enter-active,
.slide-fade-leave-active { transition: all 0.25s ease; }
.slide-fade-enter-from { opacity: 0; transform: translateX(20px); }
.slide-fade-leave-to   { opacity: 0; transform: translateX(-20px); }

:deep(.el-form-item) { margin-bottom: 18px; }
:deep(.el-form-item__error) { color: #f87171; font-size: 12px; }

// ===================== 响应式 =====================
@media (max-width: 768px) {
  .login-card { width: calc(100vw - 32px); flex-direction: column; }
  .card-left { width: 100%; padding: 28px 24px; flex-direction: row; align-items: center; gap: 20px; }
  .feature-list, .left-footer { display: none; }
  .card-right { padding: 24px; }
}
</style>
