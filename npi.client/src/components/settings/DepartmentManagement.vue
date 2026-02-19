<template>
  <v-container fluid class="pa-6">
    <v-row>
      <v-col cols="12">
        <!-- Header -->
        <div class="d-flex justify-space-between align-center mb-4">
          <div>
            <h2 class="text-h5">Department Management</h2>
            <p class="text-caption text-grey">View organizational departments</p>
          </div>
          <v-chip color="info" variant="tonal">
            <v-icon start>mdi-information</v-icon>
            Read-Only
          </v-chip>
        </div>

        <!-- Departments Table -->
        <v-card>
          <v-data-table :headers="headers"
                        :items="departments"
                        :loading="loading"
                        class="elevation-1"
                        density="comfortable"
                        :items-per-page="10">
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
          </v-data-table>
        </v-card>

        <!-- Info Box -->
        <v-alert type="info" variant="tonal" class="mt-4">
          <v-alert-title>
            <v-icon class="mr-2">mdi-information</v-icon>
            Department Management
          </v-alert-title>
          Department creation and editing is currently managed by system administrators.
          Contact your administrator to request changes to departments.
        </v-alert>
      </v-col>
    </v-row>

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

  // Snackbar
  const snackbar = ref(false)
  const snackbarMessage = ref('')
  const snackbarColor = ref('success')

  // Table headers
  const headers = [
    { title: 'Department', value: 'dept_name', width: '40%' },
    { title: 'Users', value: 'user_count', width: '20%' },
    { title: 'Created', value: 'created_at', width: '20%' }
  ]

  // Computed
  const departments = computed(() => settingsStore.departments)
  const users = computed(() => settingsStore.users)

  // Methods
  function getDepartmentColor(deptName) {
    const colors = {
      'Sales': 'blue',
      'Technical': 'green',
      'Purchaser': 'orange',
      'Purchasing': 'orange',
      'QA': 'purple',
      'Production': 'red',
      'Others': 'grey'
    }
    return colors[deptName] || 'grey'
  }

  function getDepartmentIcon(deptName) {
    const icons = {
      'Sales': 'mdi-chart-line',
      'Technical': 'mdi-cog',
      'Purchaser': 'mdi-cart',
      'Purchasing': 'mdi-cart',
      'QA': 'mdi-check-decagram',
      'Production': 'mdi-factory',
      'Others': 'mdi-domain'
    }
    return icons[deptName] || 'mdi-domain'
  }

  function getUserCount(deptId) {
    return users.value.filter(u => u.dept_id === deptId).length
  }

  function formatDate(date) {
    if (!date) return 'N/A'
    return new Date(date).toLocaleDateString('en-GB', {
      day: '2-digit',
      month: 'short',
      year: 'numeric'
    })
  }

  onMounted(async () => {
    loading.value = true
    try {
      await Promise.all([
        settingsStore.fetchDepartments(),
        settingsStore.fetchUsers()
      ])
    } catch (error) {
      snackbarMessage.value = 'Error loading departments'
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
