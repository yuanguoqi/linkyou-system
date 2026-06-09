<script setup lang="ts">
import { ref, reactive, computed, watch, nextTick } from 'vue'
import { ElMessage } from 'element-plus'
import type { FormInstance, FormRules } from 'element-plus'
import { useI18n } from 'vue-i18n'
import { menuApi } from '@/api/modules/menus'
import type { MenuDto, CreateMenuDto } from '@/api/modules/menus'

const { t } = useI18n()

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

// 常用图标列表
const iconOptions = [
  'Odometer', 'Setting', 'User', 'UserFilled', 'OfficeBuilding',
  'Document', 'Menu', 'Tools', 'Home', 'DataLine',
  'Bell', 'Calendar', 'ChatDotRound', 'Collection', 'Connection',
  'Coordinate', 'CopyDocument', 'CreditCard', 'DataBoard', 'DataAnalysis',
  'Delete', 'Discount', 'Edit', 'Files', 'Film',
  'Flag', 'Folder', 'FolderOpened', 'Goods', 'Grid',
  'Headset', 'Help', 'Histogram', 'HotWater', 'IceCream',
  'InfoFilled', 'Key', 'Link', 'List', 'Location',
  'Lock', 'Magnet', 'Management', 'Medal', 'Memo',
  'MessageBox', 'Mic', 'Monitor', 'More', 'Mouse',
  'Mug', 'Notification', 'Paperclip', 'Pen', 'Phone',
  'Picture', 'Platform', 'Position', 'Postcard', 'Present',
  'Printer', 'Promotion', 'Reading', 'Refresh', 'RefreshRight',
  'Right', 'ScaleToOriginal', 'School', 'Search', 'Select',
  'Sell', 'Service', 'Ship', 'ShoppingBag', 'ShoppingCart',
  'Stamp', 'Star', 'Sunny', 'Ticket', 'Timer',
  'Trophy', 'Umbrella', 'Upload', 'VideoCamera', 'Wallet',
  'Warning', 'Watch', 'ZoomIn', 'ZoomOut',
]

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
const rules = computed<FormRules>(() => ({
  name: [
    { required: true, message: t('validation.menuNameRequired'), trigger: 'blur' },
  ],
  path: [
    { required: true, message: t('validation.routePathRequired'), trigger: 'blur' },
  ],
}))

// ── Computed ───────────────────────────────────────────
const isEdit = computed(() => !!props.editData)
const dialogTitle = computed(() => isEdit.value ? t('menu.editMenu') : t('menu.createMenu'))

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
      ElMessage.success(t('menu.updateSuccess'))
    } else {
      await menuApi.create(payload)
      ElMessage.success(t('menu.createSuccess'))
    }

    emit('saved')
    handleClose()
  } catch (err: unknown) {
    const axiosErr = err as { response?: { data?: { error?: { message?: string } } } }
    ElMessage.error(axiosErr?.response?.data?.error?.message || t('common.operationFailed'))
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
      <el-form-item :label="t('menu.menuName')" prop="name">
        <el-input
          v-model="form.name"
          :placeholder="t('menu.menuNamePlaceholder')"
          clearable
        />
      </el-form-item>

      <el-form-item :label="t('menu.routePath')" prop="path">
        <el-input
          v-model="form.path"
          :placeholder="t('menu.routePathPlaceholder')"
          clearable
        />
      </el-form-item>

      <el-form-item :label="t('menu.icon')" prop="icon">
        <el-select
          v-model="form.icon"
          :placeholder="t('menu.iconPlaceholder')"
          clearable
          filterable
          style="width: 100%"
        >
          <el-option
            v-for="icon in iconOptions"
            :key="icon"
            :label="icon"
            :value="icon"
          >
            <div class="icon-option">
              <el-icon :size="16"><component :is="icon" /></el-icon>
              <span>{{ icon }}</span>
            </div>
          </el-option>
        </el-select>
      </el-form-item>

      <el-form-item :label="t('menu.parentMenu')" prop="parentId">
        <el-tree-select
          v-model="form.parentId"
          :data="parentOptions"
          :props="{ label: 'name', children: 'children' }"
          node-key="id"
          :placeholder="t('menu.parentMenuPlaceholder')"
          clearable
          check-strictly
          style="width: 100%"
        />
      </el-form-item>

      <el-form-item :label="t('menu.sort')" prop="sort">
        <el-input-number
          v-model="form.sort"
          :min="0"
          :max="9999"
          controls-position="right"
          style="width: 100%"
        />
      </el-form-item>

      <el-form-item :label="t('menu.visible')" prop="isVisible">
        <el-switch
          v-model="form.isVisible"
          :active-text="t('menu.visible')"
          :inactive-text="t('menu.hidden')"
        />
      </el-form-item>

      <el-form-item :label="t('menu.permission')" prop="permission">
        <el-input
          v-model="form.permission"
          :placeholder="t('menu.permissionPlaceholder')"
          clearable
        />
      </el-form-item>
    </el-form>

    <template #footer>
      <div class="dialog-footer">
        <el-button class="btn-ghost" @click="handleClose">{{ t('common.cancel') }}</el-button>
        <el-button class="btn-primary" :loading="loading" @click="handleSubmit">
          {{ isEdit ? t('common.update') : t('common.create') }}
        </el-button>
      </div>
    </template>
  </el-dialog>
</template>

<style scoped lang="scss">
.icon-option {
  display: flex;
  align-items: center;
  gap: 8px;
  font-size: 13px;
}

.menu-form {
  :deep(.el-form-item__label) {
    font-size: 13px;
    color: var(--text-secondary);
  }

  :deep(.el-input__wrapper),
  :deep(.el-select__wrapper) {
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
