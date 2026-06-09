<script setup lang="ts">
import { ref, reactive, computed, watch, nextTick } from 'vue'
import { ElMessage } from 'element-plus'
import type { FormInstance, FormRules } from 'element-plus'
import { useI18n } from 'vue-i18n'
import {
  identityRoleApi,
  type IdentityRoleDto,
  type CreateIdentityRoleDto,
  type UpdateIdentityRoleDto,
} from '@/api/modules/identity'
import { menuApi, type MenuDto } from '@/api/modules/menus'

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

// ── Menu Permission Tree ───────────────────────────────
const allMenus = ref<MenuDto[]>([])
const menuTreeLoading = ref(false)
const checkedMenuIds = ref<string[]>([])
const menuTreeRef = ref()

interface MenuTreeNode {
  id: string
  label: string
  children?: MenuTreeNode[]
}

const menuTreeData = computed<MenuTreeNode[]>(() => {
  return buildTree(allMenus.value, null)
})

function buildTree(menus: MenuDto[], parentId: string | null): MenuTreeNode[] {
  return menus
    .filter(m => m.parentId === parentId)
    .sort((a, b) => a.sort - b.sort)
    .map(m => ({
      id: m.id,
      label: m.name,
      children: buildTree(menus, m.id),
    }))
}

const isAdminRole = computed(() => {
  const name = form.name.toLowerCase()
  return name === 'admin' || name === 'superadmin'
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
  allMenus.value = []
  checkedMenuIds.value = []
  formRef.value?.clearValidate()
}

async function open(role?: IdentityRoleDto) {
  resetForm()

  if (role) {
    editId.value = role.id
    concurrencyStamp.value = role.concurrencyStamp
    form.name = role.name
    form.isDefault = role.isDefault
    form.isPublic = role.isPublic
  }

  visible.value = true

  // 加载菜单数据
  await loadMenuData()
}

async function loadMenuData() {
  menuTreeLoading.value = true
  try {
    // 获取所有菜单（flat list）
    const { data } = await menuApi.getList({ maxResultCount: 1000, sorting: 'sort' })
    allMenus.value = data.items

    let targetIds: string[] = []

    if (isEdit.value) {
      // 编辑模式：从 API 获取当前角色的菜单权限
      try {
        const { data: raw } = await menuApi.getRoleMenuIds(form.name)
        // ABP 可能返回 string[] 或 { items: string[] }
        const ids = Array.isArray(raw) ? raw : ((raw as any)?.items ?? [])
        targetIds = ids
      } catch {
        targetIds = isAdminRole.value
          ? allMenus.value.map(m => m.id)
          : []
      }
    } else {
      // 新增模式：admin/superadmin 默认全选
      targetIds = isAdminRole.value
        ? allMenus.value.map(m => m.id)
        : []
    }

    checkedMenuIds.value = targetIds

    // 确保 tree 组件同步 checked 状态（异步加载场景下需要手动调用）
    await nextTick()
    if (menuTreeRef.value && targetIds.length > 0) {
      menuTreeRef.value.setCheckedKeys(targetIds)
    }
  } catch {
    allMenus.value = []
    checkedMenuIds.value = []
  } finally {
    menuTreeLoading.value = false
  }
}

// 监听角色名变化，admin/superadmin 自动全选
watch(() => form.name, (newName) => {
  if (!isEdit.value && allMenus.value.length > 0) {
    const name = newName.toLowerCase()
    if (name === 'admin' || name === 'superadmin') {
      checkedMenuIds.value = allMenus.value.map(m => m.id)
    }
  }
})

function handleGrantAll() {
  checkedMenuIds.value = allMenus.value.map(m => m.id)
}

function handleClearAll() {
  checkedMenuIds.value = []
}

// eslint-disable-next-line @typescript-eslint/no-explicit-any
function handleTreeCheck(_node: any, data: any) {
  checkedMenuIds.value = (data.checkedKeys as (string | number)[]).map(String)
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
    } else {
      await identityRoleApi.create(form)
    }

    // 保存菜单权限
    await menuApi.setRoleMenuIds(form.name, checkedMenuIds.value)

    ElMessage.success(isEdit.value ? t('role.updateSuccess') : t('role.createSuccess'))
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
      class="role-form"
    >
      <el-form-item :label="t('role.roleName')" prop="name">
        <el-input
          v-model="form.name"
          :placeholder="t('role.roleNamePlaceholder')"
          :disabled="isEdit"
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

      <!-- 菜单权限 -->
      <el-form-item :label="t('role.menuPermissions')" prop="menuPermissions">
        <div class="menu-permissions-section">
          <div class="menu-permissions-header">
            <span class="menu-permissions-hint">{{ t('role.menuPermissionsHint') }}</span>
            <div class="menu-permissions-actions">
              <button type="button" class="btn-link" @click="handleGrantAll">
                {{ t('role.grantAllMenus') }}
              </button>
              <button type="button" class="btn-link btn-link--muted" @click="handleClearAll">
                {{ t('role.clearAllMenus') }}
              </button>
            </div>
          </div>
          <div class="menu-tree-wrap" v-loading="menuTreeLoading">
            <el-tree
              ref="menuTreeRef"
              :data="menuTreeData"
              show-checkbox
              node-key="id"
              :check-strictly="false"
              :props="{ children: 'children', label: 'label' }"
              :default-expand-all="true"
              :checked-keys="checkedMenuIds"
              @check="handleTreeCheck"
            >
              <template #default="{ data }">
                <span class="tree-node-label">{{ data.label }}</span>
              </template>
            </el-tree>
          </div>
        </div>
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

// ── Menu Permissions Section ────────────────────────────
.menu-permissions-section {
  width: 100%;
}

.menu-permissions-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 8px;
}

.menu-permissions-hint {
  font-size: 12px;
  color: var(--text-muted);
}

.menu-permissions-actions {
  display: flex;
  gap: 12px;
}

.btn-link {
  font-size: 12px;
  font-weight: 500;
  color: var(--primary-light);
  background: none;
  border: none;
  cursor: pointer;
  padding: 0;
  transition: color 0.2s;

  &:hover {
    color: var(--primary);
  }

  &--muted {
    color: var(--text-muted);

    &:hover {
      color: var(--text-secondary);
    }
  }
}

.menu-tree-wrap {
  border: 1px solid var(--border-subtle);
  border-radius: 8px;
  padding: 8px;
  max-height: 320px;
  overflow-y: auto;
  background: var(--bg-input);

  :deep(.el-tree) {
    --el-tree-node-hover-bg-color: var(--bg-hover);
    background: transparent;
    font-size: 13px;

    .el-tree-node__content {
      height: 32px;
      border-radius: 4px;

      &:hover {
        background: var(--bg-hover);
      }
    }

    .el-checkbox {
      --el-checkbox-checked-bg-color: #6366f1;
      --el-checkbox-checked-input-border-color: #6366f1;
      --el-checkbox-input-border: var(--border-medium);
    }

    .el-tree-node__label {
      color: var(--text-primary);
    }
  }
}

.tree-node-label {
  font-size: 13px;
  color: var(--text-primary);
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

  &.is-disabled .el-input__wrapper {
    opacity: 0.5;
    background: var(--bg-elevated) !important;
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
