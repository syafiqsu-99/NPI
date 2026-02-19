<template>
  <v-container fluid class="pa-4">
    <!-- Header -->
    <v-row>
      <v-col cols="12">
        <v-card elevation="2">
          <v-card-title class="bg-primary d-flex align-center justify-space-between">
            <div>
              <v-icon class="mr-2">mdi-clipboard-list-outline</v-icon>
              My Tasks
            </div>
            <v-btn variant="text" color="white" @click="refreshTasks">
              <v-icon start>mdi-refresh</v-icon>
              Refresh
            </v-btn>
          </v-card-title>

          <!-- Summary Cards -->
          <v-card-text class="pa-4">
            <v-row>
              <v-col cols="12" sm="6" md="3">
                <v-card variant="tonal" color="grey">
                  <v-card-text>
                    <div class="d-flex align-center justify-space-between">
                      <div>
                        <div class="text-h4 font-weight-bold">{{ taskStats.total }}</div>
                        <div class="text-caption">Total Tasks</div>
                      </div>
                      <v-icon size="48" color="grey-darken-2">mdi-clipboard-list</v-icon>
                    </div>
                  </v-card-text>
                </v-card>
              </v-col>
              <v-col cols="12" sm="6" md="3">
                <v-card variant="tonal" color="blue">
                  <v-card-text>
                    <div class="d-flex align-center justify-space-between">
                      <div>
                        <div class="text-h4 font-weight-bold">{{ taskStats.inProgress }}</div>
                        <div class="text-caption">In Progress</div>
                      </div>
                      <v-icon size="48" color="blue-darken-2">mdi-play-circle</v-icon>
                    </div>
                  </v-card-text>
                </v-card>
              </v-col>
              <v-col cols="12" sm="6" md="3">
                <v-card variant="tonal" color="success">
                  <v-card-text>
                    <div class="d-flex align-center justify-space-between">
                      <div>
                        <div class="text-h4 font-weight-bold">{{ taskStats.completed }}</div>
                        <div class="text-caption">Completed</div>
                      </div>
                      <v-icon size="48" color="success-darken-2">mdi-check-circle</v-icon>
                    </div>
                  </v-card-text>
                </v-card>
              </v-col>
              <v-col cols="12" sm="6" md="3">
                <v-card variant="tonal" color="warning">
                  <v-card-text>
                    <div class="d-flex align-center justify-space-between">
                      <div>
                        <div class="text-h4 font-weight-bold">{{ taskStats.overdue }}</div>
                        <div class="text-caption">Overdue</div>
                      </div>
                      <v-icon size="48" color="warning-darken-2">mdi-alert-circle</v-icon>
                    </div>
                  </v-card-text>
                </v-card>
              </v-col>
            </v-row>
          </v-card-text>
        </v-card>
      </v-col>
    </v-row>

    <!-- Tasks by Project -->
    <v-row class="mt-4">
      <v-col cols="12" v-for="project in projectsWithTasks" :key="project.proj_id">
        <v-card elevation="2">
          <!-- Project Header -->
          <v-card-title class="bg-grey-lighten-3">
            <v-row align="center" no-gutters>
              <v-col cols="auto">
                <v-icon class="mr-2">mdi-folder-outline</v-icon>
              </v-col>
              <v-col>
                <div class="text-h6">{{ project.proj_name }}</div>
                <div class="text-caption text-grey">
                  {{ project.tasks.length }} {{ project.tasks.length === 1 ? 'task' : 'tasks' }}
                </div>
              </v-col>
              <v-col cols="auto">
                <div class="d-flex ga-2">
                  <v-chip :color="getPriorityColor(project.priority)" size="small" variant="tonal">
                    <v-icon start size="x-small">mdi-flag</v-icon>
                    {{ project.priority }}
                  </v-chip>
                  <v-chip :color="getProjectStatusColor(project)" size="small">
                    {{ project.status }}
                  </v-chip>
                </div>
              </v-col>
            </v-row>
          </v-card-title>

          <!-- Tasks Table -->
          <v-card-text class="pa-0">
            <v-data-table-virtual :headers="headers"
                                  :items="project.tasks"
                                  :loading="loading"
                                  height="400"
                                  density="compact"
                                  fixed-header
                                  class="tasks-table">

              <template #item.title="{ item }">
                <div class="py-2">
                  <div class="font-weight-medium">
                    <v-chip size="x-small" variant="tonal" class="mr-2">
                      {{ getTaskNumber(project.proj_id, item.task_id) }}
                    </v-chip>
                    {{ item.title }}
                  </div>
                  <div v-if="item.description" class="text-caption text-grey mt-1">
                    {{ truncateText(item.description, 80) }}
                  </div>
                </div>
              </template>

              <template #item.status="{ item }">
                <v-menu>
                  <template #activator="{ props }">
                    <v-chip v-bind="props" :color="getStatusColor(item.status)" size="small" class="cursor-pointer">
                      <v-icon start size="x-small">{{ getStatusIcon(item.status) }}</v-icon>
                      {{ item.status }}
                    </v-chip>
                  </template>
                  <v-list density="compact">
                    <v-list-item v-for="status in statusOptions" :key="status" @click="updateTaskStatus(item, status)">
                      <v-list-item-title>
                        <v-icon start size="small" :color="getStatusColor(status)">{{ getStatusIcon(status) }}</v-icon>
                        {{ status }}
                      </v-list-item-title>
                    </v-list-item>
                  </v-list>
                </v-menu>
              </template>

              <template #item.dept_name="{ item }">
                <v-chip size="small" variant="outlined" color="primary">
                  {{ item.dept_name || 'N/A' }}
                </v-chip>
              </template>

              <template #item.planned_dates="{ item }">
                <div class="text-caption">
                  <div>
                    <v-icon size="x-small" class="mr-1">mdi-calendar-start</v-icon>
                    {{ formatDate(item.planned_start_date) }}
                  </div>
                  <div class="text-grey mt-1">
                    <v-icon size="x-small" class="mr-1">mdi-calendar-end</v-icon>
                    {{ formatDate(item.planned_end_date) }}
                  </div>
                  <v-chip v-if="isOverdue(item)" size="x-small" color="error" variant="tonal" class="mt-1">
                    Overdue
                  </v-chip>
                </div>
              </template>

              <template #item.actions="{ item }">
                <div class="d-flex ga-1">
                  <v-tooltip text="Upload Documents" location="top">
                    <template #activator="{ props }">
                      <v-btn icon="mdi-file-upload" size="small" variant="text" color="primary"
                             v-bind="props" @click="openUploadDialog(item, project)"></v-btn>
                    </template>
                  </v-tooltip>

                  <v-tooltip text="View Documents" location="top">
                    <template #activator="{ props }">
                      <v-badge :content="getDocumentCount(item)" :model-value="getDocumentCount(item) > 0" color="success">
                        <v-btn icon="mdi-file-document-multiple" size="small" variant="text" color="info"
                               v-bind="props" @click="openDocumentsDialog(item, project)"></v-btn>
                      </v-badge>
                    </template>
                  </v-tooltip>

                  <v-tooltip text="Task Details" location="top">
                    <template #activator="{ props }">
                      <v-btn icon="mdi-eye" size="small" variant="text"
                             v-bind="props" @click="openTaskDetailsDialog(item, project)"></v-btn>
                    </template>
                  </v-tooltip>
                </div>
              </template>
            </v-data-table-virtual>
          </v-card-text>
        </v-card>
      </v-col>

      <v-col cols="12" v-if="projectsWithTasks.length === 0 && !loading">
        <v-card>
          <v-card-text class="text-center pa-8">
            <v-icon size="64" color="grey">mdi-clipboard-text-off</v-icon>
            <div class="text-h6 mt-4 text-grey">No projects assigned to you</div>
            <div class="text-caption text-grey">
              Projects will appear here once you are added to a project team
            </div>
          </v-card-text>
        </v-card>
      </v-col>
    </v-row>

    <!-- Upload Documents Dialog -->
    <v-dialog v-model="uploadDialog" max-width="600px" persistent>
      <v-card>
        <v-card-title class="bg-primary">
          <v-icon class="mr-2">mdi-file-upload</v-icon>
          Upload Documents - {{ selectedTask?.title }}
        </v-card-title>

        <v-card-text class="pt-4">
          <!--
            Shows the server-side folder path so the user can see where
            files will land. This is DISPLAY ONLY — it is never sent to
            the backend. The backend resolves the path from task_id.
          -->
          <v-alert type="info" variant="tonal" class="mb-4">
            <div class="text-caption">
              <strong>Upload Location:</strong><br>
              Project: {{ selectedProject?.proj_name }}<br>
              Task Folder:
              <code>{{ previewTaskFolderName }}</code>
            </div>
          </v-alert>

          <v-file-upload v-model="uploadFiles"
                         label="Select Files"
                         multiple
                         chips
                         clearable
                         show-size
                         prepend-icon="mdi-paperclip"
                         variant="outlined">
            <template #selection="{ fileNames }">
              <template v-for="(fileName, index) in fileNames" :key="fileName">
                <v-chip size="small" label closable @click:close="removeFile(index)" class="mr-2 mb-2">
                  {{ fileName }}
                </v-chip>
              </template>
            </template>
          </v-file-upload>

          <v-textarea v-model="uploadDescription"
                      label="Description (Optional)"
                      variant="outlined"
                      rows="3"
                      class="mt-4">
          </v-textarea>

          <v-alert type="success" variant="tonal" class="mt-4">
            <div class="text-caption">
              <v-icon size="small" class="mr-1">mdi-check-circle</v-icon>
              <strong>All file types and sizes accepted</strong>
            </div>
            <div class="text-caption mt-1">
              Files are stored in task-specific folders managed by the server
            </div>
          </v-alert>
        </v-card-text>

        <v-card-actions class="pa-4">
          <v-spacer></v-spacer>
          <v-btn variant="text" @click="closeUploadDialog">Cancel</v-btn>
          <v-btn color="primary" variant="elevated"
                 :disabled="!uploadFiles || uploadFiles.length === 0"
                 :loading="uploading"
                 @click="uploadDocuments">
            <v-icon start>mdi-upload</v-icon>
            Upload {{ uploadFiles?.length || 0 }} {{ uploadFiles?.length === 1 ? 'File' : 'Files' }}
          </v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>

    <!-- View Documents Dialog -->
    <v-dialog v-model="documentsDialog" max-width="900px">
      <v-card>
        <v-card-title class="bg-primary d-flex align-center justify-space-between">
          <div>
            <v-icon class="mr-2">mdi-file-document-multiple</v-icon>
            Task Documents - {{ selectedTask?.title }}
          </div>
          <v-btn icon="mdi-close" variant="text" color="white" @click="documentsDialog = false"></v-btn>
        </v-card-title>

        <v-card-subtitle class="pa-3 bg-grey-lighten-4">
          <v-icon size="small" class="mr-1">mdi-folder</v-icon>
          {{ previewTaskFolderPath }}
        </v-card-subtitle>

        <v-card-text class="pa-0">
          <v-list v-if="taskDocuments.length > 0">
            <v-list-item v-for="doc in taskDocuments" :key="doc.file_id" class="border-b">
              <template #prepend>
                <v-icon :color="getFileIconColor(doc.file_name)">{{ getFileIcon(doc.file_name) }}</v-icon>
              </template>
              <v-list-item-title>{{ doc.file_name }}</v-list-item-title>
              <v-list-item-subtitle>
                <div class="text-caption">
                  {{ formatFileSize(doc.file_size) }} •
                  Uploaded {{ formatDateTime(doc.uploaded_at) }} by {{ doc.uploaded_by_name }}
                </div>
                <div v-if="doc.description" class="text-caption text-grey mt-1">{{ doc.description }}</div>
              </v-list-item-subtitle>
              <template #append>
                <div class="d-flex ga-2">
                  <v-btn icon="mdi-download" size="small" variant="text" @click="downloadDocument(doc)"></v-btn>
                  <v-btn icon="mdi-delete" size="small" variant="text" color="error"
                         @click="confirmDeleteDocument(doc)"></v-btn>
                </div>
              </template>
            </v-list-item>
          </v-list>
          <v-card-text v-else class="text-center pa-8">
            <v-icon size="48" color="grey">mdi-file-document-off</v-icon>
            <div class="text-caption text-grey mt-2">No documents uploaded yet</div>
          </v-card-text>
        </v-card-text>

        <v-card-actions class="pa-4">
          <v-btn color="primary" variant="text" prepend-icon="mdi-upload" @click="openUploadFromDocuments">
            Upload More
          </v-btn>
          <v-spacer></v-spacer>
          <v-btn variant="text" @click="documentsDialog = false">Close</v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>

    <!-- Task Details Dialog -->
    <v-dialog v-model="detailsDialog" max-width="900px">
      <v-card v-if="selectedTask">
        <v-card-title class="bg-primary d-flex align-center justify-space-between">
          <span>{{ selectedTask.title }}</span>
          <v-btn icon="mdi-close" variant="text" color="white" @click="detailsDialog = false"></v-btn>
        </v-card-title>
        <v-card-text class="pt-4">
          <v-row>
            <v-col cols="12" md="6">
              <v-card variant="outlined">
                <v-card-title class="text-subtitle-1">Task Information</v-card-title>
                <v-card-text>
                  <div class="mb-2"><strong>Project:</strong> {{ selectedProject?.proj_name }}</div>
                  <div class="mb-2">
                    <strong>Task Number:</strong>
                    {{ getTaskNumber(selectedProject?.proj_id, selectedTask.task_id) }}
                  </div>
                  <div class="mb-2">
                    <strong>Status:</strong>
                    <v-chip :color="getStatusColor(selectedTask.status)" size="small" class="ml-2">
                      {{ selectedTask.status }}
                    </v-chip>
                  </div>
                  <div class="mb-2">
                    <strong>Project Priority:</strong>
                    <v-chip :color="getPriorityColor(selectedProject?.priority)" size="small" class="ml-2">
                      {{ selectedProject?.priority }}
                    </v-chip>
                  </div>
                  <div class="mb-2"><strong>Department:</strong> {{ selectedTask.dept_name || 'N/A' }}</div>
                  <div class="mb-2"><strong>Duration:</strong> {{ selectedTask.duration || 0 }} days</div>
                  <div class="mb-2">
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
                <v-card-title class="text-subtitle-1">Timeline</v-card-title>
                <v-card-text>
                  <div class="mb-3">
                    <div class="text-caption text-grey mb-1">Planned</div>
                    <div>{{ formatDate(selectedTask.planned_start_date) }} - {{ formatDate(selectedTask.planned_end_date) }}</div>
                  </div>
                  <div class="mb-3">
                    <div class="text-caption text-grey mb-1">Actual</div>
                    <div>
                      {{ formatDate(selectedTask.actual_start_date) || 'Not started' }} -
                      {{ formatDate(selectedTask.actual_end_date) || 'Not completed' }}
                    </div>
                  </div>
                  <div class="mb-3">
                    <div class="text-caption text-grey mb-1">Created</div>
                    <div>{{ formatDateTime(selectedTask.created_at) }}</div>
                  </div>
                  <div v-if="selectedTask.completed_at">
                    <div class="text-caption text-grey mb-1">Completed</div>
                    <div>{{ formatDateTime(selectedTask.completed_at) }}</div>
                  </div>
                </v-card-text>
              </v-card>
            </v-col>
            <v-col cols="12" v-if="selectedTask.description">
              <v-card variant="outlined">
                <v-card-title class="text-subtitle-1">Description</v-card-title>
                <v-card-text>{{ selectedTask.description }}</v-card-text>
              </v-card>
            </v-col>
          </v-row>
        </v-card-text>
        <v-card-actions>
          <v-spacer></v-spacer>
          <v-btn variant="text" @click="detailsDialog = false">Close</v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>

    <!-- Delete Document Confirmation -->
    <v-dialog v-model="deleteDocDialog" max-width="400px">
      <v-card>
        <v-card-title class="bg-error">Confirm Delete</v-card-title>
        <v-card-text class="pt-4">
          Are you sure you want to delete "{{ docToDelete?.file_name }}"? This action cannot be undone.
        </v-card-text>
        <v-card-actions>
          <v-spacer></v-spacer>
          <v-btn variant="text" @click="deleteDocDialog = false">Cancel</v-btn>
          <v-btn color="error" variant="elevated" :loading="deleting" @click="deleteDocument">Delete</v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>

    <!-- Snackbar -->
    <v-snackbar v-model="snackbar" :color="snackbarColor">
      {{ snackbarMessage }}
      <template #actions>
        <v-btn variant="text" @click="snackbar = false">Close</v-btn>
      </template>
    </v-snackbar>
  </v-container>
