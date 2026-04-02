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

                    <v-data-table v-else
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

                      <template #item.priority="{ item }">
                        <v-chip :color="getPriorityColor(item.priority)" size="small" variant="tonal">
                          {{ item.priority }}
                        </v-chip>
                      </template>

                      <template #item.status="{ item }">
                        <v-chip :color="getStatusColor(item.status)" size="small" variant="tonal">
                          {{ item.status }}
                        </v-chip>
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
                        <v-btn v-if="canManageProject(item)"
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
                        <v-btn v-if="canDeleteProject(item)"
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
                    </v-data-table>
                  </v-card-text>
                </v-card>
              </v-window-item>
            </v-window>
          </v-card-text>
        </v-card>
      </v-col>
    </v-row>

    <!-- Project Detail Dialog -->
    <v-dialog v-model="showDetailDialog" max-width="600" persistent>
      <v-card v-if="selectedProject" class="dialog-card">
        <v-card-title class="bg-primary text-white d-flex align-center justify-space-between">
          <v-icon class="mr-2">mdi-information</v-icon>
          {{ selectedProject.proj_no }} — {{ selectedProject.proj_name }}
        </v-card-title>

        <v-card-text class="pa-6">
          <!-- Stage pipeline visual -->
          <v-card variant="outlined" class="mb-4 pa-3">
            <div class="text-caption text-grey-darken-1 mb-2 font-weight-bold">NPI STAGE PIPELINE</div>
            <div class="d-flex align-center flex-wrap ga-1">
              <template v-for="(stage, idx) in getProjectStages(selectedProject)" :key="stage.id">
                <div class="pipeline-node"
                     :class="`pipeline-node--${stage.status}`"
                     :style="{ borderColor: getStageBorderColor(stage), background: getStageBgColor(stage) }">
                  <div class="text-caption font-weight-bold">{{ stage.id }}</div>
                  <div class="text-caption" style="font-size:10px; line-height:1.1">{{ stage.shortName }}</div>
                  <v-icon v-if="stage.status === 'completed'" size="x-small" color="success">mdi-check</v-icon>
                  <v-icon v-else-if="stage.status === 'active'" size="x-small" color="primary">mdi-play</v-icon>
                  <v-icon v-else-if="stage.status === 'skipped'" size="x-small" color="grey">mdi-minus</v-icon>
                </div>
                <v-icon v-if="idx < getProjectStages(selectedProject).length - 1"
                        color="grey-lighten-1" size="small">mdi-arrow-right</v-icon>
              </template>
            </div>
          </v-card>

          <v-row>
            <v-col cols="12" md="6"><strong>Customer:</strong> {{ selectedProject.customer_name || 'N/A' }}</v-col>
            <v-col cols="12" md="6">
              <strong>Status:</strong>
              <v-chip :color="getStatusColor(selectedProject.status)" size="small" variant="tonal" class="ml-2">
                {{ selectedProject.status }}
              </v-chip>
            </v-col>
            <v-col cols="12" md="6">
              <strong>Priority:</strong>
              <v-chip :color="getPriorityColor(selectedProject.priority)" size="small" variant="tonal" class="ml-2">
                {{ selectedProject.priority }}
              </v-chip>
            </v-col>
            <v-col cols="12" md="6"><strong>Start Date:</strong> {{ formatDate(selectedProject.project_start_date) }}</v-col>
            <v-col cols="12" md="6"><strong>Target Completion:</strong> {{ formatDate(selectedProject.target_completion_date) }}</v-col>
            <v-col cols="12" md="6">
              <strong>Optional Stages:</strong>
              <v-chip v-if="selectedProject.pilot_mould_required" size="x-small" color="purple" variant="tonal" class="ml-1">2.0 Pilot Mould</v-chip>
              <v-chip v-if="selectedProject.machine_purchase_required" size="x-small" color="teal" variant="tonal" class="ml-1">3.0 Machine</v-chip>
              <span v-if="!selectedProject.pilot_mould_required && !selectedProject.machine_purchase_required"
                    class="ml-2 text-grey text-caption">None</span>
            </v-col>
            <v-col cols="12">
              <strong>Description:</strong>
              <p class="mt-2">{{ selectedProject.description || 'No description available' }}</p>
            </v-col>
          </v-row>
        </v-card-text>

        <v-card-actions>
          <v-spacer />
          <v-btn variant="text" @click="showDetailDialog = false">Close</v-btn>
          <v-btn v-if="canManageProject(selectedProject)" color="primary"
                 @click="manageProject(selectedProject.proj_id)">
            Manage Project
          </v-btn>
          <v-btn v-if="selectedProject.status === 'In Progress' || selectedProject.status === 'Completed'"
                 color="success" @click="viewGantt(selectedProject.proj_id)">
            View Gantt
          </v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>

    <!-- ── Delete Project Dialog ── -->
    <v-dialog v-model="deleteProjectDialog" max-width="600" persistent>
      <v-card class="dialog-card">
        <v-card-text class="pa-6">
          <div class="d-flex align-center ga-4 mb-3">
            <div class="delete-icon-wrap">
              <v-icon color="error" size="26">mdi-folder-remove</v-icon>
            </div>
            <div>
              <div class="text-subtitle-1 font-weight-bold">Delete Project?</div>
              <div class="text-body-2 text-medium-emphasis">
                <strong>{{ deleteProjectTarget?.proj_no }}</strong> —
                {{ deleteProjectTarget?.proj_name }}
              </div>
            </div>
          </div>

          <v-alert type="error" variant="tonal" density="compact" class="mb-4">
            All tasks, team assignments, and stage data will be removed.
            Files on disk are <strong>not</strong> deleted.
          </v-alert>

          <div class="d-flex ga-2 justify-end">
            <v-btn variant="text" @click="deleteProjectDialog = false">Cancel</v-btn>
            <v-btn color="error" variant="flat" rounded="lg"
                   :loading="deletingProject" @click="confirmDeleteProject">
              <v-icon start>mdi-delete</v-icon>
              Delete Project
            </v-btn>
          </div>
        </v-card-text>
      </v-card>
    </v-dialog>

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

