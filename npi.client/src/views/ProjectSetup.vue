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
              <v-btn variant="text"
                     color="white"
                     @click="$router.push('/projects')">
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
                <!-- Step 1: Project Info -->
                <v-stepper-window-item :value="1">
                  <v-card flat>
                    <v-card-text>
                      <h3 class="text-h6 mb-4">Project Information</h3>

                      <v-row>
                        <v-col cols="12" md="6">
                          <v-text-field v-model="project.proj_name"
                                        label="Project Name"
                                        readonly
                                        variant="outlined" />
                        </v-col>

                        <v-col cols="12" md="6">
                          <v-text-field v-model="project.proj_no"
                                        label="Project Number"
                                        readonly
                                        variant="outlined" />
                        </v-col>

                        <v-col cols="12" md="6">
                          <v-select v-model="project.priority"
                                    :items="['Low', 'Medium', 'High', 'Critical']"
                                    label="Priority"
                                    variant="outlined" />
                        </v-col>

                        <v-col cols="12" md="6">
                          <v-text-field v-model="project.status"
                                        label="Status"
                                        readonly
                                        variant="outlined" />
                        </v-col>

                        <v-col cols="12">
                          <v-textarea v-model="project.description"
                                      label="Description"
                                      rows="3"
                                      variant="outlined" />
                        </v-col>
                      </v-row>
                    </v-card-text>
                  </v-card>
                </v-stepper-window-item>

                <!-- Step 2: Team Assignment -->
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
                                <td colspan="4" class="text-center text-grey">
                                  No team members added yet
                                </td>
                              </tr>
                            </tbody>
                          </v-table>
                        </v-col>
                      </v-row>
                    </v-card-text>
                  </v-card>
                </v-stepper-window-item>

                <!-- Step 3: Tasks & Milestones -->
                <v-stepper-window-item :value="3">
                  <v-card flat>
                    <v-card-text>
                      <h3 class="text-h6 mb-4">Default Tasks (8 Steps)</h3>

                      <v-alert type="info" variant="tonal" class="mb-4">
                        These are the default NPI process tasks. You can modify dates and assignments.
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
                            <th class="col-date">Start</th>
                            <th class="col-date">End</th>
                            <th class="col-days">Working Days</th>
                            <th class="col-actions"></th>
                          </tr>
                        </thead>

                        <tbody>
                          <tr v-for="(task, index) in defaultTasks" :key="task.id ?? index">
                            <td class="text-center">{{ index + 1 }}</td>

                            <td>
                              <v-text-field v-model="task.title"
                                            variant="plain"
                                            density="compact"
                                            hide-details
                                            class="table-input" />
                            </td>

                            <td>
                              <v-select v-model="task.department"
                                        :items="departments.map(d => d.dept_name)"
                                        variant="plain"
                                        density="compact"
                                        hide-details
                                        class="table-input" />
                            </td>

                            <td>
                              <v-text-field v-model="task.start_date"
                                            type="date"
                                            variant="plain"
                                            density="compact"
                                            hide-details
                                            class="table-input"
                                            @update:model-value="recalcDurationDebounced(task)"/>
                            </td>

                            <td>
                              <v-text-field v-model="task.end_date"
                                            type="date"
                                            variant="plain"
                                            density="compact"
                                            hide-details
                                            class="table-input"
                                            @update:model-value="recalcDurationDebounced(task)"/>
                            </td>

                            <td class="text-center font-weight-medium">
                              {{ task.duration }}
                            </td>

                            <td class="text-center">
                              <v-btn icon
                                     size="small"
                                     variant="text"
                                     color="error"
                                     @click="removeTask(index)">
                                <v-icon>mdi-delete</v-icon>
                              </v-btn>
                            </td>
                          </tr>

                          <tr v-if="defaultTasks.length === 0">
                            <td colspan="7" class="text-center text-grey">
                              No tasks defined
                            </td>
                          </tr>
                        </tbody>
                      </v-table>
                    </v-card-text>
                  </v-card>
                </v-stepper-window-item>

                <!-- Step 4: Review & Launch -->
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
                              <v-icon start>mdi-folder</v-icon>
                              File Structure
                            </v-card-title>
                            <v-card-text>
                              <v-list density="compact">
                                <v-list-item v-for="dept in ['Sales', 'Purchaser', 'Technical', 'QA', 'Production', 'Others']"
                                             :key="dept">
                                  <template #prepend>
                                    <v-icon>mdi-folder</v-icon>
                                  </template>
                                  <v-list-item-title>{{ dept }}</v-list-item-title>
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
                              Tasks Summary
                            </v-card-title>
                            <v-card-text>
                              <div><strong>Total Tasks:</strong> {{ defaultTasks.length }}</div>
                              <div><strong>Estimated Duration:</strong> {{ totalDuration }} days</div>
                            </v-card-text>
                          </v-card>
                        </v-col>

                        <v-col cols="12">
                          <v-btn :color="project.status === 'Planning' ? 'success' : 'primary'"
                                 :loading="launching"
                                 @click="launchProject">
                            <v-icon start>{{ project.status === 'Planning' ? 'mdi-rocket-launch' : 'mdi-content-save' }}</v-icon>
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
                  <v-btn v-if="step > 1"
                         variant="text"
                         @click="step--">
                    Previous
                  </v-btn>
                </template>

                <template #next>
                  <v-btn v-if="step < 4"
                         color="primary"
                         @click="step++">
                    Next
                  </v-btn>
                </template>
              </v-stepper-actions>
            </v-stepper>
          </v-card-text>
        </v-card>
      </v-col>
    </v-row>

    <!-- Add Team Member Dialog -->
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
          <v-btn variant="text" @click="showAddTeamDialog = false">
            Cancel
          </v-btn>
          <v-btn color="primary" @click="addTeamMember">
            Add
          </v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>

    <!-- Success Snackbar -->
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

  const loading = ref(false)
  const launching = ref(false)
  const step = ref(1)
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
  const departments = ref([])
  const users = ref([])
  const showAddTeamDialog = ref(false)
  const snackbar = ref(false)
  const snackbarMessage = ref('')
  const snackbarColor = ref('success')
  const recalcMap = new WeakMap()
  const newMember = ref({
    dept_id: null,
    user_id: null,
    role_in_project: 'Team Member'
  })
  const holidayCache = {}

  const filteredUsers = computed(() => {
    if (!newMember.value.dept_id) return []
    return users.value.filter(u => u.dept_id === newMember.value.dept_id)
  })

  const totalDuration = computed(() => {
    if (!defaultTasks.value || defaultTasks.value.length === 0) return 0
    return defaultTasks.value.reduce((sum, task) => sum + (parseFloat(task.duration) || 0), 0)
  })

  function onDepartmentChange() {
    newMember.value.user_id = null
  }

  function addTeamMember() {
    if (!newMember.value.user_id || !newMember.value.dept_id) {
      snackbarMessage.value = 'Please select department and user'
      snackbarColor.value = 'error'
      snackbar.value = true
      return
    }

    if (teamMembers.value.some(m => m.user_id === newMember.value.user_id)) {
      snackbarMessage.value = 'User already added to team'
      snackbarColor.value = 'warning'
      snackbar.value = true
      return
    }

    const user = users.value.find(u => u.user_id === newMember.value.user_id)
    const dept = departments.value.find(d => d.dept_id === newMember.value.dept_id)

    if (!user || !dept) {
      snackbarMessage.value = 'Error: User or department not found'
      snackbarColor.value = 'error'
      snackbar.value = true
      return
    }

    teamMembers.value.push({
      user_id: newMember.value.user_id,
      dept_id: newMember.value.dept_id,
      user_name: user.username,
      dept_name: dept.dept_name,
      role_in_project: newMember.value.role_in_project
    })

    showAddTeamDialog.value = false
    newMember.value = {
      dept_id: null,
      user_id: null,
      role_in_project: 'Team Member'
    }
  }

  function removeTeamMember(index) {
    teamMembers.value.splice(index, 1)
  }

  function addTask() {
    const today = new Date().toISOString().split('T')[0]
    const tomorrow = new Date()
    tomorrow.setDate(tomorrow.getDate() + 1)
    const tomorrowStr = tomorrow.toISOString().split('T')[0]

    defaultTasks.value.push({
      task_id: null,
      order: defaultTasks.value.length + 1,
      title: 'New Task',
      department: '',
      start_date: today,
      end_date: tomorrowStr,
      duration: 1
    })
  }

  function removeTask(index) {
    defaultTasks.value.splice(index, 1)
    reorderTasks()
  }

  function reorderTasks() {
    defaultTasks.value.forEach((t, i) => {
      t.order = i + 1
    })
  }

  async function launchProject() {
    // Validate team members
    if (teamMembers.value.length === 0) {
      snackbarMessage.value = 'Please add at least one team member'
      snackbarColor.value = 'warning'
      snackbar.value = true
      return
    }

    // Validate tasks
    if (defaultTasks.value.length === 0) {
      snackbarMessage.value = 'Please add at least one task'
      snackbarColor.value = 'warning'
      snackbar.value = true
      return
    }

    launching.value = true

    try {
      // Transform tasks to match backend DTO
      const transformedTasks = defaultTasks.value.map(task => ({
        task_id: task.task_id || null,
        title: task.title,
        dept_name: task.department,
        start_date: formatDateForInput(task.start_date),
        end_date: formatDateForInput(task.end_date),
        duration: parseFloat(task.duration) || 0
      }))

      // Transform team members to match backend DTO
      const transformedTeam = teamMembers.value.map(member => ({
        user_id: member.user_id,
        dept_id: member.dept_id,
        role: member.role_in_project,
        dept_name: member.dept_name,
        user_name: member.user_name
      }))

      const payload = {
        team_members: transformedTeam,
        tasks: transformedTasks,
        milestones: []
      }

      const result = await api.post(`/project/${route.params.id}/launch`, payload)

      const success = result?.success || result?.data?.success

      if (success) {
        snackbarMessage.value = 'Project launched successfully!'
        snackbarColor.value = 'success'
        snackbar.value = true

        setTimeout(() => {
          router.push(`/projects/${route.params.id}/gantt`)
        }, 1500)
      } else {
        snackbarMessage.value = result?.message || 'Failed to launch project'
        snackbarColor.value = 'error'
        snackbar.value = true
      }
    } catch (error) {
      console.error('Launch error:', error)
      snackbarMessage.value = error?.message || 'Error launching project'
      snackbarColor.value = 'error'
      snackbar.value = true
    } finally {
      launching.value = false
    }
  }

  function formatDateForInput(date) {
    if (!date) return null
    const d = new Date(date)
    const yyyy = d.getUTCFullYear()
    const mm = String(d.getUTCMonth() + 1).padStart(2, '0')
    const dd = String(d.getUTCDate()).padStart(2, '0')

    return `${yyyy}-${mm}-${dd}`
  }

  async function holiday(year) {
    if (holidayCache[year]) return holidayCache[year]

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

    const holidays = new Set(
      data.map(h => {
        const [monthStr, dayStr] = h.date.split(' ')
        const month = monthMap[monthStr]
        const day = parseInt(dayStr, 10)
        const d = new Date(year, month, day)
        return d.toISOString().slice(0, 10)
      })
    )

    holidayCache[year] = holidays
    return holidays
  }

  function recalcDurationDebounced(task) {
    clearTimeout(recalcMap.get(task))
    recalcMap.set(task, setTimeout(() => recalcDuration(task), 200))
  }

  async function recalcDuration(task) {
    if (!task.start_date || !task.end_date) {
      task.duration = 0
      return
    }

    const start = toMalaysiaDate(task.start_date)
    const end = toMalaysiaDate(task.end_date)

    if (start > end) {
      task.duration = 0
      return
    }

    const years = new Set()
    for (let y = start.getFullYear(); y <= end.getFullYear(); y++) years.add(y)

    const holidaySets = await Promise.all(
      Array.from(years).map(y => holiday(y))
    )

    let count = 0
    const current = new Date(start)

    while (current <= end) {
      const day = current.getDay()
      const iso = current.toISOString().slice(0, 10)

      const isWeekend = day === 0 || day === 6
      const isHoliday = holidaySets.some(set => set.has(iso))

      if (!isWeekend && !isHoliday) count++

      current.setDate(current.getDate() + 1)
    }

    task.duration = count
  }

  function toMalaysiaDate(dateStr) {
    const [y, m, d] = dateStr.split('-').map(Number)
    return new Date(Date.UTC(y, m - 1, d, 0, 0, 0))
  }

  onMounted(async () => {
    loading.value = true

    try {
      // Load projects
      const projectResult = await projectStore.fetchProjectById(route.params.id)
      console.log("Project Result: ", projectResult.data);
      if (projectResult?.success && projectResult?.data) {
        project.value = {
          proj_id: projectResult.data.proj_id,
          proj_no: projectResult.data.proj_no || '',
          proj_name: projectResult.data.proj_name || '',
          priority: projectResult.data.priority || 'Medium',
          status: projectResult.data.status || 'Planning',
          description: projectResult.data.description || ''
        }

        // Load team members
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
        snackbarMessage.value = 'Failed to load project'
        snackbarColor.value = 'error'
        snackbar.value = true
      }

      // Load departments
      try {
        const deptResult = await api.get('/department')
        departments.value = deptResult?.data || deptResult || []
      } catch (error) {
        console.error('Department load error:', error)
        snackbarMessage.value = 'Warning: Could not load departments'
        snackbarColor.value = 'warning'
        snackbar.value = true
      }

      // Load users
      try {
        const userResult = await api.get('/user')
        users.value = userResult?.data || userResult || []
      } catch (error) {
        console.error('User load error:', error)
        snackbarMessage.value = 'Warning: Could not load users'
        snackbarColor.value = 'warning'
        snackbar.value = true
      }

      // Load tasks
      try {
        const tasksResult = await api.get(`/project/${route.params.id}/tasks`)
        const tasksData = tasksResult?.data || tasksResult || []

        defaultTasks.value = await Promise.all(
          tasksData.map(async (task, index) => {
            const transformed = {
              task_id: task.task_id,
              order: task.order || index + 1,
              title: task.title || '',
              department: task.dept_name || '',
              start_date: formatDateForInput(task.start_date),
              end_date: formatDateForInput(task.end_date),
              duration: 0
            }

            await recalcDuration(transformed)

            return transformed
          })
        )

      } catch (error) {
        console.error('Tasks load error:', error)
        snackbarMessage.value = 'Warning: Could not load tasks'
        snackbarColor.value = 'warning'
        snackbar.value = true
      }

    } catch (error) {
      console.error('Mount error:', error)
      snackbarMessage.value = 'Error loading project setup'
      snackbarColor.value = 'error'
      snackbar.value = true
    } finally {
      loading.value = false
    }
  })
</script>

<style setup>
  .tasks-table th,
  .tasks-table td {
    vertical-align: middle;
  }

  .tasks-table .col-index {
    width: 48px;
  }

  .tasks-table .col-task {
    width: 40%;
  }

  .tasks-table .col-dept {
    width: 16%;
  }

  .tasks-table .col-date {
    width: 12%;
  }

  .tasks-table .col-days {
    width: 64px;
    text-align: center;
  }

  .tasks-table .col-actions {
    width: 48px;
  }

  .tasks-table .table-input .v-field {
    padding-top: 0;
    padding-bottom: 0;
  }

  .tasks-table .table-input input {
    text-align: left;
  }
</style>
