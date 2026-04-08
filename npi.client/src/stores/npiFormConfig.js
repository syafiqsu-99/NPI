import { defineStore } from 'pinia'
import { ref } from 'vue'
import { api } from '@/utils/api'

export const useNpiFormConfigStore = defineStore('npiFormConfig', () => {
  const categories = ref([])
  const sections = ref([])
  const loading = ref(false)
  const error = ref(null)

  // ── Public config (used by EnquiryForm to render the dynamic form) ──────────

  async function fetchConfig() {
    loading.value = true
    error.value = null
    try {
      const result = await api.get('/npiformconfig')
      if (result?.success) {
        categories.value = result.data.categories
        sections.value = result.data.sections
      }
    } catch (err) {
      error.value = err.message
      console.error('Failed to load NPI form config', err)
    } finally {
      loading.value = false
    }
  }

  // ── Categories ────────────────────────────────────────────────────────────────

  async function fetchAllCategories() {
    loading.value = true
    try {
      const result = await api.get('/npiformconfig/categories')
      if (result?.success) categories.value = result.data
      return result
    } finally {
      loading.value = false
    }
  }

  async function createCategory(dto) {
    const result = await api.post('/npiformconfig/categories', dto)
    if (result?.success) await fetchAllCategories()
    return result
  }

  async function updateCategory(id, dto) {
    const result = await api.put(`/npiformconfig/categories/${id}`, dto)
    if (result?.success) await fetchAllCategories()
    return result
  }

  async function deleteCategory(id) {
    const result = await api.delete(`/npiformconfig/categories/${id}`)
    if (result?.success) await fetchAllCategories()
    return result
  }

  // ── Sections ──────────────────────────────────────────────────────────────────

  async function fetchAllSections() {
    loading.value = true
    try {
      const [sectionsResult, categoriesResult] = await Promise.all([
        api.get('/npiformconfig/sections'),
        api.get('/npiformconfig/categories')
      ])
      if (sectionsResult?.success) sections.value = sectionsResult.data
      if (categoriesResult?.success) categories.value = categoriesResult.data
      return sectionsResult
    } catch (err) {
      error.value = err.message
      return { success: false, message: err.message }
    } finally {
      loading.value = false
    }
  }

  async function createSection(dto) {
    const result = await api.post('/npiformconfig/sections', dto)
    if (result?.success) await fetchAllSections()
    return result
  }

  async function updateSection(id, dto) {
    const result = await api.put(`/npiformconfig/sections/${id}`, dto)
    if (result?.success) await fetchAllSections()
    return result
  }

  async function deleteSection(id) {
    const result = await api.delete(`/npiformconfig/sections/${id}`)
    if (result?.success) await fetchAllSections()
    return result
  }

  async function toggleSectionStatus(id) {
    const result = await api.patch(`/npiformconfig/sections/${id}/toggle-status`)
    if (result?.success) await fetchAllSections()
    return result
  }

  async function reorderSections(orderedIds) {
    const result = await api.patch('/npiformconfig/sections/reorder', orderedIds)
    if (result?.success) await fetchAllSections()
    return result
  }

  // ── Fields ────────────────────────────────────────────────────────────────────

  async function createField(dto) {
    const result = await api.post('/npiformconfig/fields', dto)
    if (result?.success) await fetchAllSections()
    return result
  }

  async function updateField(id, dto) {
    const result = await api.put(`/npiformconfig/fields/${id}`, dto)
    if (result?.success) await fetchAllSections()
    return result
  }

  async function deleteField(id) {
    const result = await api.delete(`/npiformconfig/fields/${id}`)
    if (result?.success) await fetchAllSections()
    return result
  }

  return {
    categories,
    sections,
    loading,
    error,

    fetchConfig,

    fetchAllCategories,
    createCategory,
    updateCategory,
    deleteCategory,

    fetchAllSections,
    createSection,
    updateSection,
    deleteSection,
    toggleSectionStatus,
    reorderSections,

    createField,
    updateField,
    deleteField,
  }
})
