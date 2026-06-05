<script setup lang="ts">
import { ref, reactive, computed, watch, nextTick } from 'vue'
import { ElMessage } from 'element-plus'
import type { FormInstance, FormRules } from 'element-plus'
import { menuApi } from '@/api/modules/menus'
import type { MenuDto, CreateMenuDto } from '@/api/modules/menus'

// ── Tree node type (MenuDto + optional children) ──────
interface MenuTreeNode extends MenuDto {
  children?: MenuTreeNode[]
}

// ── Props & Emits ──────────────────────────────────────
interface Props {
  visible: boolean
  editData?: MenuDto | null
  menuTree: MenuTreeNode[]
}

const props = withDefaults(defineProps<Props>(), {
  editData: null,
  menuTree: () => [],
})

const emit = defineEmits<{
  'update:visible': [value: boolean]
  saved: []
}>()

// ── State ──────────────────────────────────────────────
const formRef = ref<FormInstance>()
const loading = ref(false)

const defaultForm = (): CreateMenuDto => ({
  name: '',
  path: '',
  icon: '',
  parentId: undefined,
  sort: 0,
  isVisible: true,
  permission: '',
})

const form = reactive<CreateMenuDto>(defaultForm())

// ── Rules ──────────────────────────────────────────────
const rules: FormRules = {
  name: [
    { required: true, message: '请输入菜单名称', trigger: 'blur' },
  ],
  path: [
    { required: true, message: '请输入路由路径', trigger: 'blur' },
  ],
}

// ── Computed ───────────────────────────────────────────
const isEdit = computed(() => !!props.editData)
const dialogTitle = computed(() => isEdit.value ? '编辑菜单' : '新增菜单')

// ── Watchers ───────────────────────────────────────────
watch(() => props.visible, (val) => {
  if (val) {
    if (props.editData) {
      Object.assign(form, {
        name: props.editData.name,
        path: props.editData.path,
        icon: props.editData.icon || '',
        parentId: props.editData.parentId || undefined,
        sort: props.editData.sort,
        isVisible: props.editData.isVisible,
        permission: props.editData.permission || '',
      })
    } else {
      Object.assign(form, defaultForm())
    }
    nextTick(() => formRef.value?.clearValidate())
  }
})

// ── Methods ────────────────────────────────────────────
function handleClose() {
  emit('update:visible', false)
}

async function handleSubmit() {
  try {
    await formRef.value?.validate()
  } catch {
    return
  }

  loading.value = true
  try {
    const payload: CreateMenuDto = {
      name: form.name,
      path: form.path,
      icon: form.icon || undefined,
      parentId: form.parentId || undefined,
      sort: form.sort ?? 0,
      isVisible: form.isVisible ?? true,
      permission: form.permission || undefined,
    }

    if (isEdit.value && props.editData) {
      await menuApi.update(props.editData.id, payload)
      ElMessage.success('菜单更新成功')
    } else {
      await menuApi.create(payload)
      ElMessage.success('菜单创建成功')
    }

    emit('saved')
    handleClose()
  } catch (err: unknown) {
    const axiosErr = err as { response?: { data?: { error?: { message?: string } } } }
    ElMessage.error(axiosErr?.response?.data?.error?.message || '操作失败')
  } finally {
    loading.value = false
  }
}

// ── Filter parent menu options (exclude self and descendants when editing) ──
function filterMenuOptions(menus: MenuTreeNode[], excludeId?: string): MenuTreeNode[] {
  return menus
    .filter(m => m.id !== excludeId)
    .map(m => ({
      ...m,
      children: m.children ? filterMenuOptions(m.children, excludeId) : undefined,
    }))
}

const parentOptions = computed(() => {
  const excludeId = isEdit.value ? props.editData?.id : undefined
  return filterMenuOptions(props.menuTree, excludeId)
})
</script>

