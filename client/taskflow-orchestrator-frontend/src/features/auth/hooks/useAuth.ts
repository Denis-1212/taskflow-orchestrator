import { useMutation } from '@tanstack/react-query'
import { useNavigate } from 'react-router-dom'
import { toast } from 'sonner'
import { authService } from '../services/authService'
import { useAuthStore } from '../store/authStore'
import { LoginRequest, RegisterRequest } from '../types/auth.types'

export function useLogin() {
  const navigate = useNavigate()
  const setAuth = useAuthStore((state) => state.setAuth)

  return useMutation({
    mutationFn: (data: LoginRequest) => authService.login(data),
    onSuccess: (response) => {
      setAuth(response.user, response.accessToken, response.refreshToken)
      toast.success('Welcome back!')
      navigate('/dashboard')
    },
    onError: (error: any) => {
      const message = error.response?.data?.message || 'Invalid email or password'
      toast.error(message)
    },
  })
}

export function useRegister() {
  const navigate = useNavigate()
  const setAuth = useAuthStore((state) => state.setAuth)

  return useMutation({
    mutationFn: (data: RegisterRequest) => authService.register(data),
    onSuccess: (response) => {
      setAuth(response.user, response.accessToken, response.refreshToken)
      toast.success('Account created successfully!')
      navigate('/dashboard')
    },
    onError: (error: any) => {
      const message = error.response?.data?.message || 'Registration failed'
      toast.error(message)
    },
  })
}

export function useLogout() {
  const navigate = useNavigate()
  const clearAuth = useAuthStore((state) => state.clearAuth)
  const refreshToken = useAuthStore((state) => state.refreshToken)

  return useMutation({
    mutationFn: () => {
      if (refreshToken) {
        return authService.logout(refreshToken)
      }
      return Promise.resolve()
    },
    onSuccess: () => {
      clearAuth()
      toast.success('Logged out successfully')
      navigate('/login')
    },
    onError: () => {
      clearAuth()
      navigate('/login')
    },
  })
}
