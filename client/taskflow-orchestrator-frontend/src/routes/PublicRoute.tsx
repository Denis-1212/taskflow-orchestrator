import { Navigate, Outlet } from 'react-router-dom'

export function PublicRoute() {
  const accessToken = localStorage.getItem('accessToken')
  
  if (accessToken) {
    return <Navigate to="/dashboard" replace />
  }

  return <Outlet />
}
