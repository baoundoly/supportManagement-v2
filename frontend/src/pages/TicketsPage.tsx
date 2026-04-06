import { useState } from 'react'
import { Link } from 'react-router-dom'
import { useQuery } from 'react-query'
import api from '../services/api'
import { TicketSummary, TicketStatus, TicketPriority, PagedResult } from '../types'
import { getStatusClass, getStatusLabel, getPriorityClass, getPriorityLabel, formatDate } from '../utils/helpers'
import { Plus, Search, AlertTriangle } from 'lucide-react'

export default function TicketsPage() {
  const [page, setPage] = useState(1)
  const [search, setSearch] = useState('')
  const [status, setStatus] = useState('')
  const [priority, setPriority] = useState('')

  const { data, isLoading } = useQuery(
    ['tickets', page, search, status, priority],
    () => api.get('/tickets', {
      params: { page, pageSize: 20, searchTerm: search || undefined, status: status || undefined, priority: priority || undefined }
    }).then(r => r.data as PagedResult<TicketSummary>),
    { keepPreviousData: true }
  )

  return (
    <div className="p-8">
      <div className="flex items-center justify-between mb-6">
        <h1 className="text-2xl font-bold text-gray-900">Support Tickets</h1>
        <Link to="/tickets/new" className="btn-primary flex items-center gap-2">
          <Plus size={18} />
          New Ticket
        </Link>
      </div>

      {/* Filters */}
      <div className="card mb-6">
        <div className="flex flex-wrap gap-4">
          <div className="flex-1 min-w-64 relative">
            <Search size={16} className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-400" />
            <input
              type="text"
              placeholder="Search tickets..."
              className="input-field pl-9"
              value={search}
              onChange={e => { setSearch(e.target.value); setPage(1) }}
            />
          </div>
          <select className="input-field w-40" value={status} onChange={e => { setStatus(e.target.value); setPage(1) }}>
            <option value="">All Status</option>
            {Object.entries(TicketStatus).filter(([k]) => isNaN(Number(k))).map(([label, val]) => (
              <option key={val} value={val}>{label.replace(/([A-Z])/g, ' $1').trim()}</option>
            ))}
          </select>
          <select className="input-field w-40" value={priority} onChange={e => { setPriority(e.target.value); setPage(1) }}>
            <option value="">All Priority</option>
            {Object.entries(TicketPriority).filter(([k]) => isNaN(Number(k))).map(([label, val]) => (
              <option key={val} value={val}>{label}</option>
            ))}
          </select>
        </div>
      </div>

      {/* Table */}
      <div className="card p-0">
        <div className="overflow-x-auto">
          <table className="w-full text-sm">
            <thead>
              <tr className="border-b border-gray-200 bg-gray-50">
                {['Ticket No', 'Subject', 'Category', 'Priority', 'Status', 'Assigned To', 'Due Date', 'Created'].map(h => (
                  <th key={h} className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">{h}</th>
                ))}
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-100">
              {isLoading && (
                <tr><td colSpan={8} className="px-4 py-8 text-center text-gray-500">Loading...</td></tr>
              )}
              {data?.items.map(ticket => (
                <tr key={ticket.id} className="hover:bg-gray-50 transition-colors">
                  <td className="px-4 py-3">
                    <div className="flex items-center gap-1">
                      <Link to={`/tickets/${ticket.id}`} className="font-mono text-primary-600 hover:text-primary-700 font-medium">
                        {ticket.ticketNo}
                      </Link>
                      {ticket.isSlaBreached && <AlertTriangle size={14} className="text-red-500" />}
                    </div>
                  </td>
                  <td className="px-4 py-3">
                    <Link to={`/tickets/${ticket.id}`} className="text-gray-900 hover:text-primary-600 font-medium max-w-xs truncate block">
                      {ticket.subject}
                    </Link>
                  </td>
                  <td className="px-4 py-3 text-gray-500">{ticket.categoryName}</td>
                  <td className="px-4 py-3"><span className={getPriorityClass(ticket.priority)}>{getPriorityLabel(ticket.priority)}</span></td>
                  <td className="px-4 py-3"><span className={getStatusClass(ticket.status)}>{getStatusLabel(ticket.status)}</span></td>
                  <td className="px-4 py-3 text-gray-500">{ticket.assignedUserName ?? ticket.assignedTeamName ?? '-'}</td>
                  <td className="px-4 py-3 text-gray-500 text-xs">{formatDate(ticket.resolutionDueAt)}</td>
                  <td className="px-4 py-3 text-gray-500 text-xs">{formatDate(ticket.createdAt)}</td>
                </tr>
              ))}
              {!isLoading && data?.items.length === 0 && (
                <tr><td colSpan={8} className="px-4 py-8 text-center text-gray-500">No tickets found</td></tr>
              )}
            </tbody>
          </table>
        </div>

        {/* Pagination */}
        {data && data.totalPages > 1 && (
          <div className="px-4 py-3 border-t border-gray-200 flex items-center justify-between">
            <p className="text-sm text-gray-500">
              Showing {(page - 1) * 20 + 1}–{Math.min(page * 20, data.totalCount)} of {data.totalCount}
            </p>
            <div className="flex gap-2">
              <button className="btn-secondary text-sm py-1 px-3" disabled={page === 1} onClick={() => setPage(p => p - 1)}>Previous</button>
              <button className="btn-secondary text-sm py-1 px-3" disabled={page >= data.totalPages} onClick={() => setPage(p => p + 1)}>Next</button>
            </div>
          </div>
        )}
      </div>
    </div>
  )
}
