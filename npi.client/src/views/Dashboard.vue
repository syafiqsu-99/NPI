<template>
  <v-container fluid fill-height class="d-flex flex-column">
    <v-row no-gutters align="center" justify="center">
      <v-col cols="12" class="text-center">
        <h2 class="font-weight-bold">
          <v-icon class="mr-2">mdi-clipboard-list-outline</v-icon>
          Dashboard
        </h2>
      </v-col>
    </v-row>

    <!-- Loading State -->
    <v-row v-if="loading" no-gutters class="fill-height" align="center" justify="center">
      <v-col cols="auto">
        <v-progress-circular indeterminate color="primary" size="64"></v-progress-circular>
      </v-col>
    </v-row>

    <template v-else>
      <!-- KPI ROW -->
      <v-row no-gutters>
        <v-col v-for="kpi in kpis" :key="kpi.title" cols="3">
          <v-card class="fill-height ms-1" :color="kpi.color" variant="tonal">
            <v-card-title class="text-caption">
              {{ kpi.title }}
            </v-card-title>
            <v-card-text class="text-h4 font-weight-bold">
              {{ kpi.value }}
            </v-card-text>
          </v-card>
        </v-col>
      </v-row>

      <!-- MAIN CONTENT -->
      <v-row no-gutters>
        <!-- LEFT COLUMN -->
        <v-col cols="4" class="fill-height d-flex flex-column">
          <!-- CALENDAR -->
          <v-card class="flex-grow-1 ma-1 d-flex flex-column">
            <v-sheet class="d-flex" tile>
              <v-btn class="ma-2"
                     variant="text"
                     icon
                     @click="$refs.calendar.prev()">
                <v-icon>mdi-chevron-left</v-icon>
              </v-btn>
              <v-spacer></v-spacer>
              <v-card-title>Project Calendar</v-card-title>
              <v-spacer></v-spacer>
              <v-btn class="ma-2"
                     variant="text"
                     icon
                     @click="$refs.calendar.next()">
                <v-icon>mdi-chevron-right</v-icon>
              </v-btn>
            </v-sheet>

            <v-sheet class="flex-grow-1" height="450">
              <v-calendar ref="calendar"
                          type="month"
                          :event-height="20"
                          :event-margin-bottom="0"
                          :weekdays="[0, 1, 2, 3, 4, 5, 6]"
                          :events="calendarEvents"
                          :event-color="e => e.color"
                          class="fill-height"
                          @click:event="onCalendarClick" />
            </v-sheet>
          </v-card>

          <!-- OVERDUE TASKS -->
          <v-card class="flex-grow-1 ma-1">
            <v-card-title>
              <div class="d-flex align-center justify-space-between w-100">
                <span>Overdue Tasks</span>
                <v-chip size="small" color="error" v-if="overdueTasks.length > 0">
                  {{ overdueTasks.length }}
                </v-chip>
              </div>
            </v-card-title>
            <v-divider />

            <v-list v-if="overdueTasks.length > 0" density="compact">
              <v-list-item v-for="t in overdueTasks"
                           :key="t.task_id"
                           @click="goToProject(t.proj_id)">
                <v-list-item-title>
                  {{ t.title }}
                </v-list-item-title>
                <v-list-item-subtitle>
                  {{ t.proj_name }} · Due {{ formatDate(t.end_date) }}
                </v-list-item-subtitle>

                <template #append>
                  <v-chip color="red" size="small">
                    {{ t.days_overdue }}d
                  </v-chip>
                </template>
              </v-list-item>
            </v-list>

            <v-card-text v-else class="text-center text-grey">
              <v-icon size="large" class="mb-2">mdi-check-circle-outline</v-icon>
              <div>No overdue tasks</div>
            </v-card-text>
          </v-card>
        </v-col>

        <!-- RIGHT COLUMN - PROJECT TIMELINE -->
        <v-col cols="8" class="fill-height">
          <v-card class="fill-height">
            <v-card-title>
              <div class="d-flex align-center justify-space-between w-100">
                <span>Projects Overview Timeline</span>
                <v-btn size="small" variant="outlined" @click="loadDashboardData">
                  <v-icon start>mdi-refresh</v-icon>
                  Refresh
                </v-btn>
              </div>
            </v-card-title>
            <v-card-subtitle class="text-caption">
              Click on a project to view detailed Gantt chart
            </v-card-subtitle>
            <v-divider />

            <v-data-table-virtual v-if="projectTimeline.length > 0"
                                  fixed-header
                                  :headers="timelineHeaders"
                                  :items="projectTimeline"
                                  height="100%"
                                  class="project-timeline-table fill-height"
                                  @click:row="(_, row) => goToProject(row.item.proj_id)">

              <!-- Project Name Column -->
              <template #item.proj_name="{ item }">
                <div class="d-flex align-center">
                  <v-chip :color="projectStatusColor(item.status)"
                          variant="tonal"
                          size="small"
                          class="mr-2">
                    {{ item.status }}
                  </v-chip>
                  <span class="text-body-2">{{ item.proj_name }}</span>
                </div>
              </template>

              <!-- Progress Column -->
              <template #item.progress="{ item }">
                <div class="d-flex align-center" style="min-width: 100px;">
                  <v-progress-linear :model-value="item.progress"
                                     :color="getProgressColor(item.progress)"
                                     height="6"
                                     rounded
                                     class="mr-2"
                                     style="width: 60px;">
                  </v-progress-linear>
                  <span class="text-caption">{{ item.progress }}%</span>
                </div>
              </template>

              <!-- Timeline Columns - Project Duration Bars -->
              <template v-for="col in timelineWeeks"
                        :key="col.value"
                        #[`item.${col.value}`]="{ item }">
                <div class="timeline-cell" :class="{ 'is-today': col.isToday }">
                  <div v-if="shouldShowProjectBar(item, col)"
                       class="project-bar"
                       :class="getProjectBarClass(item)"
                       :style="getProjectBarStyle(item, col)">
                    <v-tooltip location="top">
                      <template #activator="{ props }">
                        <div v-bind="props" class="project-bar-content"></div>
                      </template>
                      <div>
                        <strong>{{ item.proj_name }}</strong><br>
                        Start: {{ formatDate(item.project_start_date) }}<br>
                        Target: {{ formatDate(item.target_completion_date) }}<br>
                        Progress: {{ item.progress }}%<br>
                        Status: {{ item.status }}
                      </div>
                    </v-tooltip>
                  </div>

                  <!-- Milestone markers -->
                  <div v-if="hasUpcomingMilestone(item, col)"
                       class="timeline-milestone">
                    <v-tooltip location="top">
                      <template #activator="{ props }">
                        <v-icon v-bind="props" size="x-small" color="warning">
                          mdi-flag-variant
                        </v-icon>
                      </template>
                      Upcoming Milestone
                    </v-tooltip>
                  </div>
                </div>
              </template>

            </v-data-table-virtual>

            <v-card-text v-else class="text-center text-grey pa-8">
              <v-icon size="64" class="mb-4">mdi-folder-open-outline</v-icon>
              <div>No projects to display</div>
            </v-card-text>
          </v-card>
        </v-col>
      </v-row>
    </template>

    <!-- Snackbar for errors -->
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
  import { useRouter } from 'vue-router'
  import { api } from '@/utils/api'

  const router = useRouter()

  // State
  const loading = ref(false)
  const projects = ref([])
  const milestones = ref([])
  const tasks = ref([])
  const snackbar = ref(false)
  const snackbarMessage = ref('')
  const snackbarColor = ref('success')

  // Navigation
  const goToProject = projId => {
    router.push(`/projects/${projId}/gantt`)
  }

  const onCalendarClick = ({ event }) => {
    router.push(`/projects/${event.proj_id}/gantt`)
  }

  // Date formatting
  const formatDate = d => {
    if (!d) return 'N/A'
    return new Date(d).toLocaleDateString('en-GB', {
      day: '2-digit',
      month: 'short',
      year: 'numeric'
    })
  }

  // Status color
  const projectStatusColor = status => {
    if (status === 'Completed') return 'green'
    if (status === 'On Hold') return 'red'
    if (status === 'Planning') return 'grey'
    return 'amber'
  }

  const getProgressColor = (progress) => {
    if (progress >= 100) return 'success'
    if (progress >= 50) return 'primary'
    if (progress > 0) return 'warning'
    return 'grey'
  }

  // KPIs
  const kpis = computed(() => [
    {
      title: 'In Progress',
      value: projects.value.filter(p => p.status === 'In Progress').length,
      color: 'amber',
    },
    {
      title: 'On Hold',
      value: projects.value.filter(p => p.status === 'On Hold').length,
      color: 'red',
    },
    {
      title: 'Completed',
      value: projects.value.filter(p => p.status === 'Completed').length,
      color: 'green',
    },
    {
      title: 'Total Projects',
      value: projects.value.length,
      color: 'blue',
    },
  ])

  // Overdue Tasks
  const overdueTasks = computed(() => {
    const today = new Date()
    today.setHours(0, 0, 0, 0)

    return tasks.value
      .filter(t => {
        if (t.status === 'Completed') return false
        const endDate = new Date(t.end_date)
        endDate.setHours(0, 0, 0, 0)
        return endDate < today
      })
      .map(t => {
        const endDate = new Date(t.end_date)
        const diffTime = today - endDate
        const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24))

        return {
          ...t,
          days_overdue: diffDays
        }
      })
      .sort((a, b) => b.days_overdue - a.days_overdue)
      .slice(0, 10) // Show top 10
  })

  // Calendar Events
  const calendarEvents = computed(() =>
    projects.value.map(p => ({
      name: p.proj_name,
      start: p.project_start_date,
      end: p.target_completion_date,
      color: projectStatusColor(p.status),
      allDay: true,
      proj_id: p.proj_id,
    }))
  )

  // Timeline for Projects (Weeks view)
  const firstMonday = computed(() => {
    const d = new Date()
    d.setDate(d.getDate() - d.getDay() + 1)
    d.setDate(d.getDate() - 14) // Start 2 weeks ago
    return d
  })

  const timelineWeeks = computed(() => {
    const weeks = []
    const today = new Date()
    today.setHours(0, 0, 0, 0)

    for (let i = 0; i < 20; i++) {
      const weekStart = new Date(firstMonday.value)
      weekStart.setDate(firstMonday.value.getDate() + i * 7)

      const weekEnd = new Date(weekStart)
      weekEnd.setDate(weekStart.getDate() + 6)

      const isToday = today >= weekStart && today <= weekEnd

      weeks.push({
        title: weekStart.toLocaleDateString('en-GB', { day: 'numeric', month: 'short' }),
        value: `week_${i}`,
        width: '50px',
        align: 'center',
        sortable: false,
        weekStart,
        weekEnd,
        isToday
      })
    }

    return weeks
  })

  const timelineHeaders = computed(() => [
    {
      title: 'PROJECT',
      value: 'proj_name',
      width: '300px',
      fixed: true,
      sortable: false
    },
    {
      title: 'PROGRESS',
      value: 'progress',
      width: '120px',
      sortable: false
    },
    ...timelineWeeks.value,
  ])

  // Project Timeline with Progress
  const projectTimeline = computed(() => {
    return projects.value.map(p => {
      // Calculate project progress from tasks
      const projectTasks = tasks.value.filter(t => t.proj_id === p.proj_id)
      let progress = 0

      if (projectTasks.length > 0) {
        const totalProgress = projectTasks.reduce((sum, t) => sum + (t.per_complete || 0), 0)
        progress = Math.round(totalProgress / projectTasks.length)
      }

      return {
        proj_id: p.proj_id,
        proj_name: p.proj_name,
        status: p.status,
        project_start_date: p.project_start_date,
        target_completion_date: p.target_completion_date,
        progress
      }
    })
  })

  // Timeline Helper Functions
  function shouldShowProjectBar(project, week) {
    if (!project.project_start_date || !project.target_completion_date) return false

    const projectStart = new Date(project.project_start_date)
    const projectEnd = new Date(project.target_completion_date)
    projectStart.setHours(0, 0, 0, 0)
    projectEnd.setHours(23, 59, 59, 999)

    return projectStart <= week.weekEnd && projectEnd >= week.weekStart
  }

  function getProjectBarStyle(project, week) {
    const projectStart = new Date(project.project_start_date)
    const projectEnd = new Date(project.target_completion_date)
    projectStart.setHours(0, 0, 0, 0)
    projectEnd.setHours(23, 59, 59, 999)

    const weekDuration = week.weekEnd.getTime() - week.weekStart.getTime()

    let left = 0
    let width = 100

    // Project starts and ends within this week
    if (projectStart >= week.weekStart && projectEnd <= week.weekEnd) {
      left = ((projectStart.getTime() - week.weekStart.getTime()) / weekDuration) * 100
      width = ((projectEnd.getTime() - projectStart.getTime()) / weekDuration) * 100
    }
    // Project starts before week
    else if (projectStart < week.weekStart && projectEnd <= week.weekEnd) {
      left = 0
      width = ((projectEnd.getTime() - week.weekStart.getTime()) / weekDuration) * 100
    }
    // Project ends after week
    else if (projectStart >= week.weekStart && projectEnd > week.weekEnd) {
      left = ((projectStart.getTime() - week.weekStart.getTime()) / weekDuration) * 100
      width = ((week.weekEnd.getTime() - projectStart.getTime()) / weekDuration) * 100
    }
    // Project spans entire week
    else {
      left = 0
      width = 100
    }

    return {
      left: `${left}%`,
      width: `${width}%`,
      '--progress': `${project.progress}%`
    }
  }

  function getProjectBarClass(project) {
    const status = project.status?.toLowerCase().replace(' ', '-')
    return [`project-bar-${status}`]
  }

  function hasUpcomingMilestone(project, week) {
    return milestones.value.some(m => {
      if (m.proj_id !== project.proj_id || !m.planned_date || m.actual_date) return false

      const mDate = new Date(m.planned_date)
      mDate.setHours(0, 0, 0, 0)

      return mDate >= week.weekStart && mDate <= week.weekEnd
    })
  }

  // Data Loading
  async function loadDashboardData() {
    loading.value = true

    try {
      // Load all projects
      const projectsResult = await api.get('/project')
      if (projectsResult?.success && projectsResult?.data) {
        projects.value = projectsResult.data
      } else if (Array.isArray(projectsResult)) {
        projects.value = projectsResult
      } else {
        projects.value = []
      }

      // Load all tasks for overdue calculation and progress
      const tasksPromises = projects.value.map(p =>
        api.get(`/project/${p.proj_id}/tasks`)
          .then(result => {
            const taskData = result?.data || result || []
            return taskData.map(t => ({
              ...t,
              proj_id: p.proj_id,
              proj_name: p.proj_name
            }))
          })
          .catch(() => [])
      )

      const allTasks = await Promise.all(tasksPromises)
      tasks.value = allTasks.flat()

      // Load all milestones
      const milestonesPromises = projects.value.map(p =>
        api.get(`/project/${p.proj_id}`)
          .then(result => {
            const milestoneData = result?.data || result || []
            return milestoneData.map(m => ({
              ...m,
              proj_id: p.proj_id
            }))
          })
          .catch(() => [])
      )

      const allMilestones = await Promise.all(milestonesPromises)
      milestones.value = allMilestones.flat()

    } catch (error) {
      console.error('Error loading dashboard data:', error)
      snackbarMessage.value = 'Failed to load dashboard data'
      snackbarColor.value = 'error'
      snackbar.value = true
    } finally {
      loading.value = false
    }
  }

  onMounted(() => {
    loadDashboardData()
  })
