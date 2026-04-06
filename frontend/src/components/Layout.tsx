import { Outlet, NavLink, useNavigate } from 'react-router-dom'
import { useAuthStore } from '../store/authStore'
import { UserRole } from '../types'
import { getRoleLabel } from '../utils/helpers'
import api from '../services/api'
import {
  LayoutDashboard, Ticket, Users, BarChart3, BookOpen, Bell, User, LogOut, Shield
} from 'lucide-react'
import { useQuery } from 'react-query'
import { Notification } from '../types'

export default function Layout() {
  const { user, refreshToken, logout } = useAuthStore()
  const navigate = useNavigate()

  const { data: unread } = useQuery(
    'unread-notifications',
    () => api.get('/notifications?unreadOnly=true').then(r => r.data as Notification[]),
    { refetchInterval: 30000 }
  )

  const handleLogout = async () => {
    try {
      await api.post('/auth/logout', { refreshToken })
    } catch {}
    logout()
    navigate('/login')
  }

  const navItems = [
    { to: '/dashboard', label: 'Dashboard', icon: LayoutDashboard },
    { to: '/tickets', label: 'Tickets', icon: Ticket },
    { to: '/knowledge-base', label: 'Knowledge Base', icon: BookOpen },
    ...(user?.role === UserRole.Admin ? [
      { to: '/users', label: 'Users', icon: Users },
      { to: '/audit', label: 'Audit Log', icon: Shield },
    ] : []),
    ...(user?.role && [UserRole.Admin, UserRole.TeamLead].includes(user.role) ? [
      { to: '/reports', label: 'Reports', icon: BarChart3 },
    ] : []),
  ]

  return (
    <div className="flex h-screen bg-gray-50">
      {/* Sidebar */}
      <aside className="w-64 bg-white border-r border-gray-200 flex flex-col">
        <div className="p-6 border-b border-gray-200">
          <h1 className="text-xl font-bold text-primary-700">SupportDesk</h1>
          <p className="text-xs text-gray-500 mt-1">{getRoleLabel(user?.role ?? UserRole.EndUser)}</p>
        </div>

        <nav className="flex-1 p-4 space-y-1">
          {navItems.map(({ to, label, icon: Icon }) => (
            <NavLink
              key={to}
              to={to}
              className={({ isActive }) =>
                `flex items-center gap-3 px-3 py-2.5 rounded-lg text-sm font-medium transition-colors ${
                  isActive
                    ? 'bg-primary-50 text-primary-700'
                    : 'text-gray-600 hover:bg-gray-100 hover:text-gray-900'
                }`
              }
            >
              <Icon size={18} />
              {label}
            </NavLink>
          ))}
        </nav>

        <div className="p-4 border-t border-gray-200 space-y-1">
          <NavLink
            to="/notifications"
            className={({ isActive }) =>
              `flex items-center gap-3 px-3 py-2.5 rounded-lg text-sm font-medium transition-colors ${
                isActive ? 'bg-primary-50 text-primary-700' : 'text-gray-600 hover:bg-gray-100'
              }`
            }
          >
            <div className="relative">
              <Bell size={18} />
              {(unread?.length ?? 0) > 0 && (
                <span className="absolute -top-1 -right-1 w-4 h-4 bg-red-500 text-white text-xs rounded-full flex items-center justify-center">
                  {Math.min(unread?.length ?? 0, 9)}
                </span>
              )}
            </div>
            Notifications
          </NavLink>
          <NavLink
            to="/profile"
            className={({ isActive }) =>
              `flex items-center gap-3 px-3 py-2.5 rounded-lg text-sm font-medium transition-colors ${
                isActive ? 'bg-primary-50 text-primary-700' : 'text-gray-600 hover:bg-gray-100'
              }`
            }
          >
            <User size={18} />
            Profile
          </NavLink>
          <button
            onClick={handleLogout}
            className="flex items-center gap-3 px-3 py-2.5 rounded-lg text-sm font-medium text-gray-600 hover:bg-red-50 hover:text-red-600 transition-colors w-full"
          >
            <LogOut size={18} />
            Sign Out
          </button>
        </div>
      </aside>

      {/* Main content */}
      <main className="flex-1 overflow-auto">
        <Outlet />
      </main>
    </div>
  )
}
