<template>
  <div class="module-root d-flex flex-column pa-3 ga-3">

    <!-- Row 1: Page title -->
    <div class="flex-shrink-0">
      <h2 class="text-h6 font-weight-bold">System Settings</h2>
    </div>

    <!-- Row 2: Stat summary card -->
    <v-card class="flex-shrink-0" elevation="1">
      <v-card-text class="pa-3">
        <v-row dense>
          <v-col cols="6" sm="3">
            <v-card variant="tonal" color="blue">
              <v-card-text class="pa-3">
                <div class="text-h5 font-weight-bold">{{ systemStats.totalUsers }}</div>
                <div class="text-caption">Total Users</div>
              </v-card-text>
            </v-card>
          </v-col>
          <v-col cols="6" sm="3">
            <v-card variant="tonal" color="green">
              <v-card-text class="pa-3">
                <div class="text-h5 font-weight-bold">{{ systemStats.activeProjects }}</div>
                <div class="text-caption">Active Projects</div>
              </v-card-text>
            </v-card>
          </v-col>
          <v-col cols="6" sm="3">
            <v-card variant="tonal" color="orange">
              <v-card-text class="pa-3">
                <div class="text-h5 font-weight-bold">{{ systemStats.totalTasks }}</div>
                <div class="text-caption">Total Tasks</div>
              </v-card-text>
            </v-card>
          </v-col>
          <v-col cols="6" sm="3">
            <v-card variant="tonal" color="purple">
              <v-card-text class="pa-3">
                <div class="text-h5 font-weight-bold">{{ systemStats.storageUsed }}</div>
                <div class="text-caption">Storage Used</div>
              </v-card-text>
            </v-card>
          </v-col>
        </v-row>
      </v-card-text>
    </v-card>

    <!-- Row 3: Settings cards — scrollable -->
    <div class="flex-grow-1 overflow-y-auto">
      <v-row dense>

        <!-- Application Settings -->
        <v-col cols="12" md="6">
          <v-card elevation="1">
            <v-card-title class="bg-primary text-white text-subtitle-1">
              <v-icon class="mr-2">mdi-application-cog</v-icon>
              Application Settings
            </v-card-title>
            <v-card-text class="pa-3">
              <v-list density="compact">
                <v-list-item>
                  <template #prepend>
                    <v-icon>mdi-web</v-icon>
                  </template>
                  <v-list-item-title>Application Name</v-list-item-title>
                  <v-list-item-subtitle>NPI Project Management</v-list-item-subtitle>
                </v-list-item>
                <v-list-item>
                  <template #prepend>
                    <v-icon>mdi-tag</v-icon>
                  </template>
                  <v-list-item-title>Version</v-list-item-title>
                  <v-list-item-subtitle>1.0.0</v-list-item-subtitle>
                </v-list-item>
                <v-list-item>
                  <template #prepend>
                    <v-icon>mdi-clock</v-icon>
                  </template>
                  <v-list-item-title>Timezone</v-list-item-title>
                  <v-list-item-subtitle>Asia/Kuala_Lumpur (GMT+8)</v-list-item-subtitle>
                </v-list-item>
              </v-list>
            </v-card-text>
          </v-card>
        </v-col>

        <!-- File Storage Settings -->
        <v-col cols="12" md="6">
          <v-card elevation="1">
            <v-card-title class="bg-primary text-white text-subtitle-1">
              <v-icon class="mr-2">mdi-folder-cog</v-icon>
              File Storage
            </v-card-title>
            <v-card-text class="pa-3">
              <v-list density="compact">
                <v-list-item>
                  <template #prepend>
                    <v-icon>mdi-folder</v-icon>
                  </template>
                  <v-list-item-title>Storage Path</v-list-item-title>
                  <v-list-item-subtitle>D:\NPI_Projects</v-list-item-subtitle>
                </v-list-item>
                <v-list-item>
                  <template #prepend>
                    <v-icon>mdi-file-document-multiple</v-icon>
                  </template>
                  <v-list-item-title>Max File Size</v-list-item-title>
                  <v-list-item-subtitle>10 MB</v-list-item-subtitle>
                </v-list-item>
                <v-list-item>
                  <template #prepend>
                    <v-icon>mdi-file-check</v-icon>
                  </template>
                  <v-list-item-title>Allowed Types</v-list-item-title>
                  <v-list-item-subtitle>PDF, DOCX, XLSX, PNG, JPG</v-list-item-subtitle>
                </v-list-item>
              </v-list>
            </v-card-text>
          </v-card>
        </v-col>

        <!-- Security Settings -->
        <v-col cols="12" md="6">
          <v-card elevation="1">
            <v-card-title class="bg-primary text-white text-subtitle-1">
              <v-icon class="mr-2">mdi-shield-lock</v-icon>
              Security
            </v-card-title>
            <v-card-text class="pa-3">
              <v-list density="compact">
                <v-list-item>
                  <template #prepend>
                    <v-icon>mdi-lock</v-icon>
                  </template>
                  <v-list-item-title>Password Policy</v-list-item-title>
                  <v-list-item-subtitle>Minimum 6 characters</v-list-item-subtitle>
                </v-list-item>
                <v-list-item>
                  <template #prepend>
                    <v-icon>mdi-timer</v-icon>
                  </template>
                  <v-list-item-title>Session Timeout</v-list-item-title>
                  <v-list-item-subtitle>8 hours</v-list-item-subtitle>
                </v-list-item>
                <v-list-item>
                  <template #prepend>
                    <v-icon>mdi-security</v-icon>
                  </template>
                  <v-list-item-title>Authentication</v-list-item-title>
                  <v-list-item-subtitle>JWT Token-based</v-list-item-subtitle>
                </v-list-item>
              </v-list>
            </v-card-text>
          </v-card>
        </v-col>

        <!-- Notifications -->
        <v-col cols="12" md="6">
          <v-card elevation="1">
            <v-card-title class="bg-primary text-white text-subtitle-1">
              <v-icon class="mr-2">mdi-bell-cog</v-icon>
              Notifications
            </v-card-title>
            <v-card-text class="pa-3">
              <v-switch v-model="emailNotifications"
                        label="Email Notifications" color="primary" hide-details class="mb-2" />
              <v-switch v-model="taskReminders"
                        label="Task Due Date Reminders" color="primary" hide-details class="mb-2" />
              <v-switch v-model="projectUpdates"
                        label="Project Status Updates" color="primary" hide-details />
            </v-card-text>
          </v-card>
        </v-col>

        <!-- System Actions -->
        <v-col cols="12">
          <v-card elevation="1">
            <v-card-title class="bg-primary text-white text-subtitle-1">
              <v-icon class="mr-2">mdi-cog-sync</v-icon>
              System Actions
            </v-card-title>
            <v-card-text class="pa-3">
              <v-row dense>
                <v-col cols="12" md="4">
                  <v-btn color="info" variant="tonal" block
                         prepend-icon="mdi-database-export" @click="exportData">
                    Export Data
                  </v-btn>
                </v-col>
                <v-col cols="12" md="4">
                  <v-btn color="warning" variant="tonal" block
                         prepend-icon="mdi-database-import" @click="importData">
                    Import Data
                  </v-btn>
                </v-col>
                <v-col cols="12" md="4">
                  <v-btn color="success" variant="tonal" block
                         prepend-icon="mdi-backup-restore" @click="createBackup">
                    Create Backup
                  </v-btn>
                </v-col>
              </v-row>
            </v-card-text>
          </v-card>
        </v-col>

      </v-row>
    </div>

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

  const emailNotifications = ref(true)
  const taskReminders = ref(true)
  const projectUpdates = ref(true)

  const snackbar = ref(false)
  const snackbarMessage = ref('')
  const snackbarColor = ref('success')

  const systemStats = computed(() => ({
    totalUsers: settingsStore.users.length,
    activeProjects: 0,   // TODO: connect to project store
    totalTasks: 0,       // TODO: connect to task store
    storageUsed: '0 MB'  // TODO: calculate actual storage
  }))

  function exportData() {
    snackbarMessage.value = 'Export functionality coming soon'
    snackbarColor.value = 'info'; snackbar.value = true
  }
  function importData() {
    snackbarMessage.value = 'Import functionality coming soon'
    snackbarColor.value = 'info'; snackbar.value = true
  }
  function createBackup() {
    snackbarMessage.value = 'Backup functionality coming soon'
    snackbarColor.value = 'info'; snackbar.value = true
  }

  onMounted(async () => {
    try {
      await settingsStore.fetchUsers()
    } catch {
      snackbarMessage.value = 'Error loading system data'
      snackbarColor.value = 'error'; snackbar.value = true
    }
  })
</script>

<style scoped>
  .module-root {
    height: 100%;
    overflow: hidden;
  }
</style>
