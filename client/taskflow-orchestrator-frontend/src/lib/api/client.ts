import axios from 'axios'

export const apiClient = axios.create({
  baseURL: '',
  timeout: 30000,
  headers: {
    'Content-Type': 'application/json',
  },
})

apiClient.interceptors.request.use(
  (config) => {
    const accessToken = localStorage.getItem('accessToken')
    console.log('Request interceptor - token:', accessToken ? 'exists' : 'missing')
    if (accessToken) {
      config.headers.Authorization = `Bearer ${accessToken}`
      console.log('Added Authorization header')
    }
    console.log('Request URL:', config.url)
    return config
  },
  (error) => {
    return Promise.reject(error)
  }
)

apiClient.interceptors.response.use(
  (response) => response,
  async (error) => {
    console.error('API Error:', {
      url: error.config?.url,
      status: error.response?.status,
      message: error.response?.data?.message || error.message,
    })
    if (error.response?.status === 401) {
      localStorage.removeItem('accessToken')
      if (!window.location.pathname.includes('/login')) {
        window.location.href = '/login'
      }
    }
    return Promise.reject(error)
  }
)
