import { apiClient } from '@/lib/api/client'
import { LoginRequest, RegisterRequest, AuthResponse, User, ApiResponse } from '../types/auth.types'

export const authService = {
  async login(data: LoginRequest): Promise<AuthResponse> {
    const response = await apiClient.post<AuthResponse>('/auth/api/auth/login', data)
    return response.data
  },

  async register(data: RegisterRequest): Promise<AuthResponse> {
    const response = await apiClient.post<AuthResponse>('/auth/api/auth/register', data)
    return response.data
  },

  async refresh(refreshToken: string): Promise<AuthResponse> {
    const response = await apiClient.post<AuthResponse>('/auth/api/auth/refresh', refreshToken)
    return response.data
  },

  async logout(refreshToken: string): Promise<void> {
    await apiClient.post('/auth/api/auth/logout', refreshToken)
  },

  async getMe(): Promise<User> {
    const response = await apiClient.get<User>('/auth/api/auth/me')
    return response.data
  },

  async searchUsers(query: string): Promise<User[]> {
    const response = await apiClient.get<ApiResponse<User[]>>('/auth/api/auth/users-search', {
      params: { query }
    })
    return response.data.value
  },

  async getUserById(userId: string): Promise<User> {
    const response = await apiClient.get<ApiResponse<User>>(`/auth/api/auth/users`, { params: { userId } })
    return response.data.value
  },
}