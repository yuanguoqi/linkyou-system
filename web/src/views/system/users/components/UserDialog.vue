<script setup lang="ts">
import { ref, reactive, computed } from 'vue'
import { useI18n } from 'vue-i18n'
import { ElMessage } from 'element-plus'
import type { FormInstance, FormRules } from 'element-plus'
import { identityUserApi, identityRoleApi } from '@/api/modules/identity'
import type {
  IdentityUserDto,
  IdentityRoleDto,
  CreateIdentityUserDto,
  UpdateIdentityUserDto,
} from '@/api/modules/identity'

const { t } = useI18n()

// ── Emits & Expose ───────────────────────────────────
const emit = defineEmits<{
  (e: 'success'): void
}>()

defineExpose({ open })

// ── State ────────────────────────────────────────────
const visible = ref(false)
const submitLoading = ref(false)
const formRef = ref<FormInstance>()
const roles = ref<IdentityRoleDto[]>([])
const editId = ref<string | null>(null)
const concurrencyStamp = ref('')

const isEdit = computed(() => !!editId.value)

const form = reactive<CreateIdentityUserDto>({
  userName: '',
  email: '',
  name: '',
  surname: '',
  password: '',
  phoneNumber: '',
  isActive: true,
  roleNames: [],
})

// ── Validation Rules ─────────────────────────────────
const rules = computed<FormRules>(() => ({
  userName: [
    { required: true, message: t('validation.usernameRequired'), trigger: 'blur' },
    { min: 3, max: 20, message: t('validation.usernameLength'), trigger: 'blur' },
  ],
  email: [
    { required: true, message: t('validation.emailRequired'), trigger: 'blur' },
    { type: 'email', message: t('validation.emailInvalid'), trigger: 'blur' },
  ],
  name: [
    { max: 32, message: t('validation.nameMaxLength'), trigger: 'blur' },
  ],
  surname: [
    { max: 32, message: t('validation.surnameMaxLength'), trigger: 'blur' },
  ],
  password: isEdit.value
    ? [
        { min: 6, message: t('validation.passwordMinLength'), trigger: 'blur' },
      ]
    : [
        { required: true, message: t('validation.passwordRequired'), trigger: 'blur' },
        { min: 6, message: t('validation.passwordMinLength'), trigger: 'blur' },
      ],
}))

// ── Methods ──────────────────────────────────────────
function resetForm() {
  form.userName = ''
  form.email = ''
  form.name = ''
  form.surname = ''
  form.password = ''
  form.phoneNumber = ''
  form.isActive = true
  form.roleNames = []
  editId.value = null
  concurrencyStamp.value = ''
  formRef.value?.clearValidate()
}

async function fetchRoles() {
  try {
    const { data } = await identityRoleApi.getAllList()
    roles.value = data.items
  } catch {
    roles.value = []
  }
}

async function open(user?: IdentityUserDto) {
  resetForm()
  await fetchRoles()

  if (user) {
    editId.value = user.id
    concurrencyStamp.value = user.concurrencyStamp
    form.userName = user.userName
    form.email = user.email
    form.name = user.name ?? ''
    form.surname = user.surname ?? ''
    form.phoneNumber = user.phoneNumber ?? ''
    form.isActive = user.isActive

    // 通过 GetRoles API 获取用户已有角色（IdentityUserDto 不包含 roles）
    try {
      const { data: roleResult } = await identityUserApi.getRoles(user.id)
      form.roleNames = roleResult.items?.map(r => r.name) ?? []
    } catch {
      form.roleNames = []
    }
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
      const payload: UpdateIdentityUserDto = {
        ...form,
        concurrencyStamp: concurrencyStamp.value,
      }
      // Password is optional on edit; omit if empty
      if (!payload.password) {
        delete (payload as unknown as Record<string, unknown>).password
      }
      await identityUserApi.update(editId.value!, payload)
      ElMessage.success(t('user.updateSuccess'))
    } else {
      await identityUserApi.create(form)
      ElMessage.success(t('user.createSuccess'))
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
    :title="isEdit ? t('user.editUser') : t('user.createUser')"
    width="560px"
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
      class="user-form"
    >
      <el-form-item :label="t('user.username')" prop="userName">
        <el-input
          v-model="form.userName"
          :placeholder="t('user.usernamePlaceholder')"
          :disabled="isEdit"
          clearable
        />
      </el-form-item>

      <el-form-item :label="t('user.email')" prop="email">
        <el-input
          v-model="form.email"
          :placeholder="t('user.emailPlaceholder')"
          clearable
        />
      </el-form-item>

      <div class="form-row">
        <el-form-item :label="t('user.firstName')" prop="name" class="form-row-item">
          <el-input
            v-model="form.name"
            :placeholder="t('user.firstNamePlaceholder')"
            clearable
          />
        </el-form-item>

        <el-form-item :label="t('user.lastName')" prop="surname" class="form-row-item">
          <el-input
            v-model="form.surname"
            :placeholder="t('user.lastNamePlaceholder')"
            clearable
          />
        </el-form-item>
      </div>

      <el-form-item
        :label="t('user.password')"
        prop="password"
        :required="!isEdit"
      >
        <el-input
          v-model="form.password"
          type="password"
          :placeholder="isEdit ? t('user.passwordLeaveBlank') : t('validation.passwordRequired')"
          show-password
          clearable
        />
      </el-form-item>

      <el-form-item :label="t('user.phone')" prop="phoneNumber">
        <el-input
          v-model="form.phoneNumber"
          :placeholder="t('user.phonePlaceholder')"
          clearable
        />
      </el-form-item>

      <el-form-item :label="t('user.role')" prop="roleNames">
        <el-select
          v-model="form.roleNames"
          multiple
          collapse-tags
          collapse-tags-tooltip
          :placeholder="t('user.rolePlaceholder')"
          class="w-full"
        >
          <el-option
            v-for="role in roles"
            :key="role.id"
            :label="role.name"
            :value="role.name"
          />
        </el-select>
      </el-form-item>

      <el-form-item :label="t('common.status')" prop="isActive">
        <el-switch
          v-model="form.isActive"
          :active-text="t('common.enabled')"
          :inactive-text="t('common.disabled')"
          inline-prompt
        />
      </el-form-item>
    </el-form>

    <template #footer>
      <div class="dialog-footer">
        <button class="btn-ghost" @click="handleClose">{{ t('common.cancel') }}</button>
        <button class="btn-primary" :disabled="submitLoading" @click="handleSubmit">
          <el-icon v-if="submitLoading" :size="14" class="spin"><Loading /></el-icon>
          {{ submitLoading ? t('common.submitting') : t('common.confirm') }}
        </button>
      </div>
    </template>
  </el-dialog>
</template>

<style scoped lang="scss">
// ── Form ────────────────────────────────────────────────
.user-form {
  :deep(.el-form-item) {
    margin-bottom: 18px;

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

.form-row {
  display: flex;
  gap: 12px;

  .form-row-item {
    flex: 1;
  }
}

.w-full {
  width: 100%;
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

// ── Select Override ─────────────────────────────────────
:deep(.el-select) {
  width: 100%;

  .el-select__wrapper {
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

    .el-select__placeholder,
    .el-select__selected-item span {
      color: var(--text-primary);
      font-size: 13px;
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

// ── Spinner ─────────────────────────────────────────────
.spin {
  animation: spin 1s linear infinite;
}

@keyframes spin {
  from { transform: rotate(0deg); }
  to { transform: rotate(360deg); }
}
</style>
