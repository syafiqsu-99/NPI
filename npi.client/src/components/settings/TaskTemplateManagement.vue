<template>
  <div>
    <div class="d-flex align-center mb-4">
      <div>
        <h3 class="text-h6">Project Task Templates</h3>
        <div class="text-caption text-grey">
          Default tasks generated for each NPI stage. Department is required.
        </div>
      </div>
      <v-spacer />
      <v-btn color="primary" prepend-icon="mdi-plus" @click="openCreate">
        Add Task
      </v-btn>
    </div>

    <v-alert type="info" variant="tonal" density="compact" class="mb-4">
      Changes apply to <strong>new</strong> projects only. Existing projects keep their current tasks.
    </v-alert>

    <v-progress-linear v-if="loading" indeterminate color="primary" class="mb-3" />

    <v-expansion-panels v-model="openPanels" multiple>
      <v-expansion-panel v-for="stage in stages"
                         :key="stage.stage_id"
                         :value="stage.stage_id">
        <v-expansion-panel-title>
          <v-chip size="small" color="primary" variant="tonal" class="mr-3 font-weight-bold">
            {{ stage.stage_id }}
          </v-chip>
          <span class="font-weight-medium">{{ stage.stage_name }}</span>
          <v-chip v-if="!stage.is_required"
                  size="x-small"
                  color="grey"
                  variant="tonal"
                  class="ml-2">
            Optional
          </v-chip>
          <v-chip v-if="stage.auto_complete"
                  size="x-small"
                  color="success"
                  variant="tonal"
                  class="ml-2">
            Auto-complete
          </v-chip>
          <v-spacer />
          <span class="text-caption text-grey mr-3">
            {{ templatesFor(stage.stage_id).length }} tasks
          </span>
        </v-expansion-panel-title>

        <v-expansion-panel-text>
          <v-table density="compact">
            <thead>
              <tr>
                <th style="width: 90px">Code</th>
                <th>Title</th>
                <th style="width: 150px">Department</th>
                <th style="width: 90px">Days</th>
                <th style="width: 80px">Doc</th>
                <th style="width: 120px">Format</th>
                <th style="width: 110px" class="text-right">Actions</th>
              </tr>
            </thead>
            <tbody>
              <tr v-if="templatesFor(stage.stage_id).length === 0">
                <td colspan="7" class="text-center text-grey py-4">
                  No tasks defined for this stage.
                </td>
              </tr>
              <tr v-for="item in templatesFor(stage.stage_id)"
                  :key="item.template_id"
                  :class="{ 'text-disabled': !item.is_active }">
                <td>
                  <v-chip size="x-small" variant="tonal">{{ item.task_code }}</v-chip>
                </td>
                <td class="text-body-2">{{ item.title }}</td>
                <td>
                  <v-chip size="x-small" color="blue-grey" variant="tonal">
                    {{ item.dept_name || '—' }}
                  </v-chip>
                </td>
                <td class="text-body-2">{{ item.default_duration }}</td>
                <td>
                  <v-icon :color="item.has_link ? 'success' : 'grey-lighten-1'"
                          size="small">
                    {{ item.has_link ? 'mdi-paperclip' : 'mdi-minus' }}
                  </v-icon>
                </td>
                <td class="text-caption">{{ item.doc_format || '—' }}</td>
                <td class="text-right">
                  <v-btn icon="mdi-pencil"
                         size="x-small"
                         variant="text"
                         @click="openEdit(item)" />
                  <v-btn icon="mdi-delete"
                         size="x-small"
                         variant="text"
                         color="error"
                         @click="confirmDelete(item)" />
                </td>
              </tr>
            </tbody>
          </v-table>

          <v-btn size="small"
                 variant="text"
                 prepend-icon="mdi-plus"
                 class="mt-2"
                 @click="openCreate(stage.stage_id)">
            Add task to {{ stage.stage_name }}
          </v-btn>
        </v-expansion-panel-text>
      </v-expansion-panel>
    </v-expansion-panels>

    <!-- Create / Edit dialog -->
    <v-dialog v-model="dialogOpen" max-width="640" persistent>
      <v-card>
        <v-card-title>
          {{ editingId ? 'Edit Template Task' : 'Add Template Task' }}
        </v-card-title>

        <v-card-text>
          <v-form ref="formRef">
            <v-row dense>
              <v-col cols="12" md="4">
                <v-select v-model="form.stage_id"
                          :items="stageOptions"
                          item-title="label"
                          item-value="value"
                          label="Stage *"
                          :disabled="!!editingId"
                          :rules="[requiredRule]"
                          density="compact" />
              </v-col>
              <v-col cols="12" md="4">
                <v-text-field v-model="form.task_code"
                              label="Task Code *"
                              placeholder="1.9"
                              :rules="[requiredRule]"
                              density="compact" />
              </v-col>
              <v-col cols="12" md="4">
                <v-text-field v-model.number="form.default_duration"
                              label="Default Days *"
                              type="number"
                              min="1"
                              :rules="[positiveRule]"
                              density="compact" />
              </v-col>

              <v-col cols="12">
                <v-text-field v-model="form.title"
                              label="Task Title *"
                              :rules="[requiredRule]"
                              density="compact" />
              </v-col>

              <v-col cols="12" md="6">
                <v-select v-model="form.dept_id"
                          :items="departments"
                          item-title="dept_name"
                          item-value="dept_id"
                          label="Department *"
                          :rules="[requiredRule]"
                          density="compact" />
              </v-col>
              <v-col cols="12" md="6">
                <v-select v-model="form.role_gated"
                          :items="roleGateOptions"
                          item-title="dept_name"
                          item-value="dept_name"
                          label="Restricted To (optional)"
                          clearable
                          density="compact" />
              </v-col>

              <v-col cols="12" md="6">
                <v-text-field v-model="form.doc_format"
                              label="Document Format (optional)"
                              placeholder="Excel"
                              density="compact" />
              </v-col>
              <v-col cols="12" md="6" class="d-flex align-center">
                <v-switch v-model="form.has_link"
                          label="Requires document upload"
                          color="primary"
                          hide-details
                          density="compact" />
              </v-col>

              <v-col v-if="editingId" cols="12">
                <v-switch v-model="form.is_active"
                          label="Active"
                          color="success"
                          hide-details
                          density="compact" />
              </v-col>
            </v-row>
          </v-form>
        </v-card-text>

        <v-card-actions>
          <v-spacer />
          <v-btn variant="text" @click="closeDialog">Cancel</v-btn>
          <v-btn color="primary" :loading="saving" @click="save">Save</v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>

    <!-- Delete confirm -->
    <v-dialog v-model="deleteOpen" max-width="440">
      <v-card>
        <v-card-title>Delete template task?</v-card-title>
        <v-card-text>
          <strong>{{ deleteTarget?.task_code }} — {{ deleteTarget?.title }}</strong>
          will be removed from the template. Existing projects are unaffected.
        </v-card-text>
        <v-card-actions>
          <v-spacer />
          <v-btn variant="text" @click="deleteOpen = false">Cancel</v-btn>
          <v-btn color="error" :loading="deleting" @click="doDelete">Delete</v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>

    <v-snackbar v-model="snack.show" :color="snack.color" timeout="3000">
      {{ snack.text }}
    </v-snackbar>
  </div>
