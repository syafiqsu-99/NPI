<template>
  <v-container fluid fill-height class="d-flex align-center justify-center">
    <v-card elevation="6"
            max-width="420"
            class="w-100 pa-6">
      <v-card-title class="text-center text-h5 font-weight-bold">
        NPI Project Management
      </v-card-title>

      <v-card-subtitle class="text-center mb-6">
        Sign in to your account
      </v-card-subtitle>

      <v-form @submit.prevent="handleLogin">
        <v-text-field v-model="credentials.username"
                      label="Username"
                      prepend-inner-icon="mdi-account"
                      variant="outlined"
                      density="comfortable"
                      required />

        <v-text-field v-model="credentials.password"
                      label="Password"
                      prepend-inner-icon="mdi-lock"
                      type="password"
                      variant="outlined"
                      density="comfortable"
                      required />

        <v-alert v-if="errorMessage"
                 type="error"
                 variant="tonal"
                 class="mb-4">
          {{ errorMessage }}
        </v-alert>

        <v-btn type="submit"
               color="primary"
               size="large"
               block
               :loading="loading"
               :disabled="loading">
          <v-icon start>mdi-login</v-icon>
          {{ loading ? 'Signing in...' : 'Sign In' }}
        </v-btn>
      </v-form>

      <v-divider class="my-6" />

      <div class="text-center text-caption text-medium-emphasis">
        Default credentials: <strong>admin / Admin@123</strong>
      </div>
    </v-card>
  </v-container>
</template>

<script setup>
  import { ref } from 'vue'
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

  const handleLogin = async () => {
    loading.value = true
    errorMessage.value = ''

    const result = await authStore.login(credentials.value.username, credentials.value.password)

    loading.value = false

    if (result.success) {
      router.push('/')
    } else {
      errorMessage.value = result.message || 'Invalid credentials'
    }
  }
</script>