</template>

<script setup>
  import { ref, computed, onMounted } from 'vue'
  import { useAuthStore } from '@/stores/auth'
  import { api } from '@/utils/api'

  const authStore = useAuthStore()

  const loading = ref(false)
  const uploading = ref(false)
  const deleting = ref(false)
  const tasks = ref([])
  const projects = ref([])
  const projectTeams = ref([])
  const taskDocuments = ref([])

  // Dialogs
  const uploadDialog = ref(false)
  const documentsDialog = ref(false)
  const detailsDialog = ref(false)
  const deleteDocDialog = ref(false)

  // Form data
  const selectedTask = ref(null)
  const selectedProject = ref(null)
  const uploadFiles = ref([])
  const uploadDescription = ref('')
  const docToDelete = ref(null)

  // Snackbar
  const snackbar = ref(false)
  const snackbarMessage = ref('')
  const snackbarColor = ref('success')

  const statusOptions = ['Not Started', 'In Progress', 'On Hold', 'Completed', 'Cancelled']

  const headers = [
    { title: 'Task', key: 'title', width: '35%' },
    { title: 'Status', key: 'status', width: '15%' },
    { title: 'Department', key: 'dept_name', width: '15%' },
    { title: 'Planned Dates', key: 'planned_dates', width: '20%' },
    { title: 'Actions', key: 'actions', width: '15%', sortable: false }
  ]

  const currentUserId = computed(() => authStore.currentUser?.user_id)
  const isAdmin = computed(() => authStore.userRole === 'Admin')

  const userProjects = computed(() => {
    if (isAdmin.value) return projects.value
    const ids = projectTeams.value
      .filter(pt => pt.user_id === currentUserId.value)
      .map(pt => pt.proj_id)
    return projects.value.filter(p => ids.includes(p.proj_id))
  })

  const filteredTasks = computed(() => {
    const ids = userProjects.value.map(p => p.proj_id)
    return tasks.value.filter(t => ids.includes(t.proj_id))
  })

  const projectsWithTasks = computed(() => {
    const map = new Map()
    filteredTasks.value.forEach(task => {
      if (!map.has(task.proj_id)) {
        const project = userProjects.value.find(p => p.proj_id === task.proj_id)
        if (project) map.set(task.proj_id, { ...project, tasks: [] })
      }
      map.get(task.proj_id)?.tasks.push(task)
    })
    return Array.from(map.values()).sort((a, b) => a.proj_name.localeCompare(b.proj_name))
  })

  const taskStats = computed(() => ({
    total: filteredTasks.value.length,
    inProgress: filteredTasks.value.filter(t => t.status === 'In Progress').length,
    completed: filteredTasks.value.filter(t => t.status === 'Completed').length,
    overdue: filteredTasks.value.filter(t => isOverdue(t)).length
  }))

  function sanitizeForDisplay(name) {
    if (!name) return ''
    return name.replace(/[ /]/g, '_').replace(/[<>:"\\|?*\x00-\x1F]/g, '_')
  }

  const previewTaskFolderName = computed(() => {
    if (!selectedTask.value || !selectedProject.value) return ''
    const num = getTaskNumber(selectedProject.value.proj_id, selectedTask.value.task_id)
    return `${num}_${sanitizeForDisplay(selectedTask.value.title)}`
  })

  const previewTaskFolderPath = computed(() => {
    if (!selectedTask.value || !selectedProject.value) return ''
    return `${sanitizeForDisplay(selectedProject.value.proj_name)}/${previewTaskFolderName.value}`
  })

  // ── Methods ────────────────────────────────────────────────────────────────

  function getTaskNumber(projId, taskId) {
    const projectTasks = tasks.value
      .filter(t => t.proj_id === projId)
      .sort((a, b) => a.task_id - b.task_id)
    const index = projectTasks.findIndex(t => t.task_id === taskId)
    return index !== -1 ? String(index + 1).padStart(2, '0') : 'N/A'
  }

  function isOverdue(task) {
    if (task.status === 'Completed' || !task.planned_end_date) return false
    return new Date(task.planned_end_date) < new Date()
  }

  function getStatusColor(status) {
    return {
      'Not Started': 'grey', 'In Progress': 'blue', 'On Hold': 'orange',
      'Completed': 'green', 'Cancelled': 'red'
    }[status] ?? 'grey'
  }

  function getStatusIcon(status) {
    return {
      'Not Started': 'mdi-circle-outline', 'In Progress': 'mdi-play-circle',
      'On Hold': 'mdi-pause-circle', 'Completed': 'mdi-check-circle',
      'Cancelled': 'mdi-close-circle'
    }[status] ?? 'mdi-circle-outline'
  }

  function getPriorityColor(priority) {
    return { 'Low': 'grey', 'Medium': 'blue', 'High': 'orange', 'Critical': 'red' }[priority] ?? 'grey'
  }

  function getProgressColor(progress) {
    if (progress >= 100) return 'success'
    if (progress >= 50) return 'primary'
    if (progress > 0) return 'warning'
    return 'grey'
  }

  function getProjectStatusColor(project) {
    return {
      'Planning': 'grey', 'In Progress': 'blue', 'On Hold': 'orange',
      'Completed': 'green', 'Cancelled': 'red'
    }[project.status] ?? 'grey'
  }

  function getFileIcon(filename) {
    const ext = filename?.split('.').pop().toLowerCase()
    return {
      pdf: 'mdi-file-pdf-box', doc: 'mdi-file-word', docx: 'mdi-file-word',
      xls: 'mdi-file-excel', xlsx: 'mdi-file-excel',
      png: 'mdi-file-image', jpg: 'mdi-file-image', jpeg: 'mdi-file-image',
      zip: 'mdi-folder-zip', rar: 'mdi-folder-zip',
      txt: 'mdi-file-document-outline'
    }[ext] ?? 'mdi-file-document'
  }

  function getFileIconColor(filename) {
    const ext = filename?.split('.').pop().toLowerCase()
    return {
      pdf: 'red', doc: 'blue', docx: 'blue', xls: 'green', xlsx: 'green',
      png: 'purple', jpg: 'purple', jpeg: 'purple',
      zip: 'orange', rar: 'orange'
    }[ext] ?? 'grey'
  }

  function formatDate(date) {
    if (!date) return 'N/A'
    return new Date(date).toLocaleDateString('en-GB', { day: '2-digit', month: 'short', year: 'numeric' })
  }

  function formatDateTime(datetime) {
    if (!datetime) return 'N/A'
    return new Date(datetime).toLocaleString('en-GB',
      { day: '2-digit', month: 'short', year: 'numeric', hour: '2-digit', minute: '2-digit' })
  }

  function formatFileSize(bytes) {
    if (!bytes) return '0 B'
    const k = 1024, sizes = ['B', 'KB', 'MB', 'GB']
    const i = Math.floor(Math.log(bytes) / Math.log(k))
    return Math.round(bytes / Math.pow(k, i) * 100) / 100 + ' ' + sizes[i]
  }

  function truncateText(text, length) {
    if (!text) return ''
    return text.length > length ? text.substring(0, length) + '...' : text
  }

  function getDocumentCount(task) { return task.document_count || 0 }

  async function updateTaskStatus(task, newStatus) {
    try {
      const result = await api.put(`/task/${task.task_id}/status`, { status: newStatus })
      if (result?.success) {
        task.status = newStatus
        task.updated_at = new Date().toISOString()
        if (newStatus === 'Completed') {
          task.per_complete = 100
          task.actual_end_date = new Date().toISOString().split('T')[0]
        } else if (newStatus === 'In Progress' && !task.actual_start_date) {
          task.actual_start_date = new Date().toISOString().split('T')[0]
        }
        showSnack('Task status updated successfully', 'success')
      }
    } catch {
      showSnack('Failed to update task status', 'error')
    }
  }

  function openUploadDialog(task, project) {
    selectedTask.value = task
    selectedProject.value = project
    uploadFiles.value = []
    uploadDescription.value = ''
    uploadDialog.value = true
  }

  function closeUploadDialog() {
    uploadDialog.value = false
    selectedTask.value = null
    selectedProject.value = null
    uploadFiles.value = []
    uploadDescription.value = ''
  }

  function removeFile(index) { uploadFiles.value.splice(index, 1) }

  async function uploadDocuments() {
    if (!uploadFiles.value?.length) return

    uploading.value = true
    try {
      const formData = new FormData()
      formData.append('task_id', selectedTask.value.task_id)
      formData.append('proj_id', selectedTask.value.proj_id)
      formData.append('description', uploadDescription.value ?? '')

      for (const file of Array.from(uploadFiles.value)) {
        formData.append('files', file)
      }

      const result = await api.uploadFile('/file/upload', formData)

      if (result?.success) {
        showSnack(`${uploadFiles.value.length} document(s) uploaded successfully`, 'success')
        if (selectedTask.value) {
          selectedTask.value.document_count =
            (selectedTask.value.document_count || 0) + uploadFiles.value.length
        }
        closeUploadDialog()
      } else {
        showSnack(result?.message || 'Upload failed', 'error')
      }
    } catch (error) {
      console.error('Upload error:', error)
      showSnack('Failed to upload documents', 'error')
    } finally {
      uploading.value = false
    }
  }

  async function openDocumentsDialog(task, project) {
    selectedTask.value = task
    selectedProject.value = project
    documentsDialog.value = true
    await loadTaskDocuments(task.task_id)
  }

  function openUploadFromDocuments() {
    documentsDialog.value = false
    openUploadDialog(selectedTask.value, selectedProject.value)
  }

  async function loadTaskDocuments(taskId) {
    try {
      const result = await api.get(`/file/by-task/${taskId}`)
      taskDocuments.value = result?.data ?? (Array.isArray(result) ? result : [])
    } catch (error) {
      console.error('Error loading documents:', error)
    }
  }

  function downloadDocument(doc) {
    window.open(`/api/file/download/${doc.file_id}`, '_blank')
  }

  function confirmDeleteDocument(doc) {
    docToDelete.value = doc
    deleteDocDialog.value = true
  }

  async function deleteDocument() {
    deleting.value = true
    try {
      const result = await api.delete(`/file/${docToDelete.value.file_id}`)
      if (result?.success) {
        taskDocuments.value = taskDocuments.value.filter(d => d.file_id !== docToDelete.value.file_id)
        if (selectedTask.value) {
          selectedTask.value.document_count = Math.max(0, (selectedTask.value.document_count || 1) - 1)
        }
        showSnack('Document deleted successfully', 'success')
        deleteDocDialog.value = false
      }
    } catch {
      showSnack('Failed to delete document', 'error')
    } finally {
      deleting.value = false
    }
  }

  function openTaskDetailsDialog(task, project) {
    selectedTask.value = task
    selectedProject.value = project
    detailsDialog.value = true
  }

  async function refreshTasks() {
    await Promise.all([loadTasks(), loadProjects(), loadProjectTeams()])
    showSnack('Tasks refreshed', 'info')
  }

  async function loadTasks() {
    loading.value = true
    try {
      const result = await api.get('/task')
      tasks.value = result?.data ?? (Array.isArray(result) ? result : [])
    } catch {
      showSnack('Failed to load tasks', 'error')
    } finally {
      loading.value = false
    }
  }

  async function loadProjects() {
    try {
      const result = await api.get('/project')
      projects.value = result?.data ?? (Array.isArray(result) ? result : [])
    } catch (error) {
      console.error('Error loading projects:', error)
    }
  }

  async function loadProjectTeams() {
    try {
      const result = await api.get('/projectteam')
      projectTeams.value = result?.data ?? (Array.isArray(result) ? result : [])
    } catch (error) {
      console.error('Error loading project teams:', error)
    }
  }

  function showSnack(message, color = 'success') {
    snackbarMessage.value = message
    snackbarColor.value = color
    snackbar.value = true
  }

  onMounted(async () => {
    await Promise.all([loadTasks(), loadProjects(), loadProjectTeams()])
  })
</script>

<style scoped>
  .cursor-pointer {
    cursor: pointer;
  }

  .tasks-table :deep(.v-table__wrapper) {
    max-height: 400px !important;
  }

  .tasks-table :deep(th) {
    font-weight: 600 !important;
    background-color: rgb(var(--v-theme-surface));
    position: sticky !important;
    top: 0;
    z-index: 2;
  }

  .v-chip {
    font-size: 0.75rem;
  }

  .border-b {
    border-bottom: 1px solid rgba(0,0,0,0.12);
  }
</style>
