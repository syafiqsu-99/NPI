<template>
  <div class="module-root d-flex flex-column pa-3 ga-3">

    <!-- Row 1: Page title -->
    <div class="flex-shrink-0">
      <h2 class="text-h6 font-weight-bold">Department Management</h2>
    </div>

    <!-- Row 2: Search + Add button -->
    <v-card class="flex-shrink-0" elevation="1">
      <v-card-text class="pa-3">
        <v-row dense align="center">
          <v-col cols="12" sm="5">
            <v-text-field v-model="search"
                          prepend-inner-icon="mdi-magnify"
                          label="Search departments"
                          variant="outlined" density="compact"
                          clearable hide-details />
          </v-col>
          <v-col cols="auto" class="ml-auto">
            <v-btn color="primary" prepend-icon="mdi-plus"
                   size="small" @click="openCreateDialog">
              Add Department
            </v-btn>
          </v-col>
        </v-row>
      </v-card-text>
    </v-card>

    <!-- Row 3: Data table fills remaining height -->
    <v-card class="flex-grow-1 d-flex flex-column" elevation="1" style="min-height: 0;">
      <v-data-table-virtual :headers="headers"
                            :items="filteredDepartments"
                            :loading="loading"
                            density="comfortable"
                            fixed-header
                            height="300"
                            class="dept-table flex-grow-1">

        <template #item.dept_name="{ item }">
          <div class="d-flex align-center">
            <v-icon :color="getDepartmentColor(item.dept_name)" class="mr-3">
              {{ getDepartmentIcon(item.dept_name) }}
            </v-icon>
            <div>
              <div class="font-weight-medium">{{ item.dept_name }}</div>
              <div class="text-caption text-grey">{{ item.description || 'No description' }}</div>
            </div>
          </div>
        </template>

        <template #item.user_count="{ item }">
          <v-chip size="small" variant="tonal" color="primary">
            <v-icon start size="small">mdi-account-multiple</v-icon>
            {{ getUserCount(item.dept_id) }} {{ getUserCount(item.dept_id) === 1 ? 'user' : 'users' }}
          </v-chip>
        </template>

        <template #item.created_at="{ item }">
          <span class="text-caption">{{ formatDate(item.created_at) }}</span>
        </template>

        <!-- Actions: no header text, v-menu with dots trigger -->
        <template #item.actions="{ item }">
          <v-menu location="bottom end">
            <template #activator="{ props }">
              <v-btn icon="mdi-dots-vertical" variant="text" size="small" v-bind="props" />
            </template>
            <v-list density="compact" min-width="160">
              <v-list-item @click="openEditDialog(item)">
                <template #prepend>
                  <v-icon size="18">mdi-pencil</v-icon>
                </template>
                <v-list-item-title>Edit</v-list-item-title>
              </v-list-item>
              <v-divider />
              <v-list-item :disabled="getUserCount(item.dept_id) > 0"
                           @click="openDeleteDialog(item)">
                <template #prepend>
                  <v-icon size="18" color="error">mdi-delete</v-icon>
                </template>
                <v-list-item-title class="text-error">Delete</v-list-item-title>
              </v-list-item>
            </v-list>
          </v-menu>
        </template>

      </v-data-table-virtual>
    </v-card>

    <!-- Create / Edit dialog -->
    <v-dialog v-model="dialog" max-width="600">
      <v-card rounded="lg">
        <v-card-title class="bg-primary text-white text-subtitle-1 d-flex align-center justify-space-between">
          <span>{{ isEditMode ? 'Edit Department' : 'Create Department' }}</span>
          <v-btn icon="mdi-close" variant="text" size="small" @click="closeDialog" />
        </v-card-title>
        <v-divider />
        <v-card-text class="pt-4">
          <v-form ref="formRef" @submit.prevent="saveDepartment">
            <v-text-field v-model="form.dept_name"
                          label="Department Name"
                          variant="outlined"
                          density="comfortable"
                          :rules="deptNameRules"
                          maxlength="100"
                          counter
                          required />
            <v-textarea v-model="form.description"
                        label="Description"
                        variant="outlined"
                        density="comfortable"
                        rows="3"
                        maxlength="500"
                        counter
                        auto-grow />
          </v-form>
        </v-card-text>
        <v-divider />
        <v-card-actions class="pa-4">
          <v-spacer />
          <v-btn variant="text" @click="closeDialog">Cancel</v-btn>
          <v-btn color="primary" variant="elevated" :loading="saving" @click="saveDepartment">
            {{ isEditMode ? 'Update' : 'Create' }}
          </v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>

    <!-- Delete Confirmation dialog -->
    <v-dialog v-model="deleteDialog" max-width="500">
      <v-card rounded="lg">
        <v-card-title class="bg-error text-white text-subtitle-1">
          <v-icon class="mr-2">mdi-alert</v-icon>
          Delete Department
        </v-card-title>
        <v-card-text class="pt-4">
          Delete <strong>{{ selectedDepartment?.dept_name }}</strong>? This cannot be undone.
          <div class="text-caption text-grey mt-2">
            Departments with linked users or projects cannot be deleted.
          </div>
        </v-card-text>
        <v-card-actions class="pa-4">
          <v-spacer />
          <v-btn variant="text" @click="closeDeleteDialog">Cancel</v-btn>
          <v-btn color="error" variant="elevated" :loading="deleting" @click="confirmDelete">
            Delete
          </v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>

    <v-snackbar v-model="snackbar" :color="snackbarColor" :timeout="3000">
      {{ snackbarMessage }}
      <template #actions>
        <v-btn variant="text" @click="snackbar = false">Close</v-btn>
      </template>
    </v-snackbar>

  </div>
