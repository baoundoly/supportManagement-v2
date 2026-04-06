import { useState } from 'react'
import { useQuery, useQueryClient } from 'react-query'
import { useForm } from 'react-hook-form'
import toast from 'react-hot-toast'
import api from '../services/api'
import { User, UserRole, Department, SupportTeam } from '../types'
import { getRoleLabel, formatDate } from '../utils/helpers'
import { Plus, UserCheck, UserX } from 'lucide-react'

interface CreateUserForm {
  fullName: string
  email: string
  password: string
  mobile?: string
  role: UserRole
  departmentId?: string
  teamId?: string
}

export default function UsersPage() {
  const qc = useQueryClient()
  const [showModal, setShowModal] = useState(false)
  const { register, handleSubmit, reset, formState: { isSubmitting, errors } } = useForm<CreateUserForm>()

  const { data: users, isLoading } = useQuery<User[]>(
    'users',
    () => api.get('/users').then(r => r.data)
  )

  const { data: departments } = useQuery<Department[]>('departments', () => api.get('/departments').then(r => r.data))
  const { data: teams } = useQuery<SupportTeam[]>('teams', () => api.get('/departments/teams').then(r => r.data))

  const onSubmit = async (data: CreateUserForm) => {
    try {
      await api.post('/users', { ...data, role: parseInt(data.role as any) })
      toast.success('User created successfully')
      qc.invalidateQueries('users')
      setShowModal(false)
      reset()
    } catch (err: any) {
      toast.error(err?.response?.data?.message ?? 'Failed to create user')
    }
  }

  const toggleStatus = async (userId: string, isActive: boolean) => {
    try {
      await api.patch(`/users/${userId}/status`, !isActive, { headers: { 'Content-Type': 'application/json' } })
      toast.success(`User ${!isActive ? 'activated' : 'deactivated'}`)
      qc.invalidateQueries('users')
    } catch { toast.error('Failed to update user status') }
  }

  return (
    <div className="p-8">
      <div className="flex items-center justify-between mb-6">
        <h1 className="text-2xl font-bold text-gray-900">User Management</h1>
        <button className="btn-primary flex items-center gap-2" onClick={() => setShowModal(true)}>
          <Plus size={18} /> Add User
        </button>
      </div>

      <div className="card p-0">
        <table className="w-full text-sm">
          <thead>
            <tr className="bg-gray-50 border-b border-gray-200">
              {['Name', 'Email', 'Role', 'Department', 'Team', 'Status', 'Last Login', 'Actions'].map(h => (
                <th key={h} className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">{h}</th>
              ))}
            </tr>
          </thead>
          <tbody className="divide-y divide-gray-100">
            {isLoading && <tr><td colSpan={8} className="text-center py-8 text-gray-500">Loading...</td></tr>}
            {users?.map(u => (
              <tr key={u.id} className="hover:bg-gray-50">
                <td className="px-4 py-3 font-medium text-gray-900">{u.fullName}</td>
                <td className="px-4 py-3 text-gray-600">{u.email}</td>
                <td className="px-4 py-3">
                  <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-blue-100 text-blue-800">
                    {getRoleLabel(u.role)}
                  </span>
                </td>
                <td className="px-4 py-3 text-gray-500">{u.departmentName ?? '-'}</td>
                <td className="px-4 py-3 text-gray-500">{u.teamName ?? '-'}</td>
                <td className="px-4 py-3">
                  <span className={`inline-flex items-center gap-1 text-xs font-medium ${u.isActive ? 'text-green-600' : 'text-gray-400'}`}>
                    <span className={`w-2 h-2 rounded-full ${u.isActive ? 'bg-green-500' : 'bg-gray-400'}`} />
                    {u.isActive ? 'Active' : 'Inactive'}
                  </span>
                </td>
                <td className="px-4 py-3 text-gray-500 text-xs">{formatDate(u.lastLoginAt)}</td>
                <td className="px-4 py-3">
                  <button
                    className={`p-1.5 rounded-lg transition-colors ${u.isActive ? 'hover:bg-red-50 text-red-500' : 'hover:bg-green-50 text-green-500'}`}
                    onClick={() => toggleStatus(u.id, u.isActive)}
                    title={u.isActive ? 'Deactivate' : 'Activate'}
                  >
                    {u.isActive ? <UserX size={16} /> : <UserCheck size={16} />}
                  </button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      {showModal && (
        <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50 p-4">
          <div className="bg-white rounded-xl shadow-xl w-full max-w-lg">
            <div className="flex items-center justify-between p-5 border-b border-gray-200">
              <h3 className="font-semibold text-gray-900">Create New User</h3>
              <button onClick={() => { setShowModal(false); reset() }} className="text-gray-400 hover:text-gray-600">✕</button>
            </div>
            <form onSubmit={handleSubmit(onSubmit)} className="p-5 space-y-4">
              <div className="grid grid-cols-2 gap-4">
                <div>
                  <label className="label">Full Name *</label>
                  <input className="input-field" {...register('fullName', { required: true })} />
                </div>
                <div>
                  <label className="label">Email *</label>
                  <input type="email" className="input-field" {...register('email', { required: true })} />
                </div>
                <div>
                  <label className="label">Password *</label>
                  <input type="password" className="input-field" {...register('password', { required: true, minLength: 6 })} />
                </div>
                <div>
                  <label className="label">Mobile</label>
                  <input className="input-field" {...register('mobile')} />
                </div>
                <div>
                  <label className="label">Role *</label>
                  <select className="input-field" {...register('role', { required: true })}>
                    <option value={UserRole.EndUser}>End User</option>
                    <option value={UserRole.SupportAgent}>Support Agent</option>
                    <option value={UserRole.TeamLead}>Team Lead</option>
                    <option value={UserRole.Admin}>Admin</option>
                  </select>
                </div>
                <div>
                  <label className="label">Department</label>
                  <select className="input-field" {...register('departmentId')}>
                    <option value="">None</option>
                    {departments?.map(d => <option key={d.id} value={d.id}>{d.name}</option>)}
                  </select>
                </div>
              </div>
              <div className="flex gap-3 pt-2">
                <button type="submit" className="btn-primary flex-1" disabled={isSubmitting}>
                  {isSubmitting ? 'Creating...' : 'Create User'}
                </button>
                <button type="button" className="btn-secondary" onClick={() => { setShowModal(false); reset() }}>Cancel</button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  )
}
