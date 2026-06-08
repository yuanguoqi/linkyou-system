<script setup lang="ts">
import { ref, reactive, computed } from 'vue'
import { ElMessage } from 'element-plus'
import type { FormInstance, FormRules } from 'element-plus'
import { useI18n } from 'vue-i18n'
import {
  identityRoleApi,
  type IdentityRoleDto,
  type CreateIdentityRoleDto,
  type UpdateIdentityRoleDto,
} from '@/api/modules/identity'

const { t } = useI18n()

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
const rules = computed<FormRules>(() => ({
  name: [
    { required: true, message: t('validation.roleNameRequired'), trigger: 'blur' },
    { min: 2, max: 64, message: t('validation.roleNameLength'), trigger: 'blur' },
  ],
}))

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
      ElMessage.success(t('role.updateSuccess'))
    } else {
      await identityRoleApi.create(form)
      ElMessage.success(t('role.createSuccess'))
    }
    visible.value = false
    emit('success')
  } catch {
    ElMessage.error(isEdit.value ? t('common.updateFailed') : t('common.createFailed'))
  } finally {
    submitLoading.value = false
  }
}
</script>

<template>
  <el-dialog
    v-model="visible"
    :title="isEdit ? t('role.editRole') : t('role.createRole')"
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
      <el-form-item :label="t('role.roleName')" prop="name">
        <el-input
          v-model="form.name"
          :placeholder="t('role.roleNamePlaceholder')"
          clearable
          maxlength="64"
        />
      </el-form-item>

      <el-form-item :label="t('role.defaultRole')" prop="isDefault">
        <el-switch
          v-model="form.isDefault"
          :active-text="t('common.yes')"
          :inactive-text="t('common.no')"
          inline-prompt
        />
      </el-form-item>

      <el-form-item :label="t('role.publicRole')" prop="isPublic">
        <el-switch
          v-model="form.isPublic"
          :active-text="t('common.yes')"
          :inactive-text="t('common.no')"
          inline-prompt
        />
      </el-form-item>
    </el-form>

    <template #footer>
      <div class="dialog-footer">
        <button class="btn-ghost" @click="handleClose">{{ t('common.cancel') }}</button>
        <button class="btn-primary" :disabled="submitLoading" @click="handleSubmit">
          {{ submitLoading ? t('common.submitting') : t('common.confirm') }}
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
