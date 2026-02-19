<template>
  <v-container fluid class="pa-0">
    <v-row no-gutters>
      <v-col cols="12">
        <v-card elevation="2">
          <v-card-title class="bg-primary d-flex align-center justify-space-between">
            <div>
              <v-icon class="mr-2">mdi-chart-gantt</v-icon>
              Project Gantt Chart - {{ project?.proj_no }}
            </div>
            <div>
              <v-btn variant="text" color="white" class="mr-2" @click="refreshData">
                <v-icon start>mdi-refresh</v-icon>
                Refresh
              </v-btn>
              <v-btn variant="text" color="white" @click="$router.push('/projects')">
                <v-icon start>mdi-arrow-left</v-icon>
                Back to Projects
              </v-btn>
            </div>
          </v-card-title>

          <v-card-text v-if="loading" class="text-center pa-8">
            <v-progress-circular indeterminate color="primary" size="64"></v-progress-circular>
          </v-card-text>

          <v-card-text v-else class="pa-0">
            <!-- Project Summary Header -->
            <v-sheet class="pa-4 bg-grey-lighten-4">
              <v-row>
                <v-col cols="12" md="3">
                  <div class="text-caption text-grey-darken-1">Project Name</div>
                  <div class="text-subtitle-1 font-weight-medium">{{ project?.proj_name }}</div>
                </v-col>
                <v-col cols="12" md="2">
                  <div class="text-caption text-grey-darken-1">Status</div>
                  <v-chip :color="getStatusColor(project?.status)" size="small" variant="tonal">
                    {{ project?.status }}
                  </v-chip>
                </v-col>
                <v-col cols="12" md="2">
                  <div class="text-caption text-grey-darken-1">Priority</div>
                  <v-chip :color="getPriorityColor(project?.priority)" size="small" variant="tonal">
                    {{ project?.priority }}
                  </v-chip>
                </v-col>
                <v-col cols="12" md="2">
                  <div class="text-caption text-grey-darken-1">Start Date</div>
                  <div class="text-subtitle-1">{{ formatDate(project?.project_start_date) }}</div>
                </v-col>
                <v-col cols="12" md="3">
                  <div class="text-caption text-grey-darken-1">Target Completion</div>
                  <div class="text-subtitle-1">{{ formatDate(project?.target_completion_date) }}</div>
                </v-col>
              </v-row>
            </v-sheet>

            <!-- Gantt Chart Controls -->
            <v-sheet class="pa-3 border-b">
              <v-row align="center">
                <v-col cols="auto">
                  <v-btn-toggle v-model="viewMode" mandatory variant="outlined" density="compact">
                    <v-btn value="day" size="small">Day</v-btn>
                    <v-btn value="week" size="small">Week</v-btn>
                    <v-btn value="month" size="small">Month</v-btn>
                  </v-btn-toggle>
                </v-col>
                <v-col cols="auto">
                  <v-btn size="small" variant="outlined" @click="scrollToToday">
                    <v-icon start>mdi-calendar-today</v-icon>
                    Today
                  </v-btn>
                </v-col>
                <v-col cols="auto">
                  <v-checkbox v-model="showMilestones"
                              label="Show Milestones"
                              hide-details
                              density="compact"></v-checkbox>
                </v-col>
                <v-col cols="auto">
                  <v-checkbox v-model="showDependencies"
                              label="Show Dependencies"
                              hide-details
                              density="compact"></v-checkbox>
                </v-col>
                <v-spacer></v-spacer>
                <v-col cols="auto">
                  <div class="text-caption">
                    Total Tasks: <strong>{{ tasks.length }}</strong> |
                    Completed: <strong>{{ completedTasks }}</strong> |
                    Progress: <strong>{{ overallProgress }}%</strong>
                  </div>
                </v-col>
              </v-row>
            </v-sheet>

            <!-- Gantt Chart Container -->
            <div class="gantt-container" ref="ganttContainer">
              <v-data-table :headers="ganttHeaders"
                            :items="displayTasks"
                            item-value="rowId"
                            :item-class="getRowClass"
                            class="gantt-table"
                            density="compact"
                            fixed-header
                            height="600"
                            hide-default-footer
                            :items-per-page="-1">

                <!-- Task Name Column -->
                <template #item.title="{ item }">
                  <div class="d-flex flex-column">
                    <div class="text-body-2 font-weight-medium">
                      {{ item.title }}
                    </div>
                    <div class="text-caption"
                         :class="item.rowType === 'planned' ? 'text-blue' : 'text-green'">
                      {{ item.rowType === 'planned' ? 'Planned' : 'Actual' }}
                    </div>
                  </div>
                </template>

                <!-- Department Column -->
                <template #item.dept_name="{ item }">
                  <v-chip size="x-small" variant="outlined">
                    {{ item.dept_name || 'N/A' }}
                  </v-chip>
                </template>

                <!-- Duration Column -->
                <template #item.duration="{ item }">
                  <span class="text-body-2">{{ item.duration }} days</span>
                </template>

                <!-- Progress Column -->
                <template #item.per_complete="{ item }">
                  <div v-if="item.rowType === 'actual'" class="d-flex align-center" style="min-width: 120px;">
                    <v-progress-linear :model-value="item.per_complete || 0"
                                       :color="getProgressColor(item.per_complete)"
                                       height="6"
                                       rounded
                                       class="mr-2"
                                       style="width: 60px;">
                    </v-progress-linear>
                    <v-menu>
                      <template #activator="{ props }">
                        <span v-bind="props" class="text-caption cursor-pointer">
                          {{ item.per_complete || 0 }}%
                        </span>
                      </template>
                      <v-card min-width="200">
                        <v-card-text>
                          <div class="text-caption mb-2">Update Progress</div>
                          <v-slider :model-value="item.per_complete || 0"
                                    @update:model-value="(val) => updateTaskProgress(item, val)"
                                    :min="0"
                                    :max="100"
                                    :step="5"
                                    thumb-label
                                    show-ticks="always">
                          </v-slider>
                        </v-card-text>
                      </v-card>
                    </v-menu>
                  </div>
                </template>

                <!-- Timeline Columns -->
                <template v-for="col in timelineColumns"
                          :key="col.value"
                          #[`item.${col.value}`]="{ item }">
                  <div class="gantt-cell-wrapper" :class="{ 'is-today': col.isToday }">
                    <!-- Task Bar -->
                    <div v-if="shouldShowTaskBar(item, col)"
                         class="gantt-bar"
                         :class="getTaskBarClass(item)"
                         :style="getTaskBarStyle(item, col)">
                      <v-tooltip location="top">
                        <template #activator="{ props }">
                          <div v-bind="props" class="gantt-bar-content">
                            <span v-if="shouldShowLabel(item, col)" class="gantt-bar-label">
                              {{ item.title }}
                            </span>
                          </div>
                        </template>
                        <div>
                          <strong>{{ item.title }}</strong><br>
                          Type: {{ item.rowType === 'planned' ? 'Planned' : 'Actual' }}<br>
                          Start: {{ formatDate(getRowStart(item)) }}<br>
                          End: {{ formatDate(getRowEnd(item)) }}<br>
                          Progress: {{ item.per_complete || 0 }}%<br>
                          Status: {{ item.status }}

                          <div v-if="item.rowType === 'planned' && item.planned_revisions?.length" class="mt-2">
                            <strong>Revisions:</strong>
                            <div v-for="rev in item.planned_revisions"
                                 :key="rev.revision_no"
                                 class="text-caption mt-1">
                              Rev {{ rev.revision_no }}:<br>
                              {{ formatDate(rev.old_start_date) }} →
                              {{ formatDate(rev.new_start_date) }}<br>
                              Note: {{ rev.note }}
                            </div>
                          </div>
                        </div>
                      </v-tooltip>
                    </div>

                    <!-- Dependency Arrow Start -->
                    <div v-if="showDependencies && shouldShowDependencyStart(item, col)"
                         class="dependency-arrow-start">
                      <v-icon size="x-small" color="blue-grey">mdi-arrow-right</v-icon>
                    </div>

                    <!-- Milestone Marker -->
                    <div v-if="showMilestones && getMilestoneForColumn(item, col)"
                         class="gantt-milestone-marker">
                      <v-tooltip location="top">
                        <template #activator="{ props }">
                          <v-icon v-bind="props" size="small" color="warning">
                            mdi-flag-variant
                          </v-icon>
                        </template>
                        {{ getMilestoneForColumn(item, col).milestone_name }}<br>
                        Planned: {{ formatDate(getMilestoneForColumn(item, col).planned_date) }}
                      </v-tooltip>
                    </div>
                  </div>
                </template>

              </v-data-table>
            </div>

            <!-- Milestones Section -->
            <v-sheet v-if="showMilestones && milestones.length > 0" class="pa-4 border-t">
              <h3 class="text-h6 mb-3">
                <v-icon class="mr-2">mdi-flag-variant</v-icon>
                Project Milestones
              </h3>
              <v-row>
                <v-col v-for="milestone in milestones"
                       :key="milestone.milestone_id"
                       cols="12"
                       md="4">
                  <v-card variant="outlined" :class="{ 'border-warning': !milestone.actual_date }">
                    <v-card-text>
                      <div class="d-flex align-center justify-space-between">
                        <div class="flex-grow-1">
                          <div class="font-weight-medium">{{ milestone.milestone_name }}</div>
                          <div class="text-caption text-grey">
                            Planned: {{ formatDate(milestone.planned_date) }}
                          </div>
                          <div v-if="milestone.actual_date" class="text-caption">
                            Completed: {{ formatDate(milestone.actual_date) }}
                          </div>
                          <div v-if="milestone.dept_name" class="text-caption text-grey">
                            {{ milestone.dept_name }}
                          </div>
                        </div>
                        <div>
                          <v-chip v-if="milestone.actual_date"
                                  :color="milestone.is_delayed ? 'error' : 'success'"
                                  size="small">
                            {{ milestone.is_delayed ? 'Delayed' : 'On Time' }}
                          </v-chip>
                          <v-btn v-else
                                 size="small"
                                 color="primary"
                                 variant="tonal"
                                 @click="completeMilestone(milestone)">
                            Mark Complete
                          </v-btn>
                        </div>
                      </div>
                    </v-card-text>
                  </v-card>
                </v-col>
              </v-row>
            </v-sheet>

          </v-card-text>
        </v-card>
      </v-col>
    </v-row>

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
  import { ref, computed, onMounted, nextTick } from 'vue'
  import { useRoute } from 'vue-router'
  import { api } from '@/utils/api'

  const route = useRoute()

  // State
  const loading = ref(false)
  const project = ref(null)
  const tasks = ref([])
  const milestones = ref([])
  const viewMode = ref('day')
  const showMilestones = ref(true)
  const showDependencies = ref(true)
  const snackbar = ref(false)
  const snackbarMessage = ref('')
  const snackbarColor = ref('success')
  const ganttContainer = ref(null)

  // Computed
  const completedTasks = computed(() => {
    return tasks.value.filter(t => t.status === 'Completed').length
  })

  const displayTasks = computed(() => {
    const rows = []

    tasks.value.forEach(task => {
      // Planned row
      rows.push({
        ...task,
        rowType: 'planned',
        rowId: `${task.task_id}_planned`
      })

      // Actual row
      rows.push({
        ...task,
        rowType: 'actual',
        rowId: `${task.task_id}_actual`
      })
    })

    return rows
  })

  const overallProgress = computed(() => {
    if (tasks.value.length === 0) return 0
    const total = tasks.value.reduce((sum, t) => sum + (t.per_complete || 0), 0)
    return Math.round(total / tasks.value.length)
  })

  const timelineStart = computed(() => {
    if (tasks.value.length === 0) return new Date()

    const dates = tasks.value
      .map(t => new Date(t.start_date))
      .filter(d => !isNaN(d))

    if (dates.length === 0) return new Date()

    const minDate = new Date(Math.min(...dates))
    minDate.setDate(minDate.getDate() - 3)
    return minDate
  })

  const timelineEnd = computed(() => {
    if (tasks.value.length === 0) {
      const end = new Date()
      end.setMonth(end.getMonth() + 1)
      return end
    }

    const dates = tasks.value
      .map(t => new Date(t.end_date))
      .filter(d => !isNaN(d))

    if (dates.length === 0) {
      const end = new Date()
      end.setMonth(end.getMonth() + 1)
      return end
    }

    const maxDate = new Date(Math.max(...dates))
    maxDate.setDate(maxDate.getDate() + 3)
    return maxDate
  })

  const timelineColumns = computed(() => {
    const columns = []
    const start = new Date(timelineStart.value)
    const end = new Date(timelineEnd.value)
    const today = new Date()
    today.setHours(0, 0, 0, 0)

    if (viewMode.value === 'day') {
      let current = new Date(start)
      while (current <= end) {
        const isToday = current.getTime() === today.getTime()
        columns.push({
          title: current.toLocaleDateString('en-GB', { day: 'numeric', month: 'short' }),
          value: `day_${current.getTime()}`,
          width: '50px',
          align: 'center',
          sortable: false,
          date: new Date(current),
          isToday
        })
        current.setDate(current.getDate() + 1)
      }
    } else if (viewMode.value === 'week') {
      let current = new Date(start)
      current.setDate(current.getDate() - current.getDay() + 1) // Start on Monday

      while (current <= end) {
        const weekEnd = new Date(current)
        weekEnd.setDate(weekEnd.getDate() + 6)

        const isToday = today >= current && today <= weekEnd

        columns.push({
          title: current.toLocaleDateString('en-GB', { day: 'numeric', month: 'short' }),
          value: `week_${current.getTime()}`,
          width: '80px',
          align: 'center',
          sortable: false,
          date: new Date(current),
          endDate: weekEnd,
          isToday
        })
        current.setDate(current.getDate() + 7)
      }
    } else { // month
      let current = new Date(start.getFullYear(), start.getMonth(), 1)

      while (current <= end) {
        const monthEnd = new Date(current.getFullYear(), current.getMonth() + 1, 0)

        const isToday = today.getMonth() === current.getMonth() &&
          today.getFullYear() === current.getFullYear()

        columns.push({
          title: current.toLocaleDateString('en-GB', { month: 'short', year: '2-digit' }),
          value: `month_${current.getTime()}`,
          width: '100px',
          align: 'center',
          sortable: false,
          date: new Date(current),
          endDate: monthEnd,
          isToday
        })
        current.setMonth(current.getMonth() + 1)
      }
    }

    return columns
  })

  const ganttHeaders = computed(() => [
    { title: 'Task', value: 'title', width: '280px', sortable: false, fixed: true },
    { title: 'Department', value: 'dept_name', width: '120px', sortable: false },
    { title: 'Duration', value: 'duration', width: '100px', sortable: false },
    { title: 'Progress', value: 'per_complete', width: '140px', sortable: false },
    ...timelineColumns.value
  ])

  // Methods
  function getStatusColor(status) {
    const colors = {
      'Planning': 'grey',
      'Not Started': 'grey',
      'In Progress': 'blue',
      'On Hold': 'orange',
      'Completed': 'green',
      'Cancelled': 'red'
    }
    return colors[status] || 'grey'
  }

  function getStatusIcon(status) {
    const icons = {
      'Not Started': 'mdi-circle-outline',
      'In Progress': 'mdi-play-circle',
      'On Hold': 'mdi-pause-circle',
      'Completed': 'mdi-check-circle',
      'Cancelled': 'mdi-close-circle'
    }
    return icons[status] || 'mdi-circle-outline'
  }

  function getPriorityColor(priority) {
    const colors = {
      'Low': 'grey',
      'Medium': 'blue',
      'High': 'orange',
      'Critical': 'red'
    }
    return colors[priority] || 'grey'
  }

  function getProgressColor(progress) {
    if (progress >= 100) return 'success'
    if (progress >= 50) return 'primary'
    if (progress > 0) return 'warning'
    return 'grey'
  }

  function formatDate(date) {
    if (!date) return 'N/A'
    return new Date(date).toLocaleDateString('en-GB', {
      day: '2-digit',
      month: 'short',
      year: 'numeric'
    })
  }

  function shouldShowTaskBar(task, column) {
    const start = getRowStart(task)
    const end = getRowEnd(task)

    if (!start || !end) return false

    const taskStart = new Date(start)
    const taskEnd = new Date(end)

    const colStart = new Date(column.date)
    const colEnd = column.endDate ? new Date(column.endDate) : new Date(column.date)

    return taskStart <= colEnd && taskEnd >= colStart
  }

  function getRowStart(task) {
    return task.rowType === 'planned'
      ? task.planned_start_date
      : task.actual_start_date
  }

  function getRowEnd(task) {
    return task.rowType === 'planned'
      ? task.planned_end_date
      : task.actual_end_date
  }

  function getTaskBarStyle(task, column) {
    const taskStart = new Date(getRowStart(task))
    const taskEnd = new Date(getRowEnd(task))
    taskStart.setHours(0, 0, 0, 0)
    taskEnd.setHours(0, 0, 0, 0)

    const colStart = new Date(column.date)
    const colEnd = column.endDate ? new Date(column.endDate) : new Date(column.date)
    colStart.setHours(0, 0, 0, 0)
    colEnd.setHours(23, 59, 59, 999)

    const colDuration = colEnd.getTime() - colStart.getTime()

    let left = 0
    let width = 100

    // Task starts and ends within this column
    if (taskStart >= colStart && taskEnd <= colEnd) {
      left = ((taskStart.getTime() - colStart.getTime()) / colDuration) * 100
      width = ((taskEnd.getTime() - taskStart.getTime()) / colDuration) * 100
    }
    // Task starts before column
    else if (taskStart < colStart && taskEnd <= colEnd) {
      left = 0
      width = ((taskEnd.getTime() - colStart.getTime()) / colDuration) * 100
    }
    // Task ends after column
    else if (taskStart >= colStart && taskEnd > colEnd) {
      left = ((taskStart.getTime() - colStart.getTime()) / colDuration) * 100
      width = ((colEnd.getTime() - taskStart.getTime()) / colDuration) * 100
    }
    // Task spans entire column
    else {
      left = 0
      width = 100
    }

    return {
      left: `${left}%`,
      width: `${width}%`,
      '--progress': `${task.per_complete || 0}%`
    }
  }

  function getTaskBarClass(task) {
    return [
      `gantt-bar-${task.status?.toLowerCase().replace(' ', '-')}`,
    ]
  }

  function shouldShowLabel(task, column) {
    const taskStart = new Date(getRowStart(task))
    taskStart.setHours(0, 0, 0, 0)

    const colStart = new Date(column.date)
    const colEnd = column.endDate ? new Date(column.endDate) : new Date(column.date)
    colStart.setHours(0, 0, 0, 0)
    colEnd.setHours(23, 59, 59, 999)

    // Show label only on first column where task appears
    return taskStart >= colStart && taskStart <= colEnd
  }

  function shouldShowDependencyStart(task, column) {
    if (!task.parent_task_id) return false

    const taskEnd = new Date(task.end_date)
    taskEnd.setHours(0, 0, 0, 0)

    const colStart = new Date(column.date)
    const colEnd = column.endDate ? new Date(column.endDate) : new Date(column.date)
    colStart.setHours(0, 0, 0, 0)
    colEnd.setHours(23, 59, 59, 999)

    return taskEnd >= colStart && taskEnd <= colEnd
  }

  function getMilestoneForColumn(task, column) {
    if (!milestones.value.length) return null

    const colStart = new Date(column.date)
    const colEnd = column.endDate ? new Date(column.endDate) : new Date(column.date)
    colStart.setHours(0, 0, 0, 0)
    colEnd.setHours(23, 59, 59, 999)

    return milestones.value.find(m => {
      if (!m.planned_date) return false
      const mDate = new Date(m.planned_date)
      mDate.setHours(0, 0, 0, 0)
      return mDate >= colStart && mDate <= colEnd
    })
  }

  async function updateTaskStatus(task, newStatus) {
    try {
      const result = await api.put(`/task/${task.task_id}/status`, { status: newStatus })

      if (result?.success) {
        task.status = newStatus

        // Auto-update progress based on status
        if (newStatus === 'Completed') {
          task.per_complete = 100
        } else if (newStatus === 'In Progress' && task.per_complete === 0) {
          task.per_complete = 10
        } else if (newStatus === 'Not Started') {
          task.per_complete = 0
        }

        snackbarMessage.value = 'Task status updated'
        snackbarColor.value = 'success'
        snackbar.value = true
      }
    } catch (error) {
      console.error('Error updating task status:', error)
      snackbarMessage.value = 'Failed to update task status'
      snackbarColor.value = 'error'
      snackbar.value = true
    }
  }

  async function updateTaskProgress(task, progress) {
    try {
      const result = await api.put(`/task/${task.task_id}/progress`, { per_complete: progress })

      if (result?.success) {
        task.per_complete = progress

        // Auto-update status based on progress
        if (progress === 100 && task.status !== 'Completed') {
          task.status = 'Completed'
        } else if (progress > 0 && progress < 100 && task.status === 'Not Started') {
          task.status = 'In Progress'
        }

        snackbarMessage.value = 'Progress updated'
        snackbarColor.value = 'success'
        snackbar.value = true
      }
    } catch (error) {
      console.error('Error updating task progress:', error)
      snackbarMessage.value = 'Failed to update progress'
      snackbarColor.value = 'error'
      snackbar.value = true
    }
  }

  async function completeMilestone(milestone) {
    try {
      const result = await api.patch(`/project/${route.params.id}/milestones/${milestone.milestone_id}/complete`)

      if (result?.success) {
        milestone.actual_date = new Date().toISOString().split('T')[0]
        milestone.is_completed = true

        snackbarMessage.value = 'Milestone marked as complete'
        snackbarColor.value = 'success'
        snackbar.value = true
      }
    } catch (error) {
      console.error('Error completing milestone:', error)
      snackbarMessage.value = 'Failed to complete milestone'
      snackbarColor.value = 'error'
      snackbar.value = true
    }
  }

  function getRowClass(item) {
    return item.rowType === 'planned'
      ? 'gantt-row-planned'
      : 'gantt-row-actual'
  }

  function scrollToToday() {
    const todayColumn = timelineColumns.value.find(col => col.isToday)

    if (todayColumn && ganttContainer.value) {
      nextTick(() => {
        snackbarMessage.value = 'Showing today'
        snackbarColor.value = 'info'
        snackbar.value = true
      })
    }
  }

  async function refreshData() {
    await loadProjectData()
  }

  async function loadProjectData() {
    loading.value = true
    try {
      // Load project details
      const projectResult = await api.get(`/project/${route.params.id}`)
      if (projectResult?.success && projectResult?.data) {
        project.value = projectResult.data
      }

      // Load tasks
      const tasksResult = await api.get(`/project/${route.params.id}/tasks`)
      if (tasksResult?.success && tasksResult?.data) {
        tasks.value = tasksResult.data
      } else if (Array.isArray(tasksResult)) {
        tasks.value = tasksResult
      }

      // Load milestones
      const milestonesResult = await api.get(`/project/${route.params.id}`)
      if (milestonesResult?.success && milestonesResult?.data) {
        milestones.value = milestonesResult.data
      } else if (Array.isArray(milestonesResult)) {
        milestones.value = milestonesResult
      }

    } catch (error) {
      console.error('Error loading project data:', error)
      snackbarMessage.value = 'Failed to load project data'
      snackbarColor.value = 'error'
      snackbar.value = true
    } finally {
      loading.value = false
    }
  }

  onMounted(() => {
    loadProjectData()
  })
