import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { api } from '@/utils/api'

export const useDashboardStore = defineStore('dashboard', () => {
  /* -------------------- STATE -------------------- */
  const projects = ref([])
  const milestones = ref([])
  const tasks = ref([])
  const loading = ref(false)
  const error = ref(null)

  /* -------------------- TIME AXIS -------------------- */
  const firstMonday = (() => {
    const d = new Date()
    d.setDate(d.getDate() - d.getDay() + 1)
    return d
  })()

  const additionalDateHeaders = computed(() => {
    return Array.from({ length: 20 }).map((_, i) => {
      const date = new Date(firstMonday)
      date.setDate(firstMonday.getDate() + i * 7)
      return {
        title: date.toLocaleDateString('en-GB', { day: 'numeric', month: 'short' }),
        value: `week_${i}`,
        sortable: false,
        align: 'center',
      }
    })
  })

  const headersTimeline = computed(() => [
    {
      title: 'PROJECT',
      value: 'proj_name',
      width: '30%',
      fixed: true,
    },
    ...additionalDateHeaders.value,
  ])

  /* -------------------- HELPERS -------------------- */
  const projectStatusColor = status => {
    if (status === 'Completed') return 'green'
    if (status === 'On Hold') return 'red'
    if (status === 'Planning') return 'grey'
    return 'amber'
  }

  /* -------------------- GANTT MATRIX -------------------- */
  const timelineItems = computed(() => {
    return projects.value.map(p => {
      const row = {
        proj_id: p.proj_id,
        proj_name: p.proj_name,
        status: p.status,
      }

      additionalDateHeaders.value.forEach((h, index) => {
        const weekStart = new Date(firstMonday)
        weekStart.setDate(firstMonday.getDate() + index * 7)

        const weekEnd = new Date(weekStart)
        weekEnd.setDate(weekStart.getDate() + 6)

        milestones.value
          .filter(m => m.proj_id === p.proj_id)
          .forEach(m => {
            const start = new Date(m.planned_date)
            const end = m.actual_date ? new Date(m.actual_date) : new Date()

            if (start <= weekEnd && end >= weekStart) {
              row[h.value] = {
                state: end > start ? 'delayed' : 'on-track',
                milestone_name: m.milestone_name,
              }
            }
          })
      })

      return row
    })
  })

  /* -------------------- OVERDUE TASKS -------------------- */
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
      .slice(0, 10)
  })

  /* -------------------- KPI -------------------- */
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

  /* -------------------- CALENDAR -------------------- */
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

  /* -------------------- ACTIONS -------------------- */
  async function fetchDashboardData() {
    loading.value = true
    error.value = null

    try {
      // Fetch all projects
      const projectsResult = await api.get('/project')
      if (projectsResult?.success && projectsResult?.data) {
        projects.value = projectsResult.data
      } else if (Array.isArray(projectsResult)) {
        projects.value = projectsResult
      } else {
        projects.value = []
      }

      // Fetch tasks for all projects
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

      // Fetch milestones for all projects
      const milestonesPromises = projects.value.map(p =>
        api.get(`/project/${p.proj_id}/milestones`)
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

      return { success: true }
    } catch (err) {
      error.value = err.message || 'Failed to fetch dashboard data'
      console.error('Dashboard fetch error:', err)
      return { success: false, error: error.value }
    } finally {
      loading.value = false
    }
  }

  async function refreshData() {
    return await fetchDashboardData()
  }

  return {
    // State
    projects,
    milestones,
    tasks,
    loading,
    error,

    // Computed
    headersTimeline,
    additionalDateHeaders,
    timelineItems,
    overdueTasks,
    calendarEvents,
    kpis,

    // Methods
    projectStatusColor,
    fetchDashboardData,
    refreshData,
  }
})
