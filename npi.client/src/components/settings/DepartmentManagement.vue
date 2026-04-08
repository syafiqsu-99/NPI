<template>
  <v-container fluid class="pa-6">
    <v-row>
      <v-col cols="12">
        <!-- Header -->
        <div class="d-flex justify-space-between align-center mb-4">
          <div>
            <h2 class="text-h5">Department Management</h2>
            <p class="text-caption text-grey">Manage organizational departments</p>
          </div>

          <v-btn color="primary"
                 prepend-icon="mdi-plus"
                 @click="openCreateDialog">
            Add Department
          </v-btn>
        </div>

        <!-- Departments Table -->
        <v-card>
          <v-data-table-virtual :headers="headers"
                                :items="departments"
                                :loading="loading"
                                density="comfortable"
                                fixed-header
                                height="500">
            <!-- Department Name Column -->
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

            <!-- User Count Column -->
            <template #item.user_count="{ item }">
              <v-chip size="small" variant="tonal" color="primary">
                <v-icon start size="small">mdi-account-multiple</v-icon>
                {{ getUserCount(item.dept_id) }} {{ getUserCount(item.dept_id) === 1 ? 'user' : 'users' }}
              </v-chip>
            </template>

            <!-- Created Column -->
            <template #item.created_at="{ item }">
              <span class="text-caption">{{ formatDate(item.created_at) }}</span>
            </template>

            <!-- Actions Column -->
            <template #item.actions="{ item }">
              <div class="d-flex ga-2">
                <v-tooltip text="Edit Department">
                  <template #activator="{ props }">
                    <v-btn v-bind="props"
                           icon="mdi-pencil"
                           size="small"
                           variant="text"
                           color="primary"
                           @click="openEditDialog(item)" />
                  </template>
                </v-tooltip>

                <v-tooltip :text="getUserCount(item.dept_id) > 0 ? 'Cannot delete department with users' : 'Delete Department'">
                  <template #activator="{ props }">
                    <span v-bind="props">
                      <v-btn icon="mdi-delete"
                             size="small"
                             variant="text"
                             color="error"
                             :disabled="getUserCount(item.dept_id) > 0"
                             @click="openDeleteDialog(item)" />
                    </span>
                  </template>
                </v-tooltip>
              </div>
            </template>
          </v-data-table-virtual>
        </v-card>
      </v-col>
    </v-row>

    <!-- Create / Edit Dialog -->
    <v-dialog v-model="dialog" max-width="600">
      <v-card rounded="lg">
        <v-card-title class="d-flex align-center justify-space-between">
          <span class="text-h6">{{ isEditMode ? 'Edit Department' : 'Create Department' }}</span>
          <v-btn icon="mdi-close" variant="text" @click="closeDialog" />
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
          <v-btn color="primary"
                 :loading="saving"
                 @click="saveDepartment">
            {{ isEditMode ? 'Update' : 'Create' }}
          </v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>

    <!-- Delete Confirmation Dialog -->
    <v-dialog v-model="deleteDialog" max-width="500">
      <v-card rounded="lg">
        <v-card-title class="text-h6 d-flex align-center">
          <v-icon color="error" class="mr-2">mdi-alert</v-icon>
          Delete Department
        </v-card-title>

        <v-card-text>
          Are you sure you want to delete
          <strong>{{ selectedDepartment?.dept_name }}</strong>?
          <div class="text-caption text-grey mt-2">
            This action cannot be undone. Departments with linked users or projects cannot be deleted.
          </div>
        </v-card-text>

        <v-card-actions class="pa-4">
          <v-spacer />
          <v-btn variant="text" @click="closeDeleteDialog">Cancel</v-btn>
          <v-btn color="error"
                 :loading="deleting"
                 @click="confirmDelete">
            Delete
          </v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>

    <!-- Snackbar -->
    <v-snackbar v-model="snackbar" :color="snackbarColor" :timeout="3000">
      {{ snackbarMessage }}
      <template #actions>
        <v-btn variant="text" @click="snackbar = false">Close</v-btn>
      </template>
    </v-snackbar>
  </v-container>
</template>

