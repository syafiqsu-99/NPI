<template>
  <v-container fluid class="pa-0 dashboard-root d-flex flex-column">

    <!-- Compact Top Header -->
    <div class="bg-primary text-white d-flex align-center justify-space-between px-4 py-2 flex-shrink-0 shadow-sm">
      <div class="text-subtitle-1 font-weight-medium d-flex align-center">
        <v-icon class="mr-2" size="small">mdi-clipboard-list-outline</v-icon>
        My Tasks
      </div>
      <v-btn variant="text" color="white" density="compact" :loading="loading" @click="refresh">
        <v-icon start size="small">mdi-refresh</v-icon>Refresh
      </v-btn>
    </div>

    <!-- KPI Strip (Filtered to current user's ACTIVE tasks only) -->
    <div class="kpi-strip px-4 pt-3 pb-2 flex-shrink-0">
      <v-row dense>
        <v-col v-for="kpi in statCards" :key="kpi.title" cols="12" sm="6" md="3">
          <v-card class="kpi-card d-flex flex-column justify-center" variant="tonal" :color="kpi.color">
            <div class="text-caption text-uppercase text-medium-emphasis">{{ kpi.title }}</div>
            <div class="text-h4 font-weight-bold">{{ kpi.value }}</div>
          </v-card>
        </v-col>
      </v-row>
    </div>

    <!-- Filters Strip -->
    <div class="px-4 pb-2 flex-shrink-0">
      <v-row dense align="center">
        <v-col cols="12" sm="3">
          <v-text-field v-model="search"
                        label="Search tasks…"
                        prepend-inner-icon="mdi-magnify"
                        variant="outlined" density="compact"
                        hide-details clearable bg-color="white" />
        </v-col>
        <v-col cols="6" sm="2">
          <v-select v-model="filterStatus"
                    :items="['All', ...STATUS_OPTIONS]"
                    label="Status" variant="outlined"
                    density="compact" hide-details bg-color="white" />
        </v-col>
        <v-col cols="6" sm="2">
          <v-select v-model="filterPriority"
                    :items="['All', 'Low', 'Medium', 'High', 'Critical']"
                    label="Priority" variant="outlined"
                    density="compact" hide-details bg-color="white" />
        </v-col>
        <v-col cols="12" sm="5" class="d-flex justify-end align-center ga-4">
          <v-switch v-model="showAllStages"
                    label="All stages"
                    density="compact" hide-details color="primary" />
          <v-switch v-model="showCompletedProjects"
                    label="Show completed"
                    density="compact" hide-details color="success" />
        </v-col>
      </v-row>
    </div>

    <!-- Dashboard Body -->
    <div class="dashboard-body px-4 pb-4 flex-grow-1 overflow-hidden d-flex flex-column">
      <div class="projects-scroll flex-grow-1 overflow-y-auto pr-1">

        <div v-if="!loading && visibleProjects.length === 0" class="text-center pa-10">
          <v-icon size="72" color="grey-lighten-1">mdi-clipboard-text-off</v-icon>
          <div class="text-h6 mt-4 text-grey">No projects to display</div>
        </div>

        <v-skeleton-loader v-if="loading" type="card" class="mb-4" v-for="n in 3" :key="n" />

        <v-card v-for="project in visibleProjects"
                :key="project.proj_id"
                elevation="1"
                class="mb-3 border"
                :class="{ 'project-card--archived': project.isCompleted }">

          <!-- Project header -->
          <v-card-title class="project-header cursor-pointer d-flex align-center py-2 px-3"
                        :class="project.isCompleted ? 'bg-green-lighten-5' : 'bg-grey-lighten-4'"
                        @click="toggleProject(project.proj_id)">

            <v-icon class="mr-3" :color="project.isCompleted ? 'success' : 'grey-darken-2'">
              {{ project.isCompleted ? 'mdi-folder-check' : 'mdi-folder-outline' }}
            </v-icon>

            <div class="flex-grow-1">
              <div class="text-subtitle-1 font-weight-bold"
                   :class="project.isCompleted ? 'text-green-darken-3' : 'text-grey-darken-3'">
                {{ project.proj_name }}
              </div>
              <div class="text-caption"
                   :class="project.isCompleted ? 'text-green-darken-1' : 'text-grey'">
                {{ project.proj_no }} · {{ project.tasks.length }} task(s)
              </div>
            </div>

            <div class="d-flex ga-2 align-center">
              <v-chip v-if="project.isCompleted" size="small" color="success" variant="flat">
                <v-icon start size="x-small">mdi-archive</v-icon>Archived
              </v-chip>

              <v-chip v-if="project.priority" :color="getPriorityColor(project.priority)" size="small" variant="tonal">
                {{ project.priority }}
              </v-chip>

              <v-chip v-if="project.tasks.filter(t => isOverdue(t)).length > 0" size="small" color="error" variant="flat">
                <v-icon start size="x-small">mdi-clock-alert</v-icon>
                {{ project.tasks.filter(t => isOverdue(t)).length }} overdue
              </v-chip>

              <v-chip v-if="!showAllStages && !project.isCompleted" :color="getStageColor(getCurrentStageId(project.tasks))" size="small" variant="outlined" class="bg-white">
                Stage {{ getCurrentStageId(project.tasks) }}
              </v-chip>

              <v-icon :color="project.isCompleted ? 'success' : ''" class="ml-2">
                {{ isExpanded(project.proj_id) ? 'mdi-chevron-up' : 'mdi-chevron-down' }}
              </v-icon>
            </div>
          </v-card-title>

          <!-- Expanded tasks -->
          <v-expand-transition>
            <div v-show="isExpanded(project.proj_id)">
              <v-card-text class="pa-0 border-t">
                <div v-for="group in getStageGroups(project)" :key="group.stageId">

                  <!-- Stage sub-header -->
                  <div class="stage-header d-flex align-center px-4 py-1">
                    <v-chip :color="getStageColor(group.stageId)" size="x-small" variant="flat" class="mr-2 font-weight-bold">
                      {{ group.stageId }}
                    </v-chip>
                    <span class="text-caption font-weight-bold text-uppercase text-grey-darken-2">{{ group.stageName }}</span>
                    <v-spacer />
                    <span class="text-caption text-grey mr-3">
                      {{ group.tasks.filter(t => t.status === 'Completed').length }}/{{ group.tasks.length }} done
                    </span>
                    <v-progress-linear :model-value="stagePct(group.tasks)"
                                       :color="getStageColor(group.stageId)"
                                       height="4" rounded style="max-width: 80px;" />
                  </div>

                  <!-- Tasks virtual table (Removed Progress Column) -->
                  <v-data-table-virtual :headers="headers"
                                        :items="group.tasks"
                                        density="compact"
                                        hide-default-footer
                                        :items-per-page="-1"
                                        class="tasks-table"
                                        height="auto">

                    <template #item.task_code="{ item }">
                      <v-chip :color="getStageColor(item.stage_id)" size="x-small" variant="tonal" class="font-weight-bold">
                        {{ item.task_code || '—' }}
                      </v-chip>
                    </template>

                    <template #item.title="{ item }">
                      <span class="font-weight-medium text-body-2">{{ item.title }}</span>
                    </template>

                    <template #item.status="{ item }">
                      <v-menu :disabled="!canEditTaskItem(item, project.proj_id)">
                        <template #activator="{ props }">
                          <v-chip v-bind="props"
                                  :color="getStatusColor(item.status)"
                                  :variant="item.status === 'Completed' ? 'flat' : 'tonal'"
                                  size="small"
                                  :class="canEditTaskItem(item, project.proj_id) ? 'cursor-pointer' : ''">
                            <v-icon start size="x-small">{{ getStatusIcon(item.status) }}</v-icon>
                            {{ item.status }}
                            <v-badge v-if="isOverdue(item)" color="error" content="!" inline class="ml-1" />
                          </v-chip>
                        </template>
                        <v-list density="compact">
                          <v-list-item v-for="s in STATUS_OPTIONS" :key="s"
                                       @click="updateStatus(item, s, project.proj_id)">
                            <template #prepend>
                              <v-icon size="small" :color="getStatusColor(s)">{{ getStatusIcon(s) }}</v-icon>
                            </template>
                            <v-list-item-title class="text-body-2">{{ s }}</v-list-item-title>
                          </v-list-item>
                        </v-list>
                      </v-menu>
                    </template>

                    <template #item.dept_name="{ item }">
                      <v-chip size="x-small" variant="outlined" color="primary">
                        {{ item.dept_name || 'N/A' }}
                      </v-chip>
                    </template>

                    <template #item.planned_dates="{ item }">
                      <div class="text-caption lh-tight">
                        <div class="font-weight-medium">{{ formatDate(item.planned_start_date) }}</div>
                        <div class="text-grey">→ {{ formatDate(item.planned_end_date) }}</div>
                        <v-chip v-if="isOverdue(item)" size="x-small" color="error" variant="flat" class="mt-1 px-1" style="height:14px; font-size:9px">
                          Overdue
                        </v-chip>
                      </div>
                    </template>

                    <template #item.actions="{ item }">
                      <div class="d-flex ga-1">
                        <v-tooltip v-if="canEditTaskItem(item, project.proj_id)" text="Upload Files" location="top">
                          <template #activator="{ props }">
                            <v-btn icon="mdi-file-upload" size="small" density="compact" variant="text"
                                   color="primary" v-bind="props" @click="openUpload(item, project)" />
                          </template>
                        </v-tooltip>

                        <v-tooltip text="Documents" location="top">
                          <template #activator="{ props }">
                            <v-badge :content="item.document_count"
                                     :model-value="item.document_count > 0" color="success">
                              <v-btn icon="mdi-file-document-multiple" size="small" density="compact"
                                     variant="text" color="info" v-bind="props" @click="openDocs(item, project)" />
                            </v-badge>
                          </template>
                        </v-tooltip>

                        <v-tooltip text="Details" location="top">
                          <template #activator="{ props }">
                            <v-btn icon="mdi-eye" size="small" density="compact" variant="text"
                                   v-bind="props" @click="openDetails(item, project)" />
                          </template>
                        </v-tooltip>
                      </div>
                    </template>
                  </v-data-table-virtual>
                </div>
              </v-card-text>
            </div>
          </v-expand-transition>
        </v-card>

      </div>
    </div>

    <!-- ── Upload Dialog ── -->
    <v-dialog v-model="uploadDialog" max-width="560" persistent>
      <v-card>
        <v-card-title class="bg-primary text-white">
          <v-icon class="mr-2">mdi-file-upload</v-icon>
          Upload Files
        </v-card-title>
        <v-card-text class="pt-4">
          <v-alert type="info" variant="tonal" class="mb-4">
            <strong>Task:</strong> {{ selectedTask?.task_code }} — {{ selectedTask?.title }}<br>
            <strong>Project:</strong> {{ selectedProject?.proj_name }}
          </v-alert>
          <v-file-input v-model="uploadFiles" label="Select files"
                        multiple chips clearable show-size
                        prepend-icon="mdi-paperclip" variant="outlined" accept="*/*" />
          <v-textarea v-model="uploadDescription" label="Description (optional)"
                      variant="outlined" rows="2" class="mt-3" />
        </v-card-text>
        <v-card-actions class="pa-4">
          <v-spacer />
          <v-btn variant="text" @click="closeUpload">Cancel</v-btn>
          <v-btn color="primary" variant="elevated"
                 :disabled="!uploadFiles?.length" :loading="uploading"
                 @click="doUpload">
            Upload {{ uploadFiles?.length || 0 }} file(s)
          </v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>

    <!-- ── Documents Dialog ── -->
    <v-dialog v-model="docsDialog" max-width="600" persistent>
      <v-card>
        <v-card-title class="bg-primary text-white d-flex align-center justify-space-between">
          <span>
            <v-icon class="mr-2">mdi-file-document-multiple</v-icon>
            Documents — {{ selectedTask?.title }}
          </span>
          <v-btn icon="mdi-close" variant="text" color="white" @click="docsDialog = false" />
        </v-card-title>
        <v-card-text class="pa-0">
          <v-list v-if="taskDocs.length > 0">
            <v-list-item v-for="doc in taskDocs" :key="doc.file_id" class="border-b">
              <template #prepend>
                <v-icon :color="fileIconColor(doc.file_name)">{{ fileIcon(doc.file_name) }}</v-icon>
              </template>
              <v-list-item-title>{{ doc.file_name }}</v-list-item-title>
              <v-list-item-subtitle class="text-caption">
                {{ formatSize(doc.file_size) }} · {{ formatDateTime(doc.uploaded_at) }}
              </v-list-item-subtitle>
              <template #append>
                <v-btn icon="mdi-download" size="small" variant="text" @click="downloadDoc(doc)" />
                <v-btn v-if="!isViewer"
                       icon="mdi-delete" size="small" variant="text" color="error"
                       @click="confirmDeleteDoc(doc)" />
              </template>
            </v-list-item>
          </v-list>
          <v-card-text v-else class="text-center pa-8">
            <v-icon size="48" color="grey">mdi-file-document-off</v-icon>
            <div class="text-caption text-grey mt-2">No documents yet</div>
          </v-card-text>
        </v-card-text>
        <v-card-actions class="pa-4">
          <v-btn v-if="!isViewer" color="primary" variant="text"
                 prepend-icon="mdi-upload" @click="switchToUpload">
            Upload More
          </v-btn>
          <v-spacer />
          <v-btn variant="text" @click="docsDialog = false">Close</v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>

    <!-- ── Task Details Dialog ── -->
    <v-dialog v-model="detailsDialog" max-width="640" persistent>
      <v-card v-if="selectedTask">
        <v-card-title class="bg-primary text-white d-flex align-center justify-space-between">
          <span>
            <v-chip size="small" :color="getStageColor(selectedTask.stage_id)"
                    variant="tonal" class="mr-2 bg-white">
              {{ selectedTask.task_code || '—' }}
            </v-chip>
            {{ selectedTask.title }}
          </span>
          <v-btn icon="mdi-close" variant="text" color="white" @click="detailsDialog = false" />
        </v-card-title>
        <v-card-text class="pt-4">
          <v-row>
            <v-col cols="12" md="6">
              <v-card variant="outlined">
                <v-card-title class="text-subtitle-1 pb-0">Task Info</v-card-title>
                <v-card-text>
                  <div class="mb-2"><strong>Project:</strong> {{ selectedProject?.proj_name }}</div>
                  <div class="mb-2">
                    <strong>Status:</strong>
                    <v-chip :color="getStatusColor(selectedTask.status)" size="small" class="ml-1">
                      {{ selectedTask.status }}
                    </v-chip>
                  </div>
                  <div class="mb-2"><strong>Department:</strong> {{ selectedTask.dept_name || 'N/A' }}</div>
                  <div class="mb-2"><strong>Duration:</strong> {{ selectedTask.duration || 0 }} days</div>
                  <div>
                    <strong>Progress:</strong>
                    <v-progress-linear :model-value="selectedTask.per_complete || 0"
                                       :color="getProgressColor(selectedTask.per_complete)"
                                       height="20" rounded class="mt-1">
                      <strong>{{ selectedTask.per_complete || 0 }}%</strong>
                    </v-progress-linear>
                  </div>
                </v-card-text>
              </v-card>
            </v-col>
            <v-col cols="12" md="6">
              <v-card variant="outlined">
                <v-card-title class="text-subtitle-1 pb-0">Timeline</v-card-title>
                <v-card-text>
                  <div class="mb-3">
                    <div class="text-caption text-grey mb-1">Planned</div>
                    <div>{{ formatDate(selectedTask.planned_start_date) }} → {{ formatDate(selectedTask.planned_end_date) }}</div>
                  </div>
                  <div>
                    <div class="text-caption text-grey mb-1">Actual</div>
                    <div>
                      {{ formatDate(selectedTask.actual_start_date) || 'Not started' }} →
                      {{ formatDate(selectedTask.actual_end_date) || 'Not completed' }}
                    </div>
                  </div>
                </v-card-text>
              </v-card>
            </v-col>
          </v-row>
        </v-card-text>
        <v-card-actions>
          <v-spacer />
          <v-btn variant="text" @click="detailsDialog = false">Close</v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>

    <!-- ── Delete Confirm ── -->
    <v-dialog v-model="deleteDialog" max-width="420" persistent>
      <v-card>
        <v-card-title class="bg-error text-white">Confirm Delete</v-card-title>
        <v-card-text class="pt-4">
          Delete <strong>{{ docToDelete?.file_name }}</strong>? This cannot be undone.
        </v-card-text>
        <v-card-actions>
          <v-spacer />
          <v-btn variant="text" @click="deleteDialog = false">Cancel</v-btn>
          <v-btn color="error" variant="elevated" :loading="deleting" @click="doDelete">Delete</v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>

    <v-snackbar v-model="snackbar" :color="snackbarColor" timeout="3000">
      {{ snackbarMsg }}
      <template #actions>
        <v-btn variant="text" @click="snackbar = false">Close</v-btn>
      </template>
    </v-snackbar>
  </v-container>
</template>

<script setup>
  import { ref, computed, onMounted } from 'vue'
  import { useAuthStore } from '@/stores/auth'
  import { NPI_STAGES } from '@/stores/stageTemplate'
  import { api } from '@/utils/api'

  const authStore = useAuthStore()

  // ── Auth & Access ─────────────────────────────────────────────────────────────
  const currentUser = computed(() => authStore.user ?? authStore.currentUser)
  const isViewer = computed(() => authStore.userRole === 'Viewer')

  function canEditTaskItem(task, projId) {
    if (authStore.isAdmin || authStore.isManager) return true
    if (isViewer.value) return false

    const projectRole = authStore.getProjectRole(projId)
    if (projectRole === 'Team Lead') return true

    if (projectRole === 'Member') {
      const userDeptId = currentUser.value?.dept_id
      return task.dept_id && userDeptId && Number(task.dept_id) === Number(userDeptId)
    }

    return false
  }

  // ── State ─────────────────────────────────────────────────────────────────────
  const loading = ref(false)
  const uploading = ref(false)
  const deleting = ref(false)

  const allTasks = ref([]) // Holds all tasks across projects you are a member of

  const uploadDialog = ref(false)
  const docsDialog = ref(false)
  const detailsDialog = ref(false)
  const deleteDialog = ref(false)

  const selectedTask = ref(null)
  const selectedProject = ref(null)
  const taskDocs = ref([])
  const docToDelete = ref(null)

  const uploadFiles = ref([])
  const uploadDescription = ref('')

  const search = ref('')
  const filterStatus = ref('All')
  const filterPriority = ref('All')
  const showAllStages = ref(false)
  const showCompletedProjects = ref(false) // Toggle completed/archived projects

  const expandedProjects = ref([])

  const snackbar = ref(false)
  const snackbarMsg = ref('')
  const snackbarColor = ref('success')

  const STATUS_OPTIONS = ['Not Started', 'In Progress', 'On Hold', 'Completed', 'Cancelled']

  // Progress column has been removed from this array
  const headers = [
    { title: 'Code', key: 'task_code', width: '80px', sortable: false },
    { title: 'Task', key: 'title', width: '40%' },
    { title: 'Status', key: 'status', width: '150px' },
    { title: 'Dept', key: 'dept_name', width: '120px' },
    { title: 'Dates', key: 'planned_dates', width: '160px', sortable: false },
    { title: 'Actions', key: 'actions', width: '120px', sortable: false }
  ]

  const STAGE_COLORS = {
    '0.0': 'blue-grey', '1.0': 'primary', '2.0': 'purple',
    '3.0': 'teal', '4.0': 'indigo', '5.0': 'deep-orange'
  }

  // ── Derived View Properties ───────────────────────────────────────────────────

  // Determine Project true status first so we can properly filter everything
  const allProjectsBase = computed(() => {
    const map = new Map()
    allTasks.value.forEach(t => {
      if (!map.has(t.proj_id)) {
        map.set(t.proj_id, {
          proj_id: t.proj_id,
          proj_no: t.proj_no ?? '',
          proj_name: t.proj_name ?? `Project #${t.proj_id}`,
          priority: t.proj_priority ?? null,
          status: t.proj_status ?? null,
          allTasksForProj: []
        })
      }
      map.get(t.proj_id).allTasksForProj.push(t)
    })

    return [...map.values()].map(p => {
      const isCompleted = p.status === 'Completed' ||
        (p.allTasksForProj.length > 0 && p.allTasksForProj.every(t => t.status === 'Completed' || t.status === 'Cancelled'))
      return { ...p, isCompleted }
    })
  })

  // 1. KPI Task Base: Permanently Excludes Completed Projects + Filters directly down to Active user tasks
  const mySpecificTasks = computed(() => {
    const activeProjIds = new Set(allProjectsBase.value.filter(p => !p.isCompleted).map(p => p.proj_id))
    return allTasks.value.filter(t => activeProjIds.has(t.proj_id) && canEditTaskItem(t, t.proj_id))
  })

  const statCards = computed(() => [
    { title: 'My Total Tasks', value: mySpecificTasks.value.length, color: 'primary' },
    { title: 'In Progress', value: mySpecificTasks.value.filter(t => t.status === 'In Progress').length, color: 'blue' },
    { title: 'Completed', value: mySpecificTasks.value.filter(t => t.status === 'Completed').length, color: 'green' },
    { title: 'Overdue', value: mySpecificTasks.value.filter(t => isOverdue(t)).length, color: 'orange' }
  ])

  // 2. Visible filtered list of tasks for the table data
  const filteredTasks = computed(() => {
    const visibleProjIds = new Set(
      allProjectsBase.value
        .filter(p => showCompletedProjects.value || !p.isCompleted)
        .map(p => p.proj_id)
    )

    return allTasks.value.filter(t => {
      if (!visibleProjIds.has(t.proj_id)) return false

      const q = search.value?.toLowerCase() ?? ''
      const matchSearch = !q || t.title?.toLowerCase().includes(q)
        || t.task_code?.toLowerCase().includes(q) || t.proj_name?.toLowerCase().includes(q)
      const matchStatus = filterStatus.value === 'All' || t.status === filterStatus.value
      const matchPriority = filterPriority.value === 'All' || t.priority === filterPriority.value
      return matchSearch && matchStatus && matchPriority
    })
  })

  // 3. Re-group remaining tasks back into Projects for the UI Map
  const visibleProjects = computed(() => {
    const map = new Map()
    filteredTasks.value.forEach(task => {
      if (!map.has(task.proj_id)) {
        const baseProj = allProjectsBase.value.find(p => p.proj_id === task.proj_id)
        map.set(task.proj_id, {
          ...baseProj,
          tasks: [] // Clean slate to push matching tasks only
        })
      }
      map.get(task.proj_id).tasks.push(task)
    })
    return [...map.values()].sort((a, b) => a.proj_name.localeCompare(b.proj_name))
  })

  // ── Helpers ───────────────────────────────────────────────────────────────────

  function deriveStageFromCode(code) {
    if (!code) return null
    const m = code.match(/^(\d+)\.\d+$/)
    return m ? `${m[1]}.0` : null
  }

  function getCurrentStageId(tasks) {
    const stageOrder = Object.keys(NPI_STAGES).sort((a, b) => parseFloat(a) - parseFloat(b))
    for (const sid of stageOrder) {
      const st = tasks.filter(t => (t.stage_id || deriveStageFromCode(t.task_code) || '1.0') === sid)
      if (st.length && !st.every(t => t.status === 'Completed')) return sid
    }
    return stageOrder.at(-1) ?? '1.0'
  }

  function getStageGroups(project) {
    const groups = new Map()
    const currentStage = getCurrentStageId(project.tasks)
    const visible = showAllStages.value || project.isCompleted // Archived should probably just show its remaining content freely
      ? project.tasks
      : project.tasks.filter(t => (t.stage_id || deriveStageFromCode(t.task_code) || '1.0') === currentStage)

    visible.forEach(task => {
      const sid = task.stage_id || deriveStageFromCode(task.task_code) || '1.0'
      if (!groups.has(sid)) groups.set(sid, { stageId: sid, stageName: NPI_STAGES[sid]?.name ?? sid, tasks: [] })
      groups.get(sid).tasks.push({ ...task, stage_id: sid }) // Clones for display, but mutations must apply to original
    })
    return [...groups.entries()]
      .sort(([a], [b]) => parseFloat(a) - parseFloat(b))
      .map(([, v]) => v)
  }

  function stagePct(tasks) {
    if (!tasks.length) return 0
    return (tasks.filter(t => t.status === 'Completed').length / tasks.length) * 100
  }

  function toggleProject(projId) {
    const i = expandedProjects.value.indexOf(projId)
    i > -1 ? expandedProjects.value.splice(i, 1) : expandedProjects.value.push(projId)
  }
  function isExpanded(projId) { return expandedProjects.value.includes(projId) }

  function getStageColor(sid) { return STAGE_COLORS[sid] ?? 'grey' }
  function getStatusColor(s) {
    return { 'Not Started': 'grey', 'In Progress': 'blue', 'On Hold': 'orange', 'Completed': 'green', 'Cancelled': 'red' }[s] ?? 'grey'
  }
  function getStatusIcon(s) {
    return { 'Not Started': 'mdi-circle-outline', 'In Progress': 'mdi-play-circle', 'On Hold': 'mdi-pause-circle', 'Completed': 'mdi-check-circle', 'Cancelled': 'mdi-close-circle' }[s] ?? 'mdi-circle-outline'
  }
  function getPriorityColor(p) {
    return { Low: 'grey', Medium: 'blue', High: 'orange', Critical: 'red' }[p] ?? 'grey'
  }
  function getProgressColor(v) {
    if (v >= 100) return 'success'
    if (v >= 50) return 'primary'
    if (v > 0) return 'warning'
    return 'grey'
  }

  function isOverdue(task) {
    if (task.status === 'Completed' || task.status === 'Cancelled' || !task.planned_end_date) return false
    const end = new Date(task.planned_end_date)
    end.setHours(0, 0, 0, 0)
    const today = new Date()
    today.setHours(0, 0, 0, 0)
    return end < today
  }

  function fileIcon(name) {
    const ext = name?.split('.').pop().toLowerCase()
    return { pdf: 'mdi-file-pdf-box', doc: 'mdi-file-word', docx: 'mdi-file-word', xls: 'mdi-file-excel', xlsx: 'mdi-file-excel', png: 'mdi-file-image', jpg: 'mdi-file-image', jpeg: 'mdi-file-image' }[ext] ?? 'mdi-file-document'
  }
  function fileIconColor(name) {
    const ext = name?.split('.').pop().toLowerCase()
    return { pdf: 'red', doc: 'blue', docx: 'blue', xls: 'green', xlsx: 'green', png: 'purple', jpg: 'purple', jpeg: 'purple' }[ext] ?? 'grey'
  }
  function formatDate(d) {
    if (!d) return 'N/A'
    return new Date(d).toLocaleDateString('en-GB', { day: '2-digit', month: 'short', year: 'numeric' })
  }
  function formatDateTime(d) {
    if (!d) return 'N/A'
    return new Date(d).toLocaleString('en-GB', { day: '2-digit', month: 'short', hour: '2-digit', minute: '2-digit' })
  }
  function formatSize(bytes) {
    if (!bytes) return '0 B'
    const sizes = ['B', 'KB', 'MB', 'GB']
    const i = Math.floor(Math.log(bytes) / Math.log(1024))
    return `${Math.round(bytes / Math.pow(1024, i) * 100) / 100} ${sizes[i]}`
  }

  // ── API ───────────────────────────────────────────────────────────────────────
  async function loadTasks() {
    loading.value = true
    try {
      const result = await api.get('/task/my-tasks')
      allTasks.value = result?.data ?? (Array.isArray(result) ? result : [])

      if (allTasks.value.length && expandedProjects.value.length === 0) {
        // Only target auto-expand to incomplete projects if possible
        const inProgressTask = [...allTasks.value]
          .filter(t => t.status === 'In Progress')
          .sort((a, b) => new Date(b.planned_start_date) - new Date(a.planned_start_date))[0]

        const targetId = inProgressTask?.proj_id ?? allTasks.value[0]?.proj_id
        if (targetId) expandedProjects.value = [targetId]
      }

      const uniqueProjIds = [...new Set(allTasks.value.map(t => t.proj_id))]
      await Promise.all(uniqueProjIds.map(id => authStore.fetchProjectRole(id)))
    } catch {
      showSnack('Failed to load tasks', 'error')
    } finally {
      loading.value = false
    }
  }

  async function refresh() {
    await loadTasks()
    showSnack('Tasks refreshed', 'info')
  }

  // Reactive UI update instantly applied to the true original object
  async function updateStatus(taskItemProxy, newStatus, projId) {
    if (!canEditTaskItem(taskItemProxy, projId)) {
      showSnack('You are not authorised to modify this task.', 'error')
      return
    }

    // Crucial fix: Find original item in allTasks array to ensure UI Vue reactivity updates globally
    const realTask = allTasks.value.find(t => t.task_id === taskItemProxy.task_id)
    if (!realTask) return

    const oldStatus = realTask.status
    const oldProgress = realTask.per_complete

    // Apply UI update optimistically directly onto the original task reference
    realTask.status = newStatus
    if (newStatus === 'Completed') {
      realTask.per_complete = 100
      realTask.actual_end_date = realTask.actual_end_date || new Date().toISOString().split('T')[0]
    } else if (newStatus === 'In Progress') {
      if (realTask.per_complete === 0) realTask.per_complete = 10
      realTask.actual_start_date = realTask.actual_start_date || new Date().toISOString().split('T')[0]
    } else if (newStatus === 'Not Started') {
      realTask.per_complete = 0
    }

    try {
      const result = await api.put(`/task/${realTask.task_id}/status`, { status: newStatus })
      if (result?.success) {
        showSnack('Status updated', 'success')
      } else {
        // Rollback state on failure
        realTask.status = oldStatus
        realTask.per_complete = oldProgress
        showSnack(result?.message || 'Failed to update status', 'error')
      }
    } catch {
      realTask.status = oldStatus
      realTask.per_complete = oldProgress
      showSnack('Server rejected the request. Check your permissions.', 'error')
    }
  }

  async function openUpload(task, project) {
    selectedTask.value = task
    selectedProject.value = project
    uploadFiles.value = []
    uploadDialog.value = true
  }

  function closeUpload() {
    uploadDialog.value = false
    selectedTask.value = null
    uploadFiles.value = []
  }

  async function doUpload() {
    if (!uploadFiles.value?.length) return
    uploading.value = true
    try {
      const fd = new FormData()
      fd.append('task_id', selectedTask.value.task_id)
      fd.append('proj_id', selectedTask.value.proj_id)
      fd.append('description', uploadDescription.value ?? '')
      for (const f of Array.from(uploadFiles.value)) fd.append('files', f)

      const result = await api.uploadFile('/file/upload', fd)
      if (result?.success) {
        showSnack(`${uploadFiles.value.length} file(s) uploaded`, 'success')
        closeUpload()
      } else {
        showSnack(result?.message || 'Upload failed', 'error')
      }
    } catch {
      showSnack('Upload failed', 'error')
    } finally {
      uploading.value = false
    }
  }

  async function openDocs(task, project) {
    selectedTask.value = task
    selectedProject.value = project
    docsDialog.value = true
    const result = await api.get(`/file/by-task/${task.task_id}`)
    taskDocs.value = result?.data ?? []
  }

  function downloadDoc(doc) { window.open(`/api/file/download/${doc.file_id}`, '_blank') }
  function switchToUpload() { docsDialog.value = false; openUpload(selectedTask.value, selectedProject.value) }
  function confirmDeleteDoc(doc) { docToDelete.value = doc; deleteDialog.value = true }

  async function doDelete() {
    deleting.value = true
    try {
      const result = await api.delete(`/file/${docToDelete.value.file_id}`)
      if (result?.success) {
        taskDocs.value = taskDocs.value.filter(d => d.file_id !== docToDelete.value.file_id)
        deleteDialog.value = false
        showSnack('Document deleted', 'success')
      }
    } catch {
      showSnack('Failed to delete', 'error')
    } finally {
      deleting.value = false
    }
  }

  function openDetails(task, project) {
    selectedTask.value = task
    selectedProject.value = project
    detailsDialog.value = true
  }

  function showSnack(msg, color = 'success') {
    snackbarMsg.value = msg
    snackbarColor.value = color
    snackbar.value = true
  }

  onMounted(loadTasks)
</script>

<style scoped>
  /* Base View Constraints */
  .dashboard-root {
    background: #f5f6f8;
    height: 100vh !important;
    overflow: hidden !important;
  }

  .dashboard-body {
    min-height: 0;
  }

  /* Compact KPI Styling */
  .kpi-card {
    height: 72px; /* Slimmer profile */
    border-radius: 10px;
    padding: 12px 16px;
  }

  .projects-scroll {
    overflow-y: auto;
    padding-bottom: 16px;
  }

  .project-card--archived {
    opacity: 0.88;
  }

  .project-header {
    transition: background-color 0.2s;
  }

  .cursor-pointer {
    cursor: pointer;
  }

  .stage-header {
    background-color: #fafafa;
    border-bottom: 1px solid rgba(0,0,0,0.06);
  }

  .tasks-table :deep(th) {
    font-weight: 600 !important;
    background-color: #ffffff;
  }

  .border-b {
    border-bottom: 1px solid rgba(0,0,0,0.12);
  }

  .border-t {
    border-top: 1px solid rgba(0,0,0,0.08);
  }

  .lh-tight {
    line-height: 1.3;
  }
</style>
