import { defineStore } from 'pinia'
import { computed, ref } from 'vue'

export const useDashboardStore = defineStore('dashboard', () => {
  /* -------------------- MOCK DATA -------------------- */
  const projects = ref([
    {
      proj_id: 1,
      proj_name: 'Project A',
      project_start_date: '2026-01-05',
      target_completion_date: '2026-02-20',
      status: 'In Progress',
    },
    {
      proj_id: 2,
      proj_name: 'Project B',
      project_start_date: '2026-02-01',
      target_completion_date: '2026-04-10',
      status: 'On Hold',
    },
    {
      proj_id: 3,
      proj_name: 'Project C',
      project_start_date: '2025-11-01',
      target_completion_date: '2026-02-05',
      status: 'Completed',
    },
    {
      proj_id: 4,
      proj_name: 'Project D',
      project_start_date: '2026-01-20',
      target_completion_date: '2026-05-01',
      status: 'In Progress',
    },
  ])

  const milestones = ref([
    {
      milestone_id: 1,
      proj_id: 1,
      milestone_name: 'Design',
      planned_date: '2026-02-10',
      actual_date: '2026-02-18',
    },
    {
      milestone_id: 2,
      proj_id: 1,
      milestone_name: 'Sampling',
      planned_date: '2026-02-05',
      actual_date: null,
    },
    {
      milestone_id: 3,
      proj_id: 4,
      milestone_name: 'Tooling',
      planned_date: '2026-01-25',
      actual_date: null,
    },
  ])

  const overdueTasks = ref([
    {
      task_id: 1,
      title: 'Customer Approval',
      proj_name: 'Project A',
      end_date: '2026-01-20',
      days_overdue: 10,
    },
    {
      task_id: 2,
      title: 'Design Freeze',
      proj_name: 'Project D',
      end_date: '2026-01-28',
      days_overdue: 2,
    },
  ])

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
        width: '36px',
      }
    })
  })

  const headersTimeline = computed(() => [
    {
      title: 'PROJECT',
      value: 'proj_name',
      width: '220px',
      fixed: true,
    },
    ...additionalDateHeaders.value,
  ])

  /* -------------------- HELPERS -------------------- */
  const projectStatusColor = status => {
    if (status === 'Completed') return 'green'
    if (status === 'On Hold') return 'red'
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

  return {
    headersTimeline,
    additionalDateHeaders,
    timelineItems,
    overdueTasks,
    calendarEvents,
    kpis,
    projectStatusColor,
  }
})
