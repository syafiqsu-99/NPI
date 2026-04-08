<template>
  <v-container fluid class="settings-root pa-0 d-flex flex-column">
    <v-card class="flex-grow-1 d-flex flex-column overflow-hidden" elevation="2">
      <v-card-title class="bg-primary text-white d-flex align-center pa-4 flex-shrink-0">
        <v-icon class="mr-2">mdi-cog</v-icon>
        Settings
      </v-card-title>

      <v-tabs v-model="activeTab" bg-color="primary" class="flex-shrink-0">
        <!-- User: Admin + Manager -->
        <v-tab v-if="canManageRoles" value="users">
          <v-icon start>mdi-account-multiple</v-icon>
          User Management
        </v-tab>

        <!-- System roles: Admin + Manager -->
        <v-tab v-if="canManageRoles" value="systemRoles">
          <v-icon start>mdi-shield-account</v-icon>
          System Roles
          <v-chip size="x-small" color="error" variant="outlined" class="ml-2">System</v-chip>
        </v-tab>

        <!-- Project roles: Admin + Manager -->
        <v-tab v-if="canManageRoles" value="projectRoles">
          <v-icon start>mdi-folder-account</v-icon>
          Project Roles
          <v-chip size="x-small" color="primary" variant="outlined" class="ml-2">Per-Project</v-chip>
        </v-tab>

        <!-- Departments: Admin + Manager -->
        <v-tab v-if="canManageDepts" value="departments">
          <v-icon start>mdi-domain</v-icon>
          Departments
        </v-tab>

        <!-- System settings: Admin only -->
        <v-tab v-if="isAdmin" value="system">
          <v-icon start>mdi-cog-outline</v-icon>
          System Settings
        </v-tab>

        <!-- NPI Form Config: Admin + Manager -->
        <v-tab v-if="canManageFormConfig" value="npiConfig">
          <v-icon start>mdi-form-select</v-icon>
          NPI Form Config
        </v-tab>
      </v-tabs>

      <v-window v-model="activeTab" class="flex-grow-1 overflow-hidden d-flex flex-column">

        <v-window-item value="users" class="flex-grow-1 overflow-hidden">
          <UserManagement :is-admin="isAdmin" />
        </v-window-item>

        <!-- System Roles: controls page/navigation access -->
        <v-window-item v-if="canManageRoles" value="systemRoles" class="flex-grow-1 overflow-hidden">
          <div class="pa-4">
            <v-alert type="info" variant="tonal" class="mb-4">
              <strong>System Roles</strong> control what pages and modules a user can access across
              the entire application.
              <ul class="mt-2 ml-4">
                <li><strong>Admin</strong> — Full access including Settings, delete, and all modules.</li>
                <li><strong>Manager</strong> — All modules. Settings visible but cannot create/delete Admins.</li>
                <li><strong>Member</strong> — No access to Settings. Read-only on sensitive data.</li>
              </ul>
            </v-alert>
          </div>
          <RoleManagement />
        </v-window-item>

        <!-- Project Roles: controls per-project permissions -->
        <v-window-item v-if="canManageRoles" value="projectRoles" class="flex-grow-1 overflow-hidden">
          <div class="pa-4">
            <v-alert type="info" variant="tonal" class="mb-4">
              <strong>Project Roles</strong> control what a user can do within a specific project,
              independent of their system-level role.
              <ul class="mt-2 ml-4">
                <li><strong>Team Lead</strong> — Can update project status, priority, task statuses, and upload files.</li>
                <li><strong>Member</strong> — Can update task statuses and upload files for tasks in their department.</li>
                <li><strong>Viewer</strong> — Read-only access to all project data and Gantt chart.</li>
              </ul>
            </v-alert>
            <ProjectRoleManagement />
          </div>
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
  import ProjectRoleManagement from '@/components/settings/ProjectRoleManagement.vue'
  import DepartmentManagement from '@/components/settings/DepartmentManagement.vue'
  import SystemSettings from '@/components/settings/SystemSettings.vue'
  import NpiFormConfig from '@/components/settings/NpiFormConfig.vue'

  const authStore = useAuthStore()
  const activeTab = ref('users')

  const isAdmin = computed(() => authStore.isAdmin)
  const isManager = computed(() => authStore.isManager)

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
