<template>
  <v-container fluid class="page-container pa-6 d-flex flex-column">
    <v-row class="mb-4">
      <v-col cols="12">
        <v-card class="page-header-card" elevation="2">
          <v-card-title class="bg-primary text-white d-flex align-center justify-space-between">
            <div class="page-title">
          <v-icon class="mr-2">mdi-cog</v-icon>
              Settings
            </div>
          </v-card-title>
          <v-card-text class="pa-0">
            <v-tabs v-model="activeTab" bg-color="primary">
              <v-tab value="users">
                <v-icon start>mdi-account-multiple</v-icon>
                User Management
              </v-tab>
              <v-tab value="roles">
                <v-icon start>mdi-shield-account</v-icon>
                Role Management
              </v-tab>
              <v-tab value="departments">
                <v-icon start>mdi-domain</v-icon>
                Departments
              </v-tab>
              <v-tab value="system">
                <v-icon start>mdi-cog-outline</v-icon>
                System Settings
              </v-tab>
              <v-tab value="npiConfig">
                <v-icon start>mdi-form-select</v-icon>
                NPI Form Config
              </v-tab>
            </v-tabs>

            <v-window v-model="activeTab">
              <!-- User Management Tab -->
              <v-window-item value="users">
                <UserManagement />
              </v-window-item>

              <!-- Role Management Tab -->
              <v-window-item value="roles">
                <RoleManagement />
              </v-window-item>

              <!-- Department Tab -->
              <v-window-item value="departments">
                <DepartmentManagement />
              </v-window-item>

              <!-- System Settings Tab -->
              <v-window-item value="system">
                <SystemSettings />
              </v-window-item>

              <!-- NPI Form Configuration -->
              <v-window-item value="npiConfig">
                <NpiFormConfig />
              </v-window-item>
            </v-window>
          </v-card-text>
        </v-card>
      </v-col>
    </v-row>

    <v-row no-gutters class="mb-4">
      <v-col cols="12">
      </v-col>
    </v-row>
  </v-container>
</template>

<script setup>
  import { ref } from 'vue'
  import UserManagement from '@/components/settings/UserManagement.vue'
  import RoleManagement from '@/components/settings/RoleManagement.vue'
  import DepartmentManagement from '@/components/settings/DepartmentManagement.vue'
  import SystemSettings from '@/components/settings/SystemSettings.vue'
  import NpiFormConfig from '@/components/settings/NpiFormConfig.vue'

  const activeTab = ref('users')
</script>

<style scoped>
  .v-tabs :deep(.v-tab) {
    text-transform: none;
  }
</style>
