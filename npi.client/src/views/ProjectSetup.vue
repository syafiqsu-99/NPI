<template>
  <v-container fluid class="pa-4" style="padding-bottom: 80px !important;">
    <v-row>
      <v-col cols="12">
        <v-card elevation="2">
          <v-card-title class="bg-primary">
            <div class="d-flex align-center justify-space-between w-100">
              <div class="text-white">
                <v-icon class="mr-2">mdi-cog</v-icon>
                Project Setup — {{ project?.proj_no }}
              </div>
              <v-btn variant="text" color="white" @click="$router.push('/projects')">
                <v-icon start>mdi-arrow-left</v-icon>
                Back to Projects
              </v-btn>
            </div>
          </v-card-title>

          <v-card-text v-if="loading" class="text-center pa-8">
            <v-progress-circular indeterminate color="primary" size="64" />
          </v-card-text>

          <v-card-text v-else class="pa-6">
            <v-stepper v-model="step" elevation="0">
              <v-stepper-header>
                <v-stepper-item :complete="step > 1" :value="1" title="Project Info" />
                <v-divider />
                <v-stepper-item :complete="step > 2" :value="2" title="Team Assignment" />
                <v-divider />
                <v-stepper-item :complete="step > 3" :value="3" title="NPI Stages" />
                <v-divider />
                <v-stepper-item :value="4" title="Review & Launch" />
              </v-stepper-header>

              <v-stepper-window>

                <!-- ── Step 1: Project Info ─────────────────────────────────── -->
                <v-stepper-window-item :value="1">
                  <v-card flat>
                    <v-card-text>
                      <h3 class="text-h6 mb-4">Project Information</h3>
                      <v-row dense>
                        <v-col cols="12" md="6" class="py-1">
                          <v-text-field v-model="project.proj_name"
                                        label="Project Name"
                                        variant="outlined"
                                        density="compact"
                                        hide-details
                                        :rules="[v => !!v || 'Project name is required']" />
                        </v-col>
                        <v-col cols="12" md="6" class="py-1">
                          <v-text-field v-model="project.proj_no"
                                        label="Project Number"
                                        readonly
                                        variant="outlined"
                                        density="compact"
                                        hide-details />
                        </v-col>
                        <v-col cols="12" md="6" class="py-1">
                          <v-select v-model="project.priority"
                                    :items="priorityOptions"
                                    label="Priority"
                                    variant="outlined"
                                    density="compact"
                                    hide-details />
                        </v-col>
                        <v-col cols="12" md="6" class="py-1">
                          <v-text-field v-model="project.status"
                                        label="Status"
                                        readonly
                                        variant="outlined"
                                        density="compact"
                                        hide-details />
                        </v-col>
                        <v-col cols="12" class="py-1">
                          <v-textarea v-model="project.description"
                                      label="Description"
                                      rows="3"
                                      variant="outlined"
                                      density="compact"
                                      hide-details />
                        </v-col>
                      </v-row>
                    </v-card-text>
                  </v-card>
                </v-stepper-window-item>

                <!-- ── Step 2: Team Assignment ─────────────────────────────── -->
                <v-stepper-window-item :value="2">
                  <v-card flat>
                    <v-card-text>
                      <div class="d-flex align-center justify-space-between mb-4">
                        <h3 class="text-h6">Assign Project Team</h3>
                        <v-btn color="primary" variant="flat" size="small" @click="addTeamRow">
                          <v-icon start>mdi-account-plus</v-icon>
                          Add Row
                        </v-btn>
                      </div>

                      <div v-if="teamMembers.length === 0"
                           class="text-center text-medium-emphasis py-8 border rounded">
                        <v-icon size="48" color="grey-lighten-2" class="d-block mx-auto mb-2">
                          mdi-account-group-outline
                        </v-icon>
                        Click "Add Row" to add team members.
                      </div>

                      <v-table v-else density="compact" class="team-inline-table">
                        <thead>
                          <tr>
                            <th class="team-col-dept">Department</th>
                            <th class="team-col-user">User</th>
                            <th class="team-col-role">Project Role</th>
                            <th class="team-col-del"></th>
                          </tr>
                        </thead>
                        <tbody>
                          <tr v-for="(member, idx) in teamMembers" :key="member._rowId">
                            <!-- Department selector -->
                            <td>
                              <v-select v-model="member.dept_id"
                                        :items="departments"
                                        item-title="dept_name"
                                        item-value="dept_id"
                                        placeholder="Select department"
                                        variant="outlined"
                                        density="compact"
                                        hide-details
                                        @update:model-value="onTeamDeptChange(member)" />
                            </td>

                            <!-- User selector (filtered by dept) -->
                            <td>
                              <v-select v-model="member.user_id"
                                        :items="getUsersForDept(member.dept_id)"
                                        item-title="username"
                                        item-value="user_id"
                                        placeholder="Select user"
                                        variant="outlined"
                                        density="compact"
                                        hide-details
                                        :disabled="!member.dept_id"
                                        @update:model-value="onTeamUserChange(member)" />
                            </td>

                            <!-- Project role selector -->
                            <td>
                              <v-select v-model="member.role_in_project"
                                        :items="projectRoleOptions"
                                        placeholder="Role"
                                        variant="outlined"
                                        density="compact"
                                        hide-details />
                            </td>

                            <!-- Remove row -->
                            <td class="text-center">
                              <v-btn icon size="x-small" variant="text" color="error"
                                     @click="removeTeamMember(member._rowId)">
                                <v-icon size="15">mdi-delete</v-icon>
                              </v-btn>
                            </td>
                          </tr>
                        </tbody>
                      </v-table>
                    </v-card-text>
                  </v-card>
                </v-stepper-window-item>

                <!-- ── Step 3: NPI Stages ──────────────────────────────────── -->
                <v-stepper-window-item :value="3">
                  <v-card flat>
                    <v-card-text>
                      <h3 class="text-h6 mb-2">NPI Stages & Tasks</h3>

                      <!-- Optional stage toggles -->
                      <v-card variant="outlined" class="mb-5 pa-4 bg-grey-lighten-5">
                        <div class="text-subtitle-2 font-weight-bold mb-3">
                          <v-icon size="small" class="mr-1">mdi-tune</v-icon>
                          Optional Stages
                        </div>
                        <v-switch v-for="s in optionalStages"
                                  :key="s.stage_id"
                                  v-model="stageFlags[getStageFlagKey(s.stage_id)]"
                                  :label="s.stage_name"
                                  :color="getStageColor(s.stage_id)"
                                  @update:model-value="val => onStageToggle(s.stage_id, val)" />
                      </v-card>

                      <v-alert type="info" variant="tonal" class="mb-4">
                        Tasks are pre-filled from the NPI template. You can
                        <strong>add, edit, or delete</strong> tasks within each stage.
                        Set <strong>Start Date</strong> and <strong>Working Days</strong> —
                        End Date is calculated automatically (excluding weekends &amp; Malaysian
                        public holidays).
                        <span v-if="!isFirstLaunch">
                          Add a <strong>Revision Note</strong> when changing existing task dates.
                        </span>
                      </v-alert>

                      <!-- Per-stage expansion panels -->
                      <v-expansion-panels v-model="openPanels" multiple>
                        <v-expansion-panel v-for="stageId in activeStageIds"
                                           :key="stageId">
                          <v-expansion-panel-title>
                            <div class="d-flex align-center w-100 ga-2">
                              <v-chip :color="getStageColor(stageId)"
                                      size="small"
                                      variant="tonal"
                                      class="font-weight-bold"
                                      style="min-width:44px; justify-content:center">
                                {{ stageId }}
                              </v-chip>
                              <span class="font-weight-medium">{{ getStageName(stageId) }}</span>
                              <v-chip v-if="!isStageRequired(stageId)"
                                      size="x-small"
                                      color="orange"
                                      variant="outlined">
                                Optional
                              </v-chip>
                              <v-spacer />
                              <span class="text-caption text-grey mr-2">
                                {{ getStageTaskCount(stageId) }} tasks
                              </span>
                              <v-btn size="x-small"
                                     variant="outlined"
                                     :color="getStageColor(stageId)"
                                     @click.stop="addTaskToStage(stageId)">
                                <v-icon size="13" start>mdi-plus</v-icon>
                                Add Task
                              </v-btn>
                            </div>
                          </v-expansion-panel-title>

                          <v-expansion-panel-text class="pa-0">
                            <div style="width:100%; overflow-x:hidden;">
                              <v-table class="tasks-table" density="compact" style="width:100%;">
                                <thead>
                                  <tr>
                                    <th class="col-code">Code</th>
                                    <th class="col-task">Task</th>
                                    <th class="col-dept">Department</th>
                                    <th class="col-date">Start Date</th>
                                    <th class="col-days">Working Days</th>
                                    <th class="col-enddate">End Date</th>
                                    <th v-if="!isFirstLaunch" class="col-note">
                                      Revision Note <span class="text-error">*</span>
                                    </th>
                                    <th class="col-del"></th>
                                  </tr>
                                </thead>
                                <tbody>
                                  <tr v-for="(task, tIdx) in getStageTaskRows(stageId)"
                                      :key="task._localId"
                                      :class="{
                                      'task-row--stage0': isAutoCompleteStage(stageId) && !!task.task_id
                                    }">
                                    <td class="text-center">
                                      <v-chip size="x-small"
                                              :color="getStageColor(stageId)"
                                              variant="tonal">
                                        {{ task.task_code || `${stageId}.${tIdx + 1}` }}
                                      </v-chip>
                                    </td>

                                    <!-- Title -->
                                    <td>
                                      <v-text-field v-model="task.title"
                                                    placeholder="Task name…"
                                                    variant="outlined"
                                                    density="compact"
                                                    hide-details
                                                    :disabled="isStage0DbTask(task)" />
                                    </td>

                                    <!-- Department -->
                                    <td>
                                      <v-select v-model="task.dept_id"
                                                :items="departments"
                                                item-title="dept_name"
                                                item-value="dept_id"
                                                placeholder="Select"
                                                variant="outlined"
                                                density="compact"
                                                hide-details
                                                :disabled="isStage0DbTask(task)" />
                                    </td>

                                    <!-- Start Date -->
                                    <td>
                                      <v-text-field v-model="task.start_date"
                                                    type="date"
                                                    variant="outlined"
                                                    density="compact"
                                                    hide-details
                                                    :disabled="isStage0DbTask(task)"
                                                    @update:model-value="onStartDateChange(task)" />
                                    </td>

                                    <!-- Working Days -->
                                    <td>
                                      <v-text-field v-model.number="task.working_days"
                                                    type="number"
                                                    min="1"
                                                    variant="outlined"
                                                    density="compact"
                                                    hide-details
                                                    :disabled="isStage0DbTask(task)"
                                                    @update:model-value="onWorkingDaysChange(task)" />
                                    </td>

                                    <!-- End Date (computed) -->
                                    <td class="text-center">
                                      <span v-if="task.computing"
                                            class="text-caption text-medium-emphasis">
                                        <v-progress-circular indeterminate size="12" width="2" />
                                      </span>
                                      <span v-else
                                            class="end-date-value"
                                            :class="{ 'end-date-value--set': !!task.end_date }">
                                        {{ task.end_date ? formatDateShort(task.end_date) : '—' }}
                                      </span>
                                    </td>

                                    <!-- Revision Note (edit mode only) -->
                                    <td v-if="!isFirstLaunch">
                                      <v-text-field v-model="task.revision_note"
                                                    placeholder="Why changed?"
                                                    variant="outlined"
                                                    density="compact"
                                                    hide-details
                                                    :disabled="!task.task_id || !task.has_date_change" />
                                    </td>

                                    <!-- Delete row -->
                                    <td class="text-center">
                                      <v-btn v-if="!isStage0DbTask(task)"
                                             icon
                                             size="x-small"
                                             variant="text"
                                             color="error"
                                             title="Remove task"
                                             @click="removeTask(stageId, tIdx)">
                                        <v-icon size="15">mdi-delete</v-icon>
                                      </v-btn>
                                    </td>
                                  </tr>

                                  <!-- Empty state row -->
                                  <tr v-if="getStageTaskRows(stageId).length === 0">
                                    <td :colspan="isFirstLaunch ? 7 : 8"
                                        class="text-center pa-4 text-medium-emphasis text-caption">
                                      No tasks yet —
                                      <span class="text-primary"
                                            style="cursor:pointer"
                                            @click="addTaskToStage(stageId)">
                                        add the first task
                                      </span>
                                    </td>
                                  </tr>
                                </tbody>
                              </v-table>
                            </div>
                          </v-expansion-panel-text>
                        </v-expansion-panel>
                      </v-expansion-panels>
                    </v-card-text>
                  </v-card>
                </v-stepper-window-item>

                <!-- ── Step 4: Review & Launch ─────────────────────────────── -->
                <v-stepper-window-item :value="4">
                  <v-card flat>
                    <v-card-text>
                      <h3 class="text-h6 mb-4">
                        {{ isFirstLaunch ? 'Review & Launch Project' : 'Review & Save Changes' }}
                      </h3>

                      <v-alert :type="isFirstLaunch ? 'success' : 'info'"
                               variant="tonal"
                               class="mb-4">
                        <span v-if="isFirstLaunch">
                          Review the summary below. Once launched, status changes to
                          <strong>In Progress</strong> and the Gantt becomes available.
                        </span>
                        <span v-else>
                          Changes to task dates are recorded with revision history. Ensure all
                          revision notes are filled before saving.
                        </span>
                      </v-alert>

                      <v-row dense class="mb-4">
                        <!-- Active Stages summary -->
                        <v-col cols="12" md="6" class="d-flex">
                          <v-card variant="outlined" class="w-100 d-flex flex-column">
                            <v-card-title class="text-subtitle-1">
                              <v-icon start>mdi-layers</v-icon>
                              Active NPI Stages
                            </v-card-title>
                            <v-card-text class="flex-grow-1">
                              <div v-for="sId in activeStageIds"
                                   :key="sId"
                                   class="d-flex align-center mb-2">
                                <v-chip :color="getStageColor(sId)"
                                        size="small"
                                        variant="tonal"
                                        class="mr-2">
                                  {{ sId }}
                                </v-chip>
                                <span class="text-body-2">{{ getStageName(sId) }}</span>
                                <v-spacer />
                                <span class="text-caption text-grey">
                                  {{ getStageTaskCount(sId) }} tasks
                                </span>
                              </div>
                            </v-card-text>
                          </v-card>
                        </v-col>

                        <!-- Folder Structure preview -->
                        <v-col cols="12" md="6" class="d-flex">
                          <v-card variant="outlined" class="w-100 d-flex flex-column">
                            <v-card-title class="text-subtitle-1">
                              <v-icon start>mdi-folder-multiple</v-icon>
                              Folder Structure Preview
                            </v-card-title>
                            <v-card-text class="flex-grow-1">
                              <div class="text-caption text-grey mb-3">
                                One folder per department under the project root.
                              </div>
                              <div class="folder-root mb-1">
                                <v-icon size="16" color="primary">mdi-folder</v-icon>
                                <code class="ml-1">
                                  projects/{{ sanitizeFolderName(project.proj_name) }}/
                                </code>
                              </div>
                              <v-list density="compact" class="folder-preview pl-4">
                                <v-list-item v-for="dept in folderPreviewDepts"
                                             :key="dept.dept_id"
                                             density="compact">
                                  <template #prepend>
                                    <v-icon size="16" color="amber-darken-2">mdi-folder</v-icon>
                                  </template>
                                  <v-list-item-title class="text-body-2">
                                    {{ sanitizeFolderName(dept.dept_name) }}/
                                  </v-list-item-title>
                                  <template #append>
                                    <span class="text-caption text-medium-emphasis">
                                      {{ deptTaskCount(dept.dept_id) }}
                                      task{{ deptTaskCount(dept.dept_id) !== 1 ? 's' : '' }}
                                    </span>
                                  </template>
                                </v-list-item>
                                <v-list-item v-if="folderPreviewDepts.length === 0"
                                             density="compact">
                                  <v-list-item-title class="text-caption text-medium-emphasis">
                                    No departments assigned yet
                                  </v-list-item-title>
                                </v-list-item>
                              </v-list>
                            </v-card-text>
                          </v-card>
                        </v-col>
                      </v-row>

                      <v-row dense>
                        <v-col cols="12" md="6" class="d-flex">
                          <v-card variant="outlined" class="w-100 d-flex flex-column">
                            <v-card-title class="text-subtitle-1">
                              <v-icon start>mdi-account-group</v-icon>
                              Team Summary
                            </v-card-title>
                            <v-card-text class="flex-grow-1">
                              <div class="mb-2">
                                <strong>Total Members:</strong> {{ teamMembers.length }}
                              </div>
                              <div v-for="member in teamMembers" :key="member.user_id" class="text-caption">
                                • {{ member.user_name }} ({{ member.dept_name }}) —
                                {{ member.role_in_project }}
                              </div>
                            </v-card-text>
                          </v-card>
                        </v-col>

                        <v-col cols="12" md="6" class="d-flex">
                          <v-card variant="outlined" class="w-100 d-flex flex-column">
                            <v-card-title class="text-subtitle-1">
                              <v-icon start>mdi-clipboard-list</v-icon>
                              Tasks Summary
                            </v-card-title>
                            <v-card-text class="flex-grow-1">
                              <div>
                                <strong>Total Tasks:</strong> {{ allTasks.length }}
                              </div>
                              <div><strong>Active Stages:</strong> {{ activeStageIds.length }}</div>
                              <div>
                                <strong>Est. Duration:</strong> {{ totalDuration }} working days
                              </div>
                            </v-card-text>
                          </v-card>
                        </v-col>
                      </v-row>
                    </v-card-text>
                  </v-card>
                </v-stepper-window-item>
              </v-stepper-window>
            </v-stepper>
          </v-card-text>
        </v-card>
      </v-col>
    </v-row>

    <!-- Folder Warning Dialog -->
    <v-dialog v-model="showFolderWarningDialog" max-width="640" persistent>
      <v-card>
        <v-card-title class="bg-warning text-white">
          <v-icon class="mr-2">mdi-folder-alert</v-icon>
          Folders Could Not Be Deleted
        </v-card-title>
        <v-card-text class="pa-6">
          <v-alert type="warning" variant="tonal" class="mb-4">
            Project saved successfully, but the following folders were
            <strong>not deleted</strong> because they still contain files.
          </v-alert>
          <v-list lines="two" class="border rounded mb-4">
            <v-list-item v-for="(folder, i) in folderWarnings"
                         :key="i"
                         :title="folder"
                         subtitle="Contains files — please clear manually">
              <template #prepend>
                <v-icon color="warning">mdi-folder-open</v-icon>
              </template>
            </v-list-item>
          </v-list>
        </v-card-text>
        <v-card-actions class="pa-4">
          <v-spacer />
          <v-btn color="warning" variant="elevated" @click="acknowledgeFolderWarnings">
            <v-icon start>mdi-check</v-icon>
            Understood – Continue
          </v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>

    <!-- Snackbar -->
    <v-snackbar v-model="snackbar"
                :color="snackbarColor"
                location="bottom right"
                rounded="lg">
      {{ snackbarMessage }}
      <template #actions>
        <v-btn variant="text" @click="snackbar = false">Close</v-btn>
      </template>
    </v-snackbar>
  </v-container>

  <!-- ── Sticky Footer Navigation ──────────────────────────────────────── -->
  <div class="sticky-nav-footer">
    <v-card elevation="4" class="rounded-0">
      <v-card-text class="pa-3">
        <div class="d-flex align-center justify-space-between">
          <!-- Left: Previous -->
          <v-btn v-if="step > 1"
                 variant="outlined"
                 color="grey-darken-1"
                 @click="step--">
            <v-icon start>mdi-arrow-left</v-icon>
            Previous
          </v-btn>
          <div v-else />
          <!-- Right: Next / Launch -->
          <div class="d-flex ga-2">
            <v-btn v-if="step < 4"
                   color="primary"
                   variant="flat"
                   @click="nextStep">
              Next
              <v-icon end>mdi-arrow-right</v-icon>
            </v-btn>
            <v-btn v-else
                   :color="isFirstLaunch ? 'success' : 'primary'"
                   :loading="launching"
                   variant="flat"
                   @click="launchProject">
              <v-icon start>
                {{ isFirstLaunch ? 'mdi-rocket-launch' : 'mdi-content-save' }}
              </v-icon>
              {{ isFirstLaunch ? 'Launch Project' : 'Save Changes' }}
            </v-btn>
          </div>
        </div>
      </v-card-text>
    </v-card>
  </div>