<script setup>
  import { ref, computed, onMounted } from 'vue'
  import { useSettingsStore } from '@/stores/setting.js'

  const settingsStore = useSettingsStore()

  // State
  const loading = ref(false)
  const saving = ref(false)
  const deleting = ref(false)

  // Dialogs
  const dialog = ref(false)
  const deleteDialog = ref(false)
  const isEditMode = ref(false)
  const selectedDepartment = ref(null)
  const formRef = ref(null)

  // Form
  const form = ref({
    dept_id: null,
    dept_name: '',
    description: ''
  })

  // Snackbar
  const snackbar = ref(false)
  const snackbarMessage = ref('')
  const snackbarColor = ref('success')

  // Table headers
  const headers = [
    { title: 'Department', value: 'dept_name', width: '40%' },
    { title: 'Users', value: 'user_count', width: '20%' },
    { title: 'Created', value: 'created_at', width: '20%' },
    { title: 'Actions', value: 'actions', sortable: false, width: '20%' }
  ]

  // Validation rules
  const deptNameRules = [
    v => !!v || 'Department name is required',
    v => (v && v.trim().length > 0) || 'Department name cannot be empty',
    v => (v && v.trim().length <= 100) || 'Department name must be 100 characters or less'
  ]

  // Computed
  const departments = computed(() => settingsStore.departments)
  const users = computed(() => settingsStore.users)

  // Methods
  function getDepartmentColor(deptName) {
    const colors = {
      Sales: 'blue',
      Technical: 'green',
      Purchaser: 'orange',
      Purchasing: 'orange',
      QA: 'purple',
      Production: 'red',
      Others: 'grey'
    }
    return colors[deptName] || 'grey'
  }

  function getDepartmentIcon(deptName) {
    const icons = {
      Sales: 'mdi-chart-line',
      Technical: 'mdi-cog',
      Purchaser: 'mdi-cart',
      Purchasing: 'mdi-cart',
      QA: 'mdi-check-decagram',
      Production: 'mdi-factory',
      Others: 'mdi-domain'
    }
    return icons[deptName] || 'mdi-domain'
  }

  function getUserCount(deptId) {
    return users.value.filter(u => Number(u.dept_id) === Number(deptId)).length
  }

  function formatDate(date) {
    if (!date) return 'N/A'
    return new Date(date).toLocaleDateString('en-GB', {
      day: '2-digit',
      month: 'short',
      year: 'numeric'
    })
  }

  function showSnackbar(message, color = 'success') {
    snackbarMessage.value = message
    snackbarColor.value = color
    snackbar.value = true
  }

  function resetForm() {
    form.value = {
      dept_id: null,
      dept_name: '',
      description: ''
    }
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
    form.value = {
      dept_id: item.dept_id,
      dept_name: item.dept_name ?? '',
      description: item.description ?? ''
    }
    dialog.value = true
  }

  function closeDialog() {
    dialog.value = false
    resetForm()
    selectedDepartment.value = null
  }

  function openDeleteDialog(item) {
    selectedDepartment.value = item
    deleteDialog.value = true
  }

  function closeDeleteDialog() {
    deleteDialog.value = false
    selectedDepartment.value = null
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

      let result
      if (isEditMode.value) {
        result = await settingsStore.updateDepartment(form.value.dept_id, payload)
      } else {
        result = await settingsStore.createDepartment(payload)
      }

      if (result?.success) {
        showSnackbar(
          isEditMode.value
            ? 'Department updated successfully'
            : 'Department created successfully',
          'success'
        )

        // Optional: refresh users too if other screens depend on latest relationships
        await settingsStore.fetchUsers()

        closeDialog()
      } else {
        showSnackbar(result?.message || 'Failed to save department', 'error')
      }
    } catch (error) {
      showSnackbar('An unexpected error occurred while saving department', 'error')
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
        showSnackbar('Department deleted successfully', 'success')
        closeDeleteDialog()
      } else {
        showSnackbar(result?.message || 'Failed to delete department', 'error')
      }
    } catch (error) {
      showSnackbar('An unexpected error occurred while deleting department', 'error')
    } finally {
      deleting.value = false
    }
  }

  onMounted(async () => {
    loading.value = true
    try {
      await Promise.all([
        settingsStore.fetchDepartments(),
        settingsStore.fetchUsers()
      ])
    } catch (error) {
      showSnackbar('Error loading departments', 'error')
    } finally {
      loading.value = false
    }
  })
</script>

<style scoped>
  .v-data-table-virtual :deep(th) {
    font-weight: 600 !important;
  }
</style>
