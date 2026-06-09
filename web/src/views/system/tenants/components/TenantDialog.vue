<script setup lang="ts">
import { ref, reactive, watch, computed } from 'vue'
import type { FormInstance, FormRules } from 'element-plus'
import { ElMessage } from 'element-plus'
import { useI18n } from 'vue-i18n'
import { tenantApi } from '@/api/modules/tenants'
import type { TenantDto, CreateTenantDto, UpdateTenantDto } from '@/api/modules/tenants'
import { identityUserApi } from '@/api/modules/identity'
import { formatDateTime } from '@/utils/date'

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

// 编辑时保存的 DTO 完整数据（用于回显和提交）
const editData = ref<TenantDto | null>(null)

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
watch(() => props.visible, async (val) => {
  if (val) {
    resetForm()
    if (props.mode === 'edit' && props.data) {
      // 编辑模式：从 API 获取完整 DTO 数据
      try {
        const { data } = await tenantApi.get(props.data.id)
        editData.value = data
        form.name = data.name

        // 尝试获取该租户的管理员信息
        try {
          const { data: usersData } = await identityUserApi.getList({
            filter: 'admin',
            maxResultCount: 1,
          })
          if (usersData.items.length > 0) {
            form.adminEmailAddress = usersData.items[0].email || ''
          }
        } catch {
          // 获取管理员信息失败，留空
        }
        form.adminPassword = '' // 密码不可读，留空
      } catch {
        // 如果获取失败，使用列表页传入的数据
        editData.value = props.data
        form.name = props.data.name
      }
    } else {
      // 新增模式：设置默认值
      editData.value = null
      form.adminEmailAddress = 'admin@linkyou.com'
      form.adminPassword = 'Admin@123456'
    }
  }
})

// ── Methods ────────────────────────────────────────────
function resetForm() {
  form.name = ''
  form.adminEmailAddress = ''
  form.adminPassword = ''
  editData.value = null
  formRef.value?.clearValidate()
}

function handleClose() {
  emit('update:visible', false)
}

/** 重置管理员密码为默认密码 */
function handleResetPassword() {
  form.adminPassword = 'Admin@123456'
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
        concurrencyStamp: editData.value!.concurrencyStamp,
      }
      await tenantApi.update(editData.value!.id, payload)
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
        <div class="password-row">
          <el-input
            v-model="form.adminPassword"
            type="password"
            :placeholder="isCreate ? t('tenant.adminPasswordPlaceholder') : t('tenant.leaveBlank')"
            show-password
            class="password-input"
          />
          <button
            v-if="!isCreate"
            type="button"
            class="btn btn-ghost btn-reset"
            @click="handleResetPassword"
          >
            {{ t('tenant.resetPassword') }}
          </button>
        </div>
      </el-form-item>

      <!-- 编辑模式：显示只读信息 -->
      <template v-if="!isCreate && editData">
        <el-form-item :label="t('common.createTime')">
          <el-input :model-value="formatDateTime(editData.creationTime)" disabled />
        </el-form-item>
      </template>
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

.password-row {
  display: flex;
  gap: 8px;
  width: 100%;

  .password-input {
    flex: 1;
  }

  .btn-reset {
    flex-shrink: 0;
    white-space: nowrap;
    font-size: 12px;
    padding: 0 12px;
  }
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
