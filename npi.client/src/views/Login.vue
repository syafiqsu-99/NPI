<template>
  <v-container fluid class="fill-height bg-grey-lighten-4 d-flex align-center justify-center pa-4">

    <v-card elevation="3" max-width="420" width="100%" class="rounded-lg overflow-hidden">

      <!-- ── Brand Header ─────────────────────────────────────────────── -->
      <div class="bg-primary px-6 py-8 text-center">
        <img src="/clipboard-list-outline.ico"
             alt="NPI Project Management Logo"
             width="56"
             height="56"
             class="mb-3"
             style="opacity: 0.9; object-fit: contain;" />
        <h1 class="text-h5 font-weight-bold text-white mb-1">
          NPI Project Management
        </h1>
        <div class="text-subtitle-2 text-white" style="opacity: 0.8;">
          Sign in to your account
        </div>
      </div>

      <!-- ── Login Form ───────────────────────────────────────────────── -->
      <v-card-text class="pa-8">
        <v-form @submit.prevent="handleLogin" ref="loginForm">

          <div class="text-caption text-grey-darken-1 font-weight-medium mb-1">Username</div>
          <v-text-field v-model="credentials.username"
                        placeholder="Enter your username"
                        prepend-inner-icon="mdi-account-outline"
                        variant="outlined"
                        density="comfortable"
                        color="primary"
                        class="mb-2"
                        hide-details="auto"
                        required />

          <div class="text-caption text-grey-darken-1 font-weight-medium mb-1 mt-4">Password</div>
          <v-text-field v-model="credentials.password"
                        placeholder="Enter your password"
                        prepend-inner-icon="mdi-lock-outline"
                        :append-inner-icon="showPassword ? 'mdi-eye-off' : 'mdi-eye'"
                        :type="showPassword ? 'text' : 'password'"
                        @click:append-inner="showPassword = !showPassword"
                        variant="outlined"
                        density="comfortable"
                        color="primary"
                        hide-details="auto"
                        required />

          <!-- Error Alert -->
          <v-expand-transition>
            <v-alert v-if="errorMessage"
                     type="error"
                     variant="tonal"
                     density="compact"
                     class="mt-6 text-body-2">
              {{ errorMessage }}
            </v-alert>
          </v-expand-transition>

          <!-- Submit Button -->
          <v-btn type="submit"
                 color="primary"
                 size="large"
                 block
                 elevation="2"
                 class="mt-8 font-weight-bold"
                 :loading="loading"
                 :disabled="!isFormValid || loading">
            <v-icon start>mdi-login</v-icon>
            {{ loading ? 'Signing in...' : 'Sign In' }}
          </v-btn>

        </v-form>
      </v-card-text>

      <v-divider />
      <v-card-actions class="bg-grey-lighten-5 justify-center py-3">
        <span class="text-caption text-grey-darken-1">
          Need help? Contact your system administrator.
        </span>
      </v-card-actions>

    </v-card>
  </v-container>
</template>

<script setup>
  import { ref, computed } from 'vue'
  import { useRouter } from 'vue-router'
  import { useAuthStore } from '@/stores/auth'

  const router = useRouter()
  const authStore = useAuthStore()

  const credentials = ref({
    username: '',
    password: ''
  })

  const loading = ref(false)
  const errorMessage = ref('')
  const showPassword = ref(false)

  const isFormValid = computed(() => {
    return credentials.value.username.trim().length > 0 &&
      credentials.value.password.length > 0
  })

  const handleLogin = async () => {
    if (!isFormValid.value) return

    loading.value = true
    errorMessage.value = ''

    const result = await authStore.login(credentials.value.username, credentials.value.password)

    loading.value = false

    if (result.success) {
      router.push('/')
    } else {
      errorMessage.value = result.message || 'Invalid credentials. Please try again.'
    }
  }
</script>
