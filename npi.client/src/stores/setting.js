import { defineStore } from 'pinia'
import { ref } from 'vue'
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
      const result = await api.get('/admin/usermanagement')
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
      const result = await api.get(`/admin/usermanagement/${userId}`)
      return result
    } catch (err) {
      error.value = err.message
      return { success: false, message: err.message }
    }
  }

  async function createUser(userData) {
    try {
      const result = await api.post('/admin/usermanagement', userData)
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
      const result = await api.put(`/admin/usermanagement/${userId}`, userData)
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
      const result = await api.delete(`/admin/usermanagement/${userId}`)
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
      const result = await api.patch(`/admin/usermanagement/${userId}/toggle-status`)
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
      const result = await api.put(`/admin/usermanagement/${userId}/reset-password`, {
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
      const result = await api.get('/admin/rolemanagement')
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
      const result = await api.get(`/admin/rolemanagement/${roleId}`)
      return result
    } catch (err) {
      error.value = err.message
      return { success: false, message: err.message }
    }
  }

  async function createRole(roleData) {
    try {
      const result = await api.post('/admin/rolemanagement', roleData)
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
      const result = await api.put(`/admin/rolemanagement/${roleId}`, roleData)
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
      const result = await api.delete(`/admin/rolemanagement/${roleId}`)
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
      const result = await api.patch(`/admin/rolemanagement/${roleId}/toggle-status`)
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
    deleteDepartment
  }
})