</template>

<script setup>
  import { ref, reactive, computed, onMounted } from 'vue'
  import { storeToRefs } from 'pinia'
  import { useSettingsStore } from '@/stores/setting'

  const settingsStore = useSettingsStore()

  const { stages, taskTemplates, departments, loading } = storeToRefs(settingsStore)

  const openPanels = ref([])
  const dialogOpen = ref(false)
  const deleteOpen = ref(false)
  const saving = ref(false)
  const deleting = ref(false)
  const editingId = ref(null)
  const deleteTarget = ref(null)
  const formRef = ref(null)

  const snack = reactive({ show: false, text: '', color: 'success' })

  const form = reactive({
    stage_id: '',
    task_code: '',
    title: '',
    dept_id: null,
    default_duration: 5,
    has_link: false,
    doc_format: '',
    role_gated: null,
    is_active: true
  })

  const requiredRule = v => (v !== null && v !== undefined && v !== '') || 'Required'
  const positiveRule = v => (Number(v) > 0) || 'Must be greater than 0'

  const stageOptions = computed(() =>
    stages.value.map(s => ({
      value: s.stage_id,
      label: `${s.stage_id} — ${s.stage_name}`
    }))
  )

  const roleGateOptions = computed(() => departments.value || [])

  function templatesFor(stageId) {
    return taskTemplates.value
      .filter(t => t.stage_id === stageId)
      .sort((a, b) => a.display_order - b.display_order)
  }

  function showSnack(text, color = 'success') {
    snack.text = text
    snack.color = color
    snack.show = true
  }

  function resetForm() {
    form.stage_id = ''
    form.task_code = ''
    form.title = ''
    form.dept_id = null
    form.default_duration = 5
    form.has_link = false
    form.doc_format = ''
    form.role_gated = null
    form.is_active = true
  }

  function openCreate(stageId = '') {
    editingId.value = null
    resetForm()
    if (typeof stageId === 'string' && stageId) form.stage_id = stageId
    dialogOpen.value = true
  }

  function openEdit(item) {
    editingId.value = item.template_id
    form.stage_id = item.stage_id
    form.task_code = item.task_code
    form.title = item.title
    form.dept_id = item.dept_id
    form.default_duration = item.default_duration
    form.has_link = item.has_link
    form.doc_format = item.doc_format || ''
    form.role_gated = item.role_gated
    form.is_active = item.is_active
    dialogOpen.value = true
  }

  function closeDialog() {
    dialogOpen.value = false
    editingId.value = null
  }

  async function save() {
    const { valid } = await formRef.value.validate()
    if (!valid) return

    saving.value = true
    try {
      const payload = {
        stage_id: form.stage_id,
        task_code: form.task_code,
        title: form.title,
        dept_id: form.dept_id,
        default_duration: Number(form.default_duration),
        has_link: form.has_link,
        doc_format: form.doc_format || null,
        role_gated: form.role_gated || null
      }

      const result = editingId.value
        ? await settingsStore.updateTaskTemplate(editingId.value, {
          ...payload,
          is_active: form.is_active
        })
        : await settingsStore.createTaskTemplate(payload)

      if (result?.success) {
        showSnack(result.message)
        closeDialog()
      } else {
        showSnack(result?.message || 'Save failed', 'error')
      }
    } finally {
      saving.value = false
    }
  }

  function confirmDelete(item) {
    deleteTarget.value = item
    deleteOpen.value = true
  }

  async function doDelete() {
    deleting.value = true
    try {
      const result = await settingsStore.deleteTaskTemplate(deleteTarget.value.template_id)
      if (result?.success) {
        showSnack(result.message)
        deleteOpen.value = false
      } else {
        showSnack(result?.message || 'Delete failed', 'error')
      }
    } finally {
      deleting.value = false
    }
  }

  onMounted(async () => {
    await Promise.all([
      settingsStore.fetchTaskTemplates(true),
      settingsStore.fetchDepartments()
    ])
    openPanels.value = stages.value.map(s => s.stage_id)
  })
</script>
