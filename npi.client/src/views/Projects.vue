<template>
  <v-container fluid class="page-container pa-6 d-flex flex-column">
    <v-row class="mb-4">
      <v-col cols="12">
        <v-card class="page-header-card" elevation="2">
          <v-card-title class="bg-primary text-white d-flex align-center justify-space-between">
            <div class="page-title">
              <v-icon class="mr-2">mdi-folder-multiple</v-icon>
              Projects
            </div>
          </v-card-title>
          <v-card-text class="pa-0">
            <v-tabs v-model="statusFilter" bg-color="primary">
              <v-tab value="all">
                <v-icon start>mdi-view-grid</v-icon>
                All Projects
              </v-tab>
              <v-tab value="Planning">
                <v-icon start>mdi-clipboard-text-outline</v-icon>
                Planning
              </v-tab>
              <v-tab value="In Progress">
                <v-icon start>mdi-progress-clock</v-icon>
                In Progress
              </v-tab>
              <v-tab value="On Hold">
                <v-icon start>mdi-pause-circle</v-icon>
                On Hold
              </v-tab>
              <v-tab value="Completed">
                <v-icon start>mdi-check-circle</v-icon>
                Completed
              </v-tab>
            </v-tabs>

            <v-window v-model="statusFilter">
              <v-window-item :value="statusFilter">
                <v-card elevation="2">
                  <v-card-title class="d-flex align-center justify-space-between">
                    <span>Projects List</span>
                    <v-text-field v-model="search"
                                  prepend-inner-icon="mdi-magnify"
                                  label="Search projects..."
                                  single-line hide-details density="compact"
                                  variant="outlined" class="ma-2"
                                  style="max-width: 300px;" />
                  </v-card-title>

                  <v-card-text>
                    <v-progress-linear v-if="projectStore.loading" indeterminate color="primary" />

                    <v-data-table-virtual v-else
                                          :items="filteredProjects"
                                          :headers="headers"
                                          :search="search"
                                          item-key="proj_id"
                                          class="elevation-1">

                      <template #item.proj_no="{ item }">
                        <v-chip color="primary" size="small" variant="outlined">
                          {{ item.proj_no }}
                        </v-chip>
                      </template>

                      <!-- Priority Dropdown -->
                      <template #item.priority="{ item }">
                        <v-menu :disabled="authStore.isAdmin || authStore.isManager">
                          <template #activator="{ props }">
                            <v-chip v-bind="props"
                                    :color="getPriorityColor(item.priority)"
                                    size="small" variant="tonal"
                                    :class="(authStore.isAdmin || authStore.isManager) ? 'cursor-pointer' : ''"
                                    :title="(authStore.isAdmin || authStore.isManager) ? 'Click to change priority' : ''">
                              {{ item.priority }}
                            </v-chip>
                          </template>
                          <v-list density="compact">
                            <v-list-item v-for="p in PRIORITY_OPTIONS" :key="p" @click="updatePriority(item, p)">
                              <v-list-item-title>{{ p }}</v-list-item-title>
                            </v-list-item>
                          </v-list>
                        </v-menu>
                      </template>

                      <!-- Status Dropdown -->
                      <template #item.status="{ item }">
                        <v-menu :disabled="!authStore.canManageProject(item)">
                          <template #activator="{ props }">
                            <v-chip v-bind="props"
                                    :color="getStatusColor(item.status)"
                                    size="small" variant="tonal"
                                    :class="authStore.canManageProject(item) ? 'cursor-pointer' : ''"
                                    :title="authStore.canManageProject(item) ? 'Click to change status' : ''">
                              {{ item.status }}
                            </v-chip>
                          </template>
                          <v-list density="compact">
                            <v-list-item v-for="s in PROJECT_STATUSES" :key="s" @click="updateStatus(item, s)">
                              <v-list-item-title>{{ s }}</v-list-item-title>
                            </v-list-item>
                          </v-list>
                        </v-menu>
                      </template>

                      <!-- NPI Stage Pipeline column -->
                      <template #item.stages="{ item }">
                        <div class="stage-pipeline d-flex align-center ga-1 py-1">
                          <template v-for="(stage, idx) in getProjectStages(item)" :key="stage.id">
                            <v-tooltip :text="`${stage.id} ${stage.name}: ${stage.status}`" location="top">
                              <template #activator="{ props }">
                                <div v-bind="props"
                                     class="stage-dot"
                                     :class="`stage-dot--${stage.status}`"
                                     :style="{ background: getStageDotColor(stage) }">
                                  <span class="stage-dot-label">{{ stage.id }}</span>
                                </div>
                              </template>
                            </v-tooltip>
                            <div v-if="idx < getProjectStages(item).length - 1" class="stage-arrow">›</div>
                          </template>
                        </div>
                      </template>

                      <template #item.project_start_date="{ item }">
                        {{ formatDate(item.project_start_date) }}
                      </template>

                      <template #item.target_completion_date="{ item }">
                        {{ formatDate(item.target_completion_date) }}
                      </template>

                      <template #item.actions="{ item }">
                        <!-- Setup/Manage -->
                        <v-btn v-if="authStore.canManageProject(item)"
                               icon size="small" variant="text" color="primary"
                               @click="manageProject(item.proj_id)"
                               title="Project Setup">
                          <v-icon size="18">mdi-cog</v-icon>
                        </v-btn>

                        <!-- Gantt Chart -->
                        <v-btn v-if="item.status === 'In Progress' || item.status === 'Completed'"
                               icon size="small" variant="text" color="success"
                               @click="viewGantt(item.proj_id)"
                               title="View Gantt Chart">
                          <v-icon size="18">mdi-chart-gantt</v-icon>
                        </v-btn>

                        <!-- View Details -->
                        <v-btn icon size="small" variant="text"
                               @click="viewProject(item)"
                               title="View Details">
                          <v-icon size="18">mdi-eye</v-icon>
                        </v-btn>

                        <!-- Delete (Admin only) -->
                        <v-btn v-if="authStore.canDeleteProject"
                               icon size="small" variant="text" color="error"
                               @click="openDeleteProjectDialog(item)"
                               title="Delete Project">
                          <v-icon size="18">mdi-delete</v-icon>
                        </v-btn>
                      </template>

                      <template #no-data>
                        <v-alert type="info" variant="tonal" class="ma-4">
                          No projects found. Start a project from an approved enquiry.
                        </v-alert>
                      </template>
                    </v-data-table-virtual>
                  </v-card-text>
                </v-card>
              </v-window-item>
            </v-window>
          </v-card-text>
        </v-card>
      </v-col>
    </v-row>

    <!-- Project Detail Dialog -->
    <ProjectDetailDialog v-model="showDetailDialog"
                         :project="selectedProject"
                         :team-members="projectTeamMembers"
                         :revisions="projectRevisions"
                         @manage="manageProject"
                         @gantt="viewGantt" />

    <!-- Delete Project Dialog -->
    <DeleteProjectDialog v-model="deleteProjectDialog"
                         :project="deleteProjectTarget"
                         :loading="deletingProject"
                         @confirm="confirmDeleteProject" />

    <!-- Snackbar -->
    <v-snackbar v-model="snackbar" :color="snackbarColor" location="bottom right" rounded="lg">
      {{ snackbarMessage }}
      <template #actions>
        <v-btn variant="text" @click="snackbar = false">Close</v-btn>
      </template>
    </v-snackbar>
  </v-container>
