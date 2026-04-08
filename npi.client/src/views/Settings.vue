<template>
  <v-container fluid class="settings-root pa-0 d-flex flex-column">
    <v-card class="flex-grow-1 d-flex flex-column overflow-hidden" elevation="2">
      <v-card-title class="bg-primary text-white d-flex align-center pa-4 flex-shrink-0">
        <v-icon class="mr-2">mdi-cog</v-icon>
        Settings
      </v-card-title>

      <v-tabs v-model="activeTab" bg-color="primary" class="flex-shrink-0">
        <v-tab value="users">
          <v-icon start>mdi-account-multiple</v-icon>
          User Management
        </v-tab>
        <v-tab v-if="canManageRoles" value="roles">
          <v-icon start>mdi-shield-account</v-icon>
          Role Management
        </v-tab>
        <v-tab v-if="canManageDepts" value="departments">
          <v-icon start>mdi-domain</v-icon>
          Departments
        </v-tab>
        <v-tab v-if="isAdmin" value="system">
          <v-icon start>mdi-cog-outline</v-icon>
          System Settings
        </v-tab>
        <v-tab v-if="canManageFormConfig" value="npiConfig">
          <v-icon start>mdi-form-select</v-icon>
          NPI Form Config
        </v-tab>
      </v-tabs>

      <v-window v-model="activeTab" class="flex-grow-1 overflow-hidden d-flex flex-column">
        <v-window-item value="users" class="flex-grow-1 overflow-hidden">
          <UserManagement :is-admin="isAdmin" />
        </v-window-item>
        <v-window-item v-if="canManageRoles" value="roles" class="flex-grow-1 overflow-hidden">
          <RoleManagement />
        </v-window-item>
        <v-window-item v-if="canManageDepts" value="departments" class="flex-grow-1 overflow-hidden">
          <DepartmentManagement />
        </v-window-item>
        <v-window-item v-if="isAdmin" value="system" class="flex-grow-1 overflow-hidden">
          <SystemSettings />
        </v-window-item>
        <v-window-item v-if="canManageFormConfig" value="npiConfig" class="flex-grow-1 overflow-hidden">
          <NpiFormConfig />
        </v-window-item>
      </v-window>
    </v-card>
  </v-container>
</template>

<script setup>
  import { ref, computed } from 'vue'
  import { useAuthStore } from '@/stores/auth'
  import UserManagement from '@/components/settings/UserManagement.vue'
  import RoleManagement from '@/components/settings/RoleManagement.vue'
  import DepartmentManagement from '@/components/settings/DepartmentManagement.vue'
  import SystemSettings from '@/components/settings/SystemSettings.vue'
  import NpiFormConfig from '@/components/settings/NpiFormConfig.vue'

  const authStore = useAuthStore()
  const activeTab = ref('users')

  const userRole = computed(() => authStore.user?.role)
  const isAdmin = computed(() => userRole.value === 'Admin')
  const isManager = computed(() => userRole.value === 'Manager')

  const canManageRoles = computed(() => isAdmin.value || isManager.value)
  const canManageDepts = computed(() => isAdmin.value || isManager.value)
  const canManageFormConfig = computed(() => isAdmin.value || isManager.value)
</script>

<style scoped>
  .settings-root {
    height: 100vh !important;
    overflow: hidden !important;
    background-color: #f5f6f8;
    padding: 16px;
  }

  .v-window,
  :deep(.v-window__container),
  :deep(.v-window-item) {
    height: 100% !important;
    min-height: 0 !important;
  }
</style>
