<template>
  <v-container fluid class="pa-6">
    <v-row>
      <v-col cols="12">
        <!-- Header with Actions -->
        <div class="d-flex justify-space-between align-center mb-4">
          <div>
            <h2 class="text-h5">User Management</h2>
            <p class="text-caption text-grey">Manage system users and permissions</p>
          </div>
          <v-btn color="primary" prepend-icon="mdi-account-plus" @click="openCreateDialog">
            Add User
          </v-btn>
        </div>

        <!-- Search and Filters -->
        <v-card class="mb-4">
          <v-card-text>
            <v-row>
              <v-col cols="12" md="4">
                <v-text-field v-model="search"
                              prepend-inner-icon="mdi-magnify"
                              label="Search users"
                              variant="outlined"
                              density="compact"
                              clearable
                              hide-details />
              </v-col>
              <v-col cols="12" md="3">
                <v-select v-model="filterDept"
                          :items="departments"
                          item-title="dept_name"
                          item-value="dept_id"
                          label="Filter by Department"
                          variant="outlined"
                          density="compact"
                          clearable
                          hide-details />
              </v-col>
              <v-col cols="12" md="3">
                <v-select v-model="filterRole"
                          :items="roles"
                          item-title="role_name"
                          item-value="role_id"
                          label="Filter by Role"
                          variant="outlined"
                          density="compact"
                          clearable
                          hide-details />
              </v-col>
              <v-col cols="12" md="2">
                <v-select v-model="filterStatus"
                          :items="['Active', 'Inactive']"
                          label="Status"
                          variant="outlined"
                          density="compact"
                          clearable
                          hide-details />
              </v-col>
            </v-row>
          </v-card-text>
        </v-card>

        <!-- Users Table -->
        <v-card>
          <v-data-table :headers="headers"
                        :items="filteredUsers"
                        :search="search"
                        :loading="loading"
                        class="elevation-1"
                        density="comfortable"
                        :items-per-page="10">
            <!-- User Column -->
            <template #item.username="{ item }">
              <div class="d-flex align-center py-2">
                <v-avatar color="primary" size="36" class="mr-3">
                  <span class="text-white text-subtitle-2">
                    {{ getInitials(item.full_name || item.username) }}
                  </span>
                </v-avatar>
                <div>
                  <div class="font-weight-medium">{{ item.username }}</div>
                  <div class="text-caption text-grey">{{ item.full_name }}</div>
                </div>
              </div>
            </template>

            <!-- Email Column -->
            <template #item.email="{ item }">
              <span class="text-body-2">{{ item.email || 'N/A' }}</span>
            </template>

            <!-- Department Column -->
            <template #item.dept_name="{ item }">
              <v-chip size="small" variant="tonal" color="blue">
                {{ item.dept_name || 'None' }}
              </v-chip>
            </template>

            <!-- Role Column -->
            <template #item.role_name="{ item }">
              <v-chip size="small"
                      variant="tonal"
                      :color="getRoleColor(item.role_name)">
                <v-icon start size="small">{{ getRoleIcon(item.role_name) }}</v-icon>
                {{ item.role_name || 'None' }}
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

            <!-- Created Date Column -->
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

                  <v-list-item @click="openResetPasswordDialog(item)">
                    <template #prepend>
                      <v-icon>mdi-lock-reset</v-icon>
                    </template>
                    <v-list-item-title>Reset Password</v-list-item-title>
                  </v-list-item>

                  <v-list-item @click="toggleUserStatus(item)">
                    <template #prepend>
                      <v-icon>
                        {{ item.is_active ? 'mdi-account-off' : 'mdi-account-check' }}
                      </v-icon>
                    </template>
                    <v-list-item-title>
                      {{ item.is_active ? 'Deactivate' : 'Activate' }}
                    </v-list-item-title>
                  </v-list-item>

                  <v-divider />

                  <v-list-item @click="openDeleteDialog(item)"
                               class="text-error"
                               :disabled="item.user_id === currentUserId">
                    <template #prepend>
                      <v-icon color="error">mdi-delete</v-icon>
                    </template>
                    <v-list-item-title>Delete</v-list-item-title>
                  </v-list-item>
                </v-list>
              </v-menu>
            </template>
          </v-data-table>
        </v-card>
      </v-col>
    </v-row>

    <!-- Create/Edit User Dialog -->
    <v-dialog v-model="userDialog" max-width="700px" persistent>
      <v-card>
        <v-card-title class="bg-primary">
          <span class="text-h6">{{ editMode ? 'Edit User' : 'Create New User' }}</span>
        </v-card-title>

        <v-card-text class="pt-6">
          <v-form ref="userForm" v-model="formValid">
            <v-row>
              <v-col cols="12" md="6">
                <v-text-field v-model="userFormData.username"
                              label="Username *"
                              prepend-inner-icon="mdi-account"
                              variant="outlined"
                              :rules="[rules.required, rules.username]"
                              :readonly="editMode" />
              </v-col>

              <v-col cols="12" md="6">
                <v-text-field v-model="userFormData.full_name"
                              label="Full Name"
                              prepend-inner-icon="mdi-account-box"
                              variant="outlined" />
              </v-col>

              <v-col cols="12" md="6">
                <v-text-field v-model="userFormData.email"
                              label="Email"
                              prepend-inner-icon="mdi-email"
                              variant="outlined"
                              type="email"
                              :rules="[rules.email]" />
              </v-col>

              <v-col cols="12" md="6" v-if="!editMode">
                <v-text-field v-model="userFormData.password"
                              label="Password *"
                              prepend-inner-icon="mdi-lock"
                              variant="outlined"
                              :type="showPassword ? 'text' : 'password'"
                              :append-inner-icon="showPassword ? 'mdi-eye' : 'mdi-eye-off'"
                              @click:append-inner="showPassword = !showPassword"
                              :rules="[rules.required, rules.password]" />
              </v-col>

              <v-col cols="12" md="6">
                <v-select v-model="userFormData.dept_id"
                          :items="departments"
                          item-title="dept_name"
                          item-value="dept_id"
                          label="Department"
                          prepend-inner-icon="mdi-domain"
                          variant="outlined"
                          clearable />
              </v-col>

              <v-col cols="12" md="6">
                <v-select v-model="userFormData.role_id"
                          :items="roles"
                          item-title="role_name"
                          item-value="role_id"
                          label="Role"
                          prepend-inner-icon="mdi-shield-account"
                          variant="outlined"
                          clearable />
              </v-col>

              <v-col cols="12">
                <v-switch v-model="userFormData.is_active"
                          label="Active"
                          color="success"
                          hide-details />
              </v-col>
            </v-row>
          </v-form>
        </v-card-text>

        <v-card-actions class="pa-4">
          <v-spacer />
          <v-btn variant="text" @click="closeUserDialog">Cancel</v-btn>
          <v-btn color="primary"
                 variant="elevated"
                 @click="saveUser"
                 :disabled="!formValid"
                 :loading="saving">
            {{ editMode ? 'Update' : 'Create' }}
          </v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>

    <!-- Reset Password Dialog -->
    <v-dialog v-model="resetPasswordDialog" max-width="500px" persistent>
      <v-card>
        <v-card-title class="bg-warning">
          <v-icon class="mr-2">mdi-lock-reset</v-icon>
          Reset Password
        </v-card-title>

        <v-card-text class="pt-6">
          <v-alert type="info" variant="tonal" class="mb-4">
            Reset password for user: <strong>{{ selectedUser?.username }}</strong>
          </v-alert>

          <v-form ref="resetPasswordForm" v-model="resetPasswordValid">
            <v-text-field v-model="newPassword"
                          label="New Password"
                          prepend-inner-icon="mdi-lock"
                          variant="outlined"
                          :type="showResetPassword ? 'text' : 'password'"
                          :append-inner-icon="showResetPassword ? 'mdi-eye' : 'mdi-eye-off'"
                          @click:append-inner="showResetPassword = !showResetPassword"
                          :rules="[rules.required, rules.password]" />

            <v-text-field v-model="confirmPassword"
                          label="Confirm Password"
                          prepend-inner-icon="mdi-lock-check"
                          variant="outlined"
                          :type="showResetPassword ? 'text' : 'password'"
                          :rules="[rules.required, rules.passwordMatch]" />
          </v-form>
        </v-card-text>

        <v-card-actions class="pa-4">
          <v-spacer />
          <v-btn variant="text" @click="resetPasswordDialog = false">Cancel</v-btn>
          <v-btn color="warning"
                 variant="elevated"
                 @click="resetPassword"
                 :disabled="!resetPasswordValid"
                 :loading="saving">
            Reset Password
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
            Are you sure you want to delete user <strong>{{ selectedUser?.username }}</strong>?
          </v-alert>
          <p>This action cannot be undone. If the user has related records, they will be deactivated instead of deleted.</p>
        </v-card-text>

        <v-card-actions class="pa-4">
          <v-spacer />
          <v-btn variant="text" @click="deleteDialog = false">Cancel</v-btn>
          <v-btn color="error"
                 variant="elevated"
                 @click="deleteUser"
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
  import { useAuthStore } from '@/stores/auth.js'

  const settingsStore = useSettingsStore()
  const authStore = useAuthStore()

  // State
  const loading = ref(false)
  const saving = ref(false)
  const search = ref('')
  const filterDept = ref(null)
  const filterRole = ref(null)
  const filterStatus = ref(null)

  // Dialogs
  const userDialog = ref(false)
  const resetPasswordDialog = ref(false)
  const deleteDialog = ref(false)
  const editMode = ref(false)
  const formValid = ref(false)
  const resetPasswordValid = ref(false)

  // Form data
  const userFormData = ref({
    username: '',
    password: '',
    full_name: '',
    email: '',
    dept_id: null,
    role_id: null,
    is_active: true
  })

  const selectedUser = ref(null)
  const newPassword = ref('')
  const confirmPassword = ref('')
  const showPassword = ref(false)
  const showResetPassword = ref(false)

  // Snackbar
  const snackbar = ref(false)
  const snackbarMessage = ref('')
  const snackbarColor = ref('success')

  // Table headers
  const headers = [
    { title: 'User', value: 'username', width: '25%' },
    { title: 'Email', value: 'email', width: '20%' },
    { title: 'Department', value: 'dept_name', width: '15%' },
    { title: 'Role', value: 'role_name', width: '15%' },
    { title: 'Status', value: 'is_active', width: '10%' },
    { title: 'Created', value: 'created_at', width: '10%' },
    { title: 'Actions', value: 'actions', width: '5%', sortable: false }
  ]

  // Validation rules
  const rules = {
    required: v => !!v || 'This field is required',
    username: v => (v && v.length >= 3) || 'Username must be at least 3 characters',
    email: v => !v || /.+@.+\..+/.test(v) || 'Email must be valid',
    password: v => !v || v.length >= 6 || 'Password must be at least 6 characters',
    passwordMatch: v => v === newPassword.value || 'Passwords must match'
  }

  // Computed
  const currentUserId = computed(() => authStore.currentUser?.user_id)

  const departments = computed(() => settingsStore.departments)
  const roles = computed(() => settingsStore.roles)

  const filteredUsers = computed(() => {
    let result = settingsStore.users

    if (filterDept.value) {
      result = result.filter(u => u.dept_id === filterDept.value)
    }

    if (filterRole.value) {
      result = result.filter(u => u.role_id === filterRole.value)
    }

    if (filterStatus.value) {
      const isActive = filterStatus.value === 'Active'
      result = result.filter(u => u.is_active === isActive)
    }

    return result
  })

  // Methods
  function getInitials(name) {
    if (!name) return '?'
    return name.split(' ').map(n => n[0]).join('').toUpperCase().substring(0, 2)
  }

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
    return icons[roleName] || 'mdi-account'
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
    userFormData.value = {
      username: '',
      password: '',
      full_name: '',
      email: '',
      dept_id: null,
      role_id: null,
      is_active: true
    }
    userDialog.value = true
  }

  function openEditDialog(user) {
    editMode.value = true
    selectedUser.value = user
    userFormData.value = {
      username: user.username,
      full_name: user.full_name,
      email: user.email,
      dept_id: user.dept_id,
      role_id: user.role_id,
      is_active: user.is_active
    }
    userDialog.value = true
  }

  function closeUserDialog() {
    userDialog.value = false
    editMode.value = false
    selectedUser.value = null
  }

  async function saveUser() {
    saving.value = true
    try {
      let result

      if (editMode.value) {
        result = await settingsStore.updateUser(selectedUser.value.user_id, userFormData.value)
      } else {
        result = await settingsStore.createUser(userFormData.value)
      }

      if (result?.success) {
        snackbarMessage.value = editMode.value ? 'User updated successfully' : 'User created successfully'
        snackbarColor.value = 'success'
        snackbar.value = true
        closeUserDialog()
      } else {
        snackbarMessage.value = result?.message || 'Failed to save user'
        snackbarColor.value = 'error'
        snackbar.value = true
      }
    } catch (error) {
      snackbarMessage.value = 'Error saving user'
      snackbarColor.value = 'error'
      snackbar.value = true
    } finally {
      saving.value = false
    }
  }

  function openResetPasswordDialog(user) {
    selectedUser.value = user
    newPassword.value = ''
    confirmPassword.value = ''
    resetPasswordDialog.value = true
  }

  async function resetPassword() {
    saving.value = true
    try {
      const result = await settingsStore.resetUserPassword(selectedUser.value.user_id, newPassword.value)

      if (result?.success) {
        snackbarMessage.value = 'Password reset successfully'
        snackbarColor.value = 'success'
        snackbar.value = true
        resetPasswordDialog.value = false
      } else {
        snackbarMessage.value = result?.message || 'Failed to reset password'
        snackbarColor.value = 'error'
        snackbar.value = true
      }
    } catch (error) {
      snackbarMessage.value = 'Error resetting password'
      snackbarColor.value = 'error'
      snackbar.value = true
    } finally {
      saving.value = false
    }
  }

  async function toggleUserStatus(user) {
    if (user.user_id === currentUserId.value) {
      snackbarMessage.value = 'Cannot deactivate your own account'
      snackbarColor.value = 'warning'
      snackbar.value = true
      return
    }

    try {
      const result = await settingsStore.toggleUserStatus(user.user_id)

      if (result?.success) {
        snackbarMessage.value = result.message
        snackbarColor.value = 'success'
        snackbar.value = true
      } else {
        snackbarMessage.value = result?.message || 'Failed to toggle user status'
        snackbarColor.value = 'error'
        snackbar.value = true
      }
    } catch (error) {
      snackbarMessage.value = 'Error toggling user status'
      snackbarColor.value = 'error'
      snackbar.value = true
    }
  }

  function openDeleteDialog(user) {
    selectedUser.value = user
    deleteDialog.value = true
  }

  async function deleteUser() {
    saving.value = true
    try {
      const result = await settingsStore.deleteUser(selectedUser.value.user_id)

      if (result?.success) {
        snackbarMessage.value = result.message
        snackbarColor.value = 'success'
        snackbar.value = true
        deleteDialog.value = false
      } else {
        snackbarMessage.value = result?.message || 'Failed to delete user'
        snackbarColor.value = 'error'
        snackbar.value = true
      }
    } catch (error) {
      snackbarMessage.value = 'Error deleting user'
      snackbarColor.value = 'error'
      snackbar.value = true
    } finally {
      saving.value = false
    }
  }

  onMounted(async () => {
    loading.value = true
    try {
      await Promise.all([
        settingsStore.fetchUsers(),
        settingsStore.fetchRoles(),
        settingsStore.fetchDepartments()
      ])
    } catch (error) {
      snackbarMessage.value = 'Error loading data'
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