</template>

<script setup>
  import { ref, computed, onMounted } from 'vue'
  import { useRouter } from 'vue-router'
  import { useProjectStore } from '@/stores/project'
  import { useAuthStore } from '@/stores/auth'
  import { NPI_STAGES } from '@/stores/stageTemplate'
  import { api } from '@/utils/api'
  import { openProjectStatusEmail } from '@/utils/mailtoHelper'
  import { PROJECT_STATUSES, PROJECT_STATUS_COLORS, PRIORITY_OPTIONS, PRIORITY_COLORS, STAGE_COLORS, STAGE_COLORS_HEX, STAGE_SHORT_NAMES } from '@/utils/constants'
  import { formatDate } from '@/utils/formatters'

  import ProjectDetailDialog from '@/components/projects/ProjectDetailDialog.vue'
  import DeleteProjectDialog from '@/components/projects/DeleteProjectDialog.vue'

  const router       = useRouter()
  const projectStore = useProjectStore()
  const authStore    = useAuthStore()
 
  const statusFilter     = ref('all')
  const search           = ref('')
  const showDetailDialog = ref(false)
  const selectedProject  = ref(null)
 
  const projectTeamMembers = ref([])
  const projectRevisions   = ref([])
 
  const deleteProjectDialog = ref(false)
  const deleteProjectTarget = ref(null)
  const deletingProject     = ref(false)
 
  const snackbar        = ref(false)
  const snackbarMessage = ref('')
  const snackbarColor   = ref('success')

   const headers = [
    { title: 'Project No',    key: 'proj_no',                   sortable: true },
    { title: 'Project Name',  key: 'proj_name',                 sortable: true },
    { title: 'Customer',      key: 'customer_name',             sortable: true },
    { title: 'NPI Stages',    key: 'stages',                    sortable: false, width: '220px' },
    { title: 'Priority',      key: 'priority',                  sortable: true },
    { title: 'Status',        key: 'status',                    sortable: true },
    { title: 'Start Date',    key: 'project_start_date',        sortable: true },
    { title: 'Target Date',   key: 'target_completion_date',    sortable: true },
    { title: 'Actions',       key: 'actions',                   sortable: false, align: 'center', width: '150px' },
  ]

  const filteredProjects = computed(() => {
    let list = projectStore.projects || []
    if (statusFilter.value !== 'all') list = list.filter(p => p.status === statusFilter.value)
    return list
  })

  // ── Status & priority updates ─────────────────────────────────────────────────

  async function updateStatus(project, newStatus) {
    if (project.status === newStatus) return
    try {
      const result = await api.put(`/project/${project.proj_id}/status`, { status: newStatus })
      if (result?.success || result?.data?.success) {
        project.status        = newStatus
        snackbarMessage.value = `Status updated to "${newStatus}"`
        snackbarColor.value   = 'success'
        snackbar.value        = true
 
        const notifyOn = ['In Progress', 'On Hold', 'Completed', 'Cancelled']
        if (notifyOn.includes(newStatus)) {
          let fullProject = project
          if (!project.team_members?.some(m => m.email)) {
            try {
              const detail = await api.get(`/project/${project.proj_id}`)
              fullProject = detail?.data ?? project
            } catch { /* non-critical */ }
          }
          openProjectStatusEmail(fullProject, newStatus, authStore.currentUser)
        }
      } else {
        snackbarMessage.value = result?.message ?? 'Failed to update status'
        snackbarColor.value   = 'error'
        snackbar.value        = true
      }
    } catch (err) {
      snackbarMessage.value = 'An error occurred'
      snackbarColor.value   = 'error'
      snackbar.value        = true
    }
  }

  async function updatePriority(project, newPriority) {
    if (project.priority === newPriority) return
    try {
      const payload = {
        proj_name:               project.proj_name,
        description:             project.description,
        dept_id:                 project.dept_id,
        priority:                newPriority,
        status:                  project.status,
        project_start_date:      project.project_start_date,
        target_completion_date:  project.target_completion_date,
      }
      const result = await api.put(`/project/${project.proj_id}`, payload)
      if (result?.success || result?.data?.success) {
        project.priority      = newPriority
        snackbarMessage.value = 'Project priority updated'
        snackbarColor.value   = 'success'
      } else {
        snackbarMessage.value = result?.message || 'Failed to update priority'
        snackbarColor.value   = 'error'
      }
    } catch (err) {
      snackbarMessage.value = err?.response?.data?.message || 'Unauthorized or server error'
      snackbarColor.value   = 'error'
    }
    snackbar.value = true
  }

  function getProjectStages(project) {
    const stageIds = Object.keys(NPI_STAGES).filter(id => {
      if (NPI_STAGES[id].required) return true
      if (id === '2.0') return !!project.pilot_mould_required
      if (id === '3.0') return !!project.machine_purchase_required
      return false
    })
    return stageIds.map(id => {
      const progress = project.stage_progress?.[id]
      let status = 'pending'
      if (progress?.completed) status = 'completed'
      else if (progress?.in_progress) status = 'active'
      return {
        id,
        name:      NPI_STAGES[id].name,
        shortName: STAGE_SHORT_NAMES[id] || id,
        status,
        color:     STAGE_COLORS_HEX[id] || '#9E9E9E',
      }
    })
  }

  function getStageDotColor(stage) {
    if (stage.status === 'completed') return '#4CAF50'
    if (stage.status === 'active') return stage.color
    if (stage.status === 'skipped') return '#BDBDBD'
    return '#E0E0E0'
  }
 
  function getStatusColor(status)  { return PROJECT_STATUS_COLORS[status] || 'grey' }
  function getPriorityColor(p)     { return PRIORITY_COLORS[p] || 'grey' }
 
  function manageProject(projId) { router.push(`/projects/${projId}/setup`) }
  function viewGantt(projId)     { router.push(`/projects/${projId}/gantt`) }

  async function loadProjectDetails(projectId) {
    try {
      const result = await projectStore.fetchProjectById(projectId)
      if (result?.success && result.data) {
        const project = result.data
        projectTeamMembers.value = project.team_members || []
        projectRevisions.value   = (project.revisions || [])
          .sort((a, b) => new Date(b.revision_date) - new Date(a.revision_date))
      }
    } catch {
      snackbarMessage.value = 'Failed to load project details'
      snackbarColor.value   = 'error'
      snackbar.value        = true
    }
  }

  function viewProject(project) {
    selectedProject.value = project
    showDetailDialog.value = true
    loadProjectDetails(project.proj_id)
  }

  function openDeleteProjectDialog(project) {
    deleteProjectTarget.value = project
    deleteProjectDialog.value = true
  }

  async function confirmDeleteProject() {
    if (!deleteProjectTarget.value) return
    deletingProject.value = true
    const result = await projectStore.deleteProject(deleteProjectTarget.value.proj_id)
    deletingProject.value     = false
    deleteProjectDialog.value = false
    snackbarMessage.value     = result.success ? 'Project deleted successfully' : `Error: ${result.message}`
    snackbarColor.value       = result.success ? 'success' : 'error'
    snackbar.value            = true
    deleteProjectTarget.value = null
  }
 
  onMounted(async () => {
    const result = await projectStore.fetchProjects()
    if (!result.success) {
      snackbarMessage.value = result.message || 'Failed to load projects'
      snackbarColor.value   = 'error'
      snackbar.value        = true
      return
    }
    await Promise.all(
      projectStore.projects.map(p => authStore.fetchProjectRole(p.proj_id))
    )
  })
</script>

<style scoped>
  .stage-pipeline {
    flex-wrap: nowrap;
  }

  .stage-dot {
    width: 28px;
    height: 18px;
    border-radius: 4px;
    display: flex;
    align-items: center;
    justify-content: center;
    cursor: default;
  }

  .stage-dot-label {
    font-size: 9px;
    font-weight: 700;
    color: white;
    line-height: 1;
  }

  .stage-arrow {
    font-size: 10px;
    color: #9e9e9e;
    line-height: 1;
  }

  .cursor-pointer {
    cursor: pointer;
  }
</style>
