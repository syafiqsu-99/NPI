import { defineStore } from 'pinia'
import { ref } from 'vue'
import { api } from '@/utils/api'

export const useNpiFormConfigStore = defineStore('npiFormConfig', () => {
  const categories = ref([])
  const sections = ref([])
  const loading = ref(false)

  async function fetchConfig() {
    loading.value = true
    try {
      const result = await api.get('/npiformconfig')
      if (result?.success) {
        categories.value = result.data.categories
        sections.value = result.data.sections
      }
    } catch (err) {
      console.error('Failed to load NPI form config', err)
    } finally {
      loading.value = false
    }
  }

  // Admin management methods
  async function createCategory(dto) {
    const result = await api.post('/npiformconfig/categories', dto)
    if (result?.success) await fetchConfig()
    return result
  }

  async function updateCategory(id, dto) {
    const result = await api.put(`/npiformconfig/categories/${id}`, dto)
    if (result?.success) await fetchConfig()
    return result
  }

  async function deleteCategory(id) {
    const result = await api.delete(`/npiformconfig/categories/${id}`)
    if (result?.success) await fetchConfig()
    return result
  }

  async function createField(dto) {
    const result = await api.post('/npiformconfig/fields', dto)
    if (result?.success) await fetchConfig()
    return result
  }

  async function updateField(id, dto) {
    const result = await api.put(`/npiformconfig/fields/${id}`, dto)
    if (result?.success) await fetchConfig()
    return result
  }

  async function deleteField(id) {
    const result = await api.delete(`/npiformconfig/fields/${id}`)
    if (result?.success) await fetchConfig()
    return result
  }

  return {
    categories, sections, loading,
    fetchConfig,
    createCategory, updateCategory, deleteCategory,
    createField, updateField, deleteField
  }
})
