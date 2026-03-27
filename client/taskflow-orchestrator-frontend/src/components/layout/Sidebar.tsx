import { NavLink } from 'react-router-dom'
import { LayoutDashboard, FolderKanban, CheckSquare, Bell, Shield } from 'lucide-react'
import { cn } from '@/lib/utils/cn'
import { useAuthStore } from '@/features/auth/store/authStore'

const navigation = [
  { name: 'Dashboard', href: '/dashboard', icon: LayoutDashboard },
  { name: 'Projects', href: '/projects', icon: FolderKanban },
  { name: 'Tasks', href: '/tasks', icon: CheckSquare },
  { name: 'Notifications', href: '/notifications', icon: Bell },
  { name: 'Audit Logs', href: '/admin/audit', icon: Shield, adminOnly: true },
]

export function Sidebar() {
  const { user } = useAuthStore()
  const isAdmin = user?.roles?.includes('Admin') || false

  return (
    <aside className="flex h-full w-64 flex-col border-r bg-background">
      <div className="flex h-16 items-center border-b px-6">
        <span className="text-lg font-bold">TaskFlow</span>
      </div>
      <nav className="flex-1 space-y-1 p-4">
        {navigation.map((item) => {
          if (item.adminOnly && !isAdmin) return null
          return (
            <NavLink
              key={item.name}
              to={item.href}
              className={({ isActive }) =>
                cn(
                  'flex items-center gap-3 rounded-lg px-3 py-2 text-sm font-medium transition-colors',
                  isActive
                    ? 'bg-primary text-primary-foreground'
                    : 'text-muted-foreground hover:bg-accent hover:text-accent-foreground'
                )
              }
            >
              <item.icon className="h-5 w-5" />
              {item.name}
            </NavLink>
          )
        })}
      </nav>
      <div className="border-t p-4">
        <div className="flex items-center gap-3">
          <div className="flex h-8 w-8 items-center justify-center rounded-full bg-primary/10">
            <span className="text-sm font-medium text-primary">
              {user?.fullName?.charAt(0) || 'U'}
            </span>
          </div>
          <div className="flex-1 overflow-hidden">
            <p className="truncate text-sm font-medium">{user?.fullName || 'User'}</p>
            <p className="truncate text-xs text-muted-foreground">
              {user?.roles?.join(', ') || 'User'}
            </p>
          </div>
        </div>
      </div>
    </aside>
  )
}
