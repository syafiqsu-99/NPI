<template>
  <v-container fluid class="pa-0 dashboard-root d-flex flex-column">

    <div class="bg-primary text-white d-flex align-center justify-space-between px-4 py-2 flex-shrink-0 shadow-sm">
      <div class="text-subtitle-1 font-weight-medium d-flex align-center">
        <v-icon class="mr-2" size="small">mdi-clipboard-list-outline</v-icon>
        My Tasks
      </div>
      <v-btn variant="text" color="white" density="compact" :loading="loading" @click="refresh">
        <v-icon start size="small">mdi-refresh</v-icon>Refresh
      </v-btn>
    </div>

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
                    :items="[FILTER_ALL, ...taskStatuses]"
                    label="Status" variant="outlined"
                    density="compact" hide-details bg-color="white" />
        </v-col>
        <v-col cols="6" sm="2">
          <v-select v-model="filterPriority"
                    :items="[FILTER_ALL, ...priorityOptions]"
                    label="Priority" variant="outlined"
                    density="compact" hide-details bg-color="white" />
        </v-col>
        <v-col cols="12" sm="5" class="d-flex justify-end align-center ga-4">
          <v-switch v-model="showAllStages" label="All stages"
                    density="compact" hide-details color="primary" />
          <v-switch v-model="showCompletedProjects" label="Show completed"
                    density="compact" hide-details color="success" />
        </v-col>
      </v-row>
    </div>

    <div class="dashboard-body px-4 pb-4 flex-grow-1 overflow-hidden d-flex flex-column">
      <div class="projects-scroll flex-grow-1 overflow-y-auto pr-1">

        <div v-if="!loading && visibleProjects.length === 0" class="text-center pa-10">
          <v-icon size="72" color="grey-lighten-1">mdi-clipboard-text-off</v-icon>
          <div class="text-h6 mt-4 text-grey">No projects to display</div>
        </div>

        <v-skeleton-loader v-if="loading" v-for="n in 3" :key="n" type="card" class="mb-4" />

        <v-card v-for="project in visibleProjects"
                :key="project.proj_id"
                elevation="1" class="mb-3 border"
                :class="{ 'project-card--archived': project.isCompleted }">

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
              <v-chip v-if="project.tasks.filter(t => isOverdue(t)).length > 0"
                      size="small" color="error" variant="flat">
                <v-icon start size="x-small">mdi-clock-alert</v-icon>
                {{ project.tasks.filter(t => isOverdue(t)).length }} overdue
              </v-chip>
              <v-chip v-if="!showAllStages && !project.isCompleted"
                      :color="getStageColor(getCurrentStageId(project.tasks))"
                      size="small" variant="outlined" class="bg-white">
                Stage {{ getCurrentStageId(project.tasks) }}
              </v-chip>
              <v-icon :color="project.isCompleted ? 'success' : ''" class="ml-2">
                {{ isExpanded(project.proj_id) ? 'mdi-chevron-up' : 'mdi-chevron-down' }}
              </v-icon>
            </div>
          </v-card-title>

          <v-expand-transition>
            <div v-show="isExpanded(project.proj_id)">
              <v-card-text class="pa-0 border-t">
                <div v-for="group in getStageGroups(project)" :key="group.stageId">
                  <div class="stage-header d-flex align-center px-4 py-1">
                    <v-chip :color="getStageColor(group.stageId)" size="x-small" variant="flat"
                            class="mr-2 font-weight-bold">
                      {{ group.stageId }}
                    </v-chip>
                    <span class="text-caption font-weight-bold text-uppercase text-grey-darken-2">
                      {{ group.stageName }}
                    </span>
                    <v-spacer />
                    <span class="text-caption text-grey mr-3">
                      {{ group.tasks.filter(t => t.status === 'Completed').length }}/{{ group.tasks.length }} done
                    </span>
                    <v-progress-linear :model-value="stagePct(group.tasks)"
                                       :color="getStageColor(group.stageId)"
                                       height="4" rounded style="max-width: 80px;" />
                  </div>

                  <v-data-table-virtual :headers="headers"
                                        :items="group.tasks"
                                        density="compact"
                                        hide-default-footer
                                        :items-per-page="-1"
                                        class="tasks-table clickable-rows"
                                        height="auto"
                                        @click:row="(_, { item }) => openDetails(item, project)"
                                        :row-props="({ item }) => ({
                                          class: item.task_id === highlightTaskId ? 'row-highlight' : ''
                                        })">

                    <template #item.task_code="{ item }">
                      <v-chip :color="getStageColor(item.stage_id)" size="x-small" variant="tonal"
                              class="font-weight-bold">
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
                          <v-list-item v-for="s in taskStatuses" :key="s"
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
                        <v-chip v-if="isOverdue(item)" size="x-small" color="error" variant="flat"
                                class="mt-1 px-1" style="height:14px; font-size:9px">
                          Overdue
                        </v-chip>
                      </div>
                    </template>

                    <template #item.actions="{ item }">
                      <div class="d-flex ga-1" @click.stop>
                        <v-tooltip v-if="canEditTaskItem(item, project.proj_id)"
                                   text="Upload Files" location="top">
                          <template #activator="{ props }">
                            <v-btn icon="mdi-file-upload" size="small" density="compact"
                                   variant="text" color="primary" v-bind="props"
                                   @click="openUpload(item, project)" />
                          </template>
                        </v-tooltip>

                        <v-tooltip text="Open task" location="top">
                          <template #activator="{ props }">
                            <v-badge :content="commentCounts[item.task_id]"
                                     :model-value="(commentCounts[item.task_id] || 0) > 0"
                                     color="primary">
                              <v-btn icon="mdi-comment-text-outline" size="small" density="compact"
                                     variant="text" v-bind="props"
                                     @click="openDetails(item, project)" />
                            </v-badge>
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

    <!-- Upload Dialog -->
    <v-dialog v-model="uploadDialog" max-width="560">
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

    <!-- Documents Dialog -->
    <v-dialog v-model="docsDialog" max-width="600">
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
                <v-icon :color="getFileIconColor(doc.file_name)">{{ getFileIcon(doc.file_name) }}</v-icon>
              </template>
              <v-list-item-title>{{ doc.file_name }}</v-list-item-title>
              <v-list-item-subtitle class="text-caption">
                {{ formatSize(doc.file_size) }} · {{ formatDateTime(doc.uploaded_at) }}
              </v-list-item-subtitle>
              <template #append>
                <v-btn icon="mdi-download" size="small" variant="text" @click="downloadDoc(doc)" />
                <v-btn v-if="!isViewerOnProject(selectedTask?.proj_id)"
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
          <v-btn v-if="!isViewerOnProject(selectedTask?.proj_id)"
                 color="primary" variant="text"
                 prepend-icon="mdi-upload" @click="switchToUpload">
            Upload More
          </v-btn>
          <v-spacer />
          <v-btn variant="text" @click="docsDialog = false">Close</v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>

    <!-- Task Details Dialog -->
    <v-dialog v-model="detailsDialog" max-width="640">
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
          <v-row>
            <v-col cols="12">
              <v-card variant="outlined">
                <v-card-title class="text-subtitle-1 pb-1 d-flex align-center">
                  <v-icon size="18" class="mr-2">mdi-file-document-multiple-outline</v-icon>
                  Documents
                </v-card-title>
                <v-card-text>
                  <v-list v-if="detailDocs.length" density="compact">
                    <v-list-item v-for="doc in detailDocs" :key="doc.file_id" class="px-0">
                      <template #prepend>
                        <v-icon :color="getFileIconColor(doc.file_name)">{{ getFileIcon(doc.file_name) }}</v-icon>
                      </template>
                      <v-list-item-title class="text-body-2">{{ doc.file_name }}</v-list-item-title>
                      <div v-if="doc.description" class="text-caption text-grey-darken-1 doc-description">
                        {{ doc.description }}
                      </div>
                      <div class="text-caption text-grey">
                        {{ formatSize(doc.file_size) }}
                        <span v-if="doc.uploaded_by_name"> · {{ doc.uploaded_by_name }}</span>
                        <span v-if="doc.uploaded_at"> · {{ formatDate(doc.uploaded_at) }}</span>
                      </div>
                      <template #append>
                        <v-btn icon="mdi-download" size="x-small" variant="text"
                               @click="downloadDoc(doc)" />
                      </template>
                    </v-list-item>
                  </v-list>
                  <div v-else class="text-caption text-grey py-2">No documents uploaded.</div>
                </v-card-text>
              </v-card>
            </v-col>
          </v-row>
          <v-row>
            <v-col cols="12">
              <v-card variant="outlined">
                <v-card-title class="text-subtitle-1 pb-1 d-flex align-center">
                  <v-icon size="18" class="mr-2">mdi-comment-text-multiple-outline</v-icon>
                  Comments
                  <v-chip v-if="comments.length" size="x-small" class="ml-2" variant="tonal">
                    {{ comments.length }}
                  </v-chip>
                </v-card-title>

                <v-card-text>
                  <div v-if="loadingComments" class="text-center py-4">
                    <v-progress-circular indeterminate size="22" color="primary" />
                  </div>

                  <div v-else-if="comments.length === 0" class="text-caption text-grey py-2">
                    No comments yet. Use @username to mention someone.
                  </div>

                  <div v-else class="comments-scroll">
                    <div v-for="c in comments" :key="c.comment_id" class="comment-row py-2">
                      <div class="d-flex align-center mb-1">
                        <v-avatar size="24" color="primary" class="mr-2">
                          <span class="text-caption">{{ initials(c.full_name || c.username) }}</span>
                        </v-avatar>
                        <span class="text-body-2 font-weight-medium">{{ c.full_name || c.username }}</span>
                        <span class="text-caption text-grey ml-2">{{ formatDateTime(c.created_at) }}</span>
                        <v-spacer />
                        <v-btn v-if="canDeleteComment(c)"
                               icon="mdi-delete" size="x-small" variant="text" color="error"
                               @click="removeComment(c)" />
                      </div>
                      <div class="text-body-2 comment-body">
                        <template v-for="(seg, i) in renderSegments(c.body)" :key="i">
                          <span v-if="seg.mention" class="mention">{{ seg.text }}</span>
                          <span v-else>{{ seg.text }}</span>
                        </template>
                      </div>
                    </div>
                  </div>

                  <v-divider class="my-3" />

                  <div class="mention-wrap">
                    <v-textarea v-model="newComment"
                                label="Add a comment  (type @ to mention)"
                                variant="outlined" rows="2" auto-grow density="compact"
                                hide-details maxlength="2000" counter
                                @update:model-value="onCommentInput"
                                @blur="closeMentionSoon"
                                @keydown.down.prevent="moveMention(1)"
                                @keydown.up.prevent="moveMention(-1)"
                                @keydown.enter="onMentionEnter"
                                @keydown.tab="onMentionEnter"
                                @keydown.esc="mentionOpen = false" />

                    <v-sheet v-if="mentionOpen && mentionMatches.length"
                             class="mention-list" elevation="6" rounded>
                      <v-list density="compact" max-height="220">
                        <v-list-item v-for="(u, i) in mentionMatches" :key="u.username"
                                     :active="i === mentionIndex"
                                     @mousedown.prevent="applyMention(u.username)">
                          <template #prepend>
                            <v-icon size="18">{{ u.username === 'all' ? 'mdi-account-group' : 'mdi-account' }}</v-icon>
                          </template>
                          <v-list-item-title>@{{ u.username }}</v-list-item-title>
                          <v-list-item-subtitle v-if="u.full_name">{{ u.full_name }}</v-list-item-subtitle>
                        </v-list-item>
                      </v-list>
                    </v-sheet>
                  </div>
                  <div class="d-flex justify-end mt-2">
                    <v-btn color="primary" size="small" :loading="postingComment"
                           :disabled="!newComment.trim()" @click="postComment">
                      <v-icon start>mdi-send</v-icon> Post
                    </v-btn>
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

    <!-- Delete Document Confirm -->
    <v-dialog v-model="deleteDialog" max-width="420">
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
  import { useRoute } from 'vue-router'
  import { useAuthStore } from '@/stores/auth'
  import { useSettingsStore } from '@/stores/setting'
  import { api } from '@/utils/api'
  import {
    TASK_STATUS,
    TASK_STATUSES,
    TASK_CLOSED_STATUSES,
    TASK_STATUS_COLORS,
    TASK_STATUS_ICONS,
    PROJECT_STATUS,
    PRIORITY_OPTIONS,
    PRIORITY_COLORS,
    STAGE_COLORS,
    DEFAULT_STAGE_ID,
    DEFAULT_COLOR,
    FILTER_ALL,
  } from '@/utils/constants'
  import { formatDate, formatDateTime, formatSize, getFileIcon, getFileIconColor } from '@/utils/formatters'

  const authStore = useAuthStore()
  const settingsStore = useSettingsStore()

  const currentUser = computed(() => authStore.currentUser)

  const canEditTaskItem = (task, projId) => authStore.canEditTaskItem(task, projId)
  const isViewerOnProject = (projId) => authStore.isViewerOnProject(projId)

  const loading = ref(false)
  const uploading = ref(false)
  const deleting = ref(false)

  const allTasks = ref([])

  const uploadDialog = ref(false)
  const docsDialog = ref(false)
  const detailsDialog = ref(false)
  const deleteDialog = ref(false)

  const selectedTask = ref(null)
  const selectedProject = ref(null)
  const taskDocs = ref([])
  const detailDocs = ref([])
  const docToDelete = ref(null)

  const uploadFiles = ref([])
  const uploadDescription = ref('')

  const search = ref('')
  const filterStatus = ref(FILTER_ALL)
  const filterPriority = ref(FILTER_ALL)
  const showAllStages = ref(false)
  const showCompletedProjects = ref(false)
  const expandedProjects = ref([])

  const snackbar = ref(false)
  const snackbarMsg = ref('')
  const snackbarColor = ref('success')

  const comments = ref([])
  const loadingComments = ref(false)
  const newComment = ref('')
  const postingComment = ref(false)
  const commentCounts = ref({})

  const mentionableUsers = ref([])
  const mentionOpen = ref(false)
  const mentionIndex = ref(0)
  const mentionQuery = ref('')

  const route = useRoute()
  const highlightTaskId = ref(null)

  const taskStatuses = TASK_STATUSES
  const priorityOptions = PRIORITY_OPTIONS

  const headers = [
    { title: 'Code', key: 'task_code', width: '80px', sortable: false },
    { title: 'Task', key: 'title', width: '40%' },
    { title: 'Status', key: 'status', width: '150px' },
    { title: 'Dept', key: 'dept_name', width: '120px' },
    { title: 'Dates', key: 'planned_dates', width: '160px', sortable: false },
    { title: 'Actions', key: 'actions', width: '120px', sortable: false },
  ]

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
          allTasksForProj: [],
        })
      }
      map.get(t.proj_id).allTasksForProj.push(t)
    })

    return [...map.values()].map(p => {
      const isCompleted =
        p.status === PROJECT_STATUS.COMPLETED ||
        (p.allTasksForProj.length > 0 &&
          p.allTasksForProj.every(t => TASK_CLOSED_STATUSES.includes(t.status)))
      return { ...p, isCompleted }
    })
  })

  const mySpecificTasks = computed(() => {
    const activeProjIds = new Set(
      allProjectsBase.value.filter(p => !p.isCompleted).map(p => p.proj_id)
    )
    return allTasks.value.filter(
      t => activeProjIds.has(t.proj_id) && canEditTaskItem(t, t.proj_id)
    )
  })

  const statCards = computed(() => [
    { title: 'My Total Tasks', value: mySpecificTasks.value.length, color: 'primary' },
    {
      title: TASK_STATUS.IN_PROGRESS,
      value: mySpecificTasks.value.filter(t => t.status === TASK_STATUS.IN_PROGRESS).length,
      color: 'blue',
    },
    {
      title: TASK_STATUS.COMPLETED,
      value: mySpecificTasks.value.filter(t => t.status === TASK_STATUS.COMPLETED).length,
      color: 'green',
    },
    { title: 'Overdue', value: mySpecificTasks.value.filter(t => isOverdue(t)).length, color: 'orange' },
  ])

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
        || t.task_code?.toLowerCase().includes(q)
        || t.proj_name?.toLowerCase().includes(q)
      const matchStatus = filterStatus.value === FILTER_ALL || t.status === filterStatus.value
      const matchPriority = filterPriority.value === FILTER_ALL || t.priority === filterPriority.value
      return matchSearch && matchStatus && matchPriority
    })
  })

  const visibleProjects = computed(() => {
    const map = new Map()
    filteredTasks.value.forEach(task => {
      if (!map.has(task.proj_id)) {
        const baseProj = allProjectsBase.value.find(p => p.proj_id === task.proj_id)
        map.set(task.proj_id, { ...baseProj, tasks: [] })
      }
      map.get(task.proj_id).tasks.push(task)
    })
    return [...map.values()].sort((a, b) => a.proj_name.localeCompare(b.proj_name))
  })

  async function loadCommentCounts(tasks) {
    await Promise.all(tasks.map(async t => {
      try {
        const res = await api.get(`/task/${t.task_id}/comments`)
        const list = res?.data ?? []
        if (list.length) commentCounts.value[t.task_id] = list.length
        else delete commentCounts.value[t.task_id]
      } catch { /* ignore */ }
    }))
  }

  async function applyHighlight() {
    const taskId = Number(route.query.highlight)
    const projId = Number(route.query.proj)
    if (!taskId) return

    const task = allTasks.value.find(t => t.task_id === taskId)
    const targetProj = projId || task?.proj_id
    if (!targetProj) return

    showAllStages.value = true

    const proj = allProjectsBase.value.find(p => p.proj_id === targetProj)
    if (proj?.isCompleted) showCompletedProjects.value = true

    if (!expandedProjects.value.includes(targetProj)) {
      expandedProjects.value.push(targetProj)
    }

    highlightTaskId.value = taskId

    await nextTick()
    setTimeout(() => {
      document.querySelector('.row-highlight')
        ?.scrollIntoView({ behavior: 'smooth', block: 'center' })
    }, 350)

    setTimeout(() => { highlightTaskId.value = null }, 4000)
  }

  async function loadComments(taskId) {
    loadingComments.value = true
    try {
      const res = await api.get(`/task/${taskId}/comments`)
      comments.value = res?.data ?? (Array.isArray(res) ? res : [])
      if (comments.value.length) commentCounts.value[taskId] = comments.value.length
      else delete commentCounts.value[taskId]
    } catch {
      comments.value = []
    } finally {
      loadingComments.value = false
    }
  }

  async function postComment() {
    const body = newComment.value.trim()
    if (!body || !selectedTask.value) return
    postingComment.value = true
    try {
      const taskId = selectedTask.value.task_id
      const res = await api.post(`/task/${taskId}/comments`, { body })
      if (res?.success && res.data) {
        comments.value.push(res.data)
        commentCounts.value[taskId] = comments.value.length
        newComment.value = ''
        mentionOpen.value = false
      } else {
        showSnack(res?.message || 'Failed to post comment', 'error')
      }
    } catch {
      showSnack('Failed to post comment', 'error')
    } finally {
      postingComment.value = false
    }
  }

  async function removeComment(c) {
    try {
      const res = await api.delete(`/task/comments/${c.comment_id}`)
      if (res?.success) {
        comments.value = comments.value.filter(x => x.comment_id !== c.comment_id)
        if (comments.value.length) commentCounts.value[c.task_id] = comments.value.length
        else delete commentCounts.value[c.task_id]
      } else {
        showSnack(res?.message || 'Failed to delete', 'error')
      }
    } catch {
      showSnack('Failed to delete', 'error')
    }
  }

  function canDeleteComment(c) {
    const uid = currentUser.value?.user_id
    return c.user_id === uid || authStore.isAdminOrManager
  }

  function renderSegments(body) {
    const parts = String(body ?? '').split(/(@[A-Za-z0-9._-]+)/g)
    return parts
      .filter(p => p !== '')
      .map(p => ({ text: p, mention: /^@[A-Za-z0-9._-]+$/.test(p) }))
  }

  function initials(name) {
    if (!name) return '?'
    return name.split(' ').map(n => n[0]).join('').toUpperCase().substring(0, 2)
  }

  async function loadMentionableUsers(taskId) {
    try {
      const res = await api.get(`/task/${taskId}/mentionable-users`)
      mentionableUsers.value = res?.data ?? []
    } catch {
      mentionableUsers.value = [{ username: 'all', full_name: 'Everyone on this project' }]
    }
  }

  const mentionMatches = computed(() => {
    const q = mentionQuery.value.toLowerCase()
    if (!q) return mentionableUsers.value.slice(0, 8)

    const starts = []
    const contains = []
    for (const u of mentionableUsers.value) {
      const name = u.username.toLowerCase()
      const full = (u.full_name ?? '').toLowerCase()
      if (name.startsWith(q)) starts.push(u)
      else if (name.includes(q) || full.includes(q)) contains.push(u)
    }
    return [...starts, ...contains].slice(0, 8)
  })

  function onCommentInput(val) {
    const match = /@([A-Za-z0-9._-]*)$/.exec(val ?? '')
    if (!match) {
      mentionOpen.value = false
      mentionQuery.value = ''
      return
    }
    mentionQuery.value = match[1]
    mentionIndex.value = 0
    mentionOpen.value = mentionMatches.value.length > 0
  }

  function moveMention(step) {
    if (!mentionOpen.value || !mentionMatches.value.length) return
    const n = mentionMatches.value.length
    mentionIndex.value = (mentionIndex.value + step + n) % n
  }

  function onMentionEnter(e) {
    if (!mentionOpen.value || !mentionMatches.value.length) return
    e.preventDefault()
    applyMention(mentionMatches.value[mentionIndex.value].username)
  }

  function closeMentionSoon() {
    setTimeout(() => { mentionOpen.value = false }, 150)
  }

  function applyMention(username) {
    newComment.value = newComment.value.replace(/@([A-Za-z0-9._-]*)$/, `@${username} `)
    mentionOpen.value = false
    mentionQuery.value = ''
    mentionIndex.value = 0
  }

  function deriveStageFromCode(code) {
    if (!code) return null
    const m = code.match(/^(\d+)\.\d+$/)
    return m ? `${m[1]}.0` : null
  }

  function stageIdOf(task) {
    return task.stage_id || deriveStageFromCode(task.task_code) || DEFAULT_STAGE_ID
  }

  function getCurrentStageId(tasks) {
    const stageOrder = (settingsStore.stages ?? [])
      .map(s => s.stage_id)
      .sort((a, b) => parseFloat(a) - parseFloat(b))

    for (const sid of stageOrder) {
      const st = tasks.filter(t => stageIdOf(t) === sid)
      if (st.length && !st.every(t => t.status === TASK_STATUS.COMPLETED)) return sid
    }
    return stageOrder.at(-1) ?? DEFAULT_STAGE_ID
  }

  function getStageGroups(project) {
    const groups = new Map()
    const currentStage = getCurrentStageId(project.tasks)
    const visible = showAllStages.value || project.isCompleted
      ? project.tasks
      : project.tasks.filter(t => stageIdOf(t) === currentStage)

    visible.forEach(task => {
      const sid = stageIdOf(task)
      if (!groups.has(sid)) {
        groups.set(sid, { stageId: sid, stageName: settingsStore.getStageName(sid), tasks: [] })
      }
      groups.get(sid).tasks.push({ ...task, stage_id: sid })
    })

    return [...groups.entries()]
      .sort(([a], [b]) => parseFloat(a) - parseFloat(b))
      .map(([, v]) => v)
  }

  function stagePct(tasks) {
    if (!tasks.length) return 0
    return (tasks.filter(t => t.status === TASK_STATUS.COMPLETED).length / tasks.length) * 100
  }

  function toggleProject(projId) {
    const i = expandedProjects.value.indexOf(projId)
    i > -1 ? expandedProjects.value.splice(i, 1) : expandedProjects.value.push(projId)
  }
  function isExpanded(projId) { return expandedProjects.value.includes(projId) }

  function getStageColor(sid) { return STAGE_COLORS[sid] ?? DEFAULT_COLOR }
  function getStatusColor(s) { return TASK_STATUS_COLORS[s] ?? DEFAULT_COLOR }
  function getStatusIcon(s) { return TASK_STATUS_ICONS[s] ?? 'mdi-circle-outline' }
  function getPriorityColor(p) { return PRIORITY_COLORS[p] ?? DEFAULT_COLOR }

  function getProgressColor(v) {
    if (v >= 100) return 'success'
    if (v >= 50) return 'primary'
    if (v > 0) return 'warning'
    return DEFAULT_COLOR
  }

  function isOverdue(task) {
    if (TASK_CLOSED_STATUSES.includes(task.status) || !task.planned_end_date) return false
    const end = new Date(task.planned_end_date); end.setHours(0, 0, 0, 0)
    const today = new Date(); today.setHours(0, 0, 0, 0)
    return end < today
  }

  function today() {
    return new Date().toISOString().split('T')[0]
  }

  async function loadTasks() {
    loading.value = true
    try {
      const result = await api.get('/task/my-tasks')
      allTasks.value = result?.data ?? (Array.isArray(result) ? result : [])

      if (allTasks.value.length && expandedProjects.value.length === 0) {
        const inProgress = [...allTasks.value]
          .filter(t => t.status === TASK_STATUS.IN_PROGRESS)
          .sort((a, b) => new Date(b.planned_start_date) - new Date(a.planned_start_date))[0]
        const targetId = inProgress?.proj_id ?? allTasks.value[0]?.proj_id
        if (targetId) expandedProjects.value = [targetId]
      }

      const uniqueProjIds = [...new Set(allTasks.value.map(t => t.proj_id))]
      await Promise.all(uniqueProjIds.map(id => authStore.fetchProjectRole(id)))

      await loadCommentCounts(allTasks.value)
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

  async function updateStatus(taskItemProxy, newStatus, projId) {
    if (!canEditTaskItem(taskItemProxy, projId)) {
      showSnack('You are not authorised to modify this task.', 'error')
      return
    }

    const realTask = allTasks.value.find(t => t.task_id === taskItemProxy.task_id)
    if (!realTask) return

    const projTasks = allTasks.value.filter(t => t.proj_id === projId)

    const oldCurrentStage = getCurrentStageId(
      projTasks.map(t => ({ ...t }))
    )

    const oldStatus = realTask.status
    const oldProgress = realTask.per_complete

    realTask.status = newStatus
    if (newStatus === TASK_STATUS.COMPLETED) {
      realTask.per_complete = 100
      realTask.actual_end_date = realTask.actual_end_date || today()
    } else if (newStatus === TASK_STATUS.IN_PROGRESS) {
      if (realTask.per_complete === 0) realTask.per_complete = 10
      realTask.actual_start_date = realTask.actual_start_date || today()
    } else if (newStatus === TASK_STATUS.NOT_STARTED) {
      realTask.per_complete = 0
    }

    try {
      const result = await api.put(`/task/${realTask.task_id}/status`, { status: newStatus })
      if (result?.success) {
        showSnack('Status updated', 'success')

        const newCurrentStage = getCurrentStageId(projTasks)
        if (
          oldCurrentStage !== newCurrentStage &&
          parseFloat(newCurrentStage) > parseFloat(oldCurrentStage)
        ) {
          await autoStartStageTasks(projId, newCurrentStage)
        }
      } else {
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

  async function autoStartStageTasks(projId, newStageId) {
    const projTasks = allTasks.value.filter(t => t.proj_id === projId)

    const tasksToStart = projTasks.filter(t =>
      stageIdOf(t) === newStageId && t.status === TASK_STATUS.NOT_STARTED
    )

    if (tasksToStart.length === 0) return

    let successCount = 0

    await Promise.all(tasksToStart.map(async (t) => {
      const prevStatus = t.status
      const prevProgress = t.per_complete

      t.status = TASK_STATUS.IN_PROGRESS
      t.per_complete = 10
      t.actual_start_date = t.actual_start_date || today()

      try {
        const res = await api.put(`/task/${t.task_id}/status`, { status: TASK_STATUS.IN_PROGRESS })
        if (res?.success) {
          successCount++
        } else {
          t.status = prevStatus
          t.per_complete = prevProgress
        }
      } catch (e) {
        t.status = prevStatus
        t.per_complete = prevProgress
        console.error(`Error auto-starting task ${t.task_id}:`, e)
      }
    }))

    if (successCount > 0) {
      showSnack(
        `Stage ${newStageId} started: ${successCount} task(s) auto-moved to ${TASK_STATUS.IN_PROGRESS}.`,
        'info'
      )
    }
  }

  function openUpload(task, project) {
    selectedTask.value = task
    selectedProject.value = project
    uploadFiles.value = []
    uploadDescription.value = ''
    uploadDialog.value = true
  }

  function closeUpload() {
    uploadDialog.value = false
    uploadFiles.value = []
    uploadDescription.value = ''
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

      const count = uploadFiles.value.length
      const taskId = selectedTask.value.task_id
      const result = await api.uploadFile('/file/upload', fd)
      if (result?.success) {
        showSnack(`${count} file(s) uploaded`, 'success')
        closeUpload()
        if (detailsDialog.value && selectedTask.value?.task_id === taskId) {
          await loadTaskDocuments(taskId)
        }
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
    try {
      const result = await api.get(`/file/by-task/${task.task_id}`)
      taskDocs.value = result?.data ?? []
    } catch {
      taskDocs.value = []
      showSnack('Failed to load documents', 'error')
    }
  }

  async function loadTaskDocuments(taskId) {
    try {
      const result = await api.get(`/file/by-task/${taskId}`)
      detailDocs.value = result?.data ?? []
    } catch {
      detailDocs.value = []
    }
  }

  function downloadDoc(doc) { window.open(`/api/file/download/${doc.file_id}`, '_blank') }
  function switchToUpload() { docsDialog.value = false; openUpload(selectedTask.value, selectedProject.value) }
  function confirmDeleteDoc(doc) { docToDelete.value = doc; deleteDialog.value = true }

  async function doDelete() {
    deleting.value = true
    try {
      const fileId = docToDelete.value.file_id
      const result = await api.delete(`/file/${fileId}`)
      if (result?.success) {
        taskDocs.value = taskDocs.value.filter(d => d.file_id !== fileId)
        detailDocs.value = detailDocs.value.filter(d => d.file_id !== fileId)
        deleteDialog.value = false
        showSnack('Document deleted', 'success')
      } else {
        showSnack(result?.message || 'Failed to delete', 'error')
      }
    } catch {
      showSnack('Failed to delete', 'error')
    } finally {
      deleting.value = false
    }
  }

  async function openDetails(task, project) {
    selectedTask.value = task
    selectedProject.value = project
    detailsDialog.value = true

    comments.value = []
    newComment.value = ''
    detailDocs.value = []
    mentionOpen.value = false
    mentionQuery.value = ''

    await Promise.all([
      loadComments(task.task_id),
      loadMentionableUsers(task.task_id),
      loadTaskDocuments(task.task_id),
    ])
  }

  function showSnack(msg, color = 'success') {
    snackbarMsg.value = msg
    snackbarColor.value = color
    snackbar.value = true
  }

  onMounted(async () => {
    const templateResult = await settingsStore.fetchTaskTemplates()
    if (!templateResult?.success) {
      showSnack('Warning: could not load stage definitions', 'warning')
    }
    await loadTasks()
    applyHighlight()
  })
</script>


<style scoped>
  @keyframes row-flash {
    0% {
      background-color: rgba(25, 118, 210, 0.28);
    }

    100% {
      background-color: transparent;
    }
  }

  .row-highlight {
    animation: row-flash 2.5s ease-out;
  }

  .dashboard-root {
    background: #f5f6f8;
    height: 100vh !important;
    overflow: hidden !important;
  }

  .dashboard-body {
    min-height: 0;
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

  .stage-header {
    background-color: #fafafa;
    border-bottom: 1px solid rgba(0, 0, 0, 0.06);
  }

  .tasks-table :deep(th) {
    font-weight: 600 !important;
    background-color: #ffffff;
  }

  .comments-scroll {
    max-height: 240px;
    overflow-y: auto;
  }

  .comment-row + .comment-row {
    border-top: 1px solid rgba(0,0,0,0.06);
  }

  .comment-body {
    white-space: pre-wrap;
    word-break: break-word;
  }

  .mention {
    color: #1976D2;
    font-weight: 600;
  }

  .mention-wrap {
    position: relative;
  }

  .mention-list {
    position: absolute;
    top: 100%;
    left: 0;
    z-index: 20;
    min-width: 260px;
    max-width: 100%;
    margin-top: 4px;
    overflow: hidden;
  }

  .doc-description {
    white-space: pre-wrap;
    word-break: break-word;
  }

  .clickable-rows :deep(tbody tr) {
    cursor: pointer;
  }
</style>
