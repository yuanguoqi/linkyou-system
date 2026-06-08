<script setup lang="ts">
import { ref, reactive, watch, computed } from 'vue'
import type { FormInstance, FormRules } from 'element-plus'
import { ElMessage } from 'element-plus'
import { useI18n } from 'vue-i18n'
import { tenantApi } from '@/api/modules/tenants'
import type { TenantDto, CreateTenantDto, UpdateTenantDto } from '@/api/modules/tenants'

const { t } = useI18n()

interface Props {
  visible: boolean
  mode: 'create' | 'edit'
  data: TenantDto | null
}

const props = defineProps<Props>()

const emit = defineEmits<{
  'update:visible': [value: boolean]
  success: []
}>()

// ── State ──────────────────────────────────────────────
const formRef = ref<FormInstance>()
const submitting = ref(false)

const form = reactive({
  name: '',
  adminEmailAddress: 'admin@linkyou.com',
  adminPassword: 'Admin@123456',
})

const isCreate = computed(() => props.mode === 'create')

const dialogTitle = computed(() => isCreate.value ? t('tenant.createTenant') : t('tenant.editTenant'))

// ── Validation Rules ───────────────────────────────────
const rules = computed<FormRules>(() => ({
  name: [
    { required: true, message: t('validation.tenantNameRequired'), trigger: 'blur' },
    { max: 256, message: t('validation.tenantNameMaxLength'), trigger: 'blur' },
  ],
  adminEmailAddress: isCreate.value
    ? [
        { required: true, message: t('validation.adminEmailRequired'), trigger: 'blur' },
        { type: 'email', message: t('validation.emailInvalid'), trigger: 'blur' },
      ]
    : [
        { type: 'email', message: t('validation.emailInvalid'), trigger: 'blur' },
      ],
  adminPassword: isCreate.value
    ? [
        { required: true, message: t('validation.adminPasswordRequired'), trigger: 'blur' },
        { min: 6, message: t('validation.passwordMinLength'), trigger: 'blur' },
      ]
    : [
        { min: 6, message: t('validation.passwordMinLength'), trigger: 'blur' },
      ],
}))

// ── Watchers ───────────────────────────────────────────
watch(() => props.visible, (val) => {
  if (val) {
    resetForm()
    if (props.mode === 'edit' && props.data) {
      form.name = props.data.name
    }
  }
})

// ── Methods ────────────────────────────────────────────
function resetForm() {
  form.name = ''
  form.adminEmailAddress = ''
  form.adminPassword = ''
  formRef.value?.clearValidate()
}

function handleClose() {
  emit('update:visible', false)
}

async function handleSubmit() {
  try {
    await formRef.value?.validate()
  } catch {
    return
  }

  submitting.value = true
  try {
    if (isCreate.value) {
      const payload: CreateTenantDto = {
        name: form.name,
        adminEmailAddress: form.adminEmailAddress,
        adminPassword: form.adminPassword,
      }
      await tenantApi.create(payload)
      ElMessage.success(t('tenant.createSuccess'))
    } else {
      const payload: UpdateTenantDto = {
        name: form.name,
        concurrencyStamp: props.data!.concurrencyStamp,
      }
      await tenantApi.update(props.data!.id, payload)
      ElMessage.success(t('tenant.updateSuccess'))
    }
    emit('success')
  } catch {
    // interceptor handles error toast
  } finally {
    submitting.value = false
  }
}
</script>

<template>
  <el-dialog
    :model-value="visible"
    :title="dialogTitle"
    width="480px"
    :close-on-click-modal="false"
    @close="handleClose"
  >
    <el-form
      ref="formRef"
      :model="form"
      :rules="rules"
      label-position="top"
      class="tenant-form"
    >
      <el-form-item :label="t('tenant.tenantName')" prop="name">
        <el-input
          v-model="form.name"
          :placeholder="t('tenant.tenantNamePlaceholder')"
          maxlength="256"
          show-word-limit
        />
      </el-form-item>

      <el-form-item :label="t('tenant.adminEmail')" prop="adminEmailAddress">
        <el-input
          v-model="form.adminEmailAddress"
          :placeholder="isCreate ? t('tenant.adminEmailPlaceholder') : t('tenant.leaveBlank')"
        />
      </el-form-item>

      <el-form-item :label="t('tenant.adminPassword')" prop="adminPassword">
        <el-input
          v-model="form.adminPassword"
          type="password"
          :placeholder="isCreate ? t('tenant.adminPasswordPlaceholder') : t('tenant.leaveBlank')"
          show-password
        />
      </el-form-item>
    </el-form>

    <template #footer>
      <div class="dialog-footer">
        <button class="btn btn-ghost" @click="handleClose">{{ t('common.cancel') }}</button>
        <button class="btn btn-primary" :disabled="submitting" @click="handleSubmit">
          <el-icon v-if="submitting" :size="14" class="spin"><Loading /></el-icon>
          {{ isCreate ? t('common.create') : t('common.save') }}
        </button>
      </div>
    </template>
  </el-dialog>
</template>

<style scoped lang="scss">
.tenant-form {
  padding: 4px 0;
}

.dialog-footer {
  display: flex;
  justify-content: flex-end;
  gap: 8px;
}

// ── Buttons ────────────────────────────────────────────
.btn {
  display: inline-flex;
  align-items: center;
  gap: 5px;
  padding: 7px 16px;
  border-radius: 8px;
  font-size: 12.5px;
  font-weight: 500;
  cursor: pointer;
  border: none;
  transition: all 0.2s cubic-bezier(0.4, 0, 0.2, 1);

  &:disabled {
    opacity: 0.6;
    cursor: not-allowed;
  }
}

.btn-primary {
  background: linear-gradient(135deg, #6366f1, #818cf8);
  color: #fff;
  box-shadow: 0 2px 8px rgba(99, 102, 241, 0.3);

  &:not(:disabled):hover {
    box-shadow: 0 4px 16px rgba(99, 102, 241, 0.45);
    transform: translateY(-1px);
  }
}

.btn-ghost {
  background: transparent;
  color: var(--text-secondary);
  border: 1px solid var(--border-subtle);

  &:hover {
    background: var(--bg-hover);
    color: var(--text-primary);
    border-color: var(--border-primary);
  }
}

.spin {
  animation: spin 1s linear infinite;
}

@keyframes spin {
  from { transform: rotate(0deg); }
  to { transform: rotate(360deg); }
}

// ── Deep overrides: Form items ─────────────────────────
:deep(.el-form-item__label) {
  font-family: 'Plus Jakarta Sans', 'Inter', sans-serif;
  font-size: 12.5px;
  font-weight: 600;
  color: var(--text-secondary);
  letter-spacing: -0.1px;
  padding-bottom: 6px;
}

:deep(.el-input__wrapper) {
  background: var(--input-bg);
  border: 1px solid var(--input-border);
  border-radius: 8px;
  box-shadow: none;
  transition: border-color 0.2s;

  &:hover {
    border-color: var(--border-medium);
  }

  &.is-focus {
    border-color: var(--primary);
    box-shadow: 0 0 0 2px var(--primary-dim);
  }

  .el-input__inner {
    color: var(--input-text);
    font-size: 13px;

    &::placeholder {
      color: var(--input-placeholder);
    }
  }
}
</style>
