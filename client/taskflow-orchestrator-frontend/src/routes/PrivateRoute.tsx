import { Navigate, Outlet } from 'react-router-dom'

export function PrivateRoute() {
  const accessToken = localStorage.getItem('accessToken')
  
  if (!accessToken) {
    return <Navigate to="/login" replace />
  }

  return <Outlet />
}
