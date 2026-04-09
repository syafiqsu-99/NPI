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
                        <v-menu :disabled="!canEditProject(item)">
                          <template #activator="{ props }">
                            <v-chip v-bind="props"
                                    :color="getPriorityColor(item.priority)"
                                    size="small" variant="tonal"
                                    :class="canEditProject(item) ? 'cursor-pointer' : ''"
                                    :title="canEditProject(item) ? 'Click to change priority' : ''">
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
                        <v-menu :disabled="!canEditProject(item)">
                          <template #activator="{ props }">
                            <v-chip v-bind="props"
                                    :color="getStatusColor(item.status)"
                                    size="small" variant="tonal"
                                    :class="canEditProject(item) ? 'cursor-pointer' : ''"
                                    :title="canEditProject(item) ? 'Click to change status' : ''">
                              {{ item.status }}
                            </v-chip>
                          </template>
                          <v-list density="compact">
                            <v-list-item v-for="s in STATUS_OPTIONS" :key="s" @click="updateStatus(item, s)">
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
    <v-dialog v-model="showDetailDialog" max-width="900" persistent>
      <v-card v-if="selectedProject" class="dialog-card">
        <v-card-title class="bg-primary text-white d-flex align-center justify-space-between">
          <span class="d-flex align-center gap-2">
            <v-icon>mdi-information</v-icon>
            {{ selectedProject.proj_no }} — {{ selectedProject.proj_name }}
          </span>
          <v-btn icon="mdi-close" variant="text" color="white" @click="showDetailDialog = false" />
        </v-card-title>

        <v-tabs v-model="projectDetailTab" bg-color="surface" class="px-4">
          <v-tab value="overview" class="font-weight-bold">
            <v-icon start>mdi-view-dashboard</v-icon>
            Overview
          </v-tab>
          <v-tab value="team" class="font-weight-bold">
            <v-icon start>mdi-account-group</v-icon>
            Team Members
          </v-tab>
          <v-tab value="revisions" class="font-weight-bold">
            <v-icon start>mdi-history</v-icon>
            Revision History
          </v-tab>
        </v-tabs>

        <v-window v-model="projectDetailTab" class="mt-0">
          <!-- TAB 1: OVERVIEW -->
          <v-window-item value="overview">
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

              <v-row class="ga-4">
                <v-col cols="12" md="6">
                  <div><strong>Customer:</strong> {{ selectedProject.customer_name || 'N/A' }}</div>
                </v-col>
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
                  <v-chip v-if="selectedProject.pilot_mould_required" size="x-small" color="purple" variant="tonal" class="ml-1">
                    2.0 Pilot Mould
                  </v-chip>
                  <v-chip v-if="selectedProject.machine_purchase_required" size="x-small" color="teal" variant="tonal" class="ml-1">
                    3.0 Machine
                  </v-chip>
                  <span v-if="!selectedProject.pilot_mould_required && !selectedProject.machine_purchase_required"
                        class="ml-2 text-grey text-caption">None</span>
                </v-col>
                <v-col cols="12">
                  <strong>Description:</strong>
                  <p class="mt-2">{{ selectedProject.description || 'No description available' }}</p>
                </v-col>
              </v-row>
            </v-card-text>
          </v-window-item>

          <!-- TAB 2: TEAM MEMBERS -->
          <v-window-item value="team">
            <v-card-text class="pa-6" style="max-height: 500px; overflow-y: auto;">
              <v-list v-if="projectTeamMembers.length > 0" class="bg-transparent">
                <v-list-item v-for="member in projectTeamMembers" :key="member.user_id"
                             class="border-b pa-3 d-flex align-center ga-3">
                  <!-- Avatar -->
                  <v-avatar color="primary" size="40">
                    <span class="text-white font-weight-bold text-caption">
                      {{ getInitials(member.full_name || member.user_name) }}
                    </span>
                  </v-avatar>

                  <!-- Member details -->
                  <div class="flex-grow-1">
                    <div class="font-weight-bold">{{ member.full_name || member.user_name }}</div>
                    <div class="text-caption text-grey">
                      <v-chip size="x-small" variant="tonal" color="info" class="mr-2">
                        {{ member.dept_name || 'N/A' }}
                      </v-chip>
                      <v-chip size="x-small" variant="tonal" color="success">
                        {{ member.role || 'Team Member' }}
                      </v-chip>
                    </div>
                    <div class="text-caption text-grey-darken-1 mt-1">
                      Assigned: {{ formatDate(member.assigned_at) }}
                    </div>
                  </div>
                </v-list-item>
              </v-list>
              <v-alert v-else type="info" variant="tonal" class="ma-4">
                No team members assigned to this project yet
              </v-alert>
            </v-card-text>
          </v-window-item>

          <!-- TAB 3: REVISION HISTORY -->
          <v-window-item value="revisions">
            <v-card-text class="pa-6" style="max-height: 500px; overflow-y: auto;">
              <v-timeline v-if="projectRevisions.length > 0" align="start" side="end">
                <v-timeline-item v-for="(rev, idx) in projectRevisions" :key="rev.revision_id"
                                 :dot-color="getRevisionColor(idx)"
                                 size="small">
                  <!-- Revision header -->
                  <div class="mb-2">
                    <strong class="text-body-2">Revision #{{ rev.revision_number }}</strong>
                    <v-chip size="x-small" variant="text" color="grey" class="ml-2">
                      {{ formatDateTime(rev.revision_date) }}
                    </v-chip>
                  </div>

                  <!-- Revision metadata -->
                  <div class="text-caption mb-2">
                    <div class="mb-1">
                      <strong>Updated by:</strong> {{ rev.revised_by_name || 'System' }}
                    </div>
                    <div class="mb-1">
                      <strong>Reason:</strong>
                      <span class="text-grey">{{ rev.revision_notes || 'No reason provided' }}</span>
                    </div>
                  </div>

                  <!-- Target date changes -->
                  <v-card v-if="rev.previous_target_date || rev.new_target_date"
                          variant="outlined" size="small" class="mb-3 pa-2">
                    <div class="text-caption">
                      <div v-if="rev.previous_target_date" class="mb-1">
                        <strong>Previous Target:</strong> {{ formatDate(rev.previous_target_date) }}
                      </div>
                      <div v-if="rev.new_target_date">
                        <strong>New Target:</strong> {{ formatDate(rev.new_target_date) }}
                      </div>
                    </div>
                  </v-card>

                  <!-- Task revisions within this project revision -->
                  <v-expand-transition>
                    <div v-if="showTaskRevisions[rev.revision_id]"
                         class="bg-grey-lighten-4 pa-2 rounded mt-2">
                      <div class="text-caption font-weight-bold mb-2">Task Changes:</div>
                      <v-list v-if="rev.task_revisions?.length" density="compact" class="pa-0">
                        <v-list-item v-for="tr in rev.task_revisions" :key="tr.task_id"
                                     class="text-caption pa-1">
                          <template #prepend>
                            <v-icon size="x-small" color="warning">mdi-pencil</v-icon>
                          </template>
                          <v-list-item-title class="text-caption">
                            {{ tr.task_title || `Task ${tr.task_id}` }}
                          </v-list-item-title>
                          <v-list-item-subtitle class="text-caption mt-1">
                            {{ formatDate(tr.old_start_date) }} → {{ formatDate(tr.old_end_date) }}
                            <v-icon size="x-small" class="mx-1">mdi-arrow-right</v-icon>
                            {{ formatDate(tr.new_start_date) }} → {{ formatDate(tr.new_end_date) }}
                          </v-list-item-subtitle>
                          <template v-if="tr.revision_note" #append>
                            <v-tooltip :text="tr.revision_note" location="left">
                              <template #activator="{ props }">
                                <v-icon v-bind="props" size="x-small" color="info">mdi-information</v-icon>
                              </template>
                            </v-tooltip>
                          </template>
                        </v-list-item>
                      </v-list>
                      <div v-else class="text-caption text-grey">No task changes in this revision</div>
                    </div>
                  </v-expand-transition>

                  <!-- Expand/collapse button -->
                  <v-btn v-if="rev.task_revisions?.length"
                         size="x-small" variant="text" color="primary" class="mt-1"
                         @click="showTaskRevisions[rev.revision_id] = !showTaskRevisions[rev.revision_id]">
                    {{ showTaskRevisions[rev.revision_id] ? 'Hide' : 'Show' }} Task Changes
                  </v-btn>
                </v-timeline-item>
              </v-timeline>
              <v-alert v-else type="info" variant="tonal" class="ma-4">
                No revision history available
              </v-alert>
            </v-card-text>
          </v-window-item>
        </v-window>

        <v-card-actions class="pa-4 d-flex justify-end ga-2">
          <v-btn variant="text" @click="showDetailDialog = false">Close</v-btn>
          <v-btn v-if="canManageProject(selectedProject)" color="primary" variant="elevated"
                 @click="manageProject(selectedProject.proj_id)">
            Manage Project
          </v-btn>
          <v-btn v-if="selectedProject.status === 'In Progress' || selectedProject.status === 'Completed'"
                 color="success" variant="elevated" @click="viewGantt(selectedProject.proj_id)">
            View Gantt Chart
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
  import { api } from '@/utils/api'

  const router = useRouter()
  const projectStore = useProjectStore()
  const authStore = useAuthStore()

  const statusFilter = ref('all')
  const search = ref('')
  const showDetailDialog = ref(false)
  const selectedProject = ref(null)

  const projectDetailTab = ref('overview')
  const projectTeamMembers = ref([])
  const projectRevisions = ref([])
  const showTaskRevisions = ref({})

  const deleteProjectDialog = ref(false)
  const deleteProjectTarget = ref(null)
  const deletingProject = ref(false)

  const snackbar = ref(false)
  const snackbarMessage = ref('')
  const snackbarColor = ref('success')

  const STATUS_OPTIONS = ['Planning', 'Not Started', 'In Progress', 'On Hold', 'Completed', 'Cancelled']
  const PRIORITY_OPTIONS = ['Low', 'Medium', 'High', 'Critical']

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

  const taskHeaders = [
    { title: 'Code', key: 'task_code', width: '80px' },
    { title: 'Task', key: 'title', width: '40%' },
    { title: 'Status', key: 'status', width: '120px' },
    { title: 'Assigned To', key: 'assigned_to_name', width: '150px', align: 'center' }
  ]

  const userRole = computed(() => authStore.user?.role || authStore.userRole)

  const filteredProjects = computed(() => {
    let list = projectStore.projects || []
    if (statusFilter.value !== 'all') list = list.filter(p => p.status === statusFilter.value)
    return list
  })

  function canEditProject(project) {
    if (['Admin', 'Manager'].includes(userRole.value)) return true
    return authStore.hasProjectRole(project.proj_id, 'Team Lead')
  }

  // --- Add these API Update Methods ---
  async function updateStatus(project, newStatus) {
    if (project.status === newStatus) return;

    try {
      const result = await api.put(`/project/${project.proj_id}/status`, { status: newStatus });
      if (result?.success || result?.data?.success) {
        project.status = newStatus;
        snackbarMessage.value = 'Project status updated';
        snackbarColor.value = 'success';
      } else {
        snackbarMessage.value = result?.message || 'Failed to update status';
        snackbarColor.value = 'error';
      }
    } catch (err) {
      snackbarMessage.value = err?.response?.data?.message || 'Unauthorized or server error';
      snackbarColor.value = 'error';
    }
    snackbar.value = true;
  }

  async function updatePriority(project, newPriority) {
    if (project.priority === newPriority) return;

    try {
      const payload = {
        proj_name: project.proj_name,
        description: project.description,
        dept_id: project.dept_id,
        priority: newPriority,
        status: project.status,
        project_start_date: project.project_start_date,
        target_completion_date: project.target_completion_date
      };
    
      const result = await api.put(`/project/${project.proj_id}`, payload);
      if (result?.success || result?.data?.success) {
        project.priority = newPriority;
        snackbarMessage.value = 'Project priority updated';
        snackbarColor.value = 'success';
      } else {
        snackbarMessage.value = result?.message || 'Failed to update priority';
        snackbarColor.value = 'error';
      }
    } catch (err) {
      snackbarMessage.value = err?.response?.data?.message || 'Unauthorized or server error';
      snackbarColor.value = 'error';
    }
    snackbar.value = true;
  }

  const STAGE_SHORT = {
    '0.0': 'Enquiry', '1.0': 'Proj Start', '2.0': 'Pilot Mould',
    '3.0': 'Machine', '4.0': 'Prod Mould', '5.0': 'Trial JJ'
  }

  const STAGE_COLORS_MAP = {
    '0.0': '#607D8B', '1.0': '#1976D2', '2.0': '#7B1FA2',
    '3.0': '#00796B', '4.0': '#303F9F', '5.0': '#E64A19'
  }

  function getInitials(name) {
    if (!name) return '?'
    return name.split(' ').map(n => n[0]).join('').toUpperCase().slice(0, 2)
  }

  function getRevisionColor(index) {
    const colors = ['primary', 'success', 'warning', 'error', 'info']
    return colors[index % colors.length]
  }

  function formatDate(date) {
    if (!date) return 'N/A'
    if (typeof date === 'string') {
      const parts = date.split('-')
      if (parts.length === 3)
        return new Date(parts[0], parts[1] - 1, parts[2])
          .toLocaleDateString('en-GB', { day: '2-digit', month: 'short', year: 'numeric' })
    }
    return new Date(date).toLocaleDateString('en-GB', { day: '2-digit', month: 'short', year: 'numeric' })
  }

  function formatDateTime(date) {
    if (!date) return 'N/A'
    return new Date(date).toLocaleString('en-GB', {
      day: '2-digit', month: 'short', year: 'numeric',
      hour: '2-digit', minute: '2-digit'
    })
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

  function canManageProject(project) {
    if (['Admin', 'Manager'].includes(userRole.value)) return true
    if (!['Planning', 'In Progress'].includes(project.status)) return false
    return authStore.hasProjectRole(project.proj_id, 'Team Lead')
  }

  function canDeleteProject() {
    return ['Admin', 'Manager'].includes(userRole.value)
  }

  function getStatusColor(status) {
    return { 'Planning': 'info', 'In Progress': 'primary', 'On Hold': 'warning', 'Completed': 'success', 'Cancelled': 'error' }[status] || 'grey'
  }

  function getPriorityColor(priority) {
    return { 'Low': 'success', 'Medium': 'info', 'High': 'warning', 'Critical': 'error' }[priority] || 'grey'
  }

  function manageProject(projId) { router.push(`/projects/${projId}/setup`) }
  function viewGantt(projId) { router.push(`/projects/${projId}/gantt`) }

  async function loadProjectDetails(projectId) {
    try {
      const result = await projectStore.fetchProjectById(projectId)
      if (result?.success && result.data) {
        const project = result.data

        projectTeamMembers.value = project.team_members || []

        projectRevisions.value = (project.revisions || [])
          .sort((a, b) => new Date(b.revision_date) - new Date(a.revision_date))

        // Initialize expand state for task revisions
        projectRevisions.value.forEach(rev => {
          showTaskRevisions.value[rev.revision_id] = false
        })
      }
    } catch (err) {
      console.error('Error loading project details:', err)
      snackbarMessage.value = 'Failed to load project details'
      snackbarColor.value = 'error'
      snackbar.value = true
    }
  }

  // Update viewProject function to load details
  function viewProject(project) {
    selectedProject.value = project
    projectDetailTab.value = 'overview'
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

  .cursor-pointer {
    cursor: pointer;
  }
</style>
