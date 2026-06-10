<script setup lang="ts">
import { ref, reactive, watch } from 'vue'
import { ElMessage } from 'element-plus'
import type { FormInstance, FormRules } from 'element-plus'
import { authApi } from '@/api/modules/auth'

const props = defineProps<{
  visible: boolean
}>()

const emit = defineEmits<{
  (e: 'update:visible', value: boolean): void
  (e: 'success'): void
}>()

const submitting = ref(false)
const formRef = ref<FormInstance>()

const form = reactive({
  currentPassword: '',
  newPassword: '',
  confirmPassword: '',
})

const rules: FormRules = {
  currentPassword: [
    { required: true, message: '请输入当前密码', trigger: 'blur' },
  ],
  newPassword: [
    { required: true, message: '请输入新密码', trigger: 'blur' },
    { min: 6, message: '密码至少 6 位', trigger: 'blur' },
  ],
  confirmPassword: [
    { required: true, message: '请确认新密码', trigger: 'blur' },
    {
      validator: (_rule, value, callback) => {
        if (value !== form.newPassword) {
          callback(new Error('两次密码不一致'))
        } else {
          callback()
        }
      },
      trigger: 'blur',
    },
  ],
}

watch(() => props.visible, (val) => {
  if (val) {
    form.currentPassword = ''
    form.newPassword = ''
    form.confirmPassword = ''
    formRef.value?.clearValidate()
  }
})

function handleClose() {
  emit('update:visible', false)
}

function handleVisibleChange(val: boolean) {
  emit('update:visible', val)
}

async function handleSubmit() {
  try {
    await formRef.value?.validate()
  } catch {
    return
  }

  submitting.value = true
  try {
    await authApi.changePassword({
      currentPassword: form.currentPassword,
      newPassword: form.newPassword,
    })
    ElMessage.success('密码修改成功')
    emit('update:visible', false)
    emit('success')
  } catch {
    ElMessage.error('密码修改失败')
  } finally {
    submitting.value = false
  }
}
</script>

<template>
  <el-dialog
    :model-value="visible"
    @update:model-value="handleVisibleChange"
    title="修改密码"
    width="440px"
    :close-on-click-modal="false"
    destroy-on-close
    @close="handleClose"
  >
    <el-form
      ref="formRef"
      :model="form"
      :rules="rules"
      label-width="80px"
      label-position="left"
    >
      <el-form-item label="当前密码" prop="currentPassword">
        <el-input
          v-model="form.currentPassword"
          type="password"
          show-password
          placeholder="请输入当前密码"
        />
      </el-form-item>
      <el-form-item label="新密码" prop="newPassword">
        <el-input
          v-model="form.newPassword"
          type="password"
          show-password
          placeholder="请输入新密码"
        />
      </el-form-item>
      <el-form-item label="确认密码" prop="confirmPassword">
        <el-input
          v-model="form.confirmPassword"
          type="password"
          show-password
          placeholder="请再次输入新密码"
        />
      </el-form-item>
    </el-form>
    <template #footer>
      <div class="dialog-footer">
        <button class="btn-ghost" @click="handleClose">取消</button>
        <button class="btn-primary" :disabled="submitting" @click="handleSubmit">
          {{ submitting ? '提交中...' : '确定' }}
        </button>
      </div>
    </template>
  </el-dialog>
</template>

<style scoped lang="scss">
.dialog-footer {
  display: flex;
  justify-content: flex-end;
  gap: 10px;
}

.btn-primary {
  display: inline-flex;
  align-items: center;
  gap: 5px;
  padding: 8px 20px;
  font-size: 12.5px;
  font-weight: 600;
  color: #fff;
  background: linear-gradient(135deg, #4f46e5, #6366f1);
  border: none;
  border-radius: 8px;
  cursor: pointer;
  transition: transform 0.2s, box-shadow 0.2s, opacity 0.2s;
  box-shadow: 0 4px 14px rgba(79, 70, 229, 0.3);

  &:hover:not(:disabled) { transform: translateY(-1px); box-shadow: 0 6px 20px rgba(79, 70, 229, 0.4); }
  &:disabled { opacity: 0.6; cursor: not-allowed; }
}

.btn-ghost {
  display: inline-flex;
  align-items: center;
  gap: 5px;
  padding: 8px 20px;
  font-size: 12.5px;
  font-weight: 500;
  color: var(--text-secondary);
  background: transparent;
  border: 1px solid var(--border-subtle);
  border-radius: 8px;
  cursor: pointer;
  transition: all 0.2s;

  &:hover {
    color: var(--text-primary);
    border-color: var(--border-medium);
    background: var(--bg-hover);
  }
}

:deep(.el-input__wrapper) {
  background: var(--input-bg) !important;
  border: 1px solid var(--border-subtle) !important;
  border-radius: 8px !important;
  box-shadow: none !important;

  &:hover { border-color: rgba(99, 102, 241, 0.3) !important; }
  &.is-focus {
    border-color: rgba(99, 102, 241, 0.6) !important;
    box-shadow: 0 0 0 3px rgba(99, 102, 241, 0.1) !important;
  }
  .el-input__inner { color: var(--text-primary); font-size: 13px; }
}
</style>