</script>

<style scoped>
  .gantt-container {
    overflow-x: auto;
  }

  .gantt-table :deep(.v-table__wrapper) {
    overflow-x: auto;
  }

  .gantt-table :deep(th),
  .gantt-table :deep(td) {
    white-space: nowrap;
    border-right: 1px solid rgba(0, 0, 0, 0.05);
  }

  .gantt-cell-wrapper {
    position: relative;
    height: 32px;
    min-width: 100%;
  }

    .gantt-cell-wrapper.is-today {
      background-color: rgba(33, 150, 243, 0.05);
      border-left: 2px solid rgba(33, 150, 243, 0.3);
    }

  .gantt-bar {
    position: absolute;
    height: 24px;
    border-radius: 4px;
    top: 4px;
    display: flex;
    align-items: center;
    overflow: hidden;
    transition: all 0.2s;
    cursor: pointer;
  }

    .gantt-bar:hover {
      filter: brightness(1.1);
      transform: translateY(-1px);
      box-shadow: 0 2px 4px rgba(0, 0, 0, 0.2);
      z-index: 10;
    }

  .gantt-bar-not-started {
    background: linear-gradient(to right, rgba(158, 158, 158, 0.6) 0%, rgba(158, 158, 158, 0.3) 0% );
    border: 1px solid rgba(158, 158, 158, 0.8);
  }

  .gantt-bar-in-progress {
    background: linear-gradient(to right, rgba(33, 150, 243, 0.9) 0%, rgba(33, 150, 243, 0.9) var(--progress), rgba(33, 150, 243, 0.3) var(--progress), rgba(33, 150, 243, 0.3) 100% );
    border: 1px solid rgba(33, 150, 243, 1);
  }

  .gantt-bar-completed {
    background: rgba(76, 175, 80, 0.8);
    border: 1px solid rgba(76, 175, 80, 1);
  }

  .gantt-bar-on-hold {
    background: rgba(255, 152, 0, 0.8);
    border: 1px solid rgba(255, 152, 0, 1);
  }

  .gantt-bar-content {
    padding: 0 8px;
    width: 100%;
    display: flex;
    align-items: center;
  }

  .gantt-bar-label {
    font-size: 11px;
    font-weight: 500;
    color: white;
    text-shadow: 0 1px 2px rgba(0, 0, 0, 0.3);
    white-space: nowrap;
    overflow: hidden;
    text-overflow: ellipsis;
  }

  .gantt-milestone-marker {
    position: absolute;
    bottom: 0;
    left: 50%;
    transform: translateX(-50%);
    z-index: 10;
  }

  .dependency-arrow-start {
    position: absolute;
    right: 0;
    top: 50%;
    transform: translateY(-50%);
    z-index: 5;
  }

  .border-b {
    border-bottom: 1px solid rgba(0, 0, 0, 0.12);
  }

  .border-t {
    border-top: 1px solid rgba(0, 0, 0, 0.12);
  }

  .cursor-pointer {
    cursor: pointer;
  }

  .gantt-row-planned {
    background-color: rgba(33,150,243,0.05);
  }

  .gantt-row-actual {
    background-color: rgba(76,175,80,0.05);
  }
</style>
