import { useQuery, useQueryClient } from 'react-query'
import api from '../services/api'
import { Notification } from '../types'
import { formatRelativeTime } from '../utils/helpers'
import { Bell, CheckCheck } from 'lucide-react'
import toast from 'react-hot-toast'
import { useNavigate } from 'react-router-dom'

export default function NotificationsPage() {
  const qc = useQueryClient()
  const navigate = useNavigate()

  const { data: notifications } = useQuery<Notification[]>(
    'notifications',
    () => api.get('/notifications').then(r => r.data)
  )

  const markAllRead = async () => {
    await api.post('/notifications/read-all')
    qc.invalidateQueries('notifications')
    qc.invalidateQueries('unread-notifications')
    toast.success('All notifications marked as read')
  }

  const markRead = async (id: string) => {
    await api.post(`/notifications/${id}/read`)
    qc.invalidateQueries('notifications')
    qc.invalidateQueries('unread-notifications')
  }

  const unreadCount = notifications?.filter(n => !n.isRead).length ?? 0

  return (
    <div className="p-8 max-w-2xl mx-auto">
      <div className="flex items-center justify-between mb-6">
        <div className="flex items-center gap-3">
          <Bell size={24} className="text-primary-600" />
          <h1 className="text-2xl font-bold text-gray-900">Notifications</h1>
          {unreadCount > 0 && (
            <span className="bg-red-500 text-white text-xs rounded-full px-2 py-0.5">{unreadCount}</span>
          )}
        </div>
        {unreadCount > 0 && (
          <button className="btn-secondary flex items-center gap-2 text-sm" onClick={markAllRead}>
            <CheckCheck size={16} /> Mark all read
          </button>
        )}
      </div>

      <div className="space-y-2">
        {notifications?.map(n => (
          <div
            key={n.id}
            onClick={async () => {
              if (!n.isRead) await markRead(n.id)
              if (n.actionUrl) navigate(n.actionUrl)
            }}
            className={`card cursor-pointer hover:shadow-md transition-all ${!n.isRead ? 'border-primary-200 bg-primary-50' : ''}`}
          >
            <div className="flex items-start gap-3">
              <div className={`w-2 h-2 rounded-full mt-2 flex-shrink-0 ${!n.isRead ? 'bg-primary-500' : 'bg-gray-300'}`} />
              <div className="flex-1">
                <p className="font-medium text-gray-900 text-sm">{n.title}</p>
                <p className="text-gray-600 text-sm mt-0.5">{n.message}</p>
                <p className="text-xs text-gray-400 mt-1">{formatRelativeTime(n.createdAt)}</p>
              </div>
            </div>
          </div>
        ))}
        {(!notifications || notifications.length === 0) && (
          <div className="text-center py-16 text-gray-400">
            <Bell size={48} className="mx-auto mb-3 opacity-30" />
            <p>No notifications</p>
          </div>
        )}
      </div>
    </div>
  )
}
