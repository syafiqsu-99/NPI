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

      <!-- Right panel: Team members for selected project (from ProjectTeams) -->
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
            Assign Member
          </v-btn>
        </v-card-title>
        <v-divider />

        <v-progress-linear v-if="loadingRoles" indeterminate color="primary" />

        <!-- Table fills remaining panel height; body scrolls on y-axis -->
        <div class="flex-grow-1 d-flex flex-column" style="min-height: 0; overflow: hidden;">
          <v-data-table-virtual v-if="selectedProjId"
                                :headers="roleHeaders"
                                :items="projectRoles"
                                density="comfortable"
                                :loading="loadingRoles"
                                fixed-header
                                height="100%"
                                class="role-assign-table flex-grow-1">

            <!-- User column -->
            <template #item.user_name="{ item }">
              <div class="d-flex align-center ga-2 py-1">
                <v-avatar color="primary" size="28">
                  <span class="text-white" style="font-size:10px; font-weight:600">
                    {{ getInitials(item.user_name) }}
                  </span>
                </v-avatar>
                <span class="text-body-2">{{ item.user_name }}</span>
              </div>
            </template>

            <!-- Department column -->
            <template #item.dept_name="{ item }">
              <v-chip v-if="item.dept_name" size="small" variant="tonal" color="primary">
                {{ item.dept_name }}
              </v-chip>
              <span v-else class="text-caption text-grey">—</span>
            </template>

            <!-- Project role column -->
            <template #item.role="{ item }">
              <v-chip :color="roleChipColor(item.role)" size="small" variant="tonal">
                <v-icon start size="x-small">{{ roleIcon(item.role) }}</v-icon>
                {{ item.role ?? 'Member' }}
              </v-chip>
            </template>

            <!-- Actions -->
            <template #item.actions="{ item }">
              <v-menu location="bottom end">
                <template #activator="{ props }">
                  <v-btn icon="mdi-dots-vertical" size="small" variant="text" v-bind="props" />
                </template>
                <v-list density="compact" min-width="160">
                  <v-list-item v-for="role in PROJECT_ROLES" :key="role"
                               :disabled="item.role === role"
                               @click="changeRole(item, role)">
                    <template #prepend>
                      <v-icon size="small" :color="roleChipColor(role)">{{ roleIcon(role) }}</v-icon>
                    </template>
                    <v-list-item-title>Set as {{ role }}</v-list-item-title>
                  </v-list-item>
                  <v-divider />
                  <v-list-item @click="confirmRemove(item)">
                    <template #prepend>
                      <v-icon size="small" color="error">mdi-account-remove</v-icon>
                    </template>
                    <v-list-item-title class="text-error">Remove</v-list-item-title>
                  </v-list-item>
                </v-list>
              </v-menu>
            </template>

            <!-- Empty state -->
            <template #no-data>
              <div class="d-flex flex-column align-center justify-center pa-8 text-grey">
                <v-icon size="48" color="grey-lighten-1">mdi-account-group-outline</v-icon>
                <div class="mt-2 text-body-2">No team members assigned yet</div>
              </div>
            </template>

          </v-data-table-virtual>

          <!-- No project selected state -->
          <div v-else class="d-flex flex-column align-center justify-center fill-height text-grey pa-8">
            <v-icon size="48" color="grey-lighten-1">mdi-folder-open-outline</v-icon>
            <div class="mt-2 text-body-2">Select a project on the left to manage its team</div>
          </div>
        </div>
      </v-card>

    </div>

    <!-- Assign Member dialog -->
    <v-dialog v-model="assignDialog" max-width="480" persistent>
      <v-card>
        <v-card-title class="bg-primary text-white text-subtitle-1">
          Assign Team Member
        </v-card-title>
        <v-card-text class="pt-4">
          <v-select v-model="assignForm.dept_id"
                    :items="departments"
                    item-title="dept_name"
                    item-value="dept_id"
                    label="Department *"
                    variant="outlined"
                    density="comfortable"
                    class="mb-3"
                    @update:model-value="assignForm.user_id = null" />
          <v-autocomplete v-model="assignForm.user_id"
                          :items="usersForAssignDept"
                          item-title="username"
                          item-value="user_id"
                          label="User *"
                          :disabled="!assignForm.dept_id"
                          variant="outlined"
                          density="comfortable"
                          class="mb-3" />
          <v-select v-model="assignForm.role_name"
                    :items="PROJECT_ROLES"
                    label="Project Role *"
                    variant="outlined"
                    density="comfortable" />
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

    <!-- Remove confirmation dialog -->
    <v-dialog v-model="removeDialog" max-width="440">
      <v-card>
        <v-card-title class="bg-error text-white text-subtitle-1">
          <v-icon class="mr-2">mdi-alert</v-icon>
          Remove Team Member
        </v-card-title>
        <v-card-text class="pt-4">
          Remove <strong>{{ removeTarget?.user_name }}</strong> from
          <strong>{{ selectedProject?.proj_name }}</strong>?
        </v-card-text>
        <v-card-actions class="pa-4">
          <v-spacer />
          <v-btn variant="text" @click="removeDialog = false">Cancel</v-btn>
          <v-btn color="error" variant="elevated" :loading="saving" @click="doRemove">
            Remove
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

  // ── State ─────────────────────────────────────────────────────────────────────
  const projects = ref([])
  const projectSearch = ref('')
  const selectedProjId = ref(null)
  const selectedProject = ref(null)

  // projectRoles now holds data from ProjectTeams (user_name, dept_name, role)
  const projectRoles = ref([])
  const loadingRoles = ref(false)

  const departments = ref([])
  const allUsers = ref([])

  const assignDialog = ref(false)
  const removeDialog = ref(false)
  const removeTarget = ref(null)
  const saving = ref(false)

  const assignForm = ref({ dept_id: null, user_id: null, role_name: 'Member' })

  const snackbar = ref(false)
  const snackbarMsg = ref('')
  const snackbarColor = ref('success')

  // ── Table headers — matches ProjectTeamDto fields returned by the API ─────────
  const roleHeaders = [
    { title: 'User', value: 'user_name', width: '30%' },
    { title: 'Department', value: 'dept_name', width: '28%' },
    { title: 'Project Role', value: 'role', width: '28%' },
    { title: '', value: 'actions', width: '14%', sortable: false }
  ]

  // ── Computed ──────────────────────────────────────────────────────────────────
  const filteredProjects = computed(() => {
    const q = projectSearch.value?.toLowerCase() ?? ''
    return projects.value.filter(p =>
      !q || p.proj_name.toLowerCase().includes(q) || p.proj_no.toLowerCase().includes(q)
    )
  })

  // Users filtered by selected department in the assign dialog
  const usersForAssignDept = computed(() => {
    if (!assignForm.value.dept_id) return []
    // Exclude users already assigned to this project
    const assignedIds = new Set(projectRoles.value.map(r => r.user_id))
    return allUsers.value.filter(
      u => u.dept_id === assignForm.value.dept_id && u.is_active && !assignedIds.has(u.user_id)
    )
  })

  // ── Helpers ───────────────────────────────────────────────────────────────────
  function roleChipColor(role) {
    return { 'Team Lead': 'primary', 'Member': 'info', 'Viewer': 'grey' }[role] ?? 'grey'
  }

  function roleIcon(role) {
    return {
      'Team Lead': 'mdi-account-star',
      'Member': 'mdi-account',
      'Viewer': 'mdi-eye'
    }[role] ?? 'mdi-account'
  }

  function getStatusColor(s) {
    return {
      'In Progress': 'primary', 'Completed': 'success',
      'On Hold': 'warning', 'Planning': 'grey', 'Cancelled': 'error'
    }[s] ?? 'grey'
  }

  function getInitials(name) {
    if (!name) return '?'
    return name.split(' ').map(n => n[0]).join('').toUpperCase().substring(0, 2)
  }

  function showSnack(msg, color = 'success') {
    snackbarMsg.value = msg
    snackbarColor.value = color
    snackbar.value = true
  }

  // ── Data loading ──────────────────────────────────────────────────────────────
  async function selectProject(proj) {
    selectedProjId.value = proj.proj_id
    selectedProject.value = proj
    await loadRoles(proj.proj_id)
  }

  async function loadRoles(projId) {
    loadingRoles.value = true
    try {
      const result = await api.get(`/projectteam/by-project/${projId}`)
      const teamData = result?.data ?? []

      // Enrich each team entry with dept_name from the allUsers cache
      projectRoles.value = teamData.map(member => {
        const userDetail = allUsers.value.find(u => u.user_id === member.user_id)
        return {
          ...member,
          // user_name already populated from ProjectTeamDto
          user_name: member.user_name ?? userDetail?.username ?? `User #${member.user_id}`,
          // dept_name enriched from user cache
          dept_name: userDetail?.dept_name ?? ''
        }
      })
    } catch {
      showSnack('Failed to load project team members', 'error')
      projectRoles.value = []
    } finally {
      loadingRoles.value = false
    }
  }

  // ── Assign ────────────────────────────────────────────────────────────────────
  function openAssignDialog() {
    assignForm.value = { dept_id: null, user_id: null, role_name: 'Member' }
    assignDialog.value = true
  }

  async function doAssign() {
    saving.value = true
    try {
      const result = await api.post(`/projectrole/${selectedProjId.value}/roles`, {
        user_id: assignForm.value.user_id,
        role_name: assignForm.value.role_name
      })
      if (result?.success) {
        await loadRoles(selectedProjId.value)
        assignDialog.value = false
        showSnack('Team member assigned successfully')
      } else {
        showSnack(result?.message || 'Failed to assign member', 'error')
      }
    } catch {
      showSnack('Error assigning team member', 'error')
    } finally {
      saving.value = false
    }
  }

  // ── Change role ───────────────────────────────────────────────────────────────
  async function changeRole(item, newRole) {
    try {
      const result = await api.post(`/projectrole/${selectedProjId.value}/roles`, {
        user_id: item.user_id,
        role_name: newRole
      })
      if (result?.success) {
        item.role = newRole
        showSnack(`Role updated to ${newRole}`)
      } else {
        showSnack(result?.message || 'Failed to update role', 'error')
      }
    } catch {
      showSnack('Error updating role', 'error')
    }
  }

  // ── Remove member ─────────────────────────────────────────────────────────────
  function confirmRemove(item) {
    removeTarget.value = item
    removeDialog.value = true
  }

  async function doRemove() {
    if (!removeTarget.value) return
    saving.value = true
    try {
      projectRoles.value = projectRoles.value.filter(
        r => r.user_id !== removeTarget.value.user_id
      )
      removeDialog.value = false
      showSnack(`${removeTarget.value.user_name} removed from view. Save via Project Setup to persist.`, 'info')
      removeTarget.value = null
    } catch {
      showSnack('Error removing member', 'error')
    } finally {
      saving.value = false
    }
  }

  // ── Mount ─────────────────────────────────────────────────────────────────────
  onMounted(async () => {
    const [projRes, userRes, deptRes] = await Promise.all([
      api.get('/project').catch(() => null),
      api.get('/user').catch(() => null),
      api.get('/department').catch(() => null)
    ])
    projects.value = projRes?.data ?? []
    allUsers.value = userRes?.data ?? []
    departments.value = deptRes?.data ?? []
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
    position: sticky;
    top: 0;
    z-index: 2;
  }

  .role-assign-table :deep(.v-table__wrapper) {
    height: 100%;
    overflow-y: auto;
  }
</style>
