import { defineStore } from 'pinia'
import { ref } from 'vue'
import { api } from '@/utils/api'

export const useEnquiryStore = defineStore('enquiry', () => {
  const enquiries = ref([])
  const currentEnquiry = ref(null)
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

  return {
    enquiries,
    currentEnquiry,
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
    getEnquiryPDFBlob
  }
})
