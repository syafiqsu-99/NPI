import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { api } from '@/utils/api'

export const useAuthStore = defineStore('auth', () => {
  const user = ref(null)
  const token = ref(localStorage.getItem('token') || null)
  const loading = ref(false)
  const error = ref(null)

  // ── System-level role (controls page/navigation access) ───────────────────

  const isAuthenticated = computed(() => !!token.value && !!user.value)
  const currentUser = computed(() => user.value)
  const userRole = computed(() => user.value?.role ?? 'Member')
  const userDepartment = computed(() => user.value?.department)
  const userDeptId = computed(() => user.value?.dept_id)

  const isAdmin = computed(() => userRole.value === 'Admin')
  const isManager = computed(() => userRole.value === 'Manager')
  const isMember = computed(() => userRole.value === 'Member')

  const projectRoleCache = ref({})

  const PROJECT_ROLE_HIERARCHY = ['Team Lead', 'Member', 'Viewer']

  function getProjectRole(projId) {
    if (isAdmin.value || isManager.value) return 'Team Lead'
    return projectRoleCache.value[projId] ?? null
  }

  async function fetchProjectRole(projId) {
    if (!projId || !user.value) return null
    if (isAdmin.value || isManager.value) {
      projectRoleCache.value[projId] = 'Team Lead'
      return 'Team Lead'
    }
    if (projectRoleCache.value[projId]) return projectRoleCache.value[projId]

    try {
      const result = await api.get(`/projectrole/${projId}/my-role`)
      const role = result?.data?.role_name ?? 'Viewer'
      projectRoleCache.value[projId] = role
      return role
    } catch {
      projectRoleCache.value[projId] = 'Viewer'
      return 'Viewer'
    }
  }

  function hasProjectRole(projId, minimumRole) {
    if (isAdmin.value || isManager.value) return true
    const role = getProjectRole(projId)
    if (!role) return false
    const userIdx = PROJECT_ROLE_HIERARCHY.indexOf(role)
    const minIdx = PROJECT_ROLE_HIERARCHY.indexOf(minimumRole)
    return userIdx >= 0 && minIdx >= 0 && userIdx <= minIdx
  }

  // Convenience wrappers
  const canEditProject = (projId) => hasProjectRole(projId, 'Team Lead')
  const canEditTask = (projId) => hasProjectRole(projId, 'Member')
  const canViewProject = (projId) => hasProjectRole(projId, 'Viewer')

  // ── Auth actions ──────────────────────────────────────────────────────────

  async function login(username, password) {
    loading.value = true
    error.value = null
    try {
      const response = await api.post('/Auth/login', { username, password })
      token.value = response.token
      user.value = response.user
      localStorage.setItem('token', response.token)
      return { success: true }
    } catch (err) {
      error.value = err.message
      return { success: false, message: err.message }
    } finally {
      loading.value = false
    }
  }

  async function logout() {
    try {
      await api.post('/auth/logout', {})
    } catch { /* ignore */ }
    user.value = null
    token.value = null
    projectRoleCache.value = {}
    localStorage.removeItem('token')
  }

  async function checkAuth() {
    if (!token.value) return false

    try {
      const payload = JSON.parse(atob(token.value.split('.')[1]))
      if (payload.exp * 1000 < Date.now()) {
        user.value = null
        token.value = null
        localStorage.removeItem('token')
        return false
      }
    } catch {
      user.value = null
      token.value = null
      localStorage.removeItem('token')
      return false
    }

    try {
      const response = await api.get('/auth/me')
      user.value = response
      return true
    } catch (err) {
      if (err.status === 401) {
        user.value = null
        token.value = null
        localStorage.removeItem('token')
      }
      return false
    }
  }

  return {
    // State
    user, token, loading, error,

    // System role computed
    isAuthenticated, currentUser, userRole,
    userDepartment, userDeptId,
    isAdmin, isManager, isMember,

    // Project role
    projectRoleCache,
    getProjectRole,
    fetchProjectRole,
    hasProjectRole,
    canEditProject,
    canEditTask,
    canViewProject,

    // Actions
    login, logout, checkAuth,
  }
})
