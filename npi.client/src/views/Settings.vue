<template>
  <v-container fluid fill-height class="d-flex flex-column">
    <v-row no-gutters align="center" justify="center">
      <v-col cols="12" class="text-center">
        <h2 class="font-weight-bold">
          <v-icon class="mr-2">mdi-cog</v-icon>
          System Settings
        </h2>
      </v-col>
    </v-row>

    <v-row no-gutters class="mb-4">
      <v-col cols="12">
        <v-card elevation="2">
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
            </v-window>
          </v-card-text>
        </v-card>
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

  const activeTab = ref('users')
</script>

<style scoped>
  .v-tabs :deep(.v-tab) {
    text-transform: none;
  }
</style>
