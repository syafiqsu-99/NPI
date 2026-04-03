import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { api } from '@/utils/api'

export const useAuthStore = defineStore('auth', () => {
  const user = ref(null)
  const token = ref(localStorage.getItem('token') || null)
  const loading = ref(false)
  const error = ref(null)

  const isAuthenticated = computed(() => !!token.value && !!user.value)
  const currentUser = computed(() => user.value)
  const userRole = computed(() => user.value?.role)
  const userDepartment = computed(() => user.value?.department)

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
    } catch (err) {
      console.error('Logout error:', err)
    } finally {
      user.value = null
      token.value = null
      localStorage.removeItem('token')
    }
  }

  function isTokenExpired(token) {
    try {
      const payload = JSON.parse(atob(token.split('.')[1]))
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

  async function register(userData) {
    loading.value = true
    error.value = null

    try {
      const response = await api.post('/auth/register', userData)
      return { success: true, message: response.message }
    } catch (err) {
      error.value = err.message
      return { success: false, message: err.message }
    } finally {
      loading.value = false
    }
  }

  return {
    user,
    token,
    loading,
    error,
    isAuthenticated,
    currentUser,
    userRole,
    userDepartment,
    login,
    logout,
    checkAuth,
    register
  }
})
