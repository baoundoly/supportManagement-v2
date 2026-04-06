import { useState } from 'react'
import { useQuery } from 'react-query'
import api from '../services/api'
import { DashboardSummary } from '../types'
import { Download } from 'lucide-react'

export default function ReportsPage() {
  const [from, setFrom] = useState(() => {
    const d = new Date(); d.setMonth(d.getMonth() - 1); return d.toISOString().split('T')[0]
  })
  const [to, setTo] = useState(() => new Date().toISOString().split('T')[0])

  const { data: summary } = useQuery<DashboardSummary>(
    ['reports', from, to],
    () => api.get('/reports/tickets-summary', { params: { from, to } }).then(r => r.data)
  )

  const { data: slaCompliance } = useQuery(
    ['sla-compliance', from, to],
    () => api.get('/reports/sla-compliance', { params: { from, to } }).then(r => r.data)
  )

  const handleExport = async (format: 'excel' | 'pdf') => {
    const res = await api.get(`/reports/tickets-summary/export/${format === 'excel' ? 'excel' : 'pdf'}`, {
      params: { from, to },
      responseType: 'blob'
    })
    const url = URL.createObjectURL(res.data)
    const a = document.createElement('a')
    a.href = url
    a.download = `tickets_report.${format === 'excel' ? 'xlsx' : 'pdf'}`
    a.click()
    URL.revokeObjectURL(url)
  }

  return (
    <div className="p-8">
      <div className="flex items-center justify-between mb-6">
        <h1 className="text-2xl font-bold text-gray-900">Reports & Analytics</h1>
        <div className="flex gap-2">
          <button className="btn-secondary flex items-center gap-2 text-sm" onClick={() => handleExport('excel')}>
            <Download size={16} /> Export Excel
          </button>
          <button className="btn-secondary flex items-center gap-2 text-sm" onClick={() => handleExport('pdf')}>
            <Download size={16} /> Export PDF
          </button>
        </div>
      </div>

      {/* Date filters */}
      <div className="card mb-6">
        <div className="flex gap-4 items-end">
          <div>
            <label className="label">From</label>
            <input type="date" className="input-field" value={from} onChange={e => setFrom(e.target.value)} />
          </div>
          <div>
            <label className="label">To</label>
            <input type="date" className="input-field" value={to} onChange={e => setTo(e.target.value)} />
          </div>
        </div>
      </div>

      {summary && (
        <>
          {/* KPI Cards */}
          <div className="grid grid-cols-2 lg:grid-cols-4 gap-6 mb-8">
            {[
              { label: 'Total Tickets', value: summary.totalTickets, color: 'text-blue-600' },
              { label: 'Open', value: summary.openTickets, color: 'text-yellow-600' },
              { label: 'Resolved', value: summary.resolvedTickets, color: 'text-green-600' },
              { label: 'SLA Breached', value: summary.slaBreachedTickets, color: 'text-red-600' },
              { label: 'Avg Response (min)', value: summary.averageFirstResponseMinutes, color: 'text-purple-600' },
              { label: 'Avg Resolution (min)', value: summary.averageResolutionMinutes, color: 'text-indigo-600' },
              { label: 'Reopened', value: summary.reopenedTickets, color: 'text-orange-600' },
              { label: 'SLA Compliance', value: slaCompliance ? `${slaCompliance.complianceRate}%` : '-', color: 'text-emerald-600' },
            ].map(item => (
              <div key={item.label} className="card">
                <p className="text-xs text-gray-500">{item.label}</p>
                <p className={`text-2xl font-bold mt-1 ${item.color}`}>{item.value}</p>
              </div>
            ))}
          </div>

          <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
            {/* Category Trends */}
            <div className="card">
              <h2 className="text-base font-semibold text-gray-900 mb-4">Category Breakdown</h2>
              <div className="space-y-3">
                {summary.categoryTrends.map(t => (
                  <div key={t.categoryName} className="flex items-center justify-between">
                    <span className="text-sm text-gray-600">{t.categoryName}</span>
                    <div className="flex items-center gap-3">
                      <div className="w-32 bg-gray-200 rounded-full h-2">
                        <div
                          className="bg-primary-600 h-2 rounded-full"
                          style={{ width: `${(t.count / (summary.totalTickets || 1)) * 100}%` }}
                        />
                      </div>
                      <span className="text-sm font-medium text-gray-900 w-8 text-right">{t.count}</span>
                    </div>
                  </div>
                ))}
              </div>
            </div>

            {/* Agent Performance */}
            <div className="card">
              <h2 className="text-base font-semibold text-gray-900 mb-4">Agent Performance</h2>
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
                  {summary.agentPerformance.map(a => (
                    <tr key={a.agentName} className="border-b border-gray-100 last:border-0">
                      <td className="py-2">{a.agentName}</td>
                      <td className="py-2 text-right">{a.assignedCount}</td>
                      <td className="py-2 text-right">{a.resolvedCount}</td>
                      <td className="py-2 text-right font-medium text-green-600">
                        {a.assignedCount > 0 ? Math.round(a.resolvedCount / a.assignedCount * 100) : 0}%
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </div>
        </>
      )}
    </div>
  )
}
