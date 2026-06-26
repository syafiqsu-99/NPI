import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { api } from '@/utils/api'
import { SYSTEM_ROLES, PROJECT_ROLES, PROJECT_ROLE_HIERARCHY, DEPT_NAMES } from '@/utils/constants.js'

export const useAuthStore = defineStore('auth', () => {
  const user    = ref(null)
  const token   = ref(localStorage.getItem('token') || null)
  const loading = ref(false)
  const error   = ref(null)

  // ── System-level role ────────────────────────────────────────────────────

  const isAuthenticated = computed(() => !!token.value && !!user.value)
  const currentUser     = computed(() => user.value)
  const userRole        = computed(() => user.value?.role ?? SYSTEM_ROLES.MEMBER)
  const userDepartment  = computed(() => user.value?.department)
  const userDeptId      = computed(() => user.value?.dept_id)
 
  const isAdmin   = computed(() => userRole.value === SYSTEM_ROLES.ADMIN)
  const isManager = computed(() => userRole.value === SYSTEM_ROLES.MANAGER)
  const isMember  = computed(() => userRole.value === SYSTEM_ROLES.MEMBER)

  const isSalesUser = computed(() =>
    user.value?.department === DEPT_NAMES.SALES
  )

  // ── Project-level role cache ─────────────────────────────────────────────

  const projectRoleCache = ref({})

  function getProjectRole(projId) {
    if (isAdmin.value || isManager.value) return PROJECT_ROLES.TEAM_LEAD
    return projectRoleCache.value[projId] ?? null
  }

  async function fetchProjectRole(projId) {
    if (!projId || !user.value) return null
    if (isAdmin.value || isManager.value) {
      projectRoleCache.value[projId] = PROJECT_ROLES.TEAM_LEAD
      return PROJECT_ROLES.TEAM_LEAD
    }
    if (projectRoleCache.value[projId]) return projectRoleCache.value[projId]

    try {
      const result = await api.get(`/projectrole/${projId}/my-role`)
      const role = result?.data?.role_name ?? PROJECT_ROLES.VIEWER
      projectRoleCache.value[projId] = role
      return role
    } catch {
      projectRoleCache.value[projId] = PROJECT_ROLES.VIEWER
      return PROJECT_ROLES.VIEWER
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

  // ── Shared permission helpers ────────────────────────────────────────────
  const canStartProject = computed(() => isAdmin.value || isManager.value)

  const canDeleteProject = computed(() => isAdmin.value || isManager.value)

  const canCreateEnquiry = computed(
    () => isAdmin.value || isManager.value || isSalesUser.value
  )

  function canDeleteEnquiry(enquiry) {
    if (isAdmin.value || isManager.value) return true
    return (
      isSalesUser.value &&
      enquiry.status === 'Draft' &&
      Number(enquiry.created_by) === user.value?.user_id
    )
  }

  function canManageProject(project) {
    if (isAdmin.value || isManager.value) return true
    return hasProjectRole(project.proj_id, PROJECT_ROLES.TEAM_LEAD)
  }

  // Convenience wrappers kept for backward compatibility with existing call sites.
  const canEditProject  = (projId) => hasProjectRole(projId, PROJECT_ROLES.TEAM_LEAD)
  const canEditTask     = (projId) => hasProjectRole(projId, PROJECT_ROLES.MEMBER)
  const canViewProject  = (projId) => hasProjectRole(projId, PROJECT_ROLES.VIEWER)

  // ── Auth actions ─────────────────────────────────────────────────────────

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
    try { await api.post('/auth/logout', {}) } catch { /* ignore */ }
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
    user, token, loading, error,

    isAuthenticated, currentUser, userRole,
    userDepartment, userDeptId,
    isAdmin, isManager, isMember, isSalesUser,

    projectRoleCache,
    getProjectRole, fetchProjectRole, hasProjectRole,

    canStartProject, canCreateEnquiry,
    canDeleteEnquiry, canManageProject,
    canEditProject, canEditTask, canViewProject,
    canDeleteProject,

    login, logout, checkAuth,
  }
})
