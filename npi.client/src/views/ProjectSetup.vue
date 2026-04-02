<template>
  <v-container fluid>
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
                      <v-row>
                        <v-col cols="12" md="6">
                          <v-text-field v-model="project.proj_name"
                                        label="Project Name" readonly variant="outlined" />
                        </v-col>
                        <v-col cols="12" md="6">
                          <v-text-field v-model="project.proj_no"
                                        label="Project Number" readonly variant="outlined" />
                        </v-col>
                        <v-col cols="12" md="6">
                          <v-select v-model="project.priority"
                                    :items="['Low', 'Medium', 'High', 'Critical']"
                                    label="Priority"
                                    variant="outlined"
                                    @update:model-value="priorityChanged = true" />
                        </v-col>
                        <v-col cols="12" md="6">
                          <v-text-field v-model="project.status"
                                        label="Status" readonly variant="outlined" />
                        </v-col>
                        <v-col cols="12">
                          <v-textarea v-model="project.description"
                                      label="Description" rows="3" variant="outlined" />
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
                        <v-btn color="primary" variant="flat" @click="showAddTeamDialog = true">
                          <v-icon start>mdi-account-plus</v-icon>
                          Add Team Member
                        </v-btn>
                      </div>

                      <div v-if="teamMembers.length === 0"
                           class="text-center text-medium-emphasis py-8 border rounded">
                        <v-icon size="48" color="grey-lighten-2" class="d-block mx-auto mb-2">
                          mdi-account-group-outline
                        </v-icon>
                        No team members added yet. Click "Add Team Member" to start.
                      </div>

                      <v-row v-else dense>
                        <v-col v-for="(member, index) in teamMembers" :key="index"
                               cols="12" sm="6" md="4">
                          <v-card variant="outlined" class="pa-3 d-flex align-center ga-3">
                            <v-avatar :color="avatarColor(member.user_name)" size="38">
                              <span class="text-caption font-weight-bold" style="color:white">
                                {{ initials(member.user_name) }}
                              </span>
                            </v-avatar>
                            <div class="flex-grow-1 min-width-0">
                              <div class="text-body-2 font-weight-medium text-truncate">{{ member.user_name }}</div>
                              <div class="text-caption text-medium-emphasis">{{ member.dept_name }}</div>
                              <v-chip size="x-small" color="primary" variant="tonal" class="mt-1">
                                {{ member.role_in_project }}
                              </v-chip>
                            </div>
                            <v-btn icon size="x-small" variant="text" color="error"
                                   @click="removeTeamMember(index)">
                              <v-icon size="16">mdi-close</v-icon>
                            </v-btn>
                          </v-card>
                        </v-col>
                      </v-row>
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
                        <v-row>
                          <v-col cols="12" md="6">
                            <div class="d-flex align-center justify-space-between pa-3 border rounded">
                              <div>
                                <div class="font-weight-medium">
                                  <v-chip size="x-small" color="purple" variant="tonal" class="mr-2">2.0</v-chip>
                                  Pilot Mould Fabrication
                                </div>
                                <div class="text-caption text-grey mt-1">11 tasks incl. FAI &amp; iteration loops</div>
                              </div>
                              <v-switch v-model="stageFlags.pilot_mould_required"
                                        color="purple" hide-details density="compact"
                                        @update:model-value="onStageToggle('2.0', $event)" />
                            </div>
                          </v-col>
                          <v-col cols="12" md="6">
                            <div class="d-flex align-center justify-space-between pa-3 border rounded">
                              <div>
                                <div class="font-weight-medium">
                                  <v-chip size="x-small" color="teal" variant="tonal" class="mr-2">3.0</v-chip>
                                  New Machine Purchase
                                </div>
                                <div class="text-caption text-grey mt-1">4 tasks, Technical PIC only</div>
                              </div>
                              <v-switch v-model="stageFlags.machine_purchase_required"
                                        color="teal" hide-details density="compact"
                                        @update:model-value="onStageToggle('3.0', $event)" />
                            </div>
                          </v-col>
                        </v-row>
                      </v-card>

                      <v-alert type="info" variant="tonal" class="mb-4">
                        Tasks are pre-filled from the NPI template — treat them as defaults only.
                        You can <strong>add, edit, or delete</strong> tasks within each stage to match actual work.
                        Set <strong>Start Date</strong> and <strong>Working Days</strong> — End Date is
                        calculated automatically (excluding weekends &amp; Malaysian public holidays).
                        <span v-if="!isFirstLaunch">
                          Add a <strong>Revision Note</strong> when changing existing task dates.
                        </span>
                      </v-alert>

                      <!-- Per-stage expansion panels -->
                      <v-expansion-panels v-model="openPanels" multiple>
                        <v-expansion-panel v-for="stageId in activeStageIds" :key="stageId">
                          <v-expansion-panel-title>
                            <div class="d-flex align-center w-100 ga-2">
                              <v-chip :color="getStageColor(stageId)"
                                      size="small" variant="tonal"
                                      class="font-weight-bold" style="min-width:44px; justify-content:center">
                                {{ stageId }}
                              </v-chip>
                              <span class="font-weight-medium">{{ NPI_STAGES[stageId].name }}</span>
                              <v-chip v-if="!NPI_STAGES[stageId].required"
                                      size="x-small" color="orange" variant="outlined">
                                Optional
                              </v-chip>
                              <v-spacer />
                              <span class="text-caption text-grey mr-2">
                                {{ getStageTaskCount(stageId) }} tasks
                              </span>
                              <!-- Add task button — stops expansion panel from toggling -->
                              <v-btn size="x-small" variant="outlined" :color="getStageColor(stageId)"
                                     @click.stop="addTaskToStage(stageId)">
                                <v-icon size="13" start>mdi-plus</v-icon>
                                Add Task
                              </v-btn>
                            </div>
                          </v-expansion-panel-title>

                          <v-expansion-panel-text class="pa-0">
                            <div class="overflow-x-auto">
                              <v-table class="tasks-table" density="compact">
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
                                  <tr v-for="(task, tIdx) in getStageTaskRows(stageId)" :key="task._localId"
                                      :class="{ 'task-row--stage0': stageId === '0.0' }">

                                    <!-- Code -->
                                    <td class="text-center">
                                      <v-chip size="x-small" :color="getStageColor(stageId)" variant="tonal">
                                        {{ task.task_code || `${stageId}.${tIdx+1}` }}
                                      </v-chip>
                                    </td>

                                    <!-- Title — editable inline input -->
                                    <td>
                                      <input v-model="task.title"
                                             class="inline-input"
                                             placeholder="Task name…"
                                             :disabled="stageId === '0.0'" />
                                    </td>

                                    <!-- Department — native select for speed -->
                                    <td>
                                      <select v-model="task.department"
                                              class="inline-select"
                                              :disabled="stageId === '0.0'">
                                        <option value="">— Select —</option>
                                        <option v-for="d in departments" :key="d.dept_id" :value="d.dept_name">
                                          {{ d.dept_name }}
                                        </option>
                                      </select>
                                    </td>

                                    <!-- Start Date -->
                                    <td>
                                      <input v-model="task.start_date"
                                             type="date"
                                             class="inline-input inline-input--date"
                                             :disabled="stageId === '0.0'"
                                             @change="onStartDateChange(task)" />
                                    </td>

                                    <!-- Working Days -->
                                    <td>
                                      <input v-model.number="task.working_days"
                                             type="number" min="1"
                                             class="inline-input inline-input--num"
                                             :disabled="stageId === '0.0'"
                                             @input="onWorkingDaysChange(task)" />
                                    </td>

                                    <!-- End Date — computed, read-only display -->
                                    <td class="text-center">
                                      <span v-if="task.computing" class="text-caption text-medium-emphasis">
                                        <v-progress-circular indeterminate size="12" width="2" />
                                      </span>
                                      <span v-else class="end-date-value"
                                            :class="{ 'end-date-value--set': !!task.end_date }">
                                        {{ task.end_date ? formatDateShort(task.end_date) : '—' }}
                                      </span>
                                    </td>

                                    <!-- Revision Note (edit mode only) -->
                                    <td v-if="!isFirstLaunch">
                                      <input v-model="task.revision_note"
                                             class="inline-input"
                                             placeholder="Why changed?"
                                             :disabled="!task.task_id || !task.has_date_change" />
                                    </td>

                                    <!-- Delete row -->
                                    <td class="text-center">
                                      <v-btn v-if="stageId !== '0.0'"
                                             icon size="x-small" variant="text" color="error"
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
                                      <span class="text-primary cursor-pointer"
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

                      <v-alert :type="isFirstLaunch ? 'success' : 'info'" variant="tonal" class="mb-4">
                        <span v-if="isFirstLaunch">
                          Review the summary below. Once launched, status changes to
                          <strong>In Progress</strong> and the Gantt becomes available.
                        </span>
                        <span v-else>
                          Changes to task dates are recorded with revision history.
                          Ensure all revision notes are filled before saving.
                        </span>
                      </v-alert>

                      <v-row>
                        <!-- Active Stages summary -->
                        <v-col cols="12" md="6">
                          <v-card variant="outlined">
                            <v-card-title class="text-subtitle-1">
                              <v-icon start>mdi-layers</v-icon>
                              Active NPI Stages
                            </v-card-title>
                            <v-card-text>
                              <div v-for="stageId in activeStageIds" :key="stageId"
                                   class="d-flex align-center mb-2">
                                <v-chip :color="getStageColor(stageId)" size="small" variant="tonal" class="mr-2">
                                  {{ stageId }}
                                </v-chip>
                                <span class="text-body-2">{{ NPI_STAGES[stageId].name }}</span>
                                <v-spacer />
                                <span class="text-caption text-grey">{{ getStageTaskCount(stageId) }} tasks</span>
                              </div>
                            </v-card-text>
                          </v-card>
                        </v-col>

                        <!-- Folder Structure preview -->
                        <v-col cols="12" md="6">
                          <v-card variant="outlined">
                            <v-card-title class="text-subtitle-1">
                              <v-icon start>mdi-folder-multiple</v-icon>
                              Folder Structure Preview
                            </v-card-title>
                            <v-card-text>
                              <div class="text-caption text-grey mb-3">
                                One folder per department under the project root.
                                Files uploaded by team members are stored in their department folder.
                              </div>
                              <!-- projects/{proj_name}/{department}/ — one folder per dept, no task subfolders -->
                              <div class="folder-root mb-1">
                                <v-icon size="16" color="primary">mdi-folder</v-icon>
                                <code class="ml-1">projects/{{ sanitizeFolderName(project.proj_name) }}/</code>
                              </div>
                              <v-list density="compact" class="folder-preview pl-4">
                                <v-list-item v-for="dept in folderPreviewDepts" :key="dept"
                                             density="compact">
                                  <template #prepend>
                                    <v-icon size="16" color="amber-darken-2">mdi-folder</v-icon>
                                  </template>
                                  <v-list-item-title class="text-body-2">
                                    {{ sanitizeFolderName(dept) }}/
                                  </v-list-item-title>
                                  <template #append>
                                    <span class="text-caption text-medium-emphasis">
                                      {{ deptTaskCount(dept) }} task{{ deptTaskCount(dept) !== 1 ? 's' : '' }}
                                    </span>
                                  </template>
                                </v-list-item>
                                <v-list-item v-if="folderPreviewDepts.length === 0" density="compact">
                                  <v-list-item-title class="text-caption text-medium-emphasis">
                                    No departments assigned yet
                                  </v-list-item-title>
                                </v-list-item>
                              </v-list>
                            </v-card-text>
                          </v-card>
                        </v-col>

                        <!-- Team summary -->
                        <v-col cols="12" md="6">
                          <v-card variant="outlined">
                            <v-card-title class="text-subtitle-1">
                              <v-icon start>mdi-account-group</v-icon>
                              Team Summary
                            </v-card-title>
                            <v-card-text>
                              <div class="mb-2"><strong>Total Members:</strong> {{ teamMembers.length }}</div>
                              <div v-for="member in teamMembers" :key="member.user_id" class="text-caption">
                                • {{ member.user_name }} ({{ member.dept_name }}) — {{ member.role_in_project }}
                              </div>
                            </v-card-text>
                          </v-card>
                        </v-col>

                        <!-- Tasks summary (no milestones) -->
                        <v-col cols="12" md="6">
                          <v-card variant="outlined">
                            <v-card-title class="text-subtitle-1">
                              <v-icon start>mdi-clipboard-list</v-icon>
                              Tasks Summary
                            </v-card-title>
                            <v-card-text>
                              <div><strong>Total Tasks:</strong> {{ allTasks.length }}</div>
                              <div><strong>Active Stages:</strong> {{ activeStageIds.length }}</div>
                              <div><strong>Est. Duration:</strong> {{ totalDuration }} working days</div>
                              <div class="mt-2 text-caption text-medium-emphasis">
                                NPI stages serve as project milestones.
                              </div>
                            </v-card-text>
                          </v-card>
                        </v-col>

                        <v-col cols="12">
                          <v-btn :color="isFirstLaunch ? 'success' : 'primary'"
                                 :loading="launching"
                                 size="large"
                                 variant="flat"
                                 @click="launchProject">
                            <v-icon start>{{ isFirstLaunch ? 'mdi-rocket-launch' : 'mdi-content-save' }}</v-icon>
                            {{ isFirstLaunch ? 'Launch Project' : 'Save Changes' }}
                          </v-btn>
                        </v-col>
                      </v-row>
                    </v-card-text>
                  </v-card>
                </v-stepper-window-item>
              </v-stepper-window>

              <v-stepper-actions>
                <template #prev>
                  <v-btn v-if="step > 1" variant="text" @click="step--">Previous</v-btn>
                </template>
                <template #next>
                  <v-btn v-if="step < 4" color="primary" @click="nextStep">Next</v-btn>
                </template>
              </v-stepper-actions>
            </v-stepper>
          </v-card-text>
        </v-card>
      </v-col>
    </v-row>

    <!-- Add Team Member Dialog -->
    <v-dialog v-model="showAddTeamDialog" max-width="560">
      <v-card>
        <v-card-title class="bg-primary text-white">
          <v-icon class="mr-2">mdi-account-plus</v-icon>
          Add Team Member
        </v-card-title>
        <v-card-text class="pa-6">
          <v-row>
            <v-col cols="12">
              <v-autocomplete v-model="newMember.dept_id"
                              :items="departments"
                              item-title="dept_name"
                              item-value="dept_id"
                              label="Department *"
                              @update:model-value="onDepartmentChange"
                              variant="outlined" />
            </v-col>
            <v-col cols="12">
              <v-autocomplete v-model="newMember.user_id"
                              :items="filteredUsers"
                              item-title="username"
                              item-value="user_id"
                              label="User *"
                              :disabled="!newMember.dept_id"
                              variant="outlined" />
            </v-col>
            <v-col cols="12">
              <v-select v-model="newMember.role_in_project"
                        :items="['Team Member', 'Team Lead', 'Coordinator', 'Reviewer']"
                        label="Role in Project *"
                        variant="outlined" />
            </v-col>
          </v-row>
        </v-card-text>
        <v-card-actions class="pa-4">
          <v-spacer />
          <v-btn variant="text" @click="showAddTeamDialog = false">Cancel</v-btn>
          <v-btn color="primary" variant="flat" @click="addTeamMember">Add</v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>

    <!-- Folder Warning Dialog -->
    <v-dialog v-model="showFolderWarningDialog" max-width="640" persistent>
      <v-card>
        <v-card-title class="bg-warning text-white">
          <v-icon class="mr-2">mdi-folder-alert</v-icon>
          Folders Could Not Be Deleted
        </v-card-title>
        <v-card-text class="pa-6">
          <v-alert type="warning" variant="tonal" class="mb-4">
            Project saved successfully, but the following folders were <strong>not deleted</strong>
            because they still contain files. Please clear the files manually.
          </v-alert>
          <p class="text-body-2 mb-3">
            <strong>Project root:</strong>
            <code>projects/{{ sanitizeFolderName(project.proj_name) }}/</code>
          </p>
          <v-list lines="two" class="border rounded mb-4">
            <v-list-item v-for="(folder, i) in folderWarnings" :key="i"
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
  import { useRoute, useRouter } from 'vue-router'
  import { useProjectStore } from '@/stores/project'
  import { NPI_STAGES } from '@/stores/stageTemplate'
  import { api } from '@/utils/api'

  const router = useRouter()
  const route = useRoute()
  const projectStore = useProjectStore()

  const loading = ref(false)
  const launching = ref(false)
  const step = ref(1)
  const priorityChanged = ref(false)

  const project = ref({
    proj_id: null, proj_no: '', proj_name: '',
    priority: 'Medium', status: 'Planning', description: ''
  })

  const teamMembers = ref([])

  const stageFlags = ref({
    pilot_mould_required: false,
    machine_purchase_required: false
  })

  const tasksByStage = ref({})

  const originalTaskDates = ref(new Map())

  const departments = ref([])
  const users = ref([])

  const showAddTeamDialog = ref(false)
  const showFolderWarningDialog = ref(false)
  const folderWarnings = ref([])

  const snackbar = ref(false)
  const snackbarMessage = ref('')
  const snackbarColor = ref('success')

  const newMember = ref({ dept_id: null, user_id: null, role_in_project: 'Team Member' })
  const openPanels = ref([0, 1, 2, 3, 4, 5])

  const holidayCache = {}

  const STAGE_COLORS = {
    '0.0': 'blue-grey', '1.0': 'primary', '2.0': 'purple',
    '3.0': 'teal', '4.0': 'indigo', '5.0': 'deep-orange'
  }

  function getStageColor(stageId) { return STAGE_COLORS[stageId] || 'grey' }

  const isFirstLaunch = computed(() => project.value.status === 'Planning')

  const activeStageIds = computed(() =>
    Object.keys(NPI_STAGES).filter(id => {
      if (NPI_STAGES[id].required) return true
      if (id === '2.0') return stageFlags.value.pilot_mould_required
      if (id === '3.0') return stageFlags.value.machine_purchase_required
      return false
    })
  )

  const allTasks = computed(() =>
    activeStageIds.value.flatMap(id => tasksByStage.value[id] || [])
  )

  const totalDuration = computed(() =>
    allTasks.value.reduce((s, t) => s + (Number(t.working_days) || 0), 0)
  )

  const filteredUsers = computed(() =>
    users.value.filter(u => u.dept_id === newMember.value.dept_id)
  )

  const folderPreviewDepts = computed(() => {
    const depts = new Set(
      allTasks.value
        .filter(t => t.stage_id !== '0.0' && t.department)
        .map(t => t.department)
    )
    return [...depts].sort((a, b) => a.localeCompare(b))
  })

  function deptTaskCount(dept) {
    return allTasks.value.filter(t => t.department === dept && t.stage_id !== '0.0').length
  }

  function getStageTaskRows(stageId) { return tasksByStage.value[stageId] || [] }
  function getStageTaskCount(stageId) { return getStageTaskRows(stageId).length }

  function onStageToggle(stageId, enabled) {
    if (enabled) {
      seedStage(stageId)
    } else {
      const hasSaved = (tasksByStage.value[stageId] || []).some(t => t.task_id)
      if (hasSaved) {
        showSnack(`Stage ${stageId} has saved tasks — cannot be removed once launched`, 'warning')
        if (stageId === '2.0') stageFlags.value.pilot_mould_required = true
        if (stageId === '3.0') stageFlags.value.machine_purchase_required = true
        return
      }
      delete tasksByStage.value[stageId]
    }
  }

  function seedStage(stageId) {
    if (tasksByStage.value[stageId]?.length) return
    const today = new Date().toISOString().split('T')[0]
    tasksByStage.value[stageId] = NPI_STAGES[stageId].tasks.map((t, idx) => ({
      _localId: `${stageId}_${idx}_${Date.now()}`,
      task_id: null, stage_id: stageId, task_code: t.code,
      order: idx + 1, title: t.title, department: t.pic,
      start_date: today, working_days: 5, end_date: null,
      computing: false, revision_note: '', has_date_change: false,
      has_link: t.hasLink || false, doc_format: t.docFormat || null, role_gated: t.roleGated || null,
    }))
  }

  function addTaskToStage(stageId) {
    const today = new Date().toISOString().split('T')[0]
    if (!tasksByStage.value[stageId]) tasksByStage.value[stageId] = []
    const rows = tasksByStage.value[stageId]
    rows.push({
      _localId: `${stageId}_new_${Date.now()}`,
      task_id: null, stage_id: stageId,
      task_code: `${stageId}.${rows.length + 1}`,
      order: rows.length + 1, title: '', department: '',
      start_date: today, working_days: 5, end_date: null,
      computing: false, revision_note: '', has_date_change: false,
      has_link: false, doc_format: null, role_gated: null,
    })
  }

  function removeTask(stageId, tIdx) {
    tasksByStage.value[stageId].splice(tIdx, 1)
  }

  function nextStep() {
    if (step.value === 2 && teamMembers.value.length === 0) {
      showSnack('Add at least one team member before continuing', 'warning')
      return
    }
    step.value++
  }

  function onDepartmentChange() { newMember.value.user_id = null }

  function addTeamMember() {
    if (!newMember.value.user_id || !newMember.value.dept_id) {
      showSnack('Please select department and user', 'error'); return
    }
    if (teamMembers.value.some(m => m.user_id === newMember.value.user_id)) {
      showSnack('User already added to team', 'warning'); return
    }
    const user = users.value.find(u => u.user_id === newMember.value.user_id)
    const dept = departments.value.find(d => d.dept_id === newMember.value.dept_id)
    if (!user || !dept) { showSnack('User or department not found', 'error'); return }
    teamMembers.value.push({
      user_id: newMember.value.user_id, dept_id: newMember.value.dept_id,
      user_name: user.username, dept_name: dept.dept_name,
      role_in_project: newMember.value.role_in_project
    })
    showAddTeamDialog.value = false
    newMember.value = { dept_id: null, user_id: null, role_in_project: 'Team Member' }
  }

  function removeTeamMember(index) { teamMembers.value.splice(index, 1) }

  function initials(name) {
    if (!name) return '?'
    return name.split(' ').map(n => n[0]).join('').substring(0, 2).toUpperCase()
  }
  function avatarColor(name) {
    const colors = ['primary', 'secondary', 'success', 'info', 'warning', 'purple', 'teal', 'indigo']
    if (!name) return 'grey'
    let hash = 0
    for (let i = 0; i < name.length; i++) hash = name.charCodeAt(i) + ((hash << 5) - hash)
    return colors[Math.abs(hash) % colors.length]
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
    if (!task.task_id || isFirstLaunch.value) { task.has_date_change = false; return }
    const orig = originalTaskDates.value.get(task.task_id)
    if (!orig) { task.has_date_change = false; return }
    task.has_date_change = task.start_date !== orig.start_date || task.end_date !== orig.end_date
  }

  function toMalaysiaDate(dateStr) {
    const [y, m, d] = dateStr.split('-').map(Number)
    return new Date(Date.UTC(y, m - 1, d))
  }

  async function calculateEndDate(startDate, workingDays) {
    if (!startDate || !workingDays || workingDays < 1) return null
    const start = toMalaysiaDate(startDate)
    const years = new Set()
    const est = new Date(start); est.setDate(est.getDate() + workingDays * 2)
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
      if (!res.ok) { holidayCache[year] = new Set(); return holidayCache[year] }
      const data = await res.json()
      const monthMap = { Jan: 0, Feb: 1, Mar: 2, Apr: 3, May: 4, Jun: 5, Jul: 6, Aug: 7, Sep: 8, Oct: 9, Nov: 10, Dec: 11 }
      holidayCache[year] = new Set(data.map(h => {
        const [ms, ds] = h.date.split(' ')
        return new Date(year, monthMap[ms], parseInt(ds, 10)).toISOString().slice(0, 10)
      }))
    } catch { holidayCache[year] = new Set() }
    return holidayCache[year]
  }

  function sanitizeFolderName(name) {
    if (!name) return ''
    return name.replace(/[ /]/g, '_').replace(/[<>:"\\|?*\x00-\x1F]/g, '_')
  }

  function formatDate(date) {
    if (!date) return 'N/A'
    return new Date(date).toLocaleDateString('en-GB', { day: '2-digit', month: 'short', year: 'numeric' })
  }

  function formatDateShort(date) {
    if (!date) return '—'
    const d = new Date(date.replace(/-/g, '/'))
    return d.toLocaleDateString('en-GB', { day: '2-digit', month: 'short', year: 'numeric' })
  }

  function formatDateForInput(date) {
    if (!date) return null
    const d = new Date(date)
    return `${d.getUTCFullYear()}-${String(d.getUTCMonth() + 1).padStart(2, '0')}-${String(d.getUTCDate()).padStart(2, '0')}`
  }

  async function launchProject() {
    if (teamMembers.value.length === 0) { showSnack('Add at least one team member', 'warning'); return }
    if (allTasks.value.length === 0) { showSnack('Add at least one task', 'warning'); return }
    for (const task of allTasks.value) {
      if (task.stage_id === '0.0') continue
      if (!task.title || !task.department || !task.start_date || !task.end_date) {
        showSnack(`Task "${task.task_code || task.title}" is missing required fields`, 'warning'); return
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
        dept_name: task.department,
        start_date: formatDateForInput(task.start_date),
        end_date: formatDateForInput(task.end_date),
        duration: parseFloat(task.working_days) || 0,
        revision_note: task.revision_note || null,
        has_link: task.has_link,
        doc_format: task.doc_format,
        role_gated: task.role_gated,
      }))

      const payload = {
        priority: project.value.priority,
        description: project.value.description,
        pilot_mould_required: stageFlags.value.pilot_mould_required,
        machine_purchase_required: stageFlags.value.machine_purchase_required,
        team_members: teamMembers.value.map(m => ({
          user_id: m.user_id, dept_id: m.dept_id,
          role: m.role_in_project, dept_name: m.dept_name, user_name: m.user_name
        })),
        tasks: transformedTasks,
        milestones: []
      }

      const result = await api.post(`/project/${route.params.id}/launch`, payload)
      const success = result?.success || result?.data?.success

      if (success) {
        const warnings = result?.data?.folder_warnings ?? result?.folder_warnings ?? []
        if (warnings.length > 0) {
          folderWarnings.value = warnings
          showFolderWarningDialog.value = true
        } else {
          showSnack(isFirstLaunch.value ? 'Project launched successfully!' : 'Project updated successfully!', 'success')
          setTimeout(() => router.push(`/projects/${route.params.id}/gantt`), 1500)
        }
      } else {
        showSnack(result?.message || 'Failed to launch project', 'error')
      }
    } catch (err) {
      console.error('Launch error:', err)
      showSnack(err?.message || 'Error launching project', 'error')
    } finally { launching.value = false }
  }

  function acknowledgeFolderWarnings() {
    showFolderWarningDialog.value = false
    folderWarnings.value = []
    router.push(`/projects/${route.params.id}/gantt`)
  }

  function showSnack(msg, color = 'success') {
    snackbarMessage.value = msg; snackbarColor.value = color; snackbar.value = true
  }

  onMounted(async () => {
    loading.value = true
    try {
      const pr = await projectStore.fetchProjectById(route.params.id)
      if (pr?.success && pr?.data) {
        const d = pr.data
        project.value = {
          proj_id: d.proj_id, proj_no: d.proj_no || '',
          proj_name: d.proj_name || '', priority: d.priority || 'Medium',
          status: d.status || 'Planning', description: d.description || ''
        }
        stageFlags.value.pilot_mould_required = !!d.pilot_mould_required
        stageFlags.value.machine_purchase_required = !!d.machine_purchase_required
        if (d.team_members) {
          teamMembers.value = d.team_members.map(m => ({
            user_id: m.user_id, dept_id: m.dept_id,
            user_name: m.username || m.user_name, dept_name: m.dept_name,
            role_in_project: m.role || m.role_in_project || 'Team Member'
          }))
        }
      }

      Object.keys(NPI_STAGES).forEach(id => { if (NPI_STAGES[id].required) seedStage(id) })
      if (stageFlags.value.pilot_mould_required) seedStage('2.0')
      if (stageFlags.value.machine_purchase_required) seedStage('3.0')

      try { const dr = await api.get('/department'); departments.value = dr?.data || dr || [] }
      catch { showSnack('Warning: Could not load departments', 'warning') }

      try { const ur = await api.get('/user'); users.value = ur?.data || ur || [] }
      catch { showSnack('Warning: Could not load users', 'warning') }

      try {
        const tr = await api.get(`/project/${route.params.id}/tasks`)
        const tasksData = tr?.data || tr || []
        if (tasksData.length > 0) {
          const stageGroups = {}
          tasksData.forEach(task => {
            const sid = task.stage_id || '1.0'
            if (!stageGroups[sid]) stageGroups[sid] = []
            const startDate = formatDateForInput(task.planned_start_date)
            const endDate = formatDateForInput(task.planned_end_date)
            if (task.task_id) originalTaskDates.value.set(task.task_id, { start_date: startDate, end_date: endDate })
            stageGroups[sid].push({
              _localId: `${sid}_${task.task_id}_db`,
              task_id: task.task_id, stage_id: sid,
              task_code: task.task_code || '', order: task.order || 1,
              title: task.title || '', department: task.dept_name || '',
              start_date: startDate, working_days: task.duration || 1,
              end_date: endDate, computing: false,
              revision_note: '', has_date_change: false,
              has_link: task.has_link || false,
              doc_format: task.doc_format || null,
              role_gated: task.role_gated || null,
            })
          })
          Object.keys(stageGroups).forEach(sid => { tasksByStage.value[sid] = stageGroups[sid] })

          const tasksNeedingEndDate = Object.values(tasksByStage.value)
            .flat()
            .filter(t => t.start_date && t.working_days && !t.end_date)
          if (tasksNeedingEndDate.length > 0) {
            await Promise.all(tasksNeedingEndDate.map(async t => {
              t.end_date = await calculateEndDate(t.start_date, t.working_days)
            }))
          }
        }
      } catch (err) { console.error('Tasks load error:', err) }
    } catch (err) {
      console.error('Mount error:', err)
      showSnack('Error loading project setup', 'error')
    } finally { loading.value = false }
  })
