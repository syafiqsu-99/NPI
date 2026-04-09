<template>
  <div class="module-root d-flex flex-column pa-3 ga-3">

    <!-- Row 1: Page title -->
    <div class="flex-shrink-0">
      <h2 class="text-h6 font-weight-bold">Project Role Management</h2>
    </div>

    <!-- Row 2 + 3 combined: Two-panel layout fills remaining height -->
    <div class="panels-row flex-grow-1 d-flex ga-3" style="min-height: 0;">

      <!-- Left panel: Project selector -->
      <v-card variant="outlined" class="panel-left d-flex flex-column" style="min-height: 0;">
        <v-card-title class="text-subtitle-1 font-weight-bold pa-3 flex-shrink-0">
          <v-icon start size="18">mdi-folder-multiple</v-icon>
          Select Project
        </v-card-title>
        <v-divider />
        <div class="pa-2 flex-shrink-0">
          <v-text-field v-model="projectSearch"
                        prepend-inner-icon="mdi-magnify"
                        label="Search projects…"
                        variant="outlined"
                        density="compact"
                        hide-details
                        clearable />
        </div>
        <!-- Project list scrolls internally -->
        <div class="flex-grow-1 overflow-y-auto">
          <v-list density="compact">
            <v-list-item v-for="proj in filteredProjects"
                         :key="proj.proj_id"
                         :active="selectedProjId === proj.proj_id"
                         color="primary"
                         @click="selectProject(proj)">
              <v-list-item-title>{{ proj.proj_name }}</v-list-item-title>
              <v-list-item-subtitle>{{ proj.proj_no }}</v-list-item-subtitle>
              <template #append>
                <v-chip size="x-small" :color="getStatusColor(proj.status)" variant="tonal">
                  {{ proj.status }}
                </v-chip>
              </template>
            </v-list-item>
            <v-list-item v-if="filteredProjects.length === 0">
              <v-list-item-title class="text-caption text-grey">No projects found</v-list-item-title>
            </v-list-item>
          </v-list>
        </div>
      </v-card>

      <!-- Right panel: Role assignments for selected project -->
      <v-card variant="outlined" class="panel-right d-flex flex-column" style="min-height: 0;">
        <v-card-title class="d-flex align-center justify-space-between pa-3 flex-shrink-0">
          <span class="text-subtitle-1 font-weight-bold">
            <v-icon start size="18">mdi-account-multiple</v-icon>
            {{ selectedProject?.proj_name ?? 'Select a project' }}
          </span>
          <v-btn v-if="selectedProjId"
                 color="primary" size="small" variant="flat"
                 prepend-icon="mdi-plus"
                 @click="openAssignDialog">
            Assign Role
          </v-btn>
        </v-card-title>
        <v-divider />

        <v-progress-linear v-if="loadingRoles" indeterminate color="primary" />

        <!-- Table fills remaining panel height -->
        <div class="flex-grow-1" style="min-height: 0; overflow: hidden;">
          <v-data-table-virtual v-if="selectedProjId"
                                :headers="roleHeaders"
                                :items="projectRoles"
                                density="comfortable"
                                :loading="loadingRoles"
                                fixed-header
                                height="300"
                                class="role-assign-table">

            <template #item.role_name="{ item }">
              <v-chip :color="roleChipColor(item.role_name)" size="small" variant="tonal">
                <v-icon start size="x-small">{{ roleIcon(item.role_name) }}</v-icon>
                {{ item.role_name }}
              </v-chip>
            </template>

            <template #item.actions="{ item }">
              <v-menu location="bottom end">
                <template #activator="{ props }">
                  <v-btn icon="mdi-dots-vertical" size="small" variant="text" v-bind="props" />
                </template>
                <v-list density="compact" min-width="160">
                  <v-list-item v-for="role in PROJECT_ROLES" :key="role"
                               :disabled="item.role_name === role"
                               @click="changeRole(item, role)">
                    <template #prepend>
                      <v-icon size="small" :color="roleChipColor(role)">{{ roleIcon(role) }}</v-icon>
                    </template>
                    <v-list-item-title>Set as {{ role }}</v-list-item-title>
                  </v-list-item>
                </v-list>
              </v-menu>
            </template>

          </v-data-table-virtual>

          <div v-else class="d-flex flex-column align-center justify-center fill-height text-grey pa-8">
            <v-icon size="48" color="grey-lighten-1">mdi-folder-open-outline</v-icon>
            <div class="mt-2 text-body-2">Select a project on the left to manage its roles</div>
          </div>
        </div>
      </v-card>

    </div>

    <!-- Assign Role dialog -->
    <v-dialog v-model="assignDialog" max-width="480" persistent>
      <v-card>
        <v-card-title class="bg-primary text-white text-subtitle-1">
          Assign Project Role
        </v-card-title>
        <v-card-text class="pt-4">
          <v-autocomplete v-model="assignForm.user_id"
                          :items="availableUsers"
                          item-title="username"
                          item-value="user_id"
                          label="User *"
                          variant="outlined"
                          density="comfortable" />
          <v-select v-model="assignForm.role_name"
                    :items="PROJECT_ROLES"
                    label="Project Role *"
                    variant="outlined"
                    density="comfortable"
                    class="mt-3" />
        </v-card-text>
        <v-card-actions class="pa-4">
          <v-spacer />
          <v-btn variant="text" @click="assignDialog = false">Cancel</v-btn>
          <v-btn color="primary" variant="elevated"
                 :loading="saving"
                 :disabled="!assignForm.user_id || !assignForm.role_name"
                 @click="doAssign">
            Assign
          </v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>

    <v-snackbar v-model="snackbar" :color="snackbarColor" timeout="3000">
      {{ snackbarMsg }}
      <template #actions>
        <v-btn variant="text" @click="snackbar = false">Close</v-btn>
      </template>
    </v-snackbar>

  </div>
