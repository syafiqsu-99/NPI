<template>
  <div class="module-root d-flex flex-column pa-3 ga-3">

    <!-- Row 1: Page title -->
    <div class="flex-shrink-0">
      <h2 class="text-h6 font-weight-bold">Users Management</h2>
    </div>

    <!-- Row 2: Search + Add button -->
    <v-card class="flex-shrink-0" elevation="1">
      <v-card-text class="pa-3">
        <v-row dense align="center">
          <v-col cols="12" sm="4">
            <v-text-field v-model="search"
                          prepend-inner-icon="mdi-magnify"
                          label="Search users"
                          variant="outlined" density="compact"
                          clearable hide-details />
          </v-col>
          <v-col cols="6" sm="3">
            <v-select v-model="filterDept"
                      :items="departments"
                      item-title="dept_name" item-value="dept_id"
                      label="Department" variant="outlined"
                      density="compact" clearable hide-details />
          </v-col>
          <v-col v-if="isAdmin" cols="6" sm="2">
            <v-select v-model="filterRole"
                      :items="roles"
                      item-title="role_name" item-value="role_id"
                      label="Role" variant="outlined"
                      density="compact" clearable hide-details />
          </v-col>
          <v-col cols="6" sm="2">
            <v-select v-model="filterStatus"
                      :items="['Active', 'Inactive']"
                      label="Status" variant="outlined"
                      density="compact" clearable hide-details />
          </v-col>
          <v-col cols="auto" class="ml-auto">
            <v-btn color="primary" prepend-icon="mdi-account-plus"
                   size="small" @click="openCreateDialog">
              Add User
            </v-btn>
          </v-col>
        </v-row>
      </v-card-text>
    </v-card>

    <!-- Row 3: Data table fills remaining height -->
    <v-card class="flex-grow-1 d-flex flex-column" elevation="1" style="min-height: 0;">
      <v-data-table-virtual :headers="visibleHeaders"
                            :items="filteredUsers"
                            :loading="loading"
                            density="comfortable"
                            fixed-header
                            height="300"
                            class="user-table flex-grow-1">

        <template #item.username="{ item }">
          <div class="d-flex align-center py-1 ga-3">
            <v-avatar color="primary" size="32">
              <span class="text-white text-caption font-weight-bold">
                {{ getInitials(item.full_name || item.username) }}
              </span>
            </v-avatar>
            <div>
              <div class="font-weight-medium text-body-2">{{ item.username }}</div>
              <div class="text-caption text-grey">{{ item.full_name }}</div>
            </div>
          </div>
        </template>

        <template #item.dept_name="{ item }">
          <v-chip size="small" variant="tonal" color="primary">
            {{ item.dept_name || 'None' }}
          </v-chip>
        </template>

        <template #item.role_name="{ item }">
          <v-chip v-if="isAdmin" size="small" variant="tonal"
                  :color="getRoleColor(item.role_name)">
            <v-icon start size="small">{{ getRoleIcon(item.role_name) }}</v-icon>
            {{ item.role_name || 'None' }}
          </v-chip>
        </template>

        <template #item.is_active="{ item }">
          <v-chip size="small" :color="item.is_active ? 'success' : 'grey'" variant="flat">
            {{ item.is_active ? 'Active' : 'Inactive' }}
          </v-chip>
        </template>

        <template #item.created_at="{ item }">
          <span class="text-caption">{{ formatDate(item.created_at) }}</span>
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
              <v-list-item @click="openResetPasswordDialog(item)">
                <template #prepend>
                  <v-icon size="18">mdi-lock-reset</v-icon>
                </template>
                <v-list-item-title>Reset Password</v-list-item-title>
              </v-list-item>
              <v-list-item @click="toggleUserStatus(item)">
                <template #prepend>
                  <v-icon size="18">
                    {{ item.is_active ? 'mdi-account-off' : 'mdi-account-check' }}
                  </v-icon>
                </template>
                <v-list-item-title>{{ item.is_active ? 'Deactivate' : 'Activate' }}</v-list-item-title>
              </v-list-item>
              <v-divider />
              <v-list-item v-if="isAdmin"
                           :disabled="item.user_id === currentUserId"
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
    <v-dialog v-model="userDialog" max-width="640" persistent>
      <v-card>
        <v-card-title class="bg-primary text-white text-subtitle-1">
          {{ editMode ? 'Edit User' : 'Create User' }}
        </v-card-title>
        <v-card-text class="pt-4">
          <v-form ref="userForm" v-model="formValid">
            <v-row dense>
              <v-col cols="12" md="6">
                <v-text-field v-model="userFormData.username" label="Username *"
                              prepend-inner-icon="mdi-account" variant="outlined"
                              density="comfortable"
                              :rules="[rules.required, rules.username]"
                              :readonly="editMode" />
              </v-col>
              <v-col cols="12" md="6">
                <v-text-field v-model="userFormData.full_name" label="Full Name"
                              prepend-inner-icon="mdi-account-box" variant="outlined"
                              density="comfortable" />
              </v-col>
              <v-col cols="12" md="6">
                <v-text-field v-model="userFormData.email" label="Email"
                              prepend-inner-icon="mdi-email" variant="outlined"
                              density="comfortable" type="email"
                              :rules="[rules.email]" />
              </v-col>
              <v-col v-if="!editMode" cols="12" md="6">
                <v-text-field v-model="userFormData.password" label="Password *"
                              prepend-inner-icon="mdi-lock" variant="outlined"
                              density="comfortable"
                              :type="showPassword ? 'text' : 'password'"
                              :append-inner-icon="showPassword ? 'mdi-eye' : 'mdi-eye-off'"
                              @click:append-inner="showPassword = !showPassword"
                              :rules="[rules.required, rules.password]" />
              </v-col>
              <v-col cols="12" md="6">
                <v-select v-model="userFormData.dept_id"
                          :items="departments" item-title="dept_name" item-value="dept_id"
                          label="Department" prepend-inner-icon="mdi-domain"
                          variant="outlined" density="comfortable" clearable />
              </v-col>
              <v-col v-if="isAdmin" cols="12" md="6">
                <v-select v-model="userFormData.role_id"
                          :items="rolesForAssignment"
                          item-title="role_name" item-value="role_id"
                          label="System Role" prepend-inner-icon="mdi-shield-account"
                          variant="outlined" density="comfortable" clearable />
              </v-col>
              <v-col cols="12">
                <v-switch v-model="userFormData.is_active" label="Active"
                          color="success" hide-details density="compact" />
              </v-col>
            </v-row>
          </v-form>
        </v-card-text>
        <v-card-actions class="pa-4">
          <v-spacer />
          <v-btn variant="text" @click="closeUserDialog">Cancel</v-btn>
          <v-btn color="primary" variant="elevated"
                 :disabled="!formValid" :loading="saving"
                 @click="saveUser">
            {{ editMode ? 'Update' : 'Create' }}
          </v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>

    <!-- Reset Password dialog -->
    <v-dialog v-model="resetPasswordDialog" max-width="480" persistent>
      <v-card>
        <v-card-title class="bg-warning text-white text-subtitle-1">Reset Password</v-card-title>
        <v-card-text class="pt-4">
          <v-alert type="info" variant="tonal" density="compact" class="mb-4">
            Resetting password for <strong>{{ selectedUser?.username }}</strong>
          </v-alert>
          <v-text-field v-model="newPassword" label="New Password *"
                        prepend-inner-icon="mdi-lock" variant="outlined"
                        density="comfortable"
                        :type="showNewPassword ? 'text' : 'password'"
                        :append-inner-icon="showNewPassword ? 'mdi-eye' : 'mdi-eye-off'"
                        @click:append-inner="showNewPassword = !showNewPassword"
                        :rules="[rules.required, rules.password]" />
          <v-text-field v-model="confirmPassword" label="Confirm Password *"
                        prepend-inner-icon="mdi-lock-check" variant="outlined"
                        density="comfortable"
                        :type="showNewPassword ? 'text' : 'password'"
                        :rules="[rules.required, rules.passwordMatch]" />
        </v-card-text>
        <v-card-actions class="pa-4">
          <v-spacer />
          <v-btn variant="text" @click="resetPasswordDialog = false">Cancel</v-btn>
          <v-btn color="warning" variant="elevated" :loading="saving" @click="resetPassword">
            Reset
          </v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>

    <!-- Delete Confirmation dialog -->
    <v-dialog v-model="deleteDialog" max-width="480">
      <v-card>
        <v-card-title class="bg-error text-white text-subtitle-1">
          <v-icon class="mr-2">mdi-alert</v-icon>
          Confirm Delete
        </v-card-title>
        <v-card-text class="pt-4">
          <v-alert type="error" variant="tonal" class="mb-3">
            Delete user <strong>{{ selectedUser?.username }}</strong>? This cannot be undone.
          </v-alert>
        </v-card-text>
        <v-card-actions class="pa-4">
          <v-spacer />
          <v-btn variant="text" @click="deleteDialog = false">Cancel</v-btn>
          <v-btn color="error" variant="elevated" :loading="saving" @click="deleteUser">
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
  import { useAuthStore } from '@/stores/auth'
  import { useSettingsStore } from '@/stores/setting.js'

  const props = defineProps({ isAdmin: Boolean })

  const authStore = useAuthStore()
  const settingsStore = useSettingsStore()

  const loading = ref(false)
  const saving = ref(false)
  const userDialog = ref(false)
  const resetPasswordDialog = ref(false)
  const deleteDialog = ref(false)
  const editMode = ref(false)
  const formValid = ref(false)
  const showPassword = ref(false)
  const showNewPassword = ref(false)
  const search = ref('')
  const filterDept = ref(null)
  const filterRole = ref(null)
  const filterStatus = ref(null)
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

  const isAdmin = computed(() => authStore.user?.role === 'Admin')
  const currentUserId = computed(() => authStore.currentUser?.user_id)
  const departments = computed(() => settingsStore.departments)
  const roles = computed(() => settingsStore.roles)

  const rolesForAssignment = computed(() =>
    isAdmin.value ? roles.value : roles.value.filter(r => r.role_name !== 'Admin')
  )

  const visibleHeaders = computed(() => {
    const h = [
      { title: 'User', value: 'username', width: '28%' },
      { title: 'Email', value: 'email', width: '20%' },
      { title: 'Department', value: 'dept_name', width: '14%' },
      { title: 'Status', value: 'is_active', width: '10%' },
      { title: 'Created', value: 'created_at', width: '12%' },
      { title: '', value: 'actions', width: '6%', sortable: false }
    ]
    if (isAdmin.value)
      h.splice(3, 0, { title: 'Role', value: 'role_name', width: '12%' })
    return h
  })

  // Simplified filter: single computed pass over the user list
  const filteredUsers = computed(() => {
    const users = settingsStore.users
    const dept = filterDept.value
    const role = filterRole.value
    const status = filterStatus.value
    const q = search.value?.toLowerCase() ?? ''

    return users.filter(u => {
      if (dept && u.dept_id !== dept) return false
      if (role && isAdmin.value && u.role_id !== role) return false
      if (status === 'Active' && !u.is_active) return false
      if (status === 'Inactive' && u.is_active) return false
      if (q && !u.username?.toLowerCase().includes(q) &&
        !u.full_name?.toLowerCase().includes(q) &&
        !u.email?.toLowerCase().includes(q)) return false
      return true
    })
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
    return { Admin: 'error', Manager: 'primary', 'Team Lead': 'success', Member: 'info' }[r] || 'grey'
  }
  function getRoleIcon(r) {
    return {
      Admin: 'mdi-shield-crown', Manager: 'mdi-account-tie',
      'Team Lead': 'mdi-account-star', Member: 'mdi-account'
    }[r] || 'mdi-account'
  }
  function formatDate(d) {
    return d
      ? new Date(d).toLocaleDateString('en-GB', { day: '2-digit', month: 'short', year: 'numeric' })
      : 'N/A'
  }
  function showSnack(msg, color = 'success') {
    snackbarMessage.value = msg; snackbarColor.value = color; snackbar.value = true
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
        showSnack('Password reset')
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
      showSnack('Cannot deactivate your own account', 'warning'); return
    }
    const result = await settingsStore.toggleUserStatus(user.user_id)
    showSnack(result?.message || 'Done', result?.success ? 'success' : 'error')
  }

  function openDeleteDialog(user) {
    selectedUser.value = user; deleteDialog.value = true
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
  .module-root {
    height: 100%;
    overflow: hidden;
  }

  .user-table :deep(th) {
    font-weight: 600 !important;
    font-size: 11px;
    text-transform: uppercase;
    letter-spacing: 0.4px;
    background: #fafbfc !important;
  }

  .user-table :deep(.v-table__wrapper) {
    height: 100%;
    overflow-y: auto;
  }
</style>
