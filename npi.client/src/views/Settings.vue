<template>
  <div class="settings-root d-flex flex-column">

    <!-- Settings header + tabs -->
    <v-card elevation="2" class="flex-shrink-0">
      <v-card-title class="bg-primary text-white d-flex align-center pa-3">
        <v-icon class="mr-2">mdi-cog</v-icon>
        Settings
      </v-card-title>

      <v-tabs v-model="activeTab" bg-color="grey-lighten-4" color="primary" density="compact">
        <v-tab value="users">
          <v-icon start size="18">mdi-account-multiple</v-icon>
          Users
        </v-tab>

        <v-tab v-if="isAdmin" value="systemRoles">
          <v-icon start size="18">mdi-shield-account</v-icon>
          System Roles
        </v-tab>

        <v-tab value="projectRoles">
          <v-icon start size="18">mdi-folder-account</v-icon>
          Project Roles
        </v-tab>

        <v-tab value="departments">
          <v-icon start size="18">mdi-domain</v-icon>
          Departments
        </v-tab>

        <v-tab value="npiConfig">
          <v-icon start size="18">mdi-form-select</v-icon>
          Enquiry Form
        </v-tab>

        <v-tab value="taskTemplate">
          <v-icon start size="18">mdi-cog-outline</v-icon>
          System
        </v-tab>
      </v-tabs>
    </v-card>

    <v-window v-model="activeTab" class="settings-content flex-grow-1">

      <v-window-item value="users" class="fill-height">
        <UserManagement :is-admin="isAdmin" />
      </v-window-item>

      <v-window-item v-if="isAdmin" value="systemRoles" class="fill-height">
        <RoleManagement />
      </v-window-item>

      <v-window-item value="projectRoles" class="fill-height">
        <ProjectRoleManagement />
      </v-window-item>

      <v-window-item value="departments" class="fill-height">
        <DepartmentManagement />
      </v-window-item>

      <v-window-item value="npiConfig" class="fill-height">
        <NpiFormConfig />
      </v-window-item>

      <v-window-item value="taskTemplate" class="fill-height">
        <TaskTemplate />
      </v-window-item>

    </v-window>
  </div>
</template>

<script setup>
  import { ref, computed } from 'vue'
  import { useAuthStore } from '@/stores/auth'
  import UserManagement from '@/components/settings/UserManagement.vue'
  import RoleManagement from '@/components/settings/RoleManagement.vue'
  import ProjectRoleManagement from '@/components/settings/ProjectRoleManagement.vue'
  import DepartmentManagement from '@/components/settings/DepartmentManagement.vue'
  import SystemSettings from '@/components/settings/SystemSettings.vue'
  import NpiFormConfig from '@/components/settings/NpiFormConfig.vue'
  import TaskTemplate from '@/components/settings/TaskTemplateManagement.vue'

  const authStore = useAuthStore()
  const activeTab = ref('users')

  const isAdmin = computed(() => authStore.isAdmin)
  const isManager = computed(() => authStore.isManager)
</script>

<style scoped>
  .settings-root {
    height: 100vh;
    overflow: hidden;
    background-color: #f5f6f8;
  }

  .settings-content {
    min-height: 0;
  }

  :deep(.v-window__container),
  :deep(.v-window-item) {
    height: 100% !important;
  }

  .fill-height {
    height: 100%;
    overflow: hidden;
  }
</style>