</template>

<script setup>
  import { ref, computed, onMounted } from 'vue'
  import { useRoute, useRouter } from 'vue-router'
  import { useProjectStore } from '@/stores/project'
  import { useSettingsStore } from '@/stores/setting'
  import { api } from '@/utils/api'
  import {
    PRIORITY_OPTIONS,
    PROJECT_ROLE_OPTIONS,
    PROJECT_STATUS,
    PROJECT_ROLES,
    STAGE_COLORS,
    OPTIONAL_STAGE_FLAGS,
    DEFAULT_PRIORITY,
    DEFAULT_WORKING_DAYS,
    DEFAULT_STAGE_ID,
    DEFAULT_COLOR,
    REDIRECT_DELAY_MS,
  } from '@/utils/constants'
  import {
    formatDateShort,
    formatDateForInput,
    sanitizeFolderName,
  } from '@/utils/formatters'

  const router = useRouter()
  const route = useRoute()
  const projectStore = useProjectStore()
  const settingsStore = useSettingsStore()

  const loading = ref(false)
  const launching = ref(false)
  const step = ref(1)

  const project = ref({
    proj_id: null,
    proj_no: '',
    proj_name: '',
    priority: DEFAULT_PRIORITY,
    status: PROJECT_STATUS.PLANNING,
    description: ''
  })

  const teamMembers = ref([])
  const stageFlags = ref({})
  const tasksByStage = ref({})
  const originalTaskDates = ref(new Map())

  const showFolderWarningDialog = ref(false)
  const folderWarnings = ref([])

  const snackbar = ref(false)
  const snackbarMessage = ref('')
  const snackbarColor = ref('success')

  const holidayCache = {}

  const priorityOptions = PRIORITY_OPTIONS
  const projectRoleOptions = PROJECT_ROLE_OPTIONS

  const departments = computed(() => settingsStore.assignableDepartments ?? [])
  const users = computed(() => settingsStore.users ?? [])
  const stages = computed(() => settingsStore.stages ?? [])

  const allStageIds = computed(() => stages.value.map(s => s.stage_id))

  const openPanels = computed(() => allStageIds.value.map((_, i) => i))

  const getStageColor = stageId => STAGE_COLORS[stageId] ?? DEFAULT_COLOR
  const getStageName = stageId => settingsStore.getStageName(stageId)
  const getStageFlagKey = stageId => OPTIONAL_STAGE_FLAGS[stageId] ?? null
  const isAutoCompleteStage = stageId => settingsStore.isAutoCompleteStage(stageId)
  const isStageRequired = stageId => settingsStore.isStageRequired(stageId)

  function isStage0DbTask(task) {
    return isAutoCompleteStage(task.stage_id) && !!task.task_id
  }

  const isFirstLaunch = computed(() => project.value.status === PROJECT_STATUS.PLANNING)

  const activeStageIds = computed(() =>
    allStageIds.value.filter(stageId => {
      if (isStageRequired(stageId)) return true
      const flagKey = getStageFlagKey(stageId)
      return flagKey ? !!stageFlags.value[flagKey] : false
    })
  )

  const optionalStages = computed(() =>
    stages.value.filter(s => !!OPTIONAL_STAGE_FLAGS[s.stage_id])
  )

  const allTasks = computed(() =>
    activeStageIds.value.flatMap(id => tasksByStage.value[id] || [])
  )

  const totalDuration = computed(() =>
    allTasks.value.reduce((sum, t) => sum + (Number(t.working_days) || 0), 0)
  )

  function resolveDeptIdByName(deptName) {
    if (!deptName) return null
    const match = (settingsStore.departments ?? []).find(
      d => d.dept_name?.toLowerCase() === deptName.toLowerCase()
    )
    return match?.dept_id ?? null
  }

  const folderPreviewDepts = computed(() => {
    const ids = new Set(
      allTasks.value
        .filter(t => !isAutoCompleteStage(t.stage_id) && t.dept_id)
        .map(t => t.dept_id)
    )
    return [...ids]
      .map(id => ({ dept_id: id, dept_name: settingsStore.getDeptName(id) }))
      .filter(d => d.dept_name)
      .sort((a, b) => a.dept_name.localeCompare(b.dept_name))
  })

  function deptTaskCount(deptId) {
    return allTasks.value.filter(
      t => t.dept_id === deptId && !isAutoCompleteStage(t.stage_id)
    ).length
  }

  function getStageTaskRows(stageId) {
    return tasksByStage.value[stageId] || []
  }

  function getStageTaskCount(stageId) {
    return getStageTaskRows(stageId).length
  }

  function getNextTaskCode(stageId) {
    const stagePrefix = stageId.split('.')[0]
    const existing = (tasksByStage.value[stageId] || [])
      .map(t => t.task_code)
      .filter(Boolean)
      .map(code => {
        const parts = code.split('.')
        return parts.length >= 2 ? parseInt(parts[1], 10) : 0
      })
      .filter(n => !isNaN(n))

    const max = existing.length > 0 ? Math.max(...existing) : 0
    return `${stagePrefix}.${max + 1}`
  }

  function onStageToggle(stageId, enabled) {
    if (enabled) {
      seedStage(stageId)
    } else {
      const hasSaved = (tasksByStage.value[stageId] || []).some(t => t.task_id)
      if (hasSaved) {
        showSnack(`Stage ${stageId} has saved tasks — cannot be removed once launched`, 'warning')
        const flagKey = getStageFlagKey(stageId)
        if (flagKey) stageFlags.value[flagKey] = true
        return
      }
      delete tasksByStage.value[stageId]
    }
  }

  function seedStage(stageId) {
    if (tasksByStage.value[stageId]?.length) return
    const today = new Date().toISOString().split('T')[0]
    const templates = settingsStore.getTemplatesForStage(stageId)

    tasksByStage.value[stageId] = templates.map((t, idx) => ({
      _localId: `${stageId}_${idx}_${Date.now()}`,
      task_id: null,
      stage_id: stageId,
      task_code: t.task_code,
      order: idx + 1,
      title: t.title,
      dept_id: t.dept_id ?? resolveDeptIdByName(t.dept_name),
      start_date: today,
      working_days: t.default_duration || DEFAULT_WORKING_DAYS,
      end_date: null,
      computing: false,
      revision_note: '',
      has_date_change: false,
      has_link: t.has_link || false,
    }))
  }

  function addTaskToStage(stageId) {
    const today = new Date().toISOString().split('T')[0]
    if (!tasksByStage.value[stageId]) tasksByStage.value[stageId] = []

    const newCode = getNextTaskCode(stageId)

    tasksByStage.value[stageId].push({
      _localId: `${stageId}_new_${Date.now()}`,
      task_id: null,
      stage_id: stageId,
      task_code: newCode,
      order: tasksByStage.value[stageId].length + 1,
      title: '',
      dept_id: null,
      start_date: today,
      working_days: DEFAULT_WORKING_DAYS,
      end_date: null,
      computing: false,
      revision_note: '',
      has_date_change: false,
      has_link: false,
    })
  }

  function removeTask(stageId, tIdx) {
    tasksByStage.value[stageId].splice(tIdx, 1)
  }

  function nextStep() {
    if (step.value === 2) {
      if (teamMembers.value.length === 0) {
        showSnack('Add at least one team member before continuing', 'warning')
        return
      }
      const incomplete = teamMembers.value.filter(m => !m.user_id || !m.dept_id)
      if (incomplete.length > 0) {
        showSnack('All team rows must have a department and user selected', 'warning')
        return
      }
    }
    step.value++
  }

  function removeTeamMember(rowId) {
    teamMembers.value = teamMembers.value.filter(m => m._rowId !== rowId)
  }

  async function onStartDateChange(task) {
    if (task.start_date && task.working_days) {
      task.computing = true
      task.end_date = await calculateEndDate(task.start_date, task.working_days)
      task.computing = false
    }
    checkDateChange(task)
  }

  async function onWorkingDaysChange(task) {
    if (task.start_date && task.working_days) {
      task.computing = true
      task.end_date = await calculateEndDate(task.start_date, task.working_days)
      task.computing = false
    }
    checkDateChange(task)
  }

  function checkDateChange(task) {
    if (!task.task_id || isFirstLaunch.value) {
      task.has_date_change = false
      return
    }
    const orig = originalTaskDates.value.get(task.task_id)
    if (!orig) {
      task.has_date_change = false
      return
    }
    task.has_date_change =
      task.start_date !== orig.start_date || task.end_date !== orig.end_date
  }

  function toMalaysiaDate(dateStr) {
    const [y, m, d] = dateStr.split('-').map(Number)
    return new Date(Date.UTC(y, m - 1, d))
  }

  async function calculateEndDate(startDate, workingDays) {
    if (!startDate || !workingDays || workingDays < 1) return null
    const start = toMalaysiaDate(startDate)
    const years = new Set()
    const est = new Date(start)
    est.setDate(est.getDate() + workingDays * 2)
    for (let y = start.getFullYear(); y <= est.getFullYear(); y++) years.add(y)
    const holidaySets = await Promise.all([...years].map(y => getHolidays(y)))
    let count = 0
    const cur = new Date(start)
    while (count < workingDays) {
      const day = cur.getDay()
      const iso = cur.toISOString().slice(0, 10)
      if (day !== 0 && day !== 6 && !holidaySets.some(s => s.has(iso))) count++
      if (count < workingDays) cur.setDate(cur.getDate() + 1)
    }
    return `${cur.getUTCFullYear()}-${String(cur.getUTCMonth() + 1).padStart(2, '0')}-${String(cur.getUTCDate()).padStart(2, '0')}`
  }

  async function getHolidays(year) {
    if (holidayCache[year]) return holidayCache[year]
    try {
      const res = await fetch(`https://sabah-holiday.dydxsoft.my/api/${year}.json`)
      if (!res.ok) {
        holidayCache[year] = new Set()
        return holidayCache[year]
      }
      const data = await res.json()
      const monthMap = {
        Jan: 0, Feb: 1, Mar: 2, Apr: 3, May: 4, Jun: 5,
        Jul: 6, Aug: 7, Sep: 8, Oct: 9, Nov: 10, Dec: 11
      }
      holidayCache[year] = new Set(
        data.map(h => {
          const [ms, ds] = h.date.split(' ')
          return new Date(year, monthMap[ms], parseInt(ds, 10)).toISOString().slice(0, 10)
        })
      )
    } catch {
      holidayCache[year] = new Set()
    }
    return holidayCache[year]
  }

  let teamRowSeq = 0

  function addTeamRow() {
    teamMembers.value.push({
      _rowId: `row_${Date.now()}_${teamRowSeq++}`,
      user_id: null,
      dept_id: null,
      user_name: '',
      dept_name: '',
      role_in_project: PROJECT_ROLES.MEMBER
    })
  }

  function getUsersForDept(deptId) {
    if (!deptId) return []
    return users.value.filter(u => Number(u.dept_id) === Number(deptId))
  }

  function onTeamDeptChange(member) {
    member.user_id = null
    member.user_name = ''
    member.dept_name = departments.value.find(d => Number(d.dept_id) === Number(member.dept_id))?.dept_name ?? ''
  }

  function onTeamUserChange(member) {
    const u = users.value.find(u => u.user_id === member.user_id)
    member.user_name = u?.username ?? ''
  }

  async function launchProject() {
    if (teamMembers.value.length === 0) {
      showSnack('Add at least one team member', 'warning')
      return
    }
    if (allTasks.value.length === 0) {
      showSnack('Add at least one task', 'warning')
      return
    }
    for (const task of allTasks.value) {
      if (isStage0DbTask(task)) continue
      if (!task.title || !task.dept_id || !task.start_date || !task.end_date) {
        showSnack(
          `Task "${task.task_code || task.title}" is missing required fields`,
          'warning'
        )
        return
      }
    }

    launching.value = true
    try {
      const transformedTasks = allTasks.value.map((task, idx) => ({
        task_id: task.task_id || null,
        stage_id: task.stage_id,
        task_code: task.task_code,
        order: idx + 1,
        title: task.title,
        dept_id: task.dept_id ?? null,
        dept_name: settingsStore.getDeptName(task.dept_id) || null,
        start_date: formatDateForInput(task.start_date),
        end_date: formatDateForInput(task.end_date),
        duration: parseFloat(task.working_days) || 0,
        revision_note: task.revision_note || null,
        has_link: task.has_link,
      }))

      const payload = {
        priority: project.value.priority,
        description: project.value.description,
        pilot_mould_required: stageFlags.value.pilot_mould_required,
        machine_purchase_required: stageFlags.value.machine_purchase_required,
        team_members: teamMembers.value.map(m => ({
          user_id: m.user_id,
          dept_id: m.dept_id,
          role: m.role_in_project,
          dept_name: m.dept_name,
          user_name: m.user_name
        })),
        tasks: transformedTasks
      }

      const result = await api.post(`/project/${route.params.id}/launch`, payload)
      const success = result?.success || result?.data?.success

      if (success) {
        const warnings = result?.data?.folder_warnings ?? result?.folder_warnings ?? []
        if (warnings.length > 0) {
          folderWarnings.value = warnings
          showFolderWarningDialog.value = true
        } else {
          showSnack(
            isFirstLaunch.value ? 'Project launched successfully!' : 'Project updated successfully!',
            'success'
          )
          setTimeout(() => router.push(`/projects/${route.params.id}/gantt`), 1500)
        }
      } else {
        showSnack(result?.message || 'Failed to launch project', 'error')
      }
    } catch (err) {
      console.error('Launch error:', err)
      showSnack(err?.message || 'Error launching project', 'error')
    } finally {
      launching.value = false
    }
  }

  function acknowledgeFolderWarnings() {
    showFolderWarningDialog.value = false
    folderWarnings.value = []
    router.push(`/projects/${route.params.id}/gantt`)
  }

  function showSnack(msg, color = 'success') {
    snackbarMessage.value = msg
    snackbarColor.value = color
    snackbar.value = true
  }

  onMounted(async () => {
    loading.value = true
    try {
      const [templateResult, deptResult, userResult] = await Promise.allSettled([
        settingsStore.fetchTaskTemplates(),
        settingsStore.fetchDepartments(),
        settingsStore.fetchUsers()
      ]).then(results => results.map(r => r.status === 'fulfilled' ? r.value : null))

      const failed = []
      const empty = []

      if (!templateResult?.success) failed.push('stage templates')
      if (!deptResult?.success) failed.push('departments')
      else if (departments.value.length === 0) empty.push('departments')
      if (!userResult?.success) failed.push('users')
      else if (users.value.length === 0) empty.push('users')

      if (failed.length > 0) {
        showSnack(
          `Could not load ${failed.join(', ')}. You may not have permission — contact an Admin.`,
          'error'
        )
      } else if (empty.length > 0) {
        showSnack(
          `No ${empty.join(' or ')} found. Add them under Settings before continuing.`,
          'warning'
        )
      }

      stageFlags.value = Object.fromEntries(
        optionalStages.value.map(s => [getStageFlagKey(s.stage_id), false])
      )

      const pr = await projectStore.fetchProjectById(route.params.id)
      if (pr?.success && pr?.data) {
        const d = pr.data
        project.value = {
          proj_id: d.proj_id,
          proj_no: d.proj_no || '',
          proj_name: d.proj_name || '',
          priority: d.priority || DEFAULT_PRIORITY,
          status: d.status || PROJECT_STATUS.PLANNING,
          description: d.description || ''
        }

        optionalStages.value.forEach(s => {
          const flagKey = getStageFlagKey(s.stage_id)
          if (flagKey) stageFlags.value[flagKey] = !!d[flagKey]
        })

        if (d.team_members) {
          teamMembers.value = d.team_members.map((m, i) => ({
            _rowId: `db_${m.user_id}_${i}`,
            user_id: m.user_id,
            dept_id: m.dept_id,
            user_name: m.username || m.user_name,
            dept_name: m.dept_name,
            role_in_project: m.role || m.role_in_project || PROJECT_ROLES.MEMBER
          }))
        }
      }

      allStageIds.value.forEach(id => {
        if (isStageRequired(id)) seedStage(id)
      })
      optionalStages.value.forEach(s => {
        const flagKey = getStageFlagKey(s.stage_id)
        if (flagKey && stageFlags.value[flagKey]) seedStage(s.stage_id)
      })

      const tr = await api.get(`/project/${route.params.id}/tasks`)
      const tasksData = tr?.data || tr || []
      if (tasksData.length > 0) {
        const stageGroups = {}
        tasksData.forEach(task => {
          const sid = task.stage_id || DEFAULT_STAGE_ID
          if (!stageGroups[sid]) stageGroups[sid] = []
          const startDate = formatDateForInput(task.planned_start_date)
          const endDate = formatDateForInput(task.planned_end_date)
          if (task.task_id) {
            originalTaskDates.value.set(task.task_id, { start_date: startDate, end_date: endDate })
          }
          stageGroups[sid].push({
            _localId: `${sid}_${task.task_id}_db`,
            task_id: task.task_id,
            stage_id: sid,
            task_code: task.task_code || '',
            order: task.order || 1,
            title: task.title || '',
            dept_id: task.dept_id ?? null,
            start_date: startDate,
            working_days: task.duration || 1,
            end_date: endDate,
            computing: false,
            revision_note: '',
            has_date_change: false,
            has_link: task.has_link || false,
          })
        })
        Object.keys(stageGroups).forEach(sid => {
          tasksByStage.value[sid] = stageGroups[sid]
        })

        const tasksNeedingEndDate = Object.values(tasksByStage.value)
          .flat()
          .filter(t => t.start_date && t.working_days && !t.end_date)
        if (tasksNeedingEndDate.length > 0) {
          await Promise.all(
            tasksNeedingEndDate.map(async t => {
              t.end_date = await calculateEndDate(t.start_date, t.working_days)
            })
          )
        }
      }
    } catch (err) {
      console.error('ProjectSetup mount error:', err)
      showSnack(err?.message || 'Error loading project setup', 'error')
    } finally {
      loading.value = false
    }
  })
