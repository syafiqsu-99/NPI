import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { api } from '@/utils/api'
import {
  DEPT_ICON_HINTS,
  DEFAULT_DEPT_COLOR,
  DEFAULT_DEPT_ICON,
} from '@/utils/constants'

export const useSettingsStore = defineStore('setting', () => {
  const users = ref([])
  const roles = ref([])
  const departments = ref([])
  const loading = ref(false)
  const error = ref(null)

  // ============ User Management ============
  async function fetchUsers() {
    loading.value = true
    error.value = null
    try {
      const result = await api.get('/user')
      const list = Array.isArray(result) ? result
        : Array.isArray(result?.data) ? result.data
          : []
      users.value = list
      return { success: true, data: users.value }
    } catch (err) {
      error.value = err.message
      users.value = []
      return { success: false, message: err.message }
    } finally {
      loading.value = false
    }
  }

  async function getUserById(userId) {
    try {
      const result = await api.get(`/user/${userId}`)
      return result
    } catch (err) {
      error.value = err.message
      return { success: false, message: err.message }
    }
  }

  async function createUser(userData) {
    try {
      const result = await api.post('/user', userData)
      if (result?.success) {
        await fetchUsers()
      }
      return result
    } catch (err) {
      error.value = err.message
      return { success: false, message: err.message }
    }
  }

  async function updateUser(userId, userData) {
    try {
      const result = await api.put(`/user/${userId}`, userData)
      if (result?.success) {
        await fetchUsers()
      }
      return result
    } catch (err) {
      error.value = err.message
      return { success: false, message: err.message }
    }
  }

  async function deleteUser(userId) {
    try {
      const result = await api.delete(`/user/${userId}`)
      if (result?.success) {
        await fetchUsers()
      }
      return result
    } catch (err) {
      error.value = err.message
      return { success: false, message: err.message }
    }
  }

  async function toggleUserStatus(userId) {
    try {
      const result = await api.patch(`/user/${userId}/toggle-status`)
      if (result?.success) {
        await fetchUsers()
      }
      return result
    } catch (err) {
      error.value = err.message
      return { success: false, message: err.message }
    }
  }

  async function resetUserPassword(userId, newPassword) {
    try {
      const result = await api.put(`/user/${userId}/reset-password`, {
        new_password: newPassword
      })
      return result
    } catch (err) {
      error.value = err.message
      return { success: false, message: err.message }
    }
  }

  // ============ Role Management ============
  async function fetchRoles() {
    loading.value = true
    error.value = null
    try {
      const result = await api.get('/role')
      const list = Array.isArray(result) ? result
        : Array.isArray(result?.data) ? result.data
          : []
      roles.value = list
      return { success: true, data: roles.value }
    } catch (err) {
      error.value = err.message
      return { success: false, message: err.message }
    } finally {
      loading.value = false
    }
  }

  async function getRoleById(roleId) {
    try {
      const result = await api.get(`/role/${roleId}`)
      return result
    } catch (err) {
      error.value = err.message
      return { success: false, message: err.message }
    }
  }

  async function createRole(roleData) {
    try {
      const result = await api.post('/role', roleData)
      if (result?.success) {
        await fetchRoles()
      }
      return result
    } catch (err) {
      error.value = err.message
      return { success: false, message: err.message }
    }
  }

  async function updateRole(roleId, roleData) {
    try {
      const result = await api.put(`/role/${roleId}`, roleData)
      if (result?.success) {
        await fetchRoles()
      }
      return result
    } catch (err) {
      error.value = err.message
      return { success: false, message: err.message }
    }
  }

  async function deleteRole(roleId) {
    try {
      const result = await api.delete(`/role/${roleId}`)
      if (result?.success) {
        await fetchRoles()
      }
      return result
    } catch (err) {
      error.value = err.message
      return { success: false, message: err.message }
    }
  }

  async function toggleRoleStatus(roleId) {
    try {
      const result = await api.patch(`/role/${roleId}/toggle-status`)
      if (result?.success) {
        await fetchRoles()
      }
      return result
    } catch (err) {
      error.value = err.message
      return { success: false, message: err.message }
    }
  }

  // ============ Department Management ============
  async function fetchDepartments() {
    loading.value = true
    error.value = null
    try {
      const result = await api.get('/department')
      const list = Array.isArray(result) ? result
        : Array.isArray(result?.data) ? result.data
          : []
      departments.value = list
      return { success: true, data: departments.value }
    } catch (err) {
      error.value = err.message
      return { success: false, message: err.message }
    } finally {
      loading.value = false
    }
  }

  async function getDepartmentById(deptId) {
    try {
      const result = await api.get(`/department/${deptId}`)
      return result
    } catch (err) {
      error.value = err.message
      return { success: false, message: err.message }
    }
  }

  async function createDepartment(departmentData) {
    try {
      const result = await api.post('/department', departmentData)
      if (result?.success) {
        await fetchDepartments()
      }
      return result
    } catch (err) {
      error.value = err.message
      return { success: false, message: err.message }
    }
  }

  async function updateDepartment(deptId, departmentData) {
    try {
      const result = await api.put(`/department/${deptId}`, departmentData)
      if (result?.success) {
        await fetchDepartments()
      }
      return result
    } catch (err) {
      error.value = err.message
      return { success: false, message: err.message }
    }
  }

  async function deleteDepartment(deptId) {
    try {
      const result = await api.delete(`/department/${deptId}`)
      if (result?.success) {
        await fetchDepartments()
      }
      return result
    } catch (err) {
      error.value = err.message
      return { success: false, message: err.message }
    }
  }

  // ============ Department Lookups ============

  const departmentsById = computed(() =>
    Object.fromEntries(departments.value.map(d => [d.dept_id, d]))
  )

  const departmentsByCode = computed(() =>
    Object.fromEntries(
      departments.value.filter(d => d.dept_code).map(d => [d.dept_code, d])
    )
  )

  const assignableDepartments = computed(() => {
    const all = departments.value
    const assignable = all.filter(d => d.is_assignable !== false)
    return assignable.length > 0 ? assignable : all
  })

  function findDepartment(deptId) {
    return departmentsById.value[deptId] ?? null
  }

  function findDepartmentByCode(deptCode) {
    return departmentsByCode.value[deptCode] ?? null
  }

  function getDeptName(deptId) {
    return departmentsById.value[deptId]?.dept_name ?? ''
  }

  function getDeptColor(dept) {
    const row = typeof dept === 'object' ? dept : findDepartment(dept)
    return row?.color_hex || DEFAULT_DEPT_COLOR
  }

  function getDeptIcon(dept) {
    const row = typeof dept === 'object' ? dept : findDepartment(dept)
    return DEPT_ICON_HINTS[row?.dept_code] ?? DEFAULT_DEPT_ICON
  }

  function deptIdHasCode(deptId, codes) {
    const code = departmentsById.value[deptId]?.dept_code
    return !!code && codes.includes(code)
  }

  // ============ Task Template Management ============
  const stages = ref([])
  const taskTemplates = ref([])

  const stagesById = computed(() =>
    Object.fromEntries(stages.value.map(s => [s.stage_id, s]))
  )

  const templatesByStage = computed(() => {
    const grouped = {}
    for (const t of taskTemplates.value) {
      if (!grouped[t.stage_id]) grouped[t.stage_id] = []
      grouped[t.stage_id].push(t)
    }
    for (const key of Object.keys(grouped)) {
      grouped[key].sort((a, b) => a.display_order - b.display_order)
    }
    return grouped
  })

  const requiredStageIds = computed(() =>
    stages.value.filter(s => s.is_required).map(s => s.stage_id)
  )

  const autoCompleteStageIds = computed(() =>
    stages.value.filter(s => s.auto_complete).map(s => s.stage_id)
  )

  function isStageRequired(stageId) {
    return !!stagesById.value[stageId]?.is_required
  }

  function isAutoCompleteStage(stageId) {
    return autoCompleteStageIds.value.includes(stageId)
  }

  function getStageName(stageId) {
    return stagesById.value[stageId]?.stage_name || stageId
  }

  function getTemplatesForStage(stageId) {
    return templatesByStage.value[stageId] || []
  }

  async function fetchTaskTemplates(includeInactive = false) {
    loading.value = true
    error.value = null
    try {
      const query = includeInactive ? '?include_inactive=true' : ''
      const [stageResult, templateResult] = await Promise.all([
        api.get('/tasktemplate/stages'),
        api.get(`/tasktemplate${query}`)
      ])
      stages.value = Array.isArray(stageResult) ? stageResult : []
      taskTemplates.value = Array.isArray(templateResult) ? templateResult : []
      return { success: true }
    } catch (err) {
      error.value = err.message
      return { success: false, message: err.message }
    } finally {
      loading.value = false
    }
  }

  async function createTaskTemplate(templateData) {
    try {
      const result = await api.post('/tasktemplate', templateData)
      if (result?.success) {
        await fetchTaskTemplates(true)
      }
      return result
    } catch (err) {
      error.value = err.message
      return { success: false, message: err.message }
    }
  }

  async function updateTaskTemplate(templateId, templateData) {
    try {
      const result = await api.put(`/tasktemplate/${templateId}`, templateData)
      if (result?.success) {
        await fetchTaskTemplates(true)
      }
      return result
    } catch (err) {
      error.value = err.message
      return { success: false, message: err.message }
    }
  }

  async function deleteTaskTemplate(templateId) {
    try {
      const result = await api.delete(`/tasktemplate/${templateId}`)
      if (result?.success) {
        await fetchTaskTemplates(true)
      }
      return result
    } catch (err) {
      error.value = err.message
      return { success: false, message: err.message }
    }
  }

  async function reorderTaskTemplates(stageId, orderedTemplateIds) {
    try {
      const result = await api.put(
        `/tasktemplate/stage/${stageId}/reorder`,
        orderedTemplateIds
      )
      if (result?.success) {
        await fetchTaskTemplates(true)
      }
      return result
    } catch (err) {
      error.value = err.message
      return { success: false, message: err.message }
    }
  }

  return {
    users,
    roles,
    departments,
    loading,
    error,
    // User methods
    fetchUsers,
    getUserById,
    createUser,
    updateUser,
    deleteUser,
    toggleUserStatus,
    resetUserPassword,
    // Role methods
    fetchRoles,
    getRoleById,
    createRole,
    updateRole,
    deleteRole,
    toggleRoleStatus,
    // Department methods
    fetchDepartments,
    getDepartmentById,
    createDepartment,
    updateDepartment,
    deleteDepartment,
    // Department lookups
    departmentsById,
    departmentsByCode,
    assignableDepartments,
    findDepartment,
    findDepartmentByCode,
    getDeptName,
    getDeptColor,
    getDeptIcon,
    deptIdHasCode,
    // Task template state
    stages,
    taskTemplates,
    stagesById,
    templatesByStage,
    requiredStageIds,
    autoCompleteStageIds,
    // Task template methods
    isStageRequired,
    isAutoCompleteStage,
    getStageName,
    getTemplatesForStage,
    fetchTaskTemplates,
    createTaskTemplate,
    updateTaskTemplate,
    deleteTaskTemplate,
    reorderTaskTemplates
  }
})
