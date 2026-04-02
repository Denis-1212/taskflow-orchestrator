export interface LoginRequest {
  email: string
  password: string
}

export interface RegisterRequest {
  email: string
  password: string
  fullName: string
}

export interface User {
  id: string
  email: string
  fullName: string
  isActive: boolean
  roles: string[]
}

export interface AuthResponse {
  accessToken: string
  refreshToken: string
  user: User
}

export interface ApiResponse<T> {
  value: T
  isSuccess: boolean
  isFailure: boolean
  error: string | null
}