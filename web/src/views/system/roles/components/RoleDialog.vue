<script setup lang="ts">
import { ref, reactive, computed } from 'vue'
import { ElMessage } from 'element-plus'
import type { FormInstance, FormRules } from 'element-plus'
import {
  identityRoleApi,
  type IdentityRoleDto,
  type CreateIdentityRoleDto,
  type UpdateIdentityRoleDto,
} from '@/api/modules/identity'

// ── Emits & Expose ─────────────────────────────────────
const emit = defineEmits<{
  (e: 'success'): void
}>()

defineExpose({ open })

// ── State ──────────────────────────────────────────────
const visible = ref(false)
const submitLoading = ref(false)
const formRef = ref<FormInstance>()
const editId = ref<string | null>(null)
const concurrencyStamp = ref('')

const isEdit = computed(() => !!editId.value)

const form = reactive<CreateIdentityRoleDto>({
  name: '',
  isDefault: false,
  isPublic: false,
})

// ── Validation Rules ───────────────────────────────────
const rules: FormRules = {
  name: [
    { required: true, message: '请输入角色名称', trigger: 'blur' },
    { min: 2, max: 64, message: '角色名称长度 2-64 个字符', trigger: 'blur' },
  ],
}

// ── Methods ────────────────────────────────────────────
function resetForm() {
  form.name = ''
  form.isDefault = false
  form.isPublic = false
  editId.value = null
  concurrencyStamp.value = ''
  formRef.value?.clearValidate()
}

function open(role?: IdentityRoleDto) {
  resetForm()

  if (role) {
    editId.value = role.id
    concurrencyStamp.value = role.concurrencyStamp
    form.name = role.name
    form.isDefault = role.isDefault
    form.isPublic = role.isPublic
  }

  visible.value = true
}

function handleClose() {
  visible.value = false
}

async function handleSubmit() {
  try {
    await formRef.value?.validate()
  } catch {
    return
  }

  submitLoading.value = true
  try {
    if (isEdit.value) {
      const payload: UpdateIdentityRoleDto = {
        ...form,
        concurrencyStamp: concurrencyStamp.value,
      }
      await identityRoleApi.update(editId.value!, payload)
      ElMessage.success('角色更新成功')
    } else {
      await identityRoleApi.create(form)
      ElMessage.success('角色创建成功')
    }
    visible.value = false
    emit('success')
  } catch {
    ElMessage.error(isEdit.value ? '更新失败' : '创建失败')
  } finally {
    submitLoading.value = false
  }
}
</script>

<template>
  <el-dialog
    v-model="visible"
    :title="isEdit ? '编辑角色' : '新增角色'"
    width="480px"
    :close-on-click-modal="false"
    destroy-on-close
    @close="resetForm"
  >
    <el-form
      ref="formRef"
      :model="form"
      :rules="rules"
      label-width="80px"
      label-position="left"
      class="role-form"
    >
      <el-form-item label="角色名称" prop="name">
        <el-input
          v-model="form.name"
          placeholder="请输入角色名称"
          clearable
          maxlength="64"
        />
      </el-form-item>

      <el-form-item label="默认角色" prop="isDefault">
        <el-switch
          v-model="form.isDefault"
          active-text="是"
          inactive-text="否"
          inline-prompt
        />
      </el-form-item>

      <el-form-item label="公共角色" prop="isPublic">
        <el-switch
          v-model="form.isPublic"
          active-text="是"
          inactive-text="否"
          inline-prompt
        />
      </el-form-item>
    </el-form>

    <template #footer>
      <div class="dialog-footer">
        <button class="btn-ghost" @click="handleClose">取 消</button>
        <button class="btn-primary" :disabled="submitLoading" @click="handleSubmit">
          {{ submitLoading ? '提交中...' : '确 定' }}
        </button>
      </div>
    </template>
  </el-dialog>
</template>

<style scoped lang="scss">
// ── Form ────────────────────────────────────────────────
.role-form {
  :deep(.el-form-item) {
    margin-bottom: 18px;

    &:last-child {
      margin-bottom: 0;
    }

    .el-form-item__label {
      font-size: 12.5px;
      font-weight: 500;
      color: var(--text-secondary);
    }

    .el-form-item__error {
      font-size: 11.5px;
    }
  }
}

// ── Input Override ──────────────────────────────────────
:deep(.el-input) {
  .el-input__wrapper {
    background: var(--bg-input) !important;
    border: 1px solid var(--border-subtle) !important;
    border-radius: 8px !important;
    box-shadow: none !important;
    transition: border-color 0.25s cubic-bezier(0.4, 0, 0.2, 1),
                box-shadow 0.25s cubic-bezier(0.4, 0, 0.2, 1);

    &:hover {
      border-color: rgba(99, 102, 241, 0.3) !important;
    }

    &.is-focus {
      border-color: rgba(99, 102, 241, 0.6) !important;
      box-shadow: 0 0 0 3px rgba(99, 102, 241, 0.1),
                  0 0 16px rgba(99, 102, 241, 0.06) !important;
    }

    .el-input__inner {
      color: var(--text-primary);
      font-size: 13px;

      &::placeholder {
        color: var(--text-muted);
      }
    }

    .el-input__suffix .el-icon {
      color: var(--text-muted);
    }
  }
}

// ── Switch Override ─────────────────────────────────────
:deep(.el-switch) {
  --el-switch-on-color: #6366f1;
  --el-switch-off-color: var(--border-medium);

  .el-switch__label span {
    font-size: 12px;
    color: var(--text-secondary);
  }
}

// ── Dialog Footer ───────────────────────────────────────
.dialog-footer {
  display: flex;
  justify-content: flex-end;
  gap: 10px;
}

// ── Buttons ─────────────────────────────────────────────
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
  letter-spacing: 0.3px;

  &:hover:not(:disabled) {
    transform: translateY(-1px);
    box-shadow: 0 6px 20px rgba(79, 70, 229, 0.4);
  }

  &:active {
    transform: translateY(0);
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
  padding: 8px 20px;
  font-size: 12.5px;
  font-weight: 500;
  color: var(--text-secondary);
  background: transparent;
  border: 1px solid var(--border-subtle);
  border-radius: 8px;
  cursor: pointer;
  transition: all 0.2s cubic-bezier(0.4, 0, 0.2, 1);

  &:hover {
    color: var(--text-primary);
    border-color: var(--border-medium);
    background: var(--bg-hover);
  }
}
</style>
