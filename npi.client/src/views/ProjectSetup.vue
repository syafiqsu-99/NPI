<template>
  <v-container fluid>
    <v-row>
      <v-col cols="12">
        <v-card elevation="2">
          <v-card-title class="bg-primary">
            <div class="d-flex align-center justify-space-between w-100">
              <div>
                <v-icon class="mr-2">mdi-cog</v-icon>
                Project Setup - {{ project?.proj_no }}
              </div>
              <v-btn variant="text" color="white" @click="$router.push('/projects')">
                <v-icon start>mdi-arrow-left</v-icon>
                Back to Projects
              </v-btn>
            </div>
          </v-card-title>

          <v-card-text v-if="loading" class="text-center pa-8">
            <v-progress-circular indeterminate color="primary" size="64"></v-progress-circular>
          </v-card-text>

          <v-card-text v-else class="pa-6">
            <v-stepper v-model="step" elevation="0">
              <v-stepper-header>
                <v-stepper-item :complete="step > 1" :value="1" title="Project Info"></v-stepper-item>
                <v-divider></v-divider>
                <v-stepper-item :complete="step > 2" :value="2" title="Team Assignment"></v-stepper-item>
                <v-divider></v-divider>
                <v-stepper-item :complete="step > 3" :value="3" title="Tasks & Milestones"></v-stepper-item>
                <v-divider></v-divider>
                <v-stepper-item :value="4" title="Review & Launch"></v-stepper-item>
              </v-stepper-header>

              <v-stepper-window>
                <!-- ── Step 1: Project Info ─────────────────────────────── -->
                <v-stepper-window-item :value="1">
                  <v-card flat>
                    <v-card-text>
                      <h3 class="text-h6 mb-4">Project Information</h3>
                      <v-row>
                        <v-col cols="12" md="6">
                          <v-text-field v-model="project.proj_name" label="Project Name" readonly variant="outlined" />
                        </v-col>
                        <v-col cols="12" md="6">
                          <v-text-field v-model="project.proj_no" label="Project Number" readonly variant="outlined" />
                        </v-col>
                        <v-col cols="12" md="6">
                          <v-select v-model="project.priority"
                                    :items="['Low', 'Medium', 'High', 'Critical']"
                                    label="Priority"
                                    variant="outlined"
                                    @update:model-value="priorityChanged = true" />
                        </v-col>
                        <v-col cols="12" md="6">
                          <v-text-field v-model="project.status" label="Status" readonly variant="outlined" />
                        </v-col>
                        <v-col cols="12">
                          <v-textarea v-model="project.description" label="Description" rows="3" variant="outlined" />
                        </v-col>
                      </v-row>
                    </v-card-text>
                  </v-card>
                </v-stepper-window-item>

                <!-- ── Step 2: Team Assignment ─────────────────────────── -->
                <v-stepper-window-item :value="2">
                  <v-card flat>
                    <v-card-text>
                      <h3 class="text-h6 mb-4">Assign Project Team</h3>
                      <v-row>
                        <v-col cols="12">
                          <v-btn color="primary" @click="showAddTeamDialog = true">
                            <v-icon start>mdi-account-plus</v-icon>
                            Add Team Member
                          </v-btn>
                        </v-col>
                        <v-col cols="12">
                          <v-table>
                            <thead>
                              <tr>
                                <th>User</th>
                                <th>Department</th>
                                <th>Role in Project</th>
                                <th>Actions</th>
                              </tr>
                            </thead>
                            <tbody>
                              <tr v-for="(member, index) in teamMembers" :key="index">
                                <td>{{ member.user_name }}</td>
                                <td>{{ member.dept_name }}</td>
                                <td>{{ member.role_in_project }}</td>
                                <td>
                                  <v-btn icon size="small" variant="text" color="error"
                                         @click="removeTeamMember(index)">
                                    <v-icon>mdi-delete</v-icon>
                                  </v-btn>
                                </td>
                              </tr>
                              <tr v-if="teamMembers.length === 0">
                                <td colspan="4" class="text-center text-grey">No team members added yet</td>
                              </tr>
                            </tbody>
                          </v-table>
                        </v-col>
                      </v-row>
                    </v-card-text>
                  </v-card>
                </v-stepper-window-item>

                <!-- ── Step 3: Tasks & Milestones ─────────────────────── -->
                <v-stepper-window-item :value="3">
                  <v-card flat>
                    <v-card-text>
                      <h3 class="text-h6 mb-4">Tasks</h3>

                      <v-alert type="info" variant="tonal" class="mb-4">
                        <div class="d-flex align-center">
                          <v-icon class="mr-2">mdi-information</v-icon>
                          <div>
                            <strong>How to use:</strong>
                            <ul class="mt-2">
                              <li>Enter task name, department, start date, and working days</li>
                              <li>End date is calculated automatically (excluding weekends &amp; Malaysian holidays)</li>
                              <li>Check "Milestone" to mark completion milestones</li>
                              <li v-if="!isFirstLaunch">Add revision notes when changing task dates</li>
                              <li>
                                <strong>Folder structure:</strong>
                                each task maps to a folder named
                                <code>01_Task_Title /</code> inside the project folder.
                                Deleting a task will remove its folder only if it is empty.
                              </li>
                            </ul>
                          </div>
                        </div>
                      </v-alert>

                      <v-btn color="primary" class="mb-3" @click="addTask">
                        <v-icon start>mdi-plus</v-icon>
                        Add Task
                      </v-btn>

                      <v-table class="tasks-table">
                        <thead>
                          <tr>
                            <th class="col-index">#</th>
                            <th class="col-task">Task</th>
                            <th class="col-dept">Department</th>
                            <th class="col-date">Start Date</th>
                            <th class="col-days">Working Days</th>
                            <th class="col-date">End Date</th>
                            <th class="col-note" v-if="!isFirstLaunch">
                              Revision Note <span class="text-error">*</span>
                            </th>
                            <th class="col-milestone">Milestone</th>
                            <th class="col-actions"></th>
                          </tr>
                        </thead>
                        <tbody>
                          <tr v-for="(task, index) in defaultTasks" :key="task.id ?? index">
                            <td class="text-center">{{ index + 1 }}</td>

                            <td>
                              <v-text-field v-model="task.title"
                                            variant="plain" density="compact" hide-details class="table-input" />
                            </td>

                            <td>
                              <v-select v-model="task.department"
                                        :items="departments.map(d => d.dept_name)"
                                        variant="plain" density="compact" hide-details class="table-input" />
                            </td>

                            <td>
                              <v-text-field v-model="task.start_date"
                                            type="date"
                                            variant="plain" density="compact" hide-details class="table-input"
                                            @update:model-value="onStartDateChange(task)" />
                            </td>

                            <td>
                              <v-text-field v-model.number="task.working_days"
                                            type="number" min="1"
                                            variant="plain" density="compact" hide-details class="table-input"
                                            @update:model-value="onWorkingDaysChange(task)" />
                            </td>

                            <td class="text-center font-weight-medium">
                              {{ task.end_date ? formatDateShort(task.end_date) : 'N/A' }}
                            </td>

                            <td v-if="!isFirstLaunch">
                              <v-text-field v-model="task.revision_note"
                                            variant="plain" density="compact" hide-details
                                            placeholder="Why changed?"
                                            class="table-input"
                                            :disabled="!task.task_id" />
                            </td>

                            <td class="text-center">
                              <v-checkbox v-model="task.is_milestone"
                                          hide-details density="compact"
                                          @update:model-value="updateMilestoneName(task)" />
                            </td>

                            <td class="text-center">
                              <v-btn icon size="small" variant="text" color="error"
                                     @click="removeTask(index)">
                                <v-icon>mdi-delete</v-icon>
                              </v-btn>
                            </td>
                          </tr>

                          <tr v-if="defaultTasks.length === 0">
                            <td :colspan="isFirstLaunch ? 8 : 9" class="text-center text-grey">
                              No tasks defined
                            </td>
                          </tr>
                        </tbody>
                      </v-table>

                      <!-- Milestones Preview -->
                      <div v-if="generatedMilestones.length > 0" class="mt-6">
                        <h3 class="text-h6 mb-3">
                          <v-icon class="mr-2">mdi-flag-variant</v-icon>
                          Generated Milestones ({{ generatedMilestones.length }})
                        </h3>
                        <v-list density="compact" class="border rounded">
                          <v-list-item v-for="(milestone, index) in generatedMilestones"
                                       :key="index"
                                       :title="milestone.milestone_name"
                                       :subtitle="`Planned: ${formatDate(milestone.planned_date)} | Dept: ${milestone.dept_name}`">
                            <template #prepend>
                              <v-icon color="warning">mdi-flag-variant</v-icon>
                            </template>
                          </v-list-item>
                        </v-list>
                      </div>
                    </v-card-text>
                  </v-card>
                </v-stepper-window-item>

                <!-- ── Step 4: Review & Launch ─────────────────────────── -->
                <v-stepper-window-item :value="4">
                  <v-card flat>
                    <v-card-text>
                      <h3 class="text-h6 mb-4">Review & Launch Project</h3>

                      <v-alert type="success" variant="tonal" class="mb-4">
                        Project is ready to launch!
                      </v-alert>

                      <v-row>
                        <v-col cols="12" md="6">
                          <v-card variant="outlined">
                            <v-card-title class="text-subtitle-1">
                              <v-icon start>mdi-folder-multiple</v-icon>
                              Folder Structure Preview
                            </v-card-title>
                            <v-card-text>
                              <div class="text-caption text-grey mb-2">
                                Root: <code>projects/{{ sanitizeFolderName(project.proj_name) }}/</code>
                              </div>
                              <v-list density="compact">
                                <v-list-item v-for="(task, i) in defaultTasks" :key="i">
                                  <template #prepend>
                                    <v-icon size="small">mdi-folder</v-icon>
                                  </template>
                                  <v-list-item-title class="text-caption">
                                    {{ String(i + 1).padStart(2, '0') }}_{{ sanitizeFolderName(task.title) }}/
                                  </v-list-item-title>
                                </v-list-item>
                              </v-list>
                            </v-card-text>
                          </v-card>
                        </v-col>

                        <v-col cols="12" md="6">
                          <v-card variant="outlined">
                            <v-card-title class="text-subtitle-1">
                              <v-icon start>mdi-account-group</v-icon>
                              Team Summary
                            </v-card-title>
                            <v-card-text>
                              <div class="mb-2">
                                <strong>Total Members:</strong> {{ teamMembers.length }}
                              </div>
                              <div v-for="member in teamMembers" :key="member.user_id" class="text-caption">
                                • {{ member.user_name }} ({{ member.dept_name }})
                              </div>
                            </v-card-text>
                          </v-card>
                        </v-col>

                        <v-col cols="12">
                          <v-card variant="outlined">
                            <v-card-title class="text-subtitle-1">
                              <v-icon start>mdi-clipboard-list</v-icon>
                              Tasks & Milestones Summary
                            </v-card-title>
                            <v-card-text>
                              <div><strong>Total Tasks:</strong> {{ defaultTasks.length }}</div>
                              <div><strong>Milestones:</strong> {{ generatedMilestones.length }}</div>
                              <div><strong>Estimated Duration:</strong> {{ totalDuration }} working days</div>
                            </v-card-text>
                          </v-card>
                        </v-col>

                        <v-col cols="12">
                          <v-btn :color="project.status === 'Planning' ? 'success' : 'primary'"
                                 :loading="launching"
                                 @click="launchProject">
                            <v-icon start>
                              {{ project.status === 'Planning' ? 'mdi-rocket-launch' : 'mdi-content-save' }}
                            </v-icon>
                            {{ project.status === 'Planning' ? 'Launch Project' : 'Save Changes' }}
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
                  <v-btn v-if="step < 4" color="primary" @click="step++">Next</v-btn>
                </template>
              </v-stepper-actions>
            </v-stepper>
          </v-card-text>
        </v-card>
      </v-col>
    </v-row>

    <!-- ── Add Team Member Dialog ──────────────────────────────────────── -->
    <v-dialog v-model="showAddTeamDialog" max-width="600">
      <v-card>
        <v-card-title class="bg-primary">
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
          <v-spacer></v-spacer>
          <v-btn variant="text" @click="showAddTeamDialog = false">Cancel</v-btn>
          <v-btn color="primary" @click="addTeamMember">Add</v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>

    <!-- ── Folder Warning Dialog ───────────────────────────────────────── -->
    <!--
      Shown after a successful launch when one or more task folders could not
      be deleted because they still contain files.  The user must clear those
      files manually on the server.
    -->
    <v-dialog v-model="showFolderWarningDialog" max-width="640" persistent>
      <v-card>
        <v-card-title class="bg-warning text-white">
          <v-icon class="mr-2">mdi-folder-alert</v-icon>
          Folders Could Not Be Deleted
        </v-card-title>

        <v-card-text class="pa-6">
          <v-alert type="warning" variant="tonal" class="mb-4">
            The project was saved successfully, but the following task
            folder(s) were <strong>not deleted</strong> because they still
            contain files. Please remove or move the files inside each folder
            listed below, then re-save the project to clean them up.
          </v-alert>

          <p class="text-body-2 mb-3">
            <strong>Project root:</strong>
            <code>projects/{{ sanitizeFolderName(project.proj_name) }}/</code>
          </p>

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

          <v-alert type="info" variant="tonal" density="compact">
            <strong>Tip:</strong> These folders belong to tasks that were
            deleted or renamed. Once the files are removed, you can re-open
            Project Setup and save again to automatically clean up the
            empty folders.
          </v-alert>
        </v-card-text>

        <v-card-actions class="pa-4">
          <v-spacer></v-spacer>
          <v-btn color="warning" variant="elevated" @click="acknowledgeFolderWarnings">
            <v-icon start>mdi-check</v-icon>
            Understood – Continue
          </v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>

    <!-- ── Snackbar ────────────────────────────────────────────────────── -->
    <v-snackbar v-model="snackbar" :color="snackbarColor">
      {{ snackbarMessage }}
      <template v-slot:actions>
        <v-btn variant="text" @click="snackbar = false">Close</v-btn>
      </template>
    </v-snackbar>
  </v-container>