</script>

<style scoped>
  .tasks-table {
    width: 100%;
    border-collapse: collapse;
  }

  .tasks-table :deep(th),
  .tasks-table :deep(td) {
    padding: 6px 8px;
    border: 1px solid rgba(0,0,0,0.08);
    white-space: nowrap;
  }

  .tasks-table :deep(th) {
    font-size: 11px;
    font-weight: 700;
    text-transform: uppercase;
    letter-spacing: 0.4px;
    color: rgba(0,0,0,0.5);
    background: #fafbfc;
  }

  .task-row--stage0 td {
    background: rgba(0,0,0,0.02);
    color: rgba(0,0,0,0.45);
  }

  .inline-input {
    width: 100%;
    border: none;
    outline: none;
    background: transparent;
    font-size: 13px;
    padding: 4px 4px;
    border-radius: 4px;
    color: inherit;
    font-family: inherit;
    transition: background 0.15s;
  }

  .inline-input:focus {
    background: rgba(var(--v-theme-primary), 0.06);
  }

  .inline-input:disabled {
    color: rgba(0,0,0,0.35);
    cursor: not-allowed;
  }

  .inline-input--date {
    min-width: 128px;
  }

  .inline-input--num {
    width: 64px;
    text-align: center;
  }

  .inline-select {
    width: 100%;
    border: none;
    outline: none;
    background: transparent;
    font-size: 13px;
    padding: 4px 4px;
    border-radius: 4px;
    cursor: pointer;
    font-family: inherit;
  }

  .inline-select:focus {
    background: rgba(var(--v-theme-primary), 0.06);
  }

  .inline-select:disabled {
    color: rgba(0,0,0,0.35);
    cursor: not-allowed;
  }

  .end-date-value {
    font-size: 12px;
    color: rgba(0,0,0,0.38);
    font-weight: 400;
  }

  .end-date-value--set {
    color: rgb(var(--v-theme-primary));
    font-weight: 500;
  }

  .col-code {
    width: 64px;
    text-align: center;
  }

  .col-task {
    min-width: 200px;
  }

  .col-dept {
    min-width: 140px;
  }

  .col-date {
    min-width: 132px;
  }

  .col-days {
    width: 90px;
    text-align: center;
  }

  .col-enddate {
    width: 110px;
    text-align: center;
  }

  .col-note {
    min-width: 160px;
  }

  .col-del {
    width: 40px;
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
</style>