</template>

<script setup>
  import { ref, computed, onMounted } from 'vue'
  import { useSettingsStore } from '@/stores/setting.js'

  const settingsStore = useSettingsStore()

  const loading = ref(false)
  const saving = ref(false)
  const deleting = ref(false)
  const search = ref('')

  const dialog = ref(false)
  const deleteDialog = ref(false)
  const isEditMode = ref(false)
  const selectedDepartment = ref(null)
  const formRef = ref(null)

  const form = ref({ dept_id: null, dept_name: '', description: '' })

  const snackbar = ref(false)
  const snackbarMessage = ref('')
  const snackbarColor = ref('success')

  const headers = [
    { title: 'Department', value: 'dept_name', width: '40%' },
    { title: 'Users', value: 'user_count', width: '20%' },
    { title: 'Created', value: 'created_at', width: '30%' },
    { title: '', value: 'actions', sortable: false, width: '10%' }
  ]

  const deptNameRules = [
    v => !!v || 'Department name is required',
    v => (v && v.trim().length > 0) || 'Department name cannot be empty',
    v => (v && v.trim().length <= 100) || 'Must be 100 characters or less'
  ]

  const departments = computed(() => settingsStore.departments)
  const users = computed(() => settingsStore.users)

  const filteredDepartments = computed(() => {
    const q = search.value?.toLowerCase() ?? ''
    return q
      ? departments.value.filter(d => d.dept_name.toLowerCase().includes(q))
      : departments.value
  })

  function getDepartmentColor(deptName) {
    return {
      Sales: 'blue', Technical: 'green', Purchaser: 'orange',
      Purchasing: 'orange', QA: 'purple', Production: 'red', Others: 'grey'
    }[deptName] || 'grey'
  }

  function getDepartmentIcon(deptName) {
    return {
      Sales: 'mdi-chart-line', Technical: 'mdi-cog', Purchaser: 'mdi-cart',
      Purchasing: 'mdi-cart', QA: 'mdi-check-decagram', Production: 'mdi-factory', Others: 'mdi-domain'
    }[deptName] || 'mdi-domain'
  }

  function getUserCount(deptId) {
    return users.value.filter(u => Number(u.dept_id) === Number(deptId)).length
  }

  function formatDate(date) {
    if (!date) return 'N/A'
    return new Date(date).toLocaleDateString('en-GB', { day: '2-digit', month: 'short', year: 'numeric' })
  }

  function showSnackbar(message, color = 'success') {
    snackbarMessage.value = message; snackbarColor.value = color; snackbar.value = true
  }

  function resetForm() {
    form.value = { dept_id: null, dept_name: '', description: '' }
  }

  function openCreateDialog() {
    isEditMode.value = false
    selectedDepartment.value = null
    resetForm()
    dialog.value = true
  }

  function openEditDialog(item) {
    isEditMode.value = true
    selectedDepartment.value = item
    form.value = { dept_id: item.dept_id, dept_name: item.dept_name ?? '', description: item.description ?? '' }
    dialog.value = true
  }

  function closeDialog() {
    dialog.value = false
    resetForm()
    selectedDepartment.value = null
  }

  function openDeleteDialog(item) {
    selectedDepartment.value = item; deleteDialog.value = true
  }

  function closeDeleteDialog() {
    deleteDialog.value = false; selectedDepartment.value = null
  }

  async function saveDepartment() {
    const validation = await formRef.value?.validate()
    if (!validation?.valid) return

    saving.value = true
    try {
      const payload = {
        dept_name: form.value.dept_name.trim(),
        description: form.value.description?.trim() || null
      }
      const result = isEditMode.value
        ? await settingsStore.updateDepartment(form.value.dept_id, payload)
        : await settingsStore.createDepartment(payload)

      if (result?.success) {
        showSnackbar(isEditMode.value ? 'Department updated successfully' : 'Department created successfully')
        await settingsStore.fetchUsers()
        closeDialog()
      } else {
        showSnackbar(result?.message || 'Failed to save department', 'error')
      }
    } catch {
      showSnackbar('An unexpected error occurred', 'error')
    } finally {
      saving.value = false
    }
  }

  async function confirmDelete() {
    if (!selectedDepartment.value?.dept_id) return
    deleting.value = true
    try {
      const result = await settingsStore.deleteDepartment(selectedDepartment.value.dept_id)
      if (result?.success) {
        showSnackbar('Department deleted successfully')
        closeDeleteDialog()
      } else {
        showSnackbar(result?.message || 'Failed to delete department', 'error')
      }
    } catch {
      showSnackbar('An unexpected error occurred', 'error')
    } finally {
      deleting.value = false
    }
  }

  onMounted(async () => {
    loading.value = true
    try {
      await Promise.all([settingsStore.fetchDepartments(), settingsStore.fetchUsers()])
    } catch {
      showSnackbar('Error loading departments', 'error')
    } finally {
      loading.value = false
    }
  })
</script>

<style scoped>
  .module-root {
    height: 100%;
    overflow: hidden;
  }

  .dept-table :deep(th) {
    font-weight: 600 !important;
    font-size: 11px;
    text-transform: uppercase;
    letter-spacing: 0.4px;
    background: #fafbfc !important;
  }

  .dept-table :deep(.v-table__wrapper) {
    height: 100%;
    overflow-y: auto;
  }
</style>
