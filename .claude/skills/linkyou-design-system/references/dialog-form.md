# 新增/编辑弹窗表单模板

适用于 CRUD 操作中的新增和编辑弹窗，包含表单校验、加载状态、关闭逻辑。

## 完整模板

```vue
<script setup lang="ts">
import { ref, watch } from 'vue'
import { ElMessage } from 'element-plus'
import type { FormInstance, FormRules } from 'element-plus'
// import { xxxApi } from '@/api/modules/xxx'

// ---- Props & Emits ----
const props = defineProps<{
  visible: boolean
  editId?: string   // 有值=编辑，无值=新增
}>()

const emit = defineEmits<{
  'update:visible': [value: boolean]
  saved: []
}>()

// ---- 表单 ----
interface FormData {
  name: string
  // 添加更多字段
}

const formRef = ref<FormInstance>()
const submitting = ref(false)

const form = ref<FormData>({
  name: '',
})

const rules: FormRules = {
  name: [
    { required: true, message: '请输入名称', trigger: 'blur' },
    { max: 128, message: '名称不超过128个字符', trigger: 'blur' },
  ],
}

// ---- 弹窗打开时加载数据 ----
watch(
  () => props.visible,
  async (val) => {
    if (!val) return
    formRef.value?.resetFields()
    if (props.editId) {
      // const res = await xxxApi.get(props.editId)
      // form.value = { name: res.data.name }
    } else {
      form.value = { name: '' }
    }
  }
)

// ---- 提交 ----
async function handleSubmit() {
  await formRef.value?.validate()
  submitting.value = true
  try {
    if (props.editId) {
      // await xxxApi.update(props.editId, form.value)
      ElMessage.success('更新成功')
    } else {
      // await xxxApi.create(form.value)
      ElMessage.success('创建成功')
    }
    emit('saved')
  } finally {
    submitting.value = false
  }
}

function handleClose() {
  emit('update:visible', false)
}
</script>

<template>
  <el-dialog
    :model-value="visible"
    :title="editId ? '编辑 XXX' : '新增 XXX'"
    width="520px"
    :close-on-click-modal="false"
    class="dark-dialog"
    @update:model-value="handleClose"
  >
    <el-form
      ref="formRef"
      :model="form"
      :rules="rules"
      label-width="88px"
      class="dialog-form"
    >
      <el-form-item label="名称" prop="name">
        <el-input
          v-model="form.name"
          placeholder="请输入名称"
          maxlength="128"
          show-word-limit
        />
      </el-form-item>
      <!-- 更多表单项 -->
    </el-form>

    <template #footer>
      <div class="dialog-footer">
        <button class="btn-cancel" @click="handleClose">取消</button>
        <el-button
          type="primary"
          :loading="submitting"
          class="btn-confirm"
          @click="handleSubmit"
        >
          {{ submitting ? '保存中...' : '确认保存' }}
        </el-button>
      </div>
    </template>
  </el-dialog>
</template>

<style scoped lang="scss">
// 弹窗深色覆盖
:deep(.el-dialog) {
  background: var(--dialog-bg);
  border: 1px solid var(--dialog-border);
  border-radius: 16px;
  box-shadow: var(--shadow-dialog);
  backdrop-filter: blur(40px);
  -webkit-backdrop-filter: blur(40px);

  .el-dialog__header {
    padding: 20px 24px 16px;
    border-bottom: 1px solid var(--border-default);
    margin: 0;

    .el-dialog__title {
      font-family: 'Plus Jakarta Sans', 'Inter', sans-serif;
      font-size: 15px;
      font-weight: 600;
      color: var(--text-primary);
      letter-spacing: -0.3px;
    }

    .el-dialog__headerbtn .el-icon {
      color: var(--text-muted);
      &:hover { color: var(--text-secondary); }
    }
  }

  .el-dialog__body {
    padding: 20px 24px;
  }

  .el-dialog__footer {
    padding: 16px 24px;
    border-top: 1px solid var(--border-default);
  }
}

// 表单覆盖
.dialog-form {
  :deep(.el-form-item__label) {
    color: var(--text-secondary);
    font-size: 13px;
  }

  :deep(.el-input .el-input__wrapper) {
    background: var(--bg-input);
    box-shadow: 0 0 0 1px var(--border-subtle) inset;
    border-radius: 8px;
    transition: box-shadow 0.2s;

    &:hover { box-shadow: 0 0 0 1px rgba(99, 102, 241, 0.25) inset; }
    &.is-focus {
      box-shadow: 0 0 0 1px var(--primary) inset, 0 0 0 3px rgba(99, 102, 241, 0.08);
    }
  }

  :deep(.el-input__inner) {
    color: var(--text-primary);
    &::placeholder { color: var(--text-muted); }
  }

  :deep(.el-input__count) { color: var(--text-muted); }

  :deep(.el-form-item.is-error .el-input__wrapper) {
    box-shadow: 0 0 0 1px #f87171 inset;
  }

  :deep(.el-form-item__error) { color: #f87171; font-size: 11.5px; }
}

// 底部按钮
.dialog-footer {
  display: flex;
  justify-content: flex-end;
  gap: 10px;
}

.btn-cancel {
  padding: 7px 18px;
  background: transparent;
  border: 1px solid var(--border-subtle);
  border-radius: 8px;
  color: var(--text-secondary);
  font-size: 13px;
  cursor: pointer;
  transition: all 0.2s;

  &:hover {
    background: var(--primary-dim);
    border-color: var(--border-primary);
    color: var(--primary-pale);
  }
}

.btn-confirm {
  background: linear-gradient(135deg, #4f46e5, #6366f1) !important;
  border: none !important;
  border-radius: 8px !important;
  box-shadow: 0 4px 12px rgba(79, 70, 229, 0.25) !important;
  font-weight: 500 !important;
  font-size: 13px;
  transition: all 0.2s cubic-bezier(0.4, 0, 0.2, 1);

  &:hover {
    transform: translateY(-1px);
    box-shadow: 0 6px 16px rgba(79, 70, 229, 0.35) !important;
  }
}
</style>
```

## 使用说明

在父组件中引用：

```vue
<XxxDialog
  v-model:visible="dialogVisible"
  :edit-id="editId"
  @saved="handleSaved"
/>
```

其中 `handleSaved` 关闭弹窗并刷新列表：

```typescript
function handleSaved() {
  dialogVisible.value = false
  fetchList()
}
```