</script>

<style scoped>
  .project-timeline-table :deep(.v-table__wrapper) {
    cursor: pointer;
  }

  .project-timeline-table :deep(tbody tr:hover) {
    background-color: rgba(0, 0, 0, 0.04);
  }

  .project-timeline-table :deep(th),
  .project-timeline-table :deep(td) {
    border-right: 1px solid rgba(0, 0, 0, 0.05);
  }

  .timeline-cell {
    position: relative;
    height: 32px;
    min-width: 100%;
  }

    .timeline-cell.is-today {
      background-color: rgba(33, 150, 243, 0.08);
      border-left: 2px solid rgba(33, 150, 243, 0.4);
    }

  .project-bar {
    position: absolute;
    height: 20px;
    border-radius: 10px;
    top: 6px;
    display: flex;
    align-items: center;
    overflow: hidden;
    transition: all 0.2s;
    cursor: pointer;
  }

    .project-bar:hover {
      transform: translateY(-1px);
      box-shadow: 0 2px 4px rgba(0, 0, 0, 0.2);
      z-index: 10;
      height: 22px;
      top: 5px;
    }

  .project-bar-planning {
    background: linear-gradient(to right, rgba(158, 158, 158, 0.7) 0%, rgba(158, 158, 158, 0.4) 100% );
    border: 1px solid rgba(158, 158, 158, 0.8);
  }

  .project-bar-in-progress {
    background: linear-gradient(to right, rgba(33, 150, 243, 0.9) 0%, rgba(33, 150, 243, 0.9) var(--progress), rgba(33, 150, 243, 0.3) var(--progress), rgba(33, 150, 243, 0.3) 100% );
    border: 1px solid rgba(33, 150, 243, 1);
  }

  .project-bar-completed {
    background: rgba(76, 175, 80, 0.8);
    border: 1px solid rgba(76, 175, 80, 1);
  }

  .project-bar-on-hold {
    background: rgba(255, 152, 0, 0.8);
    border: 1px solid rgba(255, 152, 0, 1);
  }

  .project-bar-content {
    width: 100%;
    height: 100%;
  }

  .timeline-milestone {
    position: absolute;
    bottom: 2px;
    left: 50%;
    transform: translateX(-50%);
    z-index: 5;
  }
</style>
