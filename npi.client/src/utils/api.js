import router from '@/router'

const API_BASE_URL = '/api'

class ApiError extends Error {
  constructor(message, status, data) {
    super(message)
    this.status = status
    this.data = data
  }
}

async function fetchApi(endpoint, options = {}) {
  const token = localStorage.getItem('token')

  const defaultHeaders = {
    'Content-Type': 'application/json'
  }

  if (token) {
    defaultHeaders['Authorization'] = `Bearer ${token}`
  }

  const { responseType, ...restOptions } = options

  const config = {
    ...restOptions,
    headers: {
      ...defaultHeaders,
      ...restOptions.headers
    },
    credentials: 'include'
  }

  try {
    const url = endpoint.startsWith(API_BASE_URL)
      ? endpoint
      : `${API_BASE_URL}${endpoint}`

    const response = await fetch(url, config)

    // Handle 401 Unauthorized
    if (response.status === 401) {
      localStorage.removeItem('token')
      if (window.location.pathname !== '/login') {
        router.push('/login')
      }
      throw new ApiError('Unauthorized', 401, null)
    }

    // Handle 204 No Content
    if (response.status === 204) {
      return null
    }

    if (responseType === 'blob') {
      if (!response.ok) {
        let msg = 'An error occurred'
        try {
          const errData = await response.json()
          msg = errData.message || errData.title || msg
        } catch { /* response body not JSON */ }
        throw new ApiError(msg, response.status, null)
      }
      return await response.blob()
    }

    if (responseType === 'arraybuffer') {
      if (!response.ok) {
        let msg = 'An error occurred'
        try {
          const errData = await response.json()
          msg = errData.message || errData.title || msg
        } catch { /* ignore */ }
        throw new ApiError(msg, response.status, null)
      }
      const buffer = await response.arrayBuffer()
      return { data: buffer }
    }

    if (responseType === 'text') {
      if (!response.ok) {
        throw new ApiError('An error occurred', response.status, null)
      }
      const text = await response.text()
      return { data: text }
    }

    const data = await response.json()

    if (!response.ok) {
      throw new ApiError(
        data.message || data.title || 'An error occurred',
        response.status,
        data
      )
    }

    return data
  } catch (error) {
    if (error instanceof ApiError) throw error
    throw new ApiError(error.message, 500, null)
  }
}

export const api = {
  get: (endpoint, options = {}) =>
    fetchApi(endpoint, { method: 'GET', ...options }),

  post: (endpoint, data) =>
    fetchApi(endpoint, {
      method: 'POST',
      body: JSON.stringify(data)
    }),

  put: (endpoint, data) =>
    fetchApi(endpoint, {
      method: 'PUT',
      body: JSON.stringify(data)
    }),

  patch: (endpoint, data) =>
    fetchApi(endpoint, {
      method: 'PATCH',
      body: JSON.stringify(data)
    }),

  delete: (endpoint) =>
    fetchApi(endpoint, { method: 'DELETE' }),

  uploadFile: async (endpoint, formData) => {
    const token = localStorage.getItem('token')
    const headers = {}
    if (token) headers['Authorization'] = `Bearer ${token}`

    const url = endpoint.startsWith(API_BASE_URL) ? endpoint : `${API_BASE_URL}${endpoint}`

    const response = await fetch(url, {
      method: 'POST',
      headers,
      body: formData,
      credentials: 'include'
    })

    if (response.status === 401) {
      localStorage.removeItem('token')
      window.location.href = '/login'
      throw new ApiError('Unauthorized', 401, null)
    }

    if (!response.ok) {
      const data = await response.json()
      throw new ApiError(data.message || 'Upload failed', response.status, data)
    }

    return response.json()
  },

  downloadFile: async (endpoint, filename) => {
    const token = localStorage.getItem('token')

    const url = endpoint.startsWith(API_BASE_URL) ? endpoint : `${API_BASE_URL}${endpoint}`

    const response = await fetch(url, {
      headers: { Authorization: `Bearer ${token}` },
      credentials: 'include'
    })

    if (!response.ok) throw new ApiError('Download failed', response.status, null)

    const blob = await response.blob()
    const objectUrl = window.URL.createObjectURL(blob)
    const a = document.createElement('a')
    a.href = objectUrl
    a.download = filename
    document.body.appendChild(a)
    a.click()
    window.URL.revokeObjectURL(objectUrl)
    document.body.removeChild(a)
  },

  previewFile: async (endpoint) => {
    const token = localStorage.getItem('token')
    const response = await fetch(`${API_BASE_URL}${endpoint}`, {
      headers: { Authorization: `Bearer ${token}` },
      credentials: 'include'
    })
    if (!response.ok) throw new ApiError('Preview failed', response.status, null)
    const blob = await response.blob()
    return window.URL.createObjectURL(blob)
  }
}

export { ApiError }
