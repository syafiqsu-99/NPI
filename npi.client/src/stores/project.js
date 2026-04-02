import { defineStore } from 'pinia'
import { ref } from 'vue'
import { api } from '@/utils/api'

export const useProjectStore = defineStore('project', () => {
  const projects = ref([])
  const currentProject = ref(null)
  const loading = ref(false)
  const error = ref(null)

  async function createProjectFromEnquiry(enquiryId, projectData) {
    loading.value = true
    error.value = null
    try {
      const response = await api.post(`/project/from-enquiry/${enquiryId}`, projectData)
      const data = response.data || response
      return { success: true, data }
    } catch (err) {
      error.value = err.message
      return { success: false, message: err.message }
    } finally {
      loading.value = false
    }
  }

  async function fetchProjectById(id) {
    loading.value = true
    error.value = null
    try {
      const response = await api.get(`/project/${id}`)
      const data = response.data || response
      currentProject.value = data
      return { success: true, data }
    } catch (err) {
      error.value = err.message
      return { success: false, message: err.message }
    } finally {
      loading.value = false
    }
  }

  async function fetchProjects() {
    loading.value = true
    error.value = null
    try {
      const response = await api.get('/project')
      projects.value = response.data || response || []
      return { success: true, data: projects.value }
    } catch (err) {
      error.value = err.message
      projects.value = []
      return { success: false, message: err.message }
    } finally {
      loading.value = false
    }
  }

  async function deleteProject(id) {
    loading.value = true
    error.value = null
    try {
      await api.delete(`/project/${id}`)
      projects.value = projects.value.filter(p => p.proj_id !== id)
      return { success: true }
    } catch (err) {
      error.value = err.message
      return { success: false, message: err.message }
    } finally {
      loading.value = false
    }
  }

  return {
    projects,
    currentProject,
    loading,
    error,
    createProjectFromEnquiry,
    fetchProjectById,
    fetchProjects,
    deleteProject,
  }
})
