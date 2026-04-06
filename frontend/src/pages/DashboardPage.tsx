import { useQuery } from 'react-query'
import api from '../services/api'
import { useAuthStore } from '../store/authStore'
import { UserRole, DashboardSummary, TicketSummary } from '../types'
import { getStatusClass, getStatusLabel, getPriorityClass, getPriorityLabel, formatDate } from '../utils/helpers'
import { Link } from 'react-router-dom'
import { Ticket, Clock, CheckCircle, AlertTriangle, TrendingUp, Users } from 'lucide-react'

export default function DashboardPage() {
  const { user } = useAuthStore()

  const { data: summary } = useQuery<DashboardSummary>(
    'dashboard-summary',
    () => api.get('/reports/tickets-summary').then(r => r.data),
    { enabled: user?.role === UserRole.Admin || user?.role === UserRole.TeamLead }
  )

  const { data: myTickets } = useQuery(
    'my-tickets',
    () => api.get('/tickets?page=1&pageSize=10').then(r => r.data.items as TicketSummary[])
  )

  const statCards = summary ? [
    { label: 'Total Tickets', value: summary.totalTickets, icon: Ticket, color: 'text-blue-600', bg: 'bg-blue-50' },
    { label: 'Open Tickets', value: summary.openTickets, icon: Clock, color: 'text-yellow-600', bg: 'bg-yellow-50' },
    { label: 'Resolved', value: summary.resolvedTickets, icon: CheckCircle, color: 'text-green-600', bg: 'bg-green-50' },
    { label: 'SLA Breached', value: summary.slaBreachedTickets, icon: AlertTriangle, color: 'text-red-600', bg: 'bg-red-50' },
  ] : []

  return (
    <div className="p-8">
      <div className="flex items-center justify-between mb-8">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Dashboard</h1>
          <p className="text-gray-500 mt-1">Welcome back, {user?.fullName}</p>
        </div>
        <Link to="/tickets/new" className="btn-primary flex items-center gap-2">
          <Ticket size={18} />
          New Ticket
        </Link>
      </div>

      {/* Stats for admin/teamlead */}
      {summary && (
        <div className="grid grid-cols-2 lg:grid-cols-4 gap-6 mb-8">
          {statCards.map((card) => (
            <div key={card.label} className="card">
              <div className="flex items-center justify-between">
                <div>
                  <p className="text-sm text-gray-500">{card.label}</p>
                  <p className="text-3xl font-bold text-gray-900 mt-1">{card.value}</p>
                </div>
                <div className={`p-3 rounded-xl ${card.bg}`}>
                  <card.icon size={24} className={card.color} />
                </div>
              </div>
            </div>
          ))}
        </div>
      )}

      {/* Quick stats for all users */}
      {!summary && myTickets && (
        <div className="grid grid-cols-2 lg:grid-cols-4 gap-6 mb-8">
          {[
            { label: 'Total', value: myTickets.length, color: 'text-blue-600', bg: 'bg-blue-50' },
            { label: 'Open', value: myTickets.filter(t => t.status <= 4).length, color: 'text-yellow-600', bg: 'bg-yellow-50' },
            { label: 'Resolved', value: myTickets.filter(t => t.status === 7).length, color: 'text-green-600', bg: 'bg-green-50' },
            { label: 'Overdue', value: myTickets.filter(t => t.isSlaBreached).length, color: 'text-red-600', bg: 'bg-red-50' },
          ].map(card => (
            <div key={card.label} className="card">
              <p className="text-sm text-gray-500">{card.label}</p>
              <p className={`text-3xl font-bold mt-1 ${card.color}`}>{card.value}</p>
            </div>
          ))}
        </div>
      )}

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        {/* Recent Tickets */}
        <div className="lg:col-span-2 card">
          <div className="flex items-center justify-between mb-4">
            <h2 className="text-lg font-semibold text-gray-900">Recent Tickets</h2>
            <Link to="/tickets" className="text-sm text-primary-600 hover:text-primary-700">View all</Link>
          </div>
          <div className="space-y-3">
            {myTickets?.slice(0, 8).map(ticket => (
              <Link key={ticket.id} to={`/tickets/${ticket.id}`} className="flex items-center justify-between p-3 rounded-lg hover:bg-gray-50 transition-colors border border-gray-100">
                <div className="flex-1 min-w-0">
                  <div className="flex items-center gap-2 mb-1">
                    <span className="text-xs text-gray-500 font-mono">{ticket.ticketNo}</span>
                    {ticket.isSlaBreached && <span className="text-xs text-red-600 font-medium">⚠ SLA</span>}
                  </div>
                  <p className="text-sm font-medium text-gray-900 truncate">{ticket.subject}</p>
                  <p className="text-xs text-gray-500">{ticket.categoryName}</p>
                </div>
                <div className="flex items-center gap-2 ml-4">
                  <span className={getPriorityClass(ticket.priority)}>{getPriorityLabel(ticket.priority)}</span>
                  <span className={getStatusClass(ticket.status)}>{getStatusLabel(ticket.status)}</span>
                </div>
              </Link>
            ))}
            {(!myTickets || myTickets.length === 0) && (
              <p className="text-gray-500 text-sm text-center py-8">No tickets found</p>
            )}
          </div>
        </div>

        {/* Category Trends */}
        {summary?.categoryTrends && summary.categoryTrends.length > 0 && (
          <div className="card">
            <div className="flex items-center gap-2 mb-4">
              <TrendingUp size={18} className="text-gray-500" />
              <h2 className="text-lg font-semibold text-gray-900">Category Trends</h2>
            </div>
            <div className="space-y-3">
              {summary.categoryTrends.map(trend => (
                <div key={trend.categoryName} className="flex items-center justify-between">
                  <span className="text-sm text-gray-600">{trend.categoryName}</span>
                  <div className="flex items-center gap-2">
                    <div className="w-20 bg-gray-200 rounded-full h-1.5">
                      <div
                        className="bg-primary-600 h-1.5 rounded-full"
                        style={{ width: `${(trend.count / (summary.totalTickets || 1)) * 100}%` }}
                      />
                    </div>
                    <span className="text-sm font-medium text-gray-900">{trend.count}</span>
                  </div>
                </div>
              ))}
            </div>
          </div>
        )}

        {/* Agent Performance */}
        {summary?.agentPerformance && summary.agentPerformance.length > 0 && (
          <div className="card lg:col-span-2">
            <div className="flex items-center gap-2 mb-4">
              <Users size={18} className="text-gray-500" />
              <h2 className="text-lg font-semibold text-gray-900">Agent Performance</h2>
            </div>
            <table className="w-full text-sm">
              <thead>
                <tr className="text-left text-gray-500 border-b border-gray-200">
                  <th className="pb-2">Agent</th>
                  <th className="pb-2 text-right">Assigned</th>
                  <th className="pb-2 text-right">Resolved</th>
                  <th className="pb-2 text-right">Rate</th>
                </tr>
              </thead>
              <tbody>
                {summary.agentPerformance.map(agent => (
                  <tr key={agent.agentName} className="border-b border-gray-100 last:border-0">
                    <td className="py-2 font-medium text-gray-900">{agent.agentName}</td>
                    <td className="py-2 text-right text-gray-600">{agent.assignedCount}</td>
                    <td className="py-2 text-right text-gray-600">{agent.resolvedCount}</td>
                    <td className="py-2 text-right font-medium text-green-600">
                      {agent.assignedCount > 0 ? Math.round((agent.resolvedCount / agent.assignedCount) * 100) : 0}%
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>
    </div>
  )
}