</template>

<script setup>
  import { ref, computed, onMounted } from 'vue'
  import { api } from '@/utils/api'

  const PROJECT_ROLES = ['Team Lead', 'Member', 'Viewer']

  const projects = ref([])
  const projectSearch = ref('')
  const selectedProjId = ref(null)
  const selectedProject = ref(null)
  const projectRoles = ref([])
  const loadingRoles = ref(false)

  const assignDialog = ref(false)
  const saving = ref(false)
  const assignForm = ref({ user_id: null, role_name: 'Member' })
  const allUsers = ref([])

  const snackbar = ref(false)
  const snackbarMsg = ref('')
  const snackbarColor = ref('success')

  const roleHeaders = [
    { title: 'User', value: 'username', width: '45%' },
    { title: 'Role', value: 'role_name', width: '40%' },
    { title: '', value: 'actions', width: '15%', sortable: false }
  ]

  const filteredProjects = computed(() => {
    const q = projectSearch.value?.toLowerCase() ?? ''
    return projects.value.filter(p =>
      !q || p.proj_name.toLowerCase().includes(q) || p.proj_no.toLowerCase().includes(q)
    )
  })

  const availableUsers = computed(() => {
    const assigned = new Set(projectRoles.value.map(r => r.user_id))
    return allUsers.value.filter(u => !assigned.has(u.user_id) && u.is_active)
  })

  function roleChipColor(role) {
    return { 'Team Lead': 'primary', 'Member': 'info', 'Viewer': 'grey' }[role] ?? 'grey'
  }
  function roleIcon(role) {
    return { 'Team Lead': 'mdi-account-star', 'Member': 'mdi-account', 'Viewer': 'mdi-eye' }[role] ?? 'mdi-account'
  }
  function getStatusColor(s) {
    return { 'In Progress': 'primary', 'Completed': 'success', 'On Hold': 'warning', 'Planning': 'grey' }[s] ?? 'grey'
  }

  async function selectProject(proj) {
    selectedProjId.value = proj.proj_id
    selectedProject.value = proj
    await loadRoles(proj.proj_id)
  }

  async function loadRoles(projId) {
    loadingRoles.value = true
    try {
      const result = await api.get(`/projectrole/${projId}/roles`)
      projectRoles.value = result?.data ?? []
    } catch {
      showSnack('Failed to load project roles', 'error')
    } finally {
      loadingRoles.value = false
    }
  }

  function openAssignDialog() {
    assignForm.value = { user_id: null, role_name: 'Member' }
    assignDialog.value = true
  }

  async function doAssign() {
    saving.value = true
    try {
      const result = await api.post(`/projectrole/${selectedProjId.value}/roles`, assignForm.value)
      if (result?.success) {
        await loadRoles(selectedProjId.value)
        assignDialog.value = false
        showSnack('Role assigned', 'success')
      } else {
        showSnack(result?.message || 'Failed', 'error')
      }
    } catch {
      showSnack('Error assigning role', 'error')
    } finally {
      saving.value = false
    }
  }

  async function changeRole(item, newRole) {
    try {
      const result = await api.post(
        `/projectrole/${selectedProjId.value}/roles`,
        { user_id: item.user_id, role_name: newRole }
      )
      if (result?.success) {
        item.role_name = newRole
        showSnack(`Role updated to ${newRole}`, 'success')
      }
    } catch {
      showSnack('Failed to update role', 'error')
    }
  }

  function showSnack(msg, color = 'success') {
    snackbarMsg.value = msg; snackbarColor.value = color; snackbar.value = true
  }

  onMounted(async () => {
    const [projRes, userRes] = await Promise.all([
      api.get('/project').catch(() => null),
      api.get('/user').catch(() => null)
    ])
    projects.value = projRes?.data ?? []
    allUsers.value = userRes?.data ?? []
  })
</script>

<style scoped>
  .module-root {
    height: 100%;
    overflow: hidden;
  }

  .panels-row {
    flex: 1 1 0;
    min-height: 0;
  }

  .panel-left {
    width: 320px;
    flex-shrink: 0;
  }

  .panel-right {
    flex: 1 1 0;
    min-width: 0;
  }

  .role-assign-table :deep(th) {
    font-weight: 600 !important;
    font-size: 11px;
    text-transform: uppercase;
    letter-spacing: 0.4px;
    background: #fafbfc !important;
  }

  .role-assign-table :deep(.v-table__wrapper) {
    height: 100%;
    overflow-y: auto;
  }
</style>