<template>
  <el-dialog
    :model-value="visible"
    :title="dialogTitle"
    width="520px"
    :close-on-click-modal="false"
    destroy-on-close
    @update:model-value="handleClose"
  >
    <el-form
      ref="formRef"
      :model="form"
      :rules="rules"
      label-width="90px"
      label-position="left"
      class="menu-form"
    >
      <el-form-item label="菜单名称" prop="name">
        <el-input
          v-model="form.name"
          placeholder="请输入菜单名称"
          clearable
        />
      </el-form-item>

      <el-form-item label="路由路径" prop="path">
        <el-input
          v-model="form.path"
          placeholder="请输入路由路径，如 /system/users"
          clearable
        />
      </el-form-item>

      <el-form-item label="图标" prop="icon">
        <el-input
          v-model="form.icon"
          placeholder="请输入图标名称（可选）"
          clearable
        />
      </el-form-item>

      <el-form-item label="上级菜单" prop="parentId">
        <el-tree-select
          v-model="form.parentId"
          :data="parentOptions"
          :props="{ label: 'name', children: 'children' }"
          node-key="id"
          placeholder="请选择上级菜单（可选）"
          clearable
          check-strictly
          style="width: 100%"
        />
      </el-form-item>

      <el-form-item label="排序" prop="sort">
        <el-input-number
          v-model="form.sort"
          :min="0"
          :max="9999"
          controls-position="right"
          style="width: 100%"
        />
      </el-form-item>

      <el-form-item label="是否可见" prop="isVisible">
        <el-switch
          v-model="form.isVisible"
          active-text="可见"
          inactive-text="隐藏"
        />
      </el-form-item>

      <el-form-item label="权限标识" prop="permission">
        <el-input
          v-model="form.permission"
          placeholder="请输入权限标识（可选）"
          clearable
        />
      </el-form-item>
    </el-form>

    <template #footer>
      <div class="dialog-footer">
        <el-button class="btn-ghost" @click="handleClose">取消</el-button>
        <el-button class="btn-primary" :loading="loading" @click="handleSubmit">
          {{ isEdit ? '更新' : '创建' }}
        </el-button>
      </div>
    </template>
  </el-dialog>
</template>

<style scoped lang="scss">
.menu-form {
  :deep(.el-form-item__label) {
    font-size: 13px;
    color: var(--text-secondary);
  }

  :deep(.el-input__wrapper) {
    background: var(--input-bg) !important;
    border: 1px solid var(--input-border) !important;
    border-radius: 8px !important;
    box-shadow: none !important;
    transition: border-color 0.2s, box-shadow 0.2s;

    &:hover {
      border-color: rgba(99, 102, 241, 0.3) !important;
    }

    &.is-focus {
      border-color: rgba(99, 102, 241, 0.6) !important;
      box-shadow: 0 0 0 3px rgba(99, 102, 241, 0.1) !important;
    }

    .el-input__inner {
      color: var(--text-primary) !important;
      font-size: 13px;

      &::placeholder {
        color: var(--text-muted) !important;
      }
    }
  }

  :deep(.el-input-number) {
    .el-input__wrapper {
      padding-left: 11px;
      padding-right: 40px;
    }
  }

  :deep(.el-tree-select) {
    .el-input__wrapper {
      background: var(--input-bg) !important;
      border: 1px solid var(--input-border) !important;
      border-radius: 8px !important;
      box-shadow: none !important;
    }
  }

  :deep(.el-switch) {
    --el-switch-on-color: #6366f1;
    --el-switch-off-color: var(--border-medium);
  }
}

.dialog-footer {
  display: flex;
  justify-content: flex-end;
  gap: 8px;
}

.btn-ghost {
  background: transparent !important;
  border: 1px solid var(--border-default) !important;
  color: var(--text-secondary) !important;
  border-radius: 8px !important;
  font-size: 13px;
  height: 36px;
  padding: 0 16px;
  transition: all 0.2s;

  &:hover {
    border-color: var(--border-medium) !important;
    color: var(--text-primary) !important;
    background: var(--bg-hover) !important;
  }
}

.btn-primary {
  background: linear-gradient(135deg, #4f46e5, #6366f1) !important;
  border: none !important;
  color: #fff !important;
  border-radius: 8px !important;
  font-size: 13px;
  font-weight: 500;
  height: 36px;
  padding: 0 16px;
  box-shadow: 0 4px 12px rgba(79, 70, 229, 0.3);
  transition: all 0.2s;

  &:hover {
    transform: translateY(-1px);
    box-shadow: 0 6px 16px rgba(79, 70, 229, 0.4);
  }

  &:active {
    transform: translateY(0);
  }
}
</style>
