import { defineStore } from 'pinia'
import { ref } from 'vue'
import { api } from '@/utils/api'

export const useEnquiryStore = defineStore('enquiry', () => {
  const enquiries = ref([])
  const currentEnquiry = ref(null)
  const categories = ref([])
  const sections = ref([])
  const loading = ref(false)
  const error = ref(null)

  async function fetchEnquiries() {
    loading.value = true
    error.value = null
    try {
      const response = await api.get('/enquiry')
      enquiries.value = response.data || response || []
    } catch (err) {
      error.value = err.message
      enquiries.value = []
    } finally {
      loading.value = false
    }
  }

  async function fetchMyEnquiries() {
    loading.value = true
    error.value = null
    try {
      const response = await api.get('/enquiry/my-enquiries')
      enquiries.value = response.data || response || []
    } catch (err) {
      error.value = err.message
      enquiries.value = []
    } finally {
      loading.value = false
    }
  }

  async function fetchEnquiryById(id) {
    loading.value = true
    error.value = null
    try {
      const response = await api.get(`/enquiry/${id}`)
      const data = response.data || response
      currentEnquiry.value = data
      return { success: true, data }
    } catch (err) {
      error.value = err.message
      return { success: false, message: err.message }
    } finally {
      loading.value = false
    }
  }

  async function createEnquiry(enquiryData) {
    loading.value = true
    error.value = null
    try {
      const response = await api.post('/enquiry', enquiryData)
      await fetchEnquiries()
      return { success: true, data: response.data || response }
    } catch (err) {
      error.value = err.message
      return { success: false, message: err.message }
    } finally {
      loading.value = false
    }
  }

  async function updateEnquiry(id, enquiryData) {
    loading.value = true
    error.value = null
    try {
      const response = await api.put(`/enquiry/${id}`, enquiryData)
      await fetchEnquiries()
      return { success: true, data: response.data || response }
    } catch (err) {
      error.value = err.message
      return { success: false, message: err.message }
    } finally {
      loading.value = false
    }
  }

  async function submitEnquiry(id) {
    loading.value = true
    error.value = null
    try {
      const response = await api.post(`/enquiry/${id}/submit`, {})
      await fetchEnquiries()
      return { success: true, data: response.data || response }
    } catch (err) {
      error.value = err.message
      return { success: false, message: err.message }
    } finally {
      loading.value = false
    }
  }

  async function deleteEnquiry(id) {
    loading.value = true
    error.value = null
    try {
      await api.delete(`/enquiry/${id}`)
      await fetchEnquiries()
      return { success: true }
    } catch (err) {
      error.value = err.message
      return { success: false, message: err.message }
    } finally {
      loading.value = false
    }
  }

  async function uploadFile(enquiryId, file) {
    loading.value = true
    error.value = null
    try {
      const formData = new FormData()
      formData.append('file', file)

      const response = await api.uploadFile(`/enquiry/${enquiryId}/upload`, formData)
      return { success: true, data: response.data || response }
    } catch (err) {
      error.value = err.message
      return { success: false, message: err.message }
    } finally {
      loading.value = false
    }
  }

  async function downloadEnquiryPDF(id) {
    loading.value = true
    error.value = null
    try {
      const response = await api.get(`/enquiry/${id}/pdf`, {
        responseType: 'blob'
      })

      const url = window.URL.createObjectURL(new Blob([response]))
      const link = document.createElement('a')
      link.href = url
      link.setAttribute('download', `Enquiry_${id}.pdf`)
      document.body.appendChild(link)
      link.click()
      link.remove()
      window.URL.revokeObjectURL(url)

      return { success: true }
    } catch (err) {
      error.value = err.message
      return { success: false, message: err.message }
    } finally {
      loading.value = false
    }
  }

  async function getEnquiryPDFBlob(id) {
    loading.value = true
    error.value = null
    try {
      const blob = await api.get(`/enquiry/${id}/pdf`, {
        responseType: 'blob'
      })

      console.log("Blob received:", blob)

      const url = window.URL.createObjectURL(blob)
      console.log("Blob URL created:", url)

      return { success: true, url }
    } catch (err) {
      console.error("PDF Blob Error:", err)
      error.value = err.message
      return { success: false, message: err.message }
    } finally {
      loading.value = false
    }
  }

  // ── Form Config ─────

  async function fetchConfig() {
    loading.value = true
    error.value = null
    try {
      const result = await api.get('/formconfig')
      if (result?.success) {
        categories.value = result.data.categories
        sections.value = result.data.sections
      }
    } catch (err) {
      error.value = err.message
      console.error('Failed to load form config', err)
    } finally {
      loading.value = false
    }
  }

  // ── Form Config: Categories ─────────────────────────────────────────────────

  async function fetchAllCategories() {
    loading.value = true
    try {
      const result = await api.get('/formconfig/categories')
      if (result?.success) categories.value = result.data
      return result
    } finally {
      loading.value = false
    }
  }

  async function createCategory(dto) {
    const result = await api.post('/formconfig/categories', dto)
    if (result?.success) await fetchAllCategories()
    return result
  }

  async function updateCategory(id, dto) {
    const result = await api.put(`/formconfig/categories/${id}`, dto)
    if (result?.success) await fetchAllCategories()
    return result
  }

  async function deleteCategory(id) {
    const result = await api.delete(`/formconfig/categories/${id}`)
    if (result?.success) await fetchAllCategories()
    return result
  }

  // ── Form Config: Sections ───────────────────────────────────────────────────

  async function fetchAllSections() {
    loading.value = true
    try {
      const [sectionsResult, categoriesResult] = await Promise.all([
        api.get('/formconfig/sections'),
        api.get('/formconfig/categories')
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
    const result = await api.post('/formconfig/sections', dto)
    if (result?.success) await fetchAllSections()
    return result
  }

  async function updateSection(id, dto) {
    const result = await api.put(`/formconfig/sections/${id}`, dto)
    if (result?.success) await fetchAllSections()
    return result
  }

  async function deleteSection(id) {
    const result = await api.delete(`/formconfig/sections/${id}`)
    if (result?.success) await fetchAllSections()
    return result
  }

  async function toggleSectionStatus(id) {
    const result = await api.patch(`/formconfig/sections/${id}/toggle-status`)
    if (result?.success) await fetchAllSections()
    return result
  }

  async function reorderSections(orderedIds) {
    const result = await api.patch('/formconfig/sections/reorder', orderedIds)
    if (result?.success) await fetchAllSections()
    return result
  }

  // ── Form Config: Fields ─────────────────────────────────────────────────────

  async function createField(dto) {
    const result = await api.post('/formconfig/fields', dto)
    if (result?.success) await fetchAllSections()
    return result
  }

  async function updateField(id, dto) {
    const result = await api.put(`/formconfig/fields/${id}`, dto)
    if (result?.success) await fetchAllSections()
    return result
  }

  async function deleteField(id) {
    const result = await api.delete(`/formconfig/fields/${id}`)
    if (result?.success) await fetchAllSections()
    return result
  }

  return {
    enquiries,
    currentEnquiry,
    categories,
    sections,
    loading,
    error,
    fetchEnquiries,
    fetchMyEnquiries,
    fetchEnquiryById,
    createEnquiry,
    updateEnquiry,
    submitEnquiry,
    deleteEnquiry,
    uploadFile,
    downloadEnquiryPDF,
    getEnquiryPDFBlob,

    // Form config
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