const router = useRouter()
const projectStore = useProjectStore()
const authStore = useAuthStore()

const statusFilter = ref('all')
const search = ref('')
const showDetailDialog = ref(false)
const selectedProject = ref(null)

const deleteProjectDialog = ref(false)
const deleteProjectTarget = ref(null)
const deletingProject = ref(false)

const snackbar = ref(false)
const snackbarMessage = ref('')
const snackbarColor = ref('success')

const headers = [
  { title: 'Project No', key: 'proj_no', sortable: true },
  { title: 'Project Name', key: 'proj_name', sortable: true },
  { title: 'Customer', key: 'customer_name', sortable: true },
  { title: 'NPI Stages', key: 'stages', sortable: false, width: '220px' },
  { title: 'Priority', key: 'priority', sortable: true },
  { title: 'Status', key: 'status', sortable: true },
  { title: 'Start Date', key: 'project_start_date', sortable: true },
  { title: 'Target Date', key: 'target_completion_date', sortable: true },
  { title: 'Actions', key: 'actions', sortable: false, align: 'center', width: '150px' }
]

const userRole = computed(() => authStore.user?.role || authStore.userRole)

const filteredProjects = computed(() => {
  let list = projectStore.projects || []
  if (statusFilter.value !== 'all') list = list.filter(p => p.status === statusFilter.value)
  return list
})

// ── Stage pipeline helpers ──────────────────────────────────────────────────

const STAGE_SHORT = {
  '0.0': 'Enquiry', '1.0': 'Proj Start', '2.0': 'Pilot Mould',
  '3.0': 'Machine', '4.0': 'Prod Mould', '5.0': 'Trial JJ'
}

