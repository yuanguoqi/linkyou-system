<script setup lang="ts">
import { ref, reactive, computed } from 'vue'
import { ElMessage } from 'element-plus'
import type { FormInstance, FormRules } from 'element-plus'
import { identityUserApi, identityRoleApi } from '@/api/modules/identity'
import type {
  IdentityUserDto,
  IdentityRoleDto,
  CreateIdentityUserDto,
  UpdateIdentityUserDto,
} from '@/api/modules/identity'

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
    { required: true, message: '请输入用户名', trigger: 'blur' },
    { min: 3, max: 20, message: '用户名长度 3-20 个字符', trigger: 'blur' },
  ],
  email: [
    { required: true, message: '请输入邮箱', trigger: 'blur' },
    { type: 'email', message: '请输入有效的邮箱地址', trigger: 'blur' },
  ],
  name: [
    { max: 32, message: '名字最多 32 个字符', trigger: 'blur' },
  ],
  surname: [
    { max: 32, message: '姓氏最多 32 个字符', trigger: 'blur' },
  ],
  password: isEdit.value
    ? [
        { min: 6, message: '密码至少 6 位', trigger: 'blur' },
      ]
    : [
        { required: true, message: '请输入密码', trigger: 'blur' },
        { min: 6, message: '密码至少 6 位', trigger: 'blur' },
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
  fetchRoles()

  if (user) {
    editId.value = user.id
    concurrencyStamp.value = user.concurrencyStamp
    form.userName = user.userName
    form.email = user.email
    form.name = user.name ?? ''
    form.surname = user.surname ?? ''
    form.phoneNumber = user.phoneNumber ?? ''
    form.isActive = user.isActive
    form.roleNames = user.roles?.map(r => r.roleName) ?? []
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
      ElMessage.success('用户更新成功')
    } else {
      await identityUserApi.create(form)
      ElMessage.success('用户创建成功')
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
    :title="isEdit ? '编辑用户' : '新增用户'"
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
      <el-form-item label="用户名" prop="userName">
        <el-input
          v-model="form.userName"
          placeholder="请输入用户名"
          :disabled="isEdit"
          clearable
        />
      </el-form-item>

      <el-form-item label="邮箱" prop="email">
        <el-input
          v-model="form.email"
          placeholder="请输入邮箱地址"
          clearable
        />
      </el-form-item>

      <div class="form-row">
        <el-form-item label="名" prop="name" class="form-row-item">
          <el-input
            v-model="form.name"
            placeholder="名"
            clearable
          />
        </el-form-item>

        <el-form-item label="姓" prop="surname" class="form-row-item">
          <el-input
            v-model="form.surname"
            placeholder="姓"
            clearable
          />
        </el-form-item>
      </div>

      <el-form-item
        label="密码"
        prop="password"
        :required="!isEdit"
      >
        <el-input
          v-model="form.password"
          type="password"
          :placeholder="isEdit ? '留空则不修改密码' : '请输入密码'"
          show-password
          clearable
        />
      </el-form-item>

      <el-form-item label="手机号" prop="phoneNumber">
        <el-input
          v-model="form.phoneNumber"
          placeholder="请输入手机号（可选）"
          clearable
        />
      </el-form-item>

      <el-form-item label="角色" prop="roleNames">
        <el-select
          v-model="form.roleNames"
          multiple
          collapse-tags
          collapse-tags-tooltip
          placeholder="请选择角色"
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

      <el-form-item label="状态" prop="isActive">
        <el-switch
          v-model="form.isActive"
          active-text="启用"
          inactive-text="禁用"
          inline-prompt
        />
      </el-form-item>
    </el-form>

    <template #footer>
      <div class="dialog-footer">
        <button class="btn-ghost" @click="handleClose">取 消</button>
        <button class="btn-primary" :disabled="submitLoading" @click="handleSubmit">
          <el-icon v-if="submitLoading" :size="14" class="spin"><Loading /></el-icon>
          {{ submitLoading ? '提交中...' : '确 定' }}
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
