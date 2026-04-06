import { useState } from 'react'
import { useQuery } from 'react-query'
import api from '../services/api'
import { formatDate } from '../utils/helpers'
import { Shield } from 'lucide-react'

export default function AuditLogPage() {
  const [page, setPage] = useState(1)
  const [entityName, setEntityName] = useState('')
  const [actionType, setActionType] = useState('')

  const { data } = useQuery(
    ['audit-logs', page, entityName, actionType],
    () => api.get('/audit', {
      params: { page, pageSize: 30, entityName: entityName || undefined, actionType: actionType || undefined }
    }).then(r => r.data)
  )

  return (
    <div className="p-8">
      <div className="flex items-center gap-3 mb-6">
        <Shield size={24} className="text-primary-600" />
        <h1 className="text-2xl font-bold text-gray-900">Audit Log</h1>
      </div>

      <div className="card mb-6">
        <div className="flex gap-4">
          <div>
            <label className="label">Entity</label>
            <input className="input-field w-40" placeholder="e.g., Ticket" value={entityName} onChange={e => setEntityName(e.target.value)} />
          </div>
          <div>
            <label className="label">Action</label>
            <input className="input-field w-40" placeholder="e.g., Create" value={actionType} onChange={e => setActionType(e.target.value)} />
          </div>
        </div>
      </div>

      <div className="card p-0">
        <table className="w-full text-sm">
          <thead>
            <tr className="bg-gray-50 border-b border-gray-200">
              {['Action', 'Entity', 'Entity ID', 'Performed By', 'IP Address', 'Time'].map(h => (
                <th key={h} className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">{h}</th>
              ))}
            </tr>
          </thead>
          <tbody className="divide-y divide-gray-100">
            {data?.items?.map((log: any) => (
              <tr key={log.id} className="hover:bg-gray-50">
                <td className="px-4 py-3">
                  <span className="inline-flex items-center px-2 py-0.5 rounded text-xs font-medium bg-gray-100 text-gray-700">{log.actionType}</span>
                </td>
                <td className="px-4 py-3 text-gray-900">{log.entityName}</td>
                <td className="px-4 py-3 text-gray-500 font-mono text-xs">{log.entityId?.substring(0, 8)}...</td>
                <td className="px-4 py-3 text-gray-600">{log.performedBy ?? 'System'}</td>
                <td className="px-4 py-3 text-gray-500 text-xs">{log.ipAddress ?? '-'}</td>
                <td className="px-4 py-3 text-gray-500 text-xs">{formatDate(log.createdAt)}</td>
              </tr>
            ))}
          </tbody>
        </table>

        {data && (
          <div className="px-4 py-3 border-t border-gray-200 flex items-center justify-between">
            <p className="text-sm text-gray-500">Total: {data.totalCount} entries</p>
            <div className="flex gap-2">
              <button className="btn-secondary text-sm py-1 px-3" disabled={page === 1} onClick={() => setPage(p => p - 1)}>Previous</button>
              <button className="btn-secondary text-sm py-1 px-3" disabled={page * 30 >= data.totalCount} onClick={() => setPage(p => p + 1)}>Next</button>
            </div>
          </div>
        )}
      </div>
    </div>
  )
}
