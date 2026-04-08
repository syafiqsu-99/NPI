<template>
  <div class="user-mgmt-root d-flex flex-column pa-4" style="height:100%; overflow:hidden;">
    <!-- Header -->
    <div class="d-flex justify-space-between align-center mb-3 flex-shrink-0">
      <div>
        <h2 class="text-h5">User Management</h2>
        <p class="text-caption text-grey">Manage system users and permissions</p>
      </div>
      <v-btn color="primary" prepend-icon="mdi-account-plus" @click="openCreateDialog">
        Add User
      </v-btn>
    </div>

    <!-- Filters -->
    <v-card class="mb-3 flex-shrink-0" variant="outlined">
      <v-card-text class="pa-3">
        <v-row dense>
          <v-col cols="12" md="4">
            <v-text-field v-model="search" prepend-inner-icon="mdi-magnify"
                          label="Search users" variant="outlined" density="compact"
                          clearable hide-details />
          </v-col>
          <v-col cols="6" md="3">
            <v-select v-model="filterDept" :items="departments" item-title="dept_name"
                      item-value="dept_id" label="Department" variant="outlined"
                      density="compact" clearable hide-details />
          </v-col>
          <v-col v-if="isAdmin" cols="6" md="3">
            <v-select v-model="filterRole" :items="roles" item-title="role_name"
                      item-value="role_id" label="Role" variant="outlined"
                      density="compact" clearable hide-details />
          </v-col>
          <v-col cols="6" md="2">
            <v-select v-model="filterStatus" :items="['Active', 'Inactive']"
                      label="Status" variant="outlined" density="compact"
                      clearable hide-details />
          </v-col>
        </v-row>
      </v-card-text>
    </v-card>

    <!-- Virtual table fills remaining height -->
    <v-card class="flex-grow-1 d-flex flex-column overflow-hidden" variant="outlined">
      <v-data-table-virtual :headers="visibleHeaders"
                            :items="filteredUsers"
                            :search="search"
                            :loading="loading"
                            density="comfortable"
                            height="100%"
                            fixed-header
                            class="user-table flex-grow-1">

        <template #item.username="{ item }">
          <div class="d-flex align-center py-1">
            <v-avatar color="primary" size="34" class="mr-3">
              <span class="text-white text-caption">{{ getInitials(item.full_name || item.username) }}</span>
            </v-avatar>
            <div>
              <div class="font-weight-medium">{{ item.username }}</div>
              <div class="text-caption text-grey">{{ item.full_name }}</div>
            </div>
          </div>
        </template>

        <template #item.dept_name="{ item }">
          <v-chip size="small" variant="tonal" color="blue">{{ item.dept_name || 'None' }}</v-chip>
        </template>

        <template #item.role_name="{ item }">
          <v-chip v-if="isAdmin" size="small" variant="tonal" :color="getRoleColor(item.role_name)">
            <v-icon start size="small">{{ getRoleIcon(item.role_name) }}</v-icon>
            {{ item.role_name || 'None' }}
          </v-chip>
        </template>

        <template #item.is_active="{ item }">
          <v-chip size="small" :color="item.is_active ? 'success' : 'grey'" variant="flat">
            <v-icon start size="small">{{ item.is_active ? 'mdi-check-circle' : 'mdi-close-circle' }}</v-icon>
            {{ item.is_active ? 'Active' : 'Inactive' }}
          </v-chip>
        </template>

        <template #item.created_at="{ item }">
          <span class="text-caption">{{ formatDate(item.created_at) }}</span>
        </template>

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
                  <v-icon>{{ item.is_active ? 'mdi-account-off' : 'mdi-account-check' }}</v-icon>
                </template>
                <v-list-item-title>{{ item.is_active ? 'Deactivate' : 'Activate' }}</v-list-item-title>
              </v-list-item>
              <v-divider />
              <v-list-item v-if="isAdmin" @click="openDeleteDialog(item)"
                           :disabled="item.user_id === currentUserId">
                <template #prepend>
                  <v-icon color="error">mdi-delete</v-icon>
                </template>
                <v-list-item-title class="text-error">Delete</v-list-item-title>
              </v-list-item>
            </v-list>
          </v-menu>
        </template>
      </v-data-table-virtual>
    </v-card>

    <!-- Create/Edit Dialog -->
    <v-dialog v-model="userDialog" max-width="700" persistent>
      <v-card>
        <v-card-title class="bg-primary text-white">
          {{ editMode ? 'Edit User' : 'Create New User' }}
        </v-card-title>
        <v-card-text class="pt-4">
          <v-form ref="userForm" v-model="formValid">
            <v-row>
              <v-col cols="12" md="6">
                <v-text-field v-model="userFormData.username" label="Username *"
                              prepend-inner-icon="mdi-account" variant="outlined"
                              :rules="[rules.required, rules.username]" :readonly="editMode" />
              </v-col>
              <v-col cols="12" md="6">
                <v-text-field v-model="userFormData.full_name" label="Full Name"
                              prepend-inner-icon="mdi-account-box" variant="outlined" />
              </v-col>
              <v-col cols="12" md="6">
                <v-text-field v-model="userFormData.email" label="Email"
                              prepend-inner-icon="mdi-email" variant="outlined"
                              type="email" :rules="[rules.email]" />
              </v-col>
              <v-col v-if="!editMode" cols="12" md="6">
                <v-text-field v-model="userFormData.password" label="Password *"
                              prepend-inner-icon="mdi-lock" variant="outlined"
                              :type="showPassword ? 'text' : 'password'"
                              :append-inner-icon="showPassword ? 'mdi-eye' : 'mdi-eye-off'"
                              @click:append-inner="showPassword = !showPassword"
                              :rules="[rules.required, rules.password]" />
              </v-col>
              <v-col cols="12" md="6">
                <v-select v-model="userFormData.dept_id" :items="departments"
                          item-title="dept_name" item-value="dept_id"
                          label="Department" prepend-inner-icon="mdi-domain"
                          variant="outlined" clearable />
              </v-col>
              <!-- Role selector only shown to Admins -->
              <v-col v-if="isAdmin" cols="12" md="6">
                <v-select v-model="userFormData.role_id"
                          :items="rolesForAssignment" item-title="role_name" item-value="role_id"
                          label="Role" prepend-inner-icon="mdi-shield-account"
                          variant="outlined" clearable />
              </v-col>
              <v-col cols="12">
                <v-switch v-model="userFormData.is_active" label="Active" color="success" hide-details />
              </v-col>
            </v-row>
          </v-form>
        </v-card-text>
        <v-card-actions class="pa-4">
          <v-spacer />
          <v-btn variant="text" @click="closeUserDialog">Cancel</v-btn>
          <v-btn color="primary" variant="elevated" :disabled="!formValid"
                 :loading="saving" @click="saveUser">
            {{ editMode ? 'Update' : 'Create' }}
          </v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>

    <!-- Reset Password Dialog -->
    <v-dialog v-model="resetPasswordDialog" max-width="500" persistent>
      <v-card>
        <v-card-title class="bg-warning text-white">
          <v-icon class="mr-2">mdi-lock-reset</v-icon>Reset Password
        </v-card-title>
        <v-card-text class="pt-4">
          <v-alert type="info" variant="tonal" class="mb-4">
            Reset password for: <strong>{{ selectedUser?.username }}</strong>
          </v-alert>
          <v-form ref="resetPasswordForm" v-model="resetPasswordValid">
            <v-text-field v-model="newPassword" label="New Password"
                          prepend-inner-icon="mdi-lock" variant="outlined"
                          :type="showResetPassword ? 'text' : 'password'"
                          :append-inner-icon="showResetPassword ? 'mdi-eye' : 'mdi-eye-off'"
                          @click:append-inner="showResetPassword = !showResetPassword"
                          :rules="[rules.required, rules.password]" class="mb-3" />
            <v-text-field v-model="confirmPassword" label="Confirm Password"
                          prepend-inner-icon="mdi-lock-check" variant="outlined"
                          :type="showResetPassword ? 'text' : 'password'"
                          :rules="[rules.required, rules.passwordMatch]" />
          </v-form>
        </v-card-text>
        <v-card-actions class="pa-4">
          <v-spacer />
          <v-btn variant="text" @click="resetPasswordDialog = false">Cancel</v-btn>
          <v-btn color="warning" variant="elevated" :disabled="!resetPasswordValid"
                 :loading="saving" @click="resetPassword">
            Reset Password
          </v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>

    <!-- Delete Dialog -->
    <v-dialog v-model="deleteDialog" max-width="500">
      <v-card>
        <v-card-title class="bg-error text-white">
          <v-icon class="mr-2">mdi-alert</v-icon>Confirm Delete
        </v-card-title>
        <v-card-text class="pt-4">
          <v-alert type="error" variant="tonal" class="mb-4">
            Delete user <strong>{{ selectedUser?.username }}</strong>? This cannot be undone.
          </v-alert>
        </v-card-text>
        <v-card-actions class="pa-4">
          <v-spacer />
          <v-btn variant="text" @click="deleteDialog = false">Cancel</v-btn>
          <v-btn color="error" variant="elevated" :loading="saving" @click="deleteUser">Delete</v-btn>
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
  import { useAuthStore } from '@/stores/auth.js'

  const props = defineProps({ isAdmin: Boolean })

  const settingsStore = useSettingsStore()
  const authStore = useAuthStore()

  const loading = ref(false)
  const saving = ref(false)
  const search = ref('')
  const filterDept = ref(null)
  const filterRole = ref(null)
  const filterStatus = ref(null)
  const showPassword = ref(false)
  const showResetPassword = ref(false)
  const userDialog = ref(false)
  const resetPasswordDialog = ref(false)
  const deleteDialog = ref(false)
  const editMode = ref(false)
  const formValid = ref(false)
  const resetPasswordValid = ref(false)
  const newPassword = ref('')
  const confirmPassword = ref('')
  const selectedUser = ref(null)
  const snackbar = ref(false)
  const snackbarMessage = ref('')
  const snackbarColor = ref('success')

  const userFormData = ref({
    username: '', password: '', full_name: '', email: '',
    dept_id: null, role_id: null, is_active: true
  })

  const userRole = computed(() => authStore.user?.role)
  const isAdmin = computed(() => userRole.value === 'Admin')
  const currentUserId = computed(() => authStore.currentUser?.user_id)
  const departments = computed(() => settingsStore.departments)
  const roles = computed(() => settingsStore.roles)

  // Managers cannot assign Admin role
  const rolesForAssignment = computed(() =>
    isAdmin.value ? roles.value : roles.value.filter(r => r.role_name !== 'Admin')
  )

  // Conditionally show Role column only for Admins
  const visibleHeaders = computed(() => {
    const base = [
      { title: 'User', value: 'username', width: '28%' },
      { title: 'Email', value: 'email', width: '20%' },
      { title: 'Department', value: 'dept_name', width: '15%' },
      { title: 'Status', value: 'is_active', width: '10%' },
      { title: 'Created', value: 'created_at', width: '12%' },
      { title: 'Actions', value: 'actions', width: '8%', sortable: false }
    ]
    if (isAdmin.value) {
      base.splice(3, 0, { title: 'Role', value: 'role_name', width: '12%' })
    }
    return base
  })

  const filteredUsers = computed(() => {
    let result = settingsStore.users
    if (filterDept.value) result = result.filter(u => u.dept_id === filterDept.value)
    if (filterRole.value && isAdmin.value) result = result.filter(u => u.role_id === filterRole.value)
    if (filterStatus.value) {
      const active = filterStatus.value === 'Active'
      result = result.filter(u => u.is_active === active)
    }
    return result
  })

  const rules = {
    required: v => !!v || 'Required',
    username: v => (v && v.length >= 3) || 'At least 3 characters',
    email: v => !v || /.+@.+\..+/.test(v) || 'Invalid email',
    password: v => !v || v.length >= 6 || 'At least 6 characters',
    passwordMatch: v => v === newPassword.value || 'Passwords must match'
  }

  function getInitials(name) {
    if (!name) return '?'
    return name.split(' ').map(n => n[0]).join('').toUpperCase().substring(0, 2)
  }
  function getRoleColor(r) {
    return { Admin: 'error', Manager: 'primary', 'Team Lead': 'success', Member: 'info', User: 'grey' }[r] || 'grey'
  }
  function getRoleIcon(r) {
    return { Admin: 'mdi-shield-crown', Manager: 'mdi-account-tie', 'Team Lead': 'mdi-account-star', Member: 'mdi-account' }[r] || 'mdi-account'
  }
  function formatDate(d) {
    if (!d) return 'N/A'
    return new Date(d).toLocaleDateString('en-GB', { day: '2-digit', month: 'short', year: 'numeric' })
  }

  function openCreateDialog() {
    editMode.value = false
    const defaultRole = roles.value.find(r => r.role_name === 'Member')
    userFormData.value = {
      username: '', password: '', full_name: '', email: '',
      dept_id: null, role_id: defaultRole?.role_id ?? null, is_active: true
    }
    userDialog.value = true
  }

  function openEditDialog(user) {
    editMode.value = true
    selectedUser.value = user
    userFormData.value = {
      username: user.username, full_name: user.full_name, email: user.email,
      dept_id: user.dept_id, role_id: user.role_id, is_active: user.is_active
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
      const result = editMode.value
        ? await settingsStore.updateUser(selectedUser.value.user_id, userFormData.value)
        : await settingsStore.createUser(userFormData.value)

      if (result?.success) {
        showSnack(editMode.value ? 'User updated' : 'User created')
        closeUserDialog()
      } else {
        showSnack(result?.message || 'Failed', 'error')
      }
    } catch {
      showSnack('Error saving user', 'error')
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
        showSnack('Password reset successfully')
        resetPasswordDialog.value = false
      } else {
        showSnack(result?.message || 'Failed', 'error')
      }
    } catch {
      showSnack('Error', 'error')
    } finally {
      saving.value = false
    }
  }

  async function toggleUserStatus(user) {
    if (user.user_id === currentUserId.value) {
      showSnack('Cannot deactivate your own account', 'warning')
      return
    }
    try {
      const result = await settingsStore.toggleUserStatus(user.user_id)
      showSnack(result?.message || 'Done', result?.success ? 'success' : 'error')
    } catch {
      showSnack('Error', 'error')
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
      showSnack(result?.message || 'Done', result?.success ? 'success' : 'error')
      deleteDialog.value = false
    } catch {
      showSnack('Error', 'error')
    } finally {
      saving.value = false
    }
  }

  function showSnack(msg, color = 'success') {
    snackbarMessage.value = msg
    snackbarColor.value = color
    snackbar.value = true
  }

  onMounted(async () => {
    loading.value = true
    try {
      await Promise.all([
        settingsStore.fetchUsers(),
        settingsStore.fetchRoles(),
        settingsStore.fetchDepartments()
      ])
    } finally {
      loading.value = false
    }
  })
</script>

<style scoped>
  .user-mgmt-root {
    height: 100%;
    overflow: hidden;
  }

  .user-table :deep(.v-table__wrapper) {
    overflow-y: auto;
  }

  .user-table :deep(th) {
    font-weight: 600 !important;
  }
</style>
