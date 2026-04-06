import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { useForm } from 'react-hook-form'
import { useQuery } from 'react-query'
import toast from 'react-hot-toast'
import api from '../services/api'
import { TicketCategory, TicketPriority } from '../types'

interface CreateTicketForm {
  subject: string
  description: string
  categoryId: string
  subcategoryId?: string
  priority: number
  affectedModule?: string
  reportedSource?: string
}

export default function CreateTicketPage() {
  const navigate = useNavigate()
  const { register, handleSubmit, watch, formState: { errors, isSubmitting } } = useForm<CreateTicketForm>({
    defaultValues: { priority: TicketPriority.Medium }
  })
  const [files, setFiles] = useState<File[]>([])

  const { data: categories } = useQuery<TicketCategory[]>(
    'ticket-categories',
    () => api.get('/departments/categories').then(r => r.data)
  )

  const selectedCategory = watch('categoryId')
  const subcategories = categories?.find(c => c.id === selectedCategory)?.subcategories ?? []

  const onSubmit = async (data: CreateTicketForm) => {
    try {
      const res = await api.post('/tickets', data)
      const ticket = res.data

      for (const file of files) {
        const formData = new FormData()
        formData.append('file', file)
        await api.post(`/tickets/${ticket.id}/attachments`, formData, {
          headers: { 'Content-Type': 'multipart/form-data' }
        })
      }

      toast.success(`Ticket ${ticket.ticketNo} created successfully!`)
      navigate(`/tickets/${ticket.id}`)
    } catch (err: any) {
      toast.error(err?.response?.data?.message ?? 'Failed to create ticket')
    }
  }

  return (
    <div className="p-8 max-w-3xl mx-auto">
      <div className="mb-6">
        <h1 className="text-2xl font-bold text-gray-900">Create Support Ticket</h1>
        <p className="text-gray-500 mt-1">Submit a new issue or support request</p>
      </div>

      <form onSubmit={handleSubmit(onSubmit)} className="space-y-6">
        <div className="card space-y-5">
          <div>
            <label className="label">Subject *</label>
            <input className="input-field" placeholder="Brief description of the issue" {...register('subject', { required: 'Subject is required' })} />
            {errors.subject && <p className="text-red-500 text-xs mt-1">{errors.subject.message}</p>}
          </div>

          <div>
            <label className="label">Description *</label>
            <textarea className="input-field h-32 resize-none" placeholder="Describe the issue in detail..." {...register('description', { required: 'Description is required' })} />
            {errors.description && <p className="text-red-500 text-xs mt-1">{errors.description.message}</p>}
          </div>

          <div className="grid grid-cols-2 gap-4">
            <div>
              <label className="label">Category *</label>
              <select className="input-field" {...register('categoryId', { required: 'Category is required' })}>
                <option value="">Select category</option>
                {categories?.map(c => <option key={c.id} value={c.id}>{c.name}</option>)}
              </select>
              {errors.categoryId && <p className="text-red-500 text-xs mt-1">{errors.categoryId.message}</p>}
            </div>

            <div>
              <label className="label">Subcategory</label>
              <select className="input-field" {...register('subcategoryId')}>
                <option value="">Select subcategory</option>
                {subcategories.map(s => <option key={s.id} value={s.id}>{s.name}</option>)}
              </select>
            </div>
          </div>

          <div className="grid grid-cols-2 gap-4">
            <div>
              <label className="label">Priority *</label>
              <select className="input-field" {...register('priority', { valueAsNumber: true })}>
                <option value={TicketPriority.Low}>Low</option>
                <option value={TicketPriority.Medium}>Medium</option>
                <option value={TicketPriority.High}>High</option>
                <option value={TicketPriority.Critical}>Critical</option>
              </select>
            </div>

            <div>
              <label className="label">Affected Module</label>
              <input className="input-field" placeholder="e.g., Billing, Authentication" {...register('affectedModule')} />
            </div>
          </div>

          <div>
            <label className="label">Reported Source</label>
            <input className="input-field" placeholder="e.g., Email, Phone, Walk-in" {...register('reportedSource')} />
          </div>

          <div>
            <label className="label">Attachments</label>
            <input
              type="file"
              multiple
              accept=".jpg,.jpeg,.png,.gif,.pdf,.doc,.docx,.xls,.xlsx,.txt,.zip,.log"
              className="block w-full text-sm text-gray-500 file:mr-4 file:py-2 file:px-4 file:rounded-lg file:border-0 file:text-sm file:font-medium file:bg-primary-50 file:text-primary-700 hover:file:bg-primary-100"
              onChange={e => setFiles(Array.from(e.target.files ?? []))}
            />
            {files.length > 0 && (
              <div className="mt-2 space-y-1">
                {files.map((f, i) => <p key={i} className="text-xs text-gray-500">{f.name}</p>)}
              </div>
            )}
          </div>
        </div>

        <div className="flex gap-3">
          <button type="submit" className="btn-primary" disabled={isSubmitting}>
            {isSubmitting ? 'Creating...' : 'Create Ticket'}
          </button>
          <button type="button" className="btn-secondary" onClick={() => navigate('/tickets')}>Cancel</button>
        </div>
      </form>
    </div>
  )
}
