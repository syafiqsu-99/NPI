<template>
  <v-container fluid class="pa-6">
    <v-row>
      <v-col cols="12">
        <!-- Header -->
        <div class="d-flex justify-space-between align-center mb-4">
          <div>
            <h2 class="text-h5">Role Management</h2>
            <p class="text-caption text-grey">Define user roles and permissions</p>
          </div>
          <v-btn color="primary" prepend-icon="mdi-shield-plus" @click="openCreateDialog">
            Add Role
          </v-btn>
        </div>

        <!-- Roles Table -->
        <v-card>
          <v-data-table-virtual :headers="headers"
                                :items="roles"
                                :loading="loading"
                                class="elevation-1"
                                density="comfortable"
                                :items-per-page="10">
            <!-- Role Name Column -->
            <template #item.role_name="{ item }">
              <div class="d-flex align-center">
                <v-icon :color="getRoleColor(item.role_name)" class="mr-2">
                  {{ getRoleIcon(item.role_name) }}
                </v-icon>
                <div>
                  <div class="font-weight-medium">{{ item.role_name }}</div>
                  <div class="text-caption text-grey">{{ item.description || 'No description' }}</div>
                </div>
              </div>
            </template>

            <!-- User Count Column -->
            <template #item.user_count="{ item }">
              <v-chip size="small" variant="tonal" color="info">
                <v-icon start size="small">mdi-account-multiple</v-icon>
                {{ item.user_count }} {{ item.user_count === 1 ? 'user' : 'users' }}
              </v-chip>
            </template>

            <!-- Status Column -->
            <template #item.is_active="{ item }">
              <v-chip size="small"
                      :color="item.is_active ? 'success' : 'grey'"
                      variant="flat">
                <v-icon start size="small">
                  {{ item.is_active ? 'mdi-check-circle' : 'mdi-close-circle' }}
                </v-icon>
                {{ item.is_active ? 'Active' : 'Inactive' }}
              </v-chip>
            </template>

            <!-- Created Column -->
            <template #item.created_at="{ item }">
              <span class="text-caption">{{ formatDate(item.created_at) }}</span>
            </template>

            <!-- Actions Column -->
            <template #item.actions="{ item }">
              <v-menu>
                <template #activator="{ props }">
                  <v-btn icon="mdi-dots-vertical" variant="text" size="small" v-bind="props" />
                </template>
                <v-list density="compact">
                  <v-list-item @click="openEditDialog(item)">
                    <template #prepend>
                      <v-icon>mdi-pencil</v-icon>
                    </template>
                    <v-list-item-title>Edit</v-list-item-title>
                  </v-list-item>

                  <v-list-item @click="toggleRoleStatus(item)">
                    <template #prepend>
                      <v-icon>
                        {{ item.is_active ? 'mdi-shield-off' : 'mdi-shield-check' }}
                      </v-icon>
                    </template>
                    <v-list-item-title>
                      {{ item.is_active ? 'Deactivate' : 'Activate' }}
                    </v-list-item-title>
                  </v-list-item>

                  <v-divider />

                  <v-list-item @click="openDeleteDialog(item)"
                               class="text-error"
                               :disabled="item.user_count > 0">
                    <template #prepend>
                      <v-icon color="error">mdi-delete</v-icon>
                    </template>
                    <v-list-item-title>Delete</v-list-item-title>
                  </v-list-item>
                </v-list>
              </v-menu>
            </template>
          </v-data-table-virtual>
        </v-card>
      </v-col>
    </v-row>

    <!-- Create/Edit Role Dialog -->
    <v-dialog v-model="roleDialog" max-width="600px" persistent>
      <v-card>
        <v-card-title class="bg-primary">
          <span class="text-h6">{{ editMode ? 'Edit Role' : 'Create New Role' }}</span>
        </v-card-title>

        <v-card-text class="pt-6">
          <v-form ref="roleForm" v-model="formValid">
            <v-text-field v-model="roleFormData.role_name"
                          label="Role Name *"
                          prepend-inner-icon="mdi-shield-account"
                          variant="outlined"
                          :rules="[rules.required]"
                          class="mb-4" />

            <v-textarea v-model="roleFormData.description"
                        label="Description"
                        prepend-inner-icon="mdi-text"
                        variant="outlined"
                        rows="3"
                        class="mb-4" />

            <v-switch v-model="roleFormData.is_active"
                      label="Active"
                      color="success"
                      hide-details />
          </v-form>
        </v-card-text>

        <v-card-actions class="pa-4">
          <v-spacer />
          <v-btn variant="text" @click="closeRoleDialog">Cancel</v-btn>
          <v-btn color="primary"
                 variant="elevated"
                 @click="saveRole"
                 :disabled="!formValid"
                 :loading="saving">
            {{ editMode ? 'Update' : 'Create' }}
          </v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>

    <!-- Delete Confirmation Dialog -->
    <v-dialog v-model="deleteDialog" max-width="500px">
      <v-card>
        <v-card-title class="bg-error">
          <v-icon class="mr-2">mdi-alert</v-icon>
          Confirm Delete
        </v-card-title>

        <v-card-text class="pt-6">
          <v-alert type="error" variant="tonal" class="mb-4">
            Are you sure you want to delete role <strong>{{ selectedRole?.role_name }}</strong>?
          </v-alert>
          <p>This action cannot be undone.</p>
        </v-card-text>

        <v-card-actions class="pa-4">
          <v-spacer />
          <v-btn variant="text" @click="deleteDialog = false">Cancel</v-btn>
          <v-btn color="error"
                 variant="elevated"
                 @click="deleteRole"
                 :loading="saving">
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
  const roleDialog = ref(false)
  const deleteDialog = ref(false)
  const editMode = ref(false)
  const formValid = ref(false)

  const roleFormData = ref({
    role_name: '',
    description: '',
    is_active: true
  })

  const selectedRole = ref(null)

  // Snackbar
  const snackbar = ref(false)
  const snackbarMessage = ref('')
  const snackbarColor = ref('success')

  // Table headers
  const headers = [
    { title: 'Role', value: 'role_name', width: '30%' },
    { title: 'Users', value: 'user_count', width: '15%' },
    { title: 'Status', value: 'is_active', width: '15%' },
    { title: 'Created', value: 'created_at', width: '20%' },
    { title: 'Actions', value: 'actions', width: '10%', sortable: false }
  ]

  // Validation rules
  const rules = {
    required: v => !!v || 'This field is required'
  }

  // Computed
  const roles = computed(() => settingsStore.roles)

  // Methods
  function getRoleColor(roleName) {
    const colors = {
      'Admin': 'error',
      'Manager': 'primary',
      'Team Lead': 'success',
      'Member': 'info'
    }
    return colors[roleName] || 'grey'
  }

  function getRoleIcon(roleName) {
    const icons = {
      'Admin': 'mdi-shield-crown',
      'Manager': 'mdi-account-tie',
      'Team Lead': 'mdi-account-star',
      'Member': 'mdi-account'
    }
    return icons[roleName] || 'mdi-shield-account'
  }

  function formatDate(date) {
    if (!date) return 'N/A'
    return new Date(date).toLocaleDateString('en-GB', {
      day: '2-digit',
      month: 'short',
      year: 'numeric'
    })
  }

  function openCreateDialog() {
    editMode.value = false
    roleFormData.value = {
      role_name: '',
      description: '',
      is_active: true
    }
    roleDialog.value = true
  }

  function openEditDialog(role) {
    editMode.value = true
    selectedRole.value = role
    roleFormData.value = {
      role_name: role.role_name,
      description: role.description,
      is_active: role.is_active
    }
    roleDialog.value = true
  }

  function closeRoleDialog() {
    roleDialog.value = false
    editMode.value = false
    selectedRole.value = null
  }

  async function saveRole() {
    saving.value = true
    try {
      let result

      if (editMode.value) {
        result = await settingsStore.updateRole(selectedRole.value.role_id, roleFormData.value)
      } else {
        result = await settingsStore.createRole(roleFormData.value)
      }

      if (result?.success) {
        snackbarMessage.value = editMode.value ? 'Role updated successfully' : 'Role created successfully'
        snackbarColor.value = 'success'
        snackbar.value = true
        closeRoleDialog()
      } else {
        snackbarMessage.value = result?.message || 'Failed to save role'
        snackbarColor.value = 'error'
        snackbar.value = true
      }
    } catch (error) {
      snackbarMessage.value = 'Error saving role'
      snackbarColor.value = 'error'
      snackbar.value = true
    } finally {
      saving.value = false
    }
  }

  async function toggleRoleStatus(role) {
    try {
      const result = await settingsStore.toggleRoleStatus(role.role_id)

      if (result?.success) {
        snackbarMessage.value = result.message
        snackbarColor.value = 'success'
        snackbar.value = true
      } else {
        snackbarMessage.value = result?.message || 'Failed to toggle role status'
        snackbarColor.value = 'error'
        snackbar.value = true
      }
    } catch (error) {
      snackbarMessage.value = 'Error toggling role status'
      snackbarColor.value = 'error'
      snackbar.value = true
    }
  }

  function openDeleteDialog(role) {
    selectedRole.value = role
    deleteDialog.value = true
  }

  async function deleteRole() {
    saving.value = true
    try {
      const result = await settingsStore.deleteRole(selectedRole.value.role_id)

      if (result?.success) {
        snackbarMessage.value = result.message
        snackbarColor.value = 'success'
        snackbar.value = true
        deleteDialog.value = false
      } else {
        snackbarMessage.value = result?.message || 'Failed to delete role'
        snackbarColor.value = 'error'
        snackbar.value = true
      }
    } catch (error) {
      snackbarMessage.value = 'Error deleting role'
      snackbarColor.value = 'error'
      snackbar.value = true
    } finally {
      saving.value = false
    }
  }

  onMounted(async () => {
    loading.value = true
    try {
      await settingsStore.fetchRoles()
    } catch (error) {
      snackbarMessage.value = 'Error loading roles'
      snackbarColor.value = 'error'
      snackbar.value = true
    } finally {
      loading.value = false
    }
  })
</script>

<style scoped>
  .v-data-table :deep(th) {
    font-weight: 600 !important;
  }
</style>
