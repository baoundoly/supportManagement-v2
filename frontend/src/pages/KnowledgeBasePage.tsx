import { useState } from 'react'
import { useQuery, useQueryClient } from 'react-query'
import { useForm } from 'react-hook-form'
import toast from 'react-hot-toast'
import api from '../services/api'
import { useAuthStore } from '../store/authStore'
import { KnowledgeBaseArticle, UserRole } from '../types'
import { formatDate } from '../utils/helpers'
import { Plus, Search, BookOpen, Eye } from 'lucide-react'

export default function KnowledgeBasePage() {
  const { user } = useAuthStore()
  const qc = useQueryClient()
  const [search, setSearch] = useState('')
  const [selected, setSelected] = useState<KnowledgeBaseArticle | null>(null)
  const [showCreate, setShowCreate] = useState(false)
  const { register, handleSubmit, reset, formState: { isSubmitting } } = useForm<{ title: string; content: string; tags?: string; categoryId: string }>()

  const { data: articles } = useQuery<KnowledgeBaseArticle[]>(
    ['kb-articles', search],
    () => api.get('/kb/articles', { params: { search: search || undefined } }).then(r => r.data)
  )

  const { data: categories } = useQuery(
    'kb-categories',
    () => api.get('/kb/categories').then(r => r.data)
  )

  const canCreate = user?.role === UserRole.Admin || user?.role === UserRole.TeamLead || user?.role === UserRole.SupportAgent

  const onSubmit = async (data: any) => {
    try {
      await api.post('/kb/articles', { ...data, isPublished: true })
      toast.success('Article created')
      qc.invalidateQueries('kb-articles')
      setShowCreate(false)
      reset()
    } catch { toast.error('Failed to create article') }
  }

  return (
    <div className="p-8">
      <div className="flex items-center justify-between mb-6">
        <div className="flex items-center gap-3">
          <BookOpen size={24} className="text-primary-600" />
          <h1 className="text-2xl font-bold text-gray-900">Knowledge Base</h1>
        </div>
        {canCreate && (
          <button className="btn-primary flex items-center gap-2" onClick={() => setShowCreate(true)}>
            <Plus size={18} /> New Article
          </button>
        )}
      </div>

      <div className="relative mb-6">
        <Search size={16} className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-400" />
        <input className="input-field pl-9" placeholder="Search articles..." value={search} onChange={e => setSearch(e.target.value)} />
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        <div className="lg:col-span-1 space-y-3">
          {articles?.map(a => (
            <div key={a.id} onClick={() => setSelected(a)} className={`card cursor-pointer transition-all hover:shadow-md ${selected?.id === a.id ? 'border-primary-500 shadow-md' : ''}`}>
              <h3 className="font-medium text-gray-900 mb-1 line-clamp-2">{a.title}</h3>
              <p className="text-xs text-gray-500">{a.categoryName}</p>
              <div className="flex items-center gap-3 mt-2 text-xs text-gray-400">
                <span className="flex items-center gap-1"><Eye size={12} /> {a.viewCount}</span>
                <span>{formatDate(a.createdAt)}</span>
              </div>
            </div>
          ))}
          {articles?.length === 0 && <p className="text-gray-500 text-sm text-center py-8">No articles found</p>}
        </div>

        <div className="lg:col-span-2">
          {selected ? (
            <div className="card">
              <div className="flex items-start justify-between mb-4">
                <div>
                  <h2 className="text-xl font-bold text-gray-900">{selected.title}</h2>
                  <p className="text-sm text-gray-500 mt-1">{selected.categoryName} · {selected.createdByName}</p>
                </div>
                <span className="flex items-center gap-1 text-xs text-gray-400"><Eye size={12} /> {selected.viewCount} views</span>
              </div>
              <div className="prose prose-sm max-w-none text-gray-700 whitespace-pre-wrap">{selected.content}</div>
              {selected.tags && (
                <div className="mt-4 flex flex-wrap gap-2">
                  {selected.tags.split(',').map(t => (
                    <span key={t.trim()} className="px-2 py-1 bg-gray-100 text-gray-600 rounded text-xs">{t.trim()}</span>
                  ))}
                </div>
              )}
            </div>
          ) : (
            <div className="card flex items-center justify-center h-64">
              <div className="text-center text-gray-400">
                <BookOpen size={48} className="mx-auto mb-3 opacity-30" />
                <p>Select an article to read</p>
              </div>
            </div>
          )}
        </div>
      </div>

      {showCreate && (
        <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50 p-4">
          <div className="bg-white rounded-xl shadow-xl w-full max-w-2xl">
            <div className="flex items-center justify-between p-5 border-b border-gray-200">
              <h3 className="font-semibold text-gray-900">New Knowledge Base Article</h3>
              <button onClick={() => { setShowCreate(false); reset() }} className="text-gray-400 hover:text-gray-600">✕</button>
            </div>
            <form onSubmit={handleSubmit(onSubmit)} className="p-5 space-y-4">
              <div>
                <label className="label">Title *</label>
                <input className="input-field" {...register('title', { required: true })} />
              </div>
              <div>
                <label className="label">Category *</label>
                <select className="input-field" {...register('categoryId', { required: true })}>
                  <option value="">Select category</option>
                  {categories?.map((c: any) => <option key={c.id} value={c.id}>{c.name}</option>)}
                </select>
              </div>
              <div>
                <label className="label">Content *</label>
                <textarea className="input-field h-40 resize-none" {...register('content', { required: true })} />
              </div>
              <div>
                <label className="label">Tags (comma separated)</label>
                <input className="input-field" placeholder="e.g., network, vpn, connectivity" {...register('tags')} />
              </div>
              <div className="flex gap-3">
                <button type="submit" className="btn-primary flex-1" disabled={isSubmitting}>Publish Article</button>
                <button type="button" className="btn-secondary" onClick={() => { setShowCreate(false); reset() }}>Cancel</button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  )
}