</script>

<style scoped>
  .sticky-nav-footer {
    position: fixed;
    bottom: 0;
    left: 0;
    right: 0;
    z-index: 100;
    padding-left: 56px;
  }

  .tasks-table {
    width: 100%;
    border-collapse: collapse;
    table-layout: fixed;
  }

    .tasks-table :deep(th),
    .tasks-table :deep(td) {
      padding: 2px 4px;
      border: 1px solid rgba(0, 0, 0, 0.08);
      vertical-align: middle;
      white-space: normal;
      word-break: break-word;
      font-size: 12px;
    }

    .tasks-table :deep(th) {
      font-size: 10px;
      font-weight: 700;
      text-transform: uppercase;
      letter-spacing: 0.4px;
      color: rgba(0, 0, 0, 0.5);
      background: #fafbfc;
      white-space: nowrap;
    }

    .tasks-table :deep(.v-field) {
      font-size: 12px;
      border-radius: 0;
      background: transparent;
    }

    .tasks-table :deep(.v-field__outline) {
      display: none;
    }

    .tasks-table :deep(.v-field__input) {
      padding: 4px 6px;
      min-height: 28px;
      font-size: 12px;
      line-height: 1.3;
    }

    .tasks-table :deep(.v-input__details) {
      display: none;
    }

    .tasks-table :deep(.v-field--disabled) {
      opacity: 0.55;
    }

    .tasks-table :deep(.v-select__selection-text) {
      white-space: normal;
      word-break: break-word;
      line-height: 1.25;
    }

  .task-row--stage0 :deep(td) {
    background: rgba(0, 0, 0, 0.02);
    color: rgba(0, 0, 0, 0.45);
  }

  .end-date-value {
    font-size: 12px;
    color: rgba(0, 0, 0, 0.38);
  }

  .end-date-value--set {
    color: rgb(var(--v-theme-primary));
    font-weight: 500;
  }

  .col-code {
    width: 7%;
    text-align: center;
  }

  .col-task {
    width: 30%;
  }

  .col-dept {
    width: 16%;
  }

  .col-date {
    width: 14%;
  }

  .col-days {
    width: 9%;
    text-align: center;
  }

  .col-enddate {
    width: 11%;
    text-align: center;
  }

  .col-note {
    width: 17%;
  }

  .col-del {
    width: 5%;
    text-align: center;
  }

  .folder-preview :deep(.v-list-subheader) {
    color: rgb(var(--v-theme-primary));
    font-weight: 600;
  }

  .folder-root {
    display: flex;
    align-items: center;
    padding: 2px 0;
  }

  .team-inline-table {
    width: 100%;
    table-layout: fixed;
  }

    .team-inline-table :deep(th) {
      font-size: 10px;
      font-weight: 700;
      text-transform: uppercase;
      letter-spacing: 0.4px;
      color: rgba(0,0,0,0.5);
      background: #fafbfc;
      padding: 6px 8px !important;
      white-space: nowrap;
    }

    .team-inline-table :deep(td) {
      padding: 2px 4px !important;
      vertical-align: middle;
      white-space: normal;
      word-break: break-word;
    }

    .team-inline-table :deep(.v-field) {
      font-size: 12px;
      border-radius: 0;
      background: transparent;
    }

    .team-inline-table :deep(.v-field__outline) {
      display: none;
    }

    .team-inline-table :deep(.v-field__input) {
      padding: 4px 6px;
      min-height: 30px;
      font-size: 12px;
    }

    .team-inline-table :deep(.v-input__details) {
      display: none;
    }

  .team-col-dept {
    width: 30%;
  }

  .team-col-user {
    width: 32%;
  }

  .team-col-role {
    width: 30%;
  }

  .team-col-del {
    width: 8%;
    text-align: center;
  }
</style>