const STAGE_COLORS_MAP = {
  '0.0': '#607D8B', '1.0': '#1976D2', '2.0': '#7B1FA2',
  '3.0': '#00796B', '4.0': '#303F9F', '5.0': '#E64A19'
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
    else if (!NPI_STAGES[id].required &&
      !project.pilot_mould_required && !project.machine_purchase_required) status = 'skipped'
    return { id, name: NPI_STAGES[id].name, shortName: STAGE_SHORT[id] || id, status, color: STAGE_COLORS_MAP[id] || '#9E9E9E' }
  })
}

function getStageDotColor(stage) {
  if (stage.status === 'completed') return '#4CAF50'
  if (stage.status === 'active') return stage.color
  if (stage.status === 'skipped') return '#BDBDBD'
  return '#E0E0E0'
}

function getStageBorderColor(stage) {
  if (stage.status === 'completed') return '#4CAF50'
  if (stage.status === 'active') return stage.color
  return '#BDBDBD'
}

function getStageBgColor(stage) {
  if (stage.status === 'completed') return 'rgba(76,175,80,0.1)'
  if (stage.status === 'active') return `${stage.color}18`
  return 'rgba(0,0,0,0.03)'
}

// ── Other helpers ────────────────────────────────────────────────────────────

function canManageProject(project) {
  const isNpiOrAdmin = userRole.value === 'NPI Team' || userRole.value === 'Admin'
  const canManage = project.status === 'Planning' || project.status === 'In Progress'
  return isNpiOrAdmin && canManage
}

// Only Admins can delete projects
function canDeleteProject(project) {
  return userRole.value === 'Admin'
}

function getStatusColor(status) {
  return { 'Planning': 'info', 'In Progress': 'primary', 'On Hold': 'warning', 'Completed': 'success', 'Cancelled': 'error' }[status] || 'grey'
}

function getPriorityColor(priority) {
  return { 'Low': 'success', 'Medium': 'info', 'High': 'warning', 'Critical': 'error' }[priority] || 'grey'
}

function formatDate(date) {
  if (!date) return 'N/A'
  if (typeof date === 'string') {
    const parts = date.split('-')
    if (parts.length === 3)
      return new Date(parts[0], parts[1] - 1, parts[2]).toLocaleDateString('en-GB', { day: '2-digit', month: 'short', year: 'numeric' })
  }
  return new Date(date).toLocaleDateString('en-GB', { day: '2-digit', month: 'short', year: 'numeric' })
}

function manageProject(projId) { router.push(`/projects/${projId}/setup`) }
function viewGantt(projId) { router.push(`/projects/${projId}/gantt`) }
function viewProject(project) { selectedProject.value = project; showDetailDialog.value = true }

function openDeleteProjectDialog(project) {
  deleteProjectTarget.value = project
  deleteProjectDialog.value = true
}

async function confirmDeleteProject() {
  if (!deleteProjectTarget.value) return
  deletingProject.value = true
  const result = await projectStore.deleteProject(deleteProjectTarget.value.proj_id)
  deletingProject.value = false
  deleteProjectDialog.value = false
  snackbarMessage.value = result.success ? 'Project deleted successfully' : `Error: ${result.message}`
  snackbarColor.value = result.success ? 'success' : 'error'
  snackbar.value = true
  deleteProjectTarget.value = null
}

onMounted(async () => {
  const result = await projectStore.fetchProjects()
  if (!result.success) {
    snackbarMessage.value = result.message || 'Failed to load projects'
    snackbarColor.value = 'error'
    snackbar.value = true
  }
})
</script>

<style scoped>
  .v-data-table {
    background-color: transparent;
  }

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

  .pipeline-node {
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    padding: 6px 10px;
    border-radius: 8px;
    border: 2px solid;
    min-width: 64px;
    text-align: center;
    cursor: default;
    transition: all 0.2s;
  }

  .pipeline-node--active {
    box-shadow: 0 2px 8px rgba(0,0,0,0.15);
  }

  .delete-icon-wrap {
    width: 48px;
    height: 48px;
    border-radius: 12px;
    background: rgba(var(--v-theme-error), 0.08);
    display: flex;
    align-items: center;
    justify-content: center;
    flex-shrink: 0;
  }
</style>
