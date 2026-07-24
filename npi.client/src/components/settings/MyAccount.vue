<template>
  <v-container fluid class="pa-4">
    <v-row justify="center">
      <v-col cols="12" md="8" lg="6">

        <!-- Profile -->
        <v-card variant="outlined" class="mb-4">
          <v-card-title class="text-subtitle-1 font-weight-bold py-3">
            <v-icon start size="20">mdi-account-circle</v-icon>
            My Profile
          </v-card-title>
          <v-divider />

          <v-card-text class="pt-4">
            <v-row dense>
              <v-col cols="12" sm="6">
                <v-text-field v-model="profile.username"
                              label="Username"
                              variant="outlined" density="compact"
                              readonly disabled
                              hint="Contact an administrator to change"
                              persistent-hint />
              </v-col>

              <v-col cols="12" sm="6">
                <v-text-field v-model="profile.full_name"
                              label="Full Name"
                              variant="outlined" density="compact"
                              :rules="[v => (v || '').length <= 100 || 'Max 100 characters']" />
              </v-col>

              <v-col cols="12" sm="6">
                <v-text-field :model-value="profile.email || '—'"
                              label="Email"
                              variant="outlined" density="compact"
                              readonly disabled
                              prepend-inner-icon="mdi-lock-outline"
                              hint="Used for email drafts — Admin/Manager only"
                              persistent-hint />
              </v-col>

              <v-col cols="12" sm="6">
                <v-text-field :model-value="profile.dept_name || '—'"
                              label="Department"
                              variant="outlined" density="compact"
                              readonly disabled
                              prepend-inner-icon="mdi-lock-outline"
                              hint="Drives task assignment — Admin/Manager only"
                              persistent-hint />
              </v-col>

              <v-col cols="12" sm="6">
                <v-text-field :model-value="profile.role_name || '—'"
                              label="System Role"
                              variant="outlined" density="compact"
                              readonly disabled
                              prepend-inner-icon="mdi-lock-outline" />
              </v-col>
            </v-row>

            <div class="d-flex justify-end mt-4">
              <v-btn color="primary" variant="flat" size="small"
                     :loading="savingProfile"
                     :disabled="!profileChanged"
                     @click="saveProfile">
                <v-icon start>mdi-content-save</v-icon>
                Save Changes
              </v-btn>
            </div>
          </v-card-text>
        </v-card>

        <!-- Password -->
        <v-card variant="outlined">
          <v-card-title class="text-subtitle-1 font-weight-bold py-3">
            <v-icon start size="20">mdi-lock-reset</v-icon>
            Change Password
          </v-card-title>
          <v-divider />

          <v-card-text class="pt-4">
            <v-form ref="passwordForm">
              <v-text-field v-model="pw.current_password"
                            label="Current Password"
                            :type="showCurrent ? 'text' : 'password'"
                            :append-inner-icon="showCurrent ? 'mdi-eye-off' : 'mdi-eye'"
                            variant="outlined" density="compact"
                            class="mb-2"
                            :rules="[v => !!v || 'Current password is required']"
                            @click:append-inner="showCurrent = !showCurrent" />

              <v-text-field v-model="pw.new_password"
                            label="New Password"
                            :type="showNew ? 'text' : 'password'"
                            :append-inner-icon="showNew ? 'mdi-eye-off' : 'mdi-eye'"
                            variant="outlined" density="compact"
                            class="mb-2"
                            :rules="newPasswordRules"
                            @click:append-inner="showNew = !showNew" />

              <v-text-field v-model="pw.confirm_password"
                            label="Confirm New Password"
                            :type="showNew ? 'text' : 'password'"
                            variant="outlined" density="compact"
                            :rules="confirmRules" />
            </v-form>

            <v-alert v-if="pwError" type="error" variant="tonal"
                     density="compact" class="mt-3">
              {{ pwError }}
            </v-alert>

            <div class="d-flex justify-end mt-4">
              <v-btn color="primary" variant="flat" size="small"
                     :loading="savingPassword"
                     @click="changePassword">
                <v-icon start>mdi-key-variant</v-icon>
                Update Password
              </v-btn>
            </div>
          </v-card-text>
        </v-card>

      </v-col>
    </v-row>

    <v-snackbar v-model="snackbar" :color="snackbarColor" location="bottom right" rounded="lg">
      {{ snackbarMessage }}
      <template #actions>
        <v-btn variant="text" @click="snackbar = false">Close</v-btn>
      </template>
    </v-snackbar>
  </v-container>
</template>

<script setup>
  import { ref, computed, onMounted } from 'vue'
  import { api } from '@/utils/api'
  import { useAuthStore } from '@/stores/auth'

  const authStore = useAuthStore()

  const profile = ref({
    username: '', full_name: '', email: '',
    dept_name: '', role_name: '',
  })
  const originalFullName = ref('')

  const savingProfile = ref(false)
  const savingPassword = ref(false)
  const passwordForm = ref(null)

  const showCurrent = ref(false)
  const showNew = ref(false)
  const pwError = ref('')

  const pw = ref({ current_password: '', new_password: '', confirm_password: '' })

  const snackbar = ref(false)
  const snackbarMessage = ref('')
  const snackbarColor = ref('success')

  const profileChanged = computed(() =>
    (profile.value.full_name ?? '') !== (originalFullName.value ?? '')
  )

  const newPasswordRules = [
    v => !!v || 'New password is required',
    v => (v || '').length >= 6 || 'Must be at least 6 characters',
    v => v !== pw.value.current_password || 'New password must differ from current',
  ]

  const confirmRules = [
    v => !!v || 'Please confirm the new password',
    v => v === pw.value.new_password || 'Passwords do not match',
  ]

  function showSnack(msg, color = 'success') {
    snackbarMessage.value = msg
    snackbarColor.value = color
    snackbar.value = true
  }

  async function loadProfile() {
    try {
      const res = await api.get('/auth/my-profile')
      const d = res?.data ?? res
      if (d) {
        profile.value = {
          username: d.username ?? '',
          full_name: d.full_name ?? '',
          email: d.email ?? '',
          dept_name: d.dept_name ?? '',
          role_name: d.role_name ?? '',
        }
        originalFullName.value = profile.value.full_name
      }
    } catch {
      showSnack('Could not load your profile', 'error')
    }
  }

  async function saveProfile() {
    savingProfile.value = true
    try {
      const res = await api.put('/auth/my-profile', {
        full_name: profile.value.full_name?.trim() ?? '',
      })
      if (res?.success) {
        originalFullName.value = profile.value.full_name
        await authStore.checkAuth()
        showSnack('Profile updated')
      } else {
        showSnack(res?.message || 'Failed to update profile', 'error')
      }
    } catch (err) {
      showSnack(err.message || 'Failed to update profile', 'error')
    } finally {
      savingProfile.value = false
    }
  }

  async function changePassword() {
    pwError.value = ''

    const { valid } = await passwordForm.value.validate()
    if (!valid) return

    savingPassword.value = true
    try {
      const res = await api.put('/auth/my-password', { ...pw.value })
      if (res?.success) {
        pw.value = { current_password: '', new_password: '', confirm_password: '' }
        passwordForm.value.resetValidation()
        showSnack('Password updated')
      } else {
        pwError.value = res?.message || 'Failed to update password'
      }
    } catch (err) {
      pwError.value = err.message || 'Failed to update password'
    } finally {
      savingPassword.value = false
    }
  }

  onMounted(loadProfile)
</script>
