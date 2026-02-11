<template>
  <v-navigation-drawer v-if="authStore.isAuthenticated"
                       permanent
                       expand-on-hover
                       rail>
    <v-list>
      <v-list-item title="NPI Project Management"
                   subtitle="NPI"
                   prepend-icon="mdi-clipboard-text-outline" />
    </v-list>

    <v-divider></v-divider>

    <v-list nav>
      <v-list-item to="/" title="Dashboard" prepend-icon="mdi-view-dashboard" />
      <v-list-item to="/enquiries" title="Enquiries" prepend-icon="mdi-clipboard-list" />
      <v-list-item to="/projects" title="Projects" prepend-icon="mdi-folder-multiple" />
      <v-list-item to="/tasks" title="Tasks" prepend-icon="mdi-check-circle" />
      <v-list-item to="/files" title="Files" prepend-icon="mdi-file-document" />
    </v-list>

    <template #append>
      <v-divider />

      <v-list>
        <v-list-item :title="authStore.currentUser?.full_name"
                     :subtitle="authStore.userRole"
                     prepend-icon="mdi-account-circle" />

        <v-list-item title="Logout"
                     prepend-icon="mdi-logout"
                     @click="handleLogout" />
      </v-list>
    </template>
  </v-navigation-drawer>
</template>

<script setup>
  import { onMounted } from 'vue'
  import { useRouter } from 'vue-router'
  import { useAuthStore } from '@/stores/auth'

  const router = useRouter()
  const authStore = useAuthStore()

  const handleLogout = async () => {
    await authStore.logout()
    router.push('/login')
  }

  onMounted(async () => {
    if (authStore.token) {
      await authStore.checkAuth()
    }
  })
</script>
