import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { api } from '@/utils/api'

export const useAuthStore = defineStore('auth', () => {
  const user = ref(null)
  const token = ref(localStorage.getItem('token') || null)
  const loading = ref(false)
  const error = ref(null)

  // ── System role (navigation / page access) ───────────────────────────────
  const isAuthenticated = computed(() => !!token.value && !!user.value)
  const currentUser = computed(() => user.value)
  const userRole = computed(() => user.value?.role ?? 'Member')
  const userDepartment = computed(() => user.value?.department)
  const userDeptId = computed(() => user.value?.dept_id)

  // System role checks (for navigation guards)
  const isAdmin = computed(() => userRole.value === 'Admin')
  const isManager = computed(() => userRole.value === 'Manager')
  const isMember = computed(() => userRole.value === 'Member')

  // ── Project roles (per-project permissions) ──────────────────────────────
  // Cache: { [projectId]: roleName }
  const projectRoleCache = ref({})

  function getProjectRole(projId) {
    return projectRoleCache.value[projId] ?? null
  }

  async function fetchProjectRole(projId) {
    if (!projId || !user.value) return null

    // Admin / Manager bypass at system level
    if (isAdmin.value || isManager.value) {
      projectRoleCache.value[projId] = 'Team Lead'
      return 'Team Lead'
    }

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

  // Project role hierarchy checks
  const PROJECT_ROLE_HIERARCHY = ['Team Lead', 'Member', 'Viewer']

  function hasProjectRole(projId, minimumRole) {
    if (isAdmin.value || isManager.value) return true
    const role = getProjectRole(projId)
    if (!role) return false
    const userIdx = PROJECT_ROLE_HIERARCHY.indexOf(role)
    const minIdx = PROJECT_ROLE_HIERARCHY.indexOf(minimumRole)
    return userIdx >= 0 && minIdx >= 0 && userIdx <= minIdx
  }

  function canEditProject(projId) { return hasProjectRole(projId, 'Team Lead') }
  function canEditTask(projId) { return hasProjectRole(projId, 'Member') }
  function canViewProject(projId) { return hasProjectRole(projId, 'Viewer') }

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
    } catch { }
    user.value = null
    token.value = null
    projectRoleCache.value = {}
    localStorage.removeItem('token')
  }

  function isTokenExpired(t) {
    try {
      const payload = JSON.parse(atob(t.split('.')[1]))
      return payload.exp * 1000 < Date.now()
    } catch {
      return true
    }
  }

  async function checkAuth() {
    if (!token.value) return false
    if (isTokenExpired(token.value)) {
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
    user,
    token,
    loading,
    error,

    // Computed
    isAuthenticated,
    currentUser,
    userRole,
    userDepartment,
    userDeptId,
    isAdmin,
    isManager,
    isMember,

    // Project role helpers
    projectRoleCache,
    getProjectRole,
    fetchProjectRole,
    hasProjectRole,
    canEditProject,
    canEditTask,
    canViewProject,

    // Actions
    login,
    logout,
    checkAuth,
  }
})
