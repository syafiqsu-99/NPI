import { NOTIFICATION_TYPES } from '@/utils/constants'

const SETUP_TYPES = new Set([
  NOTIFICATION_TYPES.PLANNING_STUCK,
  NOTIFICATION_TYPES.PROJECT_PLANNING,
])

export function resolveNotificationRoute(n) {
  if (!n) return null

  if (n.enquiry_id) {
    return {
      path: '/enquiries',
      query: { highlight: String(n.enquiry_id) },
    }
  }

  if (n.task_id) {
    return {
      path: '/tasks',
      query: {
        highlight: String(n.task_id),
        ...(n.proj_id ? { proj: String(n.proj_id) } : {}),
      },
    }
  }

  if (n.proj_id) {
    const path = SETUP_TYPES.has(n.type)
      ? `/projects/${n.proj_id}/setup`
      : `/projects/${n.proj_id}/gantt`
    return { path, query: { highlight: String(n.proj_id) } }
  }

  return null
}