</template>

<script setup>
  import { ref, computed, onMounted } from 'vue'
  import { useRoute, useRouter } from 'vue-router'
  import { useProjectStore } from '@/stores/project'
  import { api } from '@/utils/api'

  const router = useRouter()
  const route = useRoute()
  const projectStore = useProjectStore()

  // ── State ──────────────────────────────────────────────────────────────────
  const loading = ref(false)
  const launching = ref(false)
  const step = ref(1)
  const priorityChanged = ref(false)

  const project = ref({
    proj_id: null,
    proj_no: '',
    proj_name: '',
    priority: 'Medium',
    status: 'Planning',
    description: ''
  })
  const teamMembers = ref([])
  const defaultTasks = ref([])
  const originalTaskDates = ref(new Map())

  const departments = ref([])
  const users = ref([])

  const showAddTeamDialog = ref(false)
  const showFolderWarningDialog = ref(false)
  const folderWarnings = ref([])   // List<string> from the backend

  const snackbar = ref(false)
  const snackbarMessage = ref('')
  const snackbarColor = ref('success')

  const newMember = ref({
    dept_id: null,
    user_id: null,
    role_in_project: 'Team Member'
  })
  const holidayCache = {}

  // ── Computed ───────────────────────────────────────────────────────────────
  const filteredUsers = computed(() =>
    users.value.filter(u => u.dept_id === newMember.value.dept_id)
  )

  const isFirstLaunch = computed(() => project.value.status === 'Planning')

  const totalDuration = computed(() =>
    defaultTasks.value.reduce((s, t) => s + (Number(t.duration) || 0), 0)
  )

  const generatedMilestones = computed(() =>
    defaultTasks.value
      .filter(t => t.is_milestone && t.end_date)
      .map((t, i) => ({
        milestone_name: t.milestone_title || t.title,
        planned_date: t.end_date,
        dept_name: t.department,
        order: i + 1
      }))
  )

  // ── Helpers ────────────────────────────────────────────────────────────────

  /**
   * Mirrors the SanitizeFolderName() logic in the C# services so the folder
   * preview in Step 4 stays accurate.
   */
  function sanitizeFolderName(name) {
    if (!name) return ''
    // Replace spaces and forward-slashes with underscores
    let result = name.replace(/[ /]/g, '_')
    // Strip characters that are invalid in Windows/Linux file names
    result = result.replace(/[<>:"\\|?*\x00-\x1F]/g, '_')
    return result
  }

  function formatDate(date) {
    if (!date) return 'N/A'
    return new Date(date).toLocaleDateString('en-GB', {
      day: '2-digit', month: 'short', year: 'numeric'
    })
  }

  function formatDateShort(date) {
    if (!date) return 'N/A'
    return new Date(date).toLocaleDateString('en-GB', {
      day: '2-digit', month: '2-digit', year: 'numeric'
    })
  }

  function checkDateChange(task) {
    if (!task.task_id || isFirstLaunch.value) {
      task.has_date_change = false
      return
    }
    const original = originalTaskDates.value.get(task.task_id)
    if (!original) { task.has_date_change = false; return }
    task.has_date_change =
      task.start_date !== original.start_date ||
      task.end_date !== original.end_date
  }

  // ── Team ───────────────────────────────────────────────────────────────────
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
    if (!user || !dept) { showSnack('Error: User or department not found', 'error'); return }

    teamMembers.value.push({
      user_id: newMember.value.user_id,
      dept_id: newMember.value.dept_id,
      user_name: user.username,
      dept_name: dept.dept_name,
      role_in_project: newMember.value.role_in_project
    })
    showAddTeamDialog.value = false
    newMember.value = { dept_id: null, user_id: null, role_in_project: 'Team Member' }
  }

  function removeTeamMember(index) { teamMembers.value.splice(index, 1) }

  // ── Tasks ──────────────────────────────────────────────────────────────────
  function addTask() {
    const today = new Date().toISOString().split('T')[0]
    defaultTasks.value.push({
      task_id: null,
      order: defaultTasks.value.length + 1,
      title: 'New Task',
      department: '',
      start_date: today,
      working_days: 5,
      end_date: null,
      original_start_date: null,
      original_end_date: null,
      is_milestone: false,
      milestone_title: '',
      revision_note: '',
      has_date_change: false
    })
    const newTask = defaultTasks.value[defaultTasks.value.length - 1]
    onWorkingDaysChange(newTask)
  }

  function removeTask(index) {
    defaultTasks.value.splice(index, 1)
    reorderTasks()
  }

  function reorderTasks() {
    defaultTasks.value.forEach((t, i) => { t.order = i + 1 })
  }

  function updateMilestoneName(task) {
    if (task.is_milestone) {
      task.milestone_title ||= task.title
    } else {
      task.milestone_title = ''
    }
  }

  async function onStartDateChange(task) {
    if (task.start_date && task.working_days)
      task.end_date = await calculateEndDate(task.start_date, task.working_days)
    checkDateChange(task)
  }

  async function onWorkingDaysChange(task) {
    if (task.start_date && task.working_days)
      task.end_date = await calculateEndDate(task.start_date, task.working_days)
    checkDateChange(task)
  }

  // ── Holiday / date calculation ─────────────────────────────────────────────
  function toMalaysiaDate(dateStr) {
    const [y, m, d] = dateStr.split('-').map(Number)
    return new Date(Date.UTC(y, m - 1, d))
  }

  async function calculateEndDate(startDate, workingDays) {
    if (!startDate || !workingDays || workingDays < 1) return null

    const start = toMalaysiaDate(startDate)
    const years = new Set()
    const estimatedEnd = new Date(start)
    estimatedEnd.setDate(estimatedEnd.getDate() + workingDays * 2)

    for (let y = start.getFullYear(); y <= estimatedEnd.getFullYear(); y++) years.add(y)

    const holidaySets = await Promise.all(Array.from(years).map(y => getHolidays(y)))

    let count = 0
    const current = new Date(start)

    while (count < workingDays) {
      const day = current.getDay()
      const iso = current.toISOString().slice(0, 10)
      const isWeekend = day === 0 || day === 6
      const isHoliday = holidaySets.some(s => s.has(iso))
      if (!isWeekend && !isHoliday) count++
      if (count < workingDays) current.setDate(current.getDate() + 1)
    }

    const yyyy = current.getUTCFullYear()
    const mm = String(current.getUTCMonth() + 1).padStart(2, '0')
    const dd = String(current.getUTCDate()).padStart(2, '0')
    return `${yyyy}-${mm}-${dd}`
  }

  async function getHolidays(year) {
    if (holidayCache[year]) return holidayCache[year]
    try {
      const res = await fetch(`https://sabah-holiday.dydxsoft.my/api/${year}.json`)
      if (!res.ok) { holidayCache[year] = new Set(); return holidayCache[year] }
      const data = await res.json()
      const monthMap = {
        Jan: 0, Feb: 1, Mar: 2, Apr: 3, May: 4, Jun: 5,
        Jul: 6, Aug: 7, Sep: 8, Oct: 9, Nov: 10, Dec: 11
      }
      holidayCache[year] = new Set(
        data.map(h => {
          const [monthStr, dayStr] = h.date.split(' ')
          const d = new Date(year, monthMap[monthStr], parseInt(dayStr, 10))
          return d.toISOString().slice(0, 10)
        })
      )
      return holidayCache[year]
    } catch {
      holidayCache[year] = new Set()
      return holidayCache[year]
    }
  }

  // ── Launch ─────────────────────────────────────────────────────────────────
  async function launchProject() {
    if (teamMembers.value.length === 0) {
      showSnack('Please add at least one team member', 'warning'); return
    }
    if (defaultTasks.value.length === 0) {
      showSnack('Please add at least one task', 'warning'); return
    }
    for (const task of defaultTasks.value) {
      if (!task.title || !task.department || !task.start_date || !task.end_date) {
        showSnack('Please complete all task fields (title, department, dates)', 'warning'); return
      }
    }

    launching.value = true

    try {
      const transformedTasks = defaultTasks.value.map(task => ({
        task_id: task.task_id || null,
        title: task.title,
        dept_name: task.department,
        start_date: formatDateForInput(task.start_date),
        end_date: formatDateForInput(task.end_date),
        duration: parseFloat(task.working_days) || 0,
        revision_note: task.revision_note || null
      }))

      const transformedTeam = teamMembers.value.map(member => ({
        user_id: member.user_id,
        dept_id: member.dept_id,
        role: member.role_in_project,
        dept_name: member.dept_name,
        user_name: member.user_name
      }))

      const transformedMilestones = defaultTasks.value
        .filter(task => task.is_milestone && task.end_date && task.department)
        .map(task => {
          const dept = departments.value.find(d => d.dept_name === task.department)
          return {
            task_id: task.task_id || null,
            milestone_name: task.milestone_title || task.title,
            planned_date: formatDateForInput(task.end_date),
            status: 'Pending',
            responsible_dept_id: dept?.dept_id || null
          }
        })

      const payload = {
        priority: project.value.priority,
        team_members: transformedTeam,
        tasks: transformedTasks,
        milestones: transformedMilestones
      }

      const result = await api.post(`/project/${route.params.id}/launch`, payload)
      const success = result?.success || result?.data?.success

      if (success) {
        // Check whether the backend returned folder warnings
        const warnings = result?.data?.folder_warnings ?? result?.folder_warnings ?? []

        if (warnings && warnings.length > 0) {
          // Show the dedicated warning dialog instead of navigating away
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
    } catch (error) {
      console.error('Launch error:', error)
      showSnack(error?.message || 'Error launching project', 'error')
    } finally {
      launching.value = false
    }
  }

  /** Called when the user clicks "Understood – Continue" in the folder-warning dialog. */
  function acknowledgeFolderWarnings() {
    showFolderWarningDialog.value = false
    folderWarnings.value = []
    // Navigate to Gantt — project was saved successfully
    router.push(`/projects/${route.params.id}/gantt`)
  }

  // ── Utilities ──────────────────────────────────────────────────────────────
  function showSnack(message, color = 'success') {
    snackbarMessage.value = message
    snackbarColor.value = color
    snackbar.value = true
  }

  function formatDateForInput(date) {
    if (!date) return null
    const d = new Date(date)
    const yyyy = d.getUTCFullYear()
    const mm = String(d.getUTCMonth() + 1).padStart(2, '0')
    const dd = String(d.getUTCDate()).padStart(2, '0')
    return `${yyyy}-${mm}-${dd}`
  }

  // ── Mount ──────────────────────────────────────────────────────────────────
  onMounted(async () => {
    loading.value = true
    try {
      // Project
      const projectResult = await projectStore.fetchProjectById(route.params.id)
      if (projectResult?.success && projectResult?.data) {
        project.value = {
          proj_id: projectResult.data.proj_id,
          proj_no: projectResult.data.proj_no || '',
          proj_name: projectResult.data.proj_name || '',
          priority: projectResult.data.priority || 'Medium',
          status: projectResult.data.status || 'Planning',
          description: projectResult.data.description || ''
        }
        if (projectResult.data.team_members) {
          teamMembers.value = projectResult.data.team_members.map(m => ({
            user_id: m.user_id,
            dept_id: m.dept_id,
            user_name: m.username || m.user_name,
            dept_name: m.dept_name,
            role_in_project: m.role || m.role_in_project || 'Team Member'
          }))
        }
      } else {
        showSnack('Failed to load project', 'error')
      }

      // Departments
      try {
        const deptResult = await api.get('/department')
        departments.value = deptResult?.data || deptResult || []
      } catch { showSnack('Warning: Could not load departments', 'warning') }

      // Users
      try {
        const userResult = await api.get('/user')
        users.value = userResult?.data || userResult || []
      } catch { showSnack('Warning: Could not load users', 'warning') }

      // Tasks
      try {
        const tasksResult = await api.get(`/project/${route.params.id}/tasks`)
        const tasksData = tasksResult?.data || tasksResult || []

        defaultTasks.value = await Promise.all(
          tasksData.map(async (task, index) => {
            const startDate = formatDateForInput(task.planned_start_date)
            const endDate = formatDateForInput(task.planned_end_date)
            if (task.task_id) {
              originalTaskDates.value.set(task.task_id, { start_date: startDate, end_date: endDate })
            }
            return {
              task_id: task.task_id,
              order: task.order || index + 1,
              title: task.title || '',
              department: task.dept_name || '',
              start_date: startDate,
              working_days: task.duration || 1,
              end_date: endDate,
              original_start_date: startDate,
              original_end_date: endDate,
              is_milestone: task.is_milestone,
              milestone_title: '',
              revision_note: '',
              has_date_change: false
            }
          })
        )
      } catch (error) {
        console.error('Tasks load error:', error)
      }
    } catch (error) {
      console.error('Mount error:', error)
      showSnack('Error loading project setup', 'error')
    } finally {
      loading.value = false
    }
  })
</script>

<style scoped>
  .tasks-table {
    width: 100%;
    border-collapse: collapse;
  }

    .tasks-table :deep(th),
    .tasks-table :deep(td) {
      padding: 8px;
      border: 1px solid rgba(0, 0, 0, 0.12);
    }

  .table-input :deep(.v-field__input) {
    min-height: 32px;
    padding: 4px 8px;
  }

  .col-index {
    width: 50px;
    text-align: center;
  }

  .col-task {
    min-width: 200px;
  }

  .col-dept {
    min-width: 150px;
  }

  .col-date {
    min-width: 140px;
  }

  .col-days {
    width: 100px;
    text-align: center;
  }

  .col-milestone {
    width: 120px;
  }

  .col-actions {
    width: 80px;
    text-align: center;
  }

  .col-note {
    min-width: 180px;
  }
</style>
