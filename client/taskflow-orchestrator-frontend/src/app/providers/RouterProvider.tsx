import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom'
import { AuthLayout } from '@/components/layout/AuthLayout'
import { MainLayout } from '@/components/layout/MainLayout'
import { PrivateRoute } from '@/routes/PrivateRoute'
import { PublicRoute } from '@/routes/PublicRoute'
import { lazy, Suspense } from 'react'

const LoginPage = lazy(() => import('@/features/auth/pages/LoginPage'))
const RegisterPage = lazy(() => import('@/features/auth/pages/RegisterPage'))
const DashboardPage = lazy(() => import('@/features/dashboard/pages/DashboardPage'))
const ProjectsPage = lazy(() => import('@/features/projects/pages/ProjectsPage'))
const TasksPage = lazy(() => import('@/features/tasks/pages/TasksPage'))
const NotificationsPage = lazy(() => import('@/features/notifications/pages/NotificationsPage'))
const AuditPage = lazy(() => import('@/features/admin/pages/AuditPage'))

function LoadingFallback() {
  return (
    <div className="flex h-screen items-center justify-center">
      <div className="h-8 w-8 animate-spin rounded-full border-4 border-primary border-t-transparent" />
    </div>
  )
}

export function RouterProvider() {
  return (
    <BrowserRouter>
      <Suspense fallback={<LoadingFallback />}>
        <Routes>
          <Route element={<PublicRoute />}>
            <Route element={<AuthLayout />}>
              <Route path="/login" element={<LoginPage />} />
              <Route path="/register" element={<RegisterPage />} />
            </Route>
          </Route>

          <Route element={<PrivateRoute />}>
            <Route element={<MainLayout />}>
              <Route path="/" element={<Navigate to="/dashboard" replace />} />
              <Route path="/dashboard" element={<DashboardPage />} />
              <Route path="/projects" element={<ProjectsPage />} />
              <Route path="/projects/:id" element={<ProjectsPage />} />
              <Route path="/tasks" element={<TasksPage />} />
              <Route path="/notifications" element={<NotificationsPage />} />
              <Route path="/admin/audit" element={<AuditPage />} />
            </Route>
          </Route>

          <Route path="*" element={<div className="flex h-screen items-center justify-center text-2xl">404 - Page Not Found</div>} />
        </Routes>
      </Suspense>
    </BrowserRouter>
  )
}
