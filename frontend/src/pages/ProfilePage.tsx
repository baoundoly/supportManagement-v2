import { useForm } from 'react-hook-form'
import { useQueryClient } from 'react-query'
import toast from 'react-hot-toast'
import api from '../services/api'
import { useAuthStore } from '../store/authStore'
import { getRoleLabel } from '../utils/helpers'
import { User } from 'lucide-react'

export default function ProfilePage() {
  const { user, setUser } = useAuthStore()
  const { register, handleSubmit, formState: { isSubmitting } } = useForm({
    defaultValues: { fullName: user?.fullName, mobile: user?.mobile, designation: user?.designation }
  })
  const { register: pwdRegister, handleSubmit: handlePwdSubmit, reset: resetPwd, formState: { isSubmitting: pwdSubmitting } } = useForm<{ currentPassword: string; newPassword: string }>()

  const onProfileUpdate = async (data: any) => {
    try {
      await api.put(`/users/${user?.id}`, {
        ...data,
        role: user?.role,
        employeeId: user?.employeeId,
        departmentId: user?.departmentId,
        teamId: user?.teamId
      })
      const me = await api.get('/auth/me')
      setUser(me.data)
      toast.success('Profile updated')
    } catch { toast.error('Failed to update profile') }
  }

  const onPasswordChange = async (data: any) => {
    try {
      await api.post('/auth/change-password', data)
      toast.success('Password changed successfully')
      resetPwd()
    } catch (err: any) {
      toast.error(err?.response?.data?.message ?? 'Failed to change password')
    }
  }

  return (
    <div className="p-8 max-w-2xl mx-auto">
      <h1 className="text-2xl font-bold text-gray-900 mb-6">My Profile</h1>

      <div className="card mb-6">
        <div className="flex items-center gap-4">
          <div className="w-16 h-16 rounded-full bg-primary-100 flex items-center justify-center">
            <User size={32} className="text-primary-600" />
          </div>
          <div>
            <h2 className="text-lg font-semibold text-gray-900">{user?.fullName}</h2>
            <p className="text-gray-500">{user?.email}</p>
            <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-blue-100 text-blue-800 mt-1">
              {getRoleLabel(user?.role ?? 1)}
            </span>
          </div>
        </div>
      </div>

      <div className="card mb-6">
        <h2 className="text-base font-semibold text-gray-900 mb-4">Personal Information</h2>
        <form onSubmit={handleSubmit(onProfileUpdate)} className="space-y-4">
          <div>
            <label className="label">Full Name</label>
            <input className="input-field" {...register('fullName', { required: true })} />
          </div>
          <div>
            <label className="label">Mobile</label>
            <input className="input-field" {...register('mobile')} />
          </div>
          <div>
            <label className="label">Designation</label>
            <input className="input-field" {...register('designation')} />
          </div>
          <div className="grid grid-cols-2 gap-4 text-sm text-gray-500">
            <div>
              <p className="label">Department</p>
              <p className="mt-1">{user?.departmentName ?? '-'}</p>
            </div>
            <div>
              <p className="label">Team</p>
              <p className="mt-1">{user?.teamName ?? '-'}</p>
            </div>
          </div>
          <button type="submit" className="btn-primary" disabled={isSubmitting}>
            {isSubmitting ? 'Saving...' : 'Save Changes'}
          </button>
        </form>
      </div>

      <div className="card">
        <h2 className="text-base font-semibold text-gray-900 mb-4">Change Password</h2>
        <form onSubmit={handlePwdSubmit(onPasswordChange)} className="space-y-4">
          <div>
            <label className="label">Current Password</label>
            <input type="password" className="input-field" {...pwdRegister('currentPassword', { required: true })} />
          </div>
          <div>
            <label className="label">New Password</label>
            <input type="password" className="input-field" {...pwdRegister('newPassword', { required: true, minLength: 6 })} />
          </div>
          <button type="submit" className="btn-primary" disabled={pwdSubmitting}>
            {pwdSubmitting ? 'Changing...' : 'Change Password'}
          </button>
        </form>
      </div>
    </div>
  )
}
