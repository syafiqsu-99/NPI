import { defineStore } from 'pinia'
import { ref } from 'vue'
import { api } from '@/utils/api'

export const useCustomerStore = defineStore('customer', () => {
  const customers = ref([])
  const currentCustomer = ref(null)
  const loading = ref(false)
  const error = ref(null)

  async function fetchCustomers() {
    loading.value = true
    error.value = null
    try {
      const response = await api.get('/customer')
      customers.value = response.data || response || []
      return { success: true, data: customers.value }
    } catch (err) {
      error.value = err.message
      customers.value = []
      return { success: false, message: err.message }
    } finally {
      loading.value = false
    }
  }

  async function fetchCustomerById(id) {
    loading.value = true
    error.value = null
    try {
      const response = await api.get(`/customer/${id}`)
      const data = response.data || response
      currentCustomer.value = data
      return { success: true, data }
    } catch (err) {
      error.value = err.message
      return { success: false, message: err.message }
    } finally {
      loading.value = false
    }
  }

  async function createCustomer(customerData) {
    loading.value = true
    error.value = null
    try {
      const response = await api.post('/customer', customerData)
      const data = response.data || response
      customers.value.push(data)
      return { success: true, data }
    } catch (err) {
      error.value = err.message
      return { success: false, message: err.message }
    } finally {
      loading.value = false
    }
  }

  async function updateCustomer(id, customerData) {
    loading.value = true
    error.value = null
    try {
      const response = await api.put(`/customer/${id}`, customerData)
      await fetchCustomers()
      return { success: true, data: response.data || response }
    } catch (err) {
      error.value = err.message
      return { success: false, message: err.message }
    } finally {
      loading.value = false
    }
  }

  return {
    customers,
    currentCustomer,
    loading,
    error,
    fetchCustomers,
    fetchCustomerById,
    createCustomer,
    updateCustomer
  }
})
