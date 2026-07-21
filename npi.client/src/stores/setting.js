import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { api } from '@/utils/api'

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
      if (result?.success && result?.data) {
        users.value = result.data
      } else if (Array.isArray(result)) {
        users.value = result
      }
      return { success: true, data: users.value }
    } catch (err) {
      error.value = err.message
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
      if (result?.success && result?.data) {
        roles.value = result.data
      } else if (Array.isArray(result)) {
        roles.value = result
      }
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
      if (result?.success && result?.data) {
        departments.value = result.data
      } else if (Array.isArray(result)) {
        departments.value = result
      }
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
    // Task template methods
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
