<template>
  <div class="module-root d-flex flex-column pa-3 ga-3">

    <!-- Row 1: Page title -->
    <div class="flex-shrink-0">
      <h2 class="text-h6 font-weight-bold">Role Management</h2>
    </div>

    <!-- Row 2: Search + Add button -->
    <v-card class="flex-shrink-0" elevation="1">
      <v-card-text class="pa-3">
        <v-row dense align="center">
          <v-col cols="12" sm="5">
            <v-text-field v-model="search"
                          prepend-inner-icon="mdi-magnify"
                          label="Search roles"
                          variant="outlined" density="compact"
                          clearable hide-details />
          </v-col>
          <v-col cols="auto" class="ml-auto">
            <v-btn color="primary" prepend-icon="mdi-shield-plus"
                   size="small" @click="openCreateDialog">
              Add Role
            </v-btn>
          </v-col>
        </v-row>
      </v-card-text>
    </v-card>

    <!-- Row 3: Data table fills remaining height -->
    <v-card class="flex-grow-1 d-flex flex-column" elevation="1" style="min-height: 0;">
      <v-data-table-virtual :headers="headers"
                            :items="filteredRoles"
                            :loading="loading"
                            density="comfortable"
                            fixed-header
                            height="300"
                            class="role-table flex-grow-1">

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

        <template #item.user_count="{ item }">
          <v-chip size="small" variant="tonal" color="info">
            <v-icon start size="small">mdi-account-multiple</v-icon>
            {{ item.user_count }} {{ item.user_count === 1 ? 'user' : 'users' }}
          </v-chip>
        </template>

        <template #item.is_active="{ item }">
          <v-chip size="small" :color="item.is_active ? 'success' : 'grey'" variant="flat">
            <v-icon start size="small">
              {{ item.is_active ? 'mdi-check-circle' : 'mdi-close-circle' }}
            </v-icon>
            {{ item.is_active ? 'Active' : 'Inactive' }}
          </v-chip>
        </template>

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
              <v-list-item @click="toggleRoleStatus(item)">
                <template #prepend>
                  <v-icon size="18">
                    {{ item.is_active ? 'mdi-shield-off' : 'mdi-shield-check' }}
                  </v-icon>
                </template>
                <v-list-item-title>{{ item.is_active ? 'Deactivate' : 'Activate' }}</v-list-item-title>
              </v-list-item>
              <v-divider />
              <v-list-item :disabled="item.user_count > 0" @click="openDeleteDialog(item)">
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
    <v-dialog v-model="roleDialog" max-width="600" persistent>
      <v-card>
        <v-card-title class="bg-primary text-white text-subtitle-1">
          {{ editMode ? 'Edit Role' : 'Create New Role' }}
        </v-card-title>
        <v-card-text class="pt-4">
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
                      label="Status"
                      color="success" hide-details />
          </v-form>
        </v-card-text>
        <v-card-actions class="pa-4">
          <v-spacer />
          <v-btn variant="text" @click="closeRoleDialog">Cancel</v-btn>
          <v-btn color="primary" variant="elevated"
                 :disabled="!formValid" :loading="saving"
                 @click="saveRole">
            {{ editMode ? 'Update' : 'Create' }}
          </v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>

    <!-- Delete Confirmation dialog -->
    <v-dialog v-model="deleteDialog" max-width="500">
      <v-card>
        <v-card-title class="bg-error text-white text-subtitle-1">
          <v-icon class="mr-2">mdi-alert</v-icon>
          Confirm Delete
        </v-card-title>
        <v-card-text class="pt-4">
          <v-alert type="error" variant="tonal" class="mb-3">
            Delete role <strong>{{ selectedRole?.role_name }}</strong>? This cannot be undone.
          </v-alert>
        </v-card-text>
        <v-card-actions class="pa-4">
          <v-spacer />
          <v-btn variant="text" @click="deleteDialog = false">Cancel</v-btn>
          <v-btn color="error" variant="elevated" :loading="saving" @click="deleteRole">
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
  const roleDialog = ref(false)
  const deleteDialog = ref(false)
  const editMode = ref(false)
  const formValid = ref(false)
  const search = ref('')

  const roleFormData = ref({ role_name: '', description: '', is_active: true })
  const selectedRole = ref(null)

  const snackbar = ref(false)
  const snackbarMessage = ref('')
  const snackbarColor = ref('success')

  const headers = [
    { title: 'Role', value: 'role_name', width: '50%' },
    { title: 'Users', value: 'user_count', width: '20%' },
    { title: 'Status', value: 'is_active', width: '20%' },
    { title: '', value: 'actions', width: '10%', sortable: false }
  ]

  const rules = { required: v => !!v || 'This field is required' }

  const roles = computed(() => settingsStore.roles)

  const filteredRoles = computed(() => {
    const q = search.value?.toLowerCase() ?? ''
    return q ? roles.value.filter(r => r.role_name.toLowerCase().includes(q)) : roles.value
  })

  function getRoleColor(roleName) {
    return { Admin: 'error', Manager: 'primary', 'Team Lead': 'success', Member: 'info' }[roleName] || 'grey'
  }

  function getRoleIcon(roleName) {
    return {
      Admin: 'mdi-shield-crown', Manager: 'mdi-account-tie',
      'Team Lead': 'mdi-account-star', Member: 'mdi-account'
    }[roleName] || 'mdi-shield-account'
  }

  function formatDate(date) {
    if (!date) return 'N/A'
    return new Date(date).toLocaleDateString('en-GB', { day: '2-digit', month: 'short', year: 'numeric' })
  }

  function showSnack(msg, color = 'success') {
    snackbarMessage.value = msg; snackbarColor.value = color; snackbar.value = true
  }

  function openCreateDialog() {
    editMode.value = false
    roleFormData.value = { role_name: '', description: '', is_active: true }
    roleDialog.value = true
  }

  function openEditDialog(role) {
    editMode.value = true
    selectedRole.value = role
    roleFormData.value = { role_name: role.role_name, description: role.description, is_active: role.is_active }
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
      const result = editMode.value
        ? await settingsStore.updateRole(selectedRole.value.role_id, roleFormData.value)
        : await settingsStore.createRole(roleFormData.value)

      if (result?.success) {
        showSnack(editMode.value ? 'Role updated successfully' : 'Role created successfully')
        closeRoleDialog()
      } else {
        showSnack(result?.message || 'Failed to save role', 'error')
      }
    } catch {
      showSnack('Error saving role', 'error')
    } finally {
      saving.value = false
    }
  }

  async function toggleRoleStatus(role) {
    try {
      const result = await settingsStore.toggleRoleStatus(role.role_id)
      showSnack(result?.message || 'Status updated', result?.success ? 'success' : 'error')
    } catch {
      showSnack('Error toggling role status', 'error')
    }
  }

  function openDeleteDialog(role) {
    selectedRole.value = role; deleteDialog.value = true
  }

  async function deleteRole() {
    saving.value = true
    try {
      const result = await settingsStore.deleteRole(selectedRole.value.role_id)
      showSnack(result?.message || 'Done', result?.success ? 'success' : 'error')
      if (result?.success) deleteDialog.value = false
    } catch {
      showSnack('Error deleting role', 'error')
    } finally {
      saving.value = false
    }
  }

  onMounted(async () => {
    loading.value = true
    try {
      await settingsStore.fetchRoles()
    } catch {
      showSnack('Error loading roles', 'error')
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

  .role-table :deep(th) {
    font-weight: 600 !important;
    font-size: 11px;
    text-transform: uppercase;
    letter-spacing: 0.4px;
    background: #fafbfc !important;
  }

  .role-table :deep(.v-table__wrapper) {
    height: 100%;
    overflow-y: auto;
  }
</style>
