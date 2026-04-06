import { useState, useEffect, useRef } from 'react'
import { useParams, useNavigate } from 'react-router-dom'
import { useQuery, useQueryClient } from 'react-query'
import { useForm } from 'react-hook-form'
import toast from 'react-hot-toast'
import api from '../services/api'
import { signalRService } from '../services/signalr'
import { useAuthStore } from '../store/authStore'
import { Ticket, ChatMessage, UserRole, TicketStatus, User, SupportTeam } from '../types'
import { getStatusClass, getStatusLabel, getPriorityClass, getPriorityLabel, formatDate, formatRelativeTime, formatFileSize } from '../utils/helpers'
import { Send, Paperclip, Clock, AlertTriangle } from 'lucide-react'

export default function TicketDetailPage() {
  const { id } = useParams<{ id: string }>()
  const navigate = useNavigate()
  const { user } = useAuthStore()
  const qc = useQueryClient()
  const [chatTab, setChatTab] = useState<'public' | 'internal'>('public')
  const [publicMessages, setPublicMessages] = useState<ChatMessage[]>([])
  const [internalMessages, setInternalMessages] = useState<ChatMessage[]>([])
  const [isTyping, setIsTyping] = useState(false)
  const [typingUser, setTypingUser] = useState('')
  const messagesEndRef = useRef<HTMLDivElement>(null)
  const { register: chatRegister, handleSubmit: handleChatSubmit, reset: resetChat, formState: { isSubmitting: chatSubmitting } } = useForm<{ message: string }>()
  const { register: resolveRegister, handleSubmit: handleResolveSubmit, formState: { isSubmitting: resolveSubmitting } } = useForm<{ rootCause: string; resolutionSummary: string; fixType?: string }>()
  const [showAssignModal, setShowAssignModal] = useState(false)
  const [showStatusModal, setShowStatusModal] = useState(false)
  const [showResolveModal, setShowResolveModal] = useState(false)
  const [showReopenModal, setShowReopenModal] = useState(false)
  const [assignUserId, setAssignUserId] = useState('')
  const [assignTeamId, setAssignTeamId] = useState('')
  const [newStatus, setNewStatus] = useState('')
  const [reopenReason, setReopenReason] = useState('')

  const { data: ticket, isLoading } = useQuery<Ticket>(
    ['ticket', id],
    () => api.get(`/tickets/${id}`).then(r => r.data)
  )

  const { data: agents } = useQuery<User[]>(
    'agents',
    () => api.get('/users?role=2').then(r => r.data),
    { enabled: user?.role === UserRole.Admin || user?.role === UserRole.TeamLead }
  )

  const { data: teams } = useQuery<SupportTeam[]>(
    'teams',
    () => api.get('/departments/teams').then(r => r.data),
    { enabled: user?.role === UserRole.Admin || user?.role === UserRole.TeamLead }
  )

  const { data: history } = useQuery(
    ['ticket-history', id],
    () => api.get(`/tickets/${id}/history`).then(r => r.data)
  )

  useEffect(() => {
    if (!id) return
    api.get(`/chat/tickets/${id}/chat/public`).then(r => setPublicMessages(r.data.reverse()))
    if (user?.role !== UserRole.EndUser) {
      api.get(`/chat/tickets/${id}/chat/internal`).then(r => setInternalMessages(r.data.reverse()))
    }
  }, [id, user?.role])

  useEffect(() => {
    if (!id) return
    const connect = async () => {
      await signalRService.connect()
      await signalRService.invoke('JoinTicketRoom', id, 'public')
      if (user?.role !== UserRole.EndUser) await signalRService.invoke('JoinTicketRoom', id, 'internal')

      const onMsg = (payload: any) => {
        const msg: ChatMessage = {
          id: payload.id ?? Date.now().toString(),
          chatRoomId: '',
          senderUserId: payload.senderId,
          senderName: payload.senderName,
          senderAvatarUrl: payload.senderAvatarUrl,
          messageText: payload.messageText,
          isEdited: false, isDeleted: false,
          sentAt: payload.sentAt, unreadCount: 0, isReadByCurrentUser: false, attachments: []
        }
        if (payload.roomType === 'public') setPublicMessages(prev => [...prev, msg])
        else setInternalMessages(prev => [...prev, msg])
      }

      const onTyping = (_tid: string, _room: string, _uid: string, name: string) => {
        setTypingUser(name)
        setIsTyping(true)
        setTimeout(() => setIsTyping(false), 3000)
      }

      signalRService.on('ReceiveMessage', onMsg)
      signalRService.on('UserTyping', onTyping)
      return () => {
        signalRService.off('ReceiveMessage', onMsg)
        signalRService.off('UserTyping', onTyping)
        signalRService.invoke('LeaveTicketRoom', id, 'public').catch(() => {})
        if (user?.role !== UserRole.EndUser) signalRService.invoke('LeaveTicketRoom', id, 'internal').catch(() => {})
      }
    }
    const cleanup = connect()
    return () => { cleanup.then(fn => fn?.()) }
  }, [id, user?.role])

  useEffect(() => {
    messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' })
  }, [publicMessages, internalMessages, chatTab])

  const sendMessage = async (data: { message: string }) => {
    if (!data.message.trim()) return
    try {
      const endpoint = `/chat/tickets/${id}/chat/${chatTab}/message`
      const res = await api.post(endpoint, { messageText: data.message, isInternal: chatTab === 'internal' })
      if (chatTab === 'public') setPublicMessages(prev => [...prev, res.data])
      else setInternalMessages(prev => [...prev, res.data])
      resetChat()
    } catch {
      toast.error('Failed to send message')
    }
  }

  const handleAssign = async () => {
    try {
      await api.post(`/tickets/${id}/assign`, { assignedToUserId: assignUserId || undefined, assignedToTeamId: assignTeamId || undefined })
      toast.success('Ticket assigned successfully')
      qc.invalidateQueries(['ticket', id])
      setShowAssignModal(false)
    } catch { toast.error('Failed to assign ticket') }
  }

  const handleStatusChange = async () => {
    try {
      await api.post(`/tickets/${id}/change-status`, { newStatus: parseInt(newStatus) })
      toast.success('Status updated')
      qc.invalidateQueries(['ticket', id])
      setShowStatusModal(false)
    } catch { toast.error('Failed to update status') }
  }

  const handleResolve = async (data: any) => {
    try {
      await api.post(`/tickets/${id}/resolve`, data)
      toast.success('Ticket resolved')
      qc.invalidateQueries(['ticket', id])
      setShowResolveModal(false)
    } catch { toast.error('Failed to resolve ticket') }
  }

  const handleClose = async () => {
    try {
      await api.post(`/tickets/${id}/close`)
      toast.success('Ticket closed')
      qc.invalidateQueries(['ticket', id])
    } catch (err: any) { toast.error(err?.response?.data?.message ?? 'Failed to close ticket') }
  }

  const handleReopen = async () => {
    try {
      await api.post(`/tickets/${id}/reopen`, { reason: reopenReason })
      toast.success('Ticket reopened')
      qc.invalidateQueries(['ticket', id])
      setShowReopenModal(false)
    } catch { toast.error('Failed to reopen ticket') }
  }

  const currentMessages = chatTab === 'public' ? publicMessages : internalMessages

  if (isLoading) return <div className="p-8 text-center text-gray-500">Loading ticket...</div>
  if (!ticket) return <div className="p-8 text-center text-gray-500">Ticket not found</div>

  const canAssign = user?.role === UserRole.Admin || user?.role === UserRole.TeamLead
  const canResolve = user?.role === UserRole.Admin || user?.role === UserRole.TeamLead || user?.role === UserRole.SupportAgent
  const canClose = ticket.status === TicketStatus.Resolved
  const canReopen = ticket.status === TicketStatus.Resolved || ticket.status === TicketStatus.Closed

  return (
    <div className="flex h-full">
      {/* Left Panel */}
      <div className="w-72 border-r border-gray-200 bg-white p-5 overflow-y-auto flex-shrink-0">
        <button onClick={() => navigate('/tickets')} className="text-sm text-primary-600 hover:text-primary-700 mb-4 flex items-center gap-1">
          ← Back to tickets
        </button>

        <div className="space-y-4">
          <div>
            <p className="text-xs text-gray-500 mb-1">Ticket No</p>
            <p className="font-mono font-bold text-gray-900">{ticket.ticketNo}</p>
          </div>

          <div>
            <p className="text-xs text-gray-500 mb-1">Status</p>
            <span className={getStatusClass(ticket.status)}>{getStatusLabel(ticket.status)}</span>
          </div>

          <div>
            <p className="text-xs text-gray-500 mb-1">Priority</p>
            <div className="flex items-center gap-2">
              <span className={getPriorityClass(ticket.priority)}>{getPriorityLabel(ticket.priority)}</span>
              {ticket.isSlaBreached && <span className="text-red-500 flex items-center gap-1 text-xs"><AlertTriangle size={12} /> SLA Breached</span>}
            </div>
          </div>

          <div>
            <p className="text-xs text-gray-500 mb-1">Category</p>
            <p className="text-sm text-gray-900">{ticket.categoryName}</p>
            {ticket.subcategoryName && <p className="text-xs text-gray-500">{ticket.subcategoryName}</p>}
          </div>

          <div>
            <p className="text-xs text-gray-500 mb-1">Created By</p>
            <p className="text-sm font-medium text-gray-900">{ticket.createdByName}</p>
          </div>

          {ticket.assignedUserName && (
            <div>
              <p className="text-xs text-gray-500 mb-1">Assigned To</p>
              <p className="text-sm font-medium text-gray-900">{ticket.assignedUserName}</p>
              {ticket.assignedTeamName && <p className="text-xs text-gray-500">{ticket.assignedTeamName}</p>}
            </div>
          )}

          {ticket.resolutionDueAt && (
            <div>
              <p className="text-xs text-gray-500 mb-1 flex items-center gap-1"><Clock size={12} /> Due Date</p>
              <p className={`text-sm font-medium ${new Date(ticket.resolutionDueAt) < new Date() ? 'text-red-600' : 'text-gray-900'}`}>
                {formatDate(ticket.resolutionDueAt)}
              </p>
            </div>
          )}

          <div>
            <p className="text-xs text-gray-500 mb-1">Created</p>
            <p className="text-sm text-gray-900">{formatDate(ticket.createdAt)}</p>
          </div>

          {ticket.reopenCount > 0 && (
            <div className="bg-orange-50 rounded-lg p-3">
              <p className="text-xs font-medium text-orange-700">Reopened {ticket.reopenCount} time{ticket.reopenCount > 1 ? 's' : ''}</p>
            </div>
          )}
        </div>

        {/* Actions */}
        <div className="mt-6 space-y-2">
          {canAssign && ticket.status !== TicketStatus.Closed && ticket.status !== TicketStatus.Cancelled && (
            <button className="btn-secondary w-full text-sm" onClick={() => setShowAssignModal(true)}>Assign Ticket</button>
          )}
          {ticket.status !== TicketStatus.Closed && ticket.status !== TicketStatus.Cancelled && ticket.status !== TicketStatus.Resolved && (
            <button className="btn-secondary w-full text-sm" onClick={() => setShowStatusModal(true)}>Change Status</button>
          )}
          {canResolve && ticket.status !== TicketStatus.Resolved && ticket.status !== TicketStatus.Closed && ticket.status !== TicketStatus.Cancelled && (
            <button className="btn-primary w-full text-sm" onClick={() => setShowResolveModal(true)}>Mark Resolved</button>
          )}
          {canClose && (
            <button className="btn-primary w-full text-sm bg-green-600 hover:bg-green-700" onClick={handleClose}>Confirm & Close</button>
          )}
          {canReopen && (
            <button className="btn-danger w-full text-sm" onClick={() => setShowReopenModal(true)}>Reopen Ticket</button>
          )}
        </div>
      </div>

      {/* Center Panel */}
      <div className="flex-1 overflow-y-auto p-6 bg-gray-50">
        <h1 className="text-xl font-bold text-gray-900 mb-2">{ticket.subject}</h1>
        {ticket.affectedModule && (
          <p className="text-sm text-gray-500 mb-4">Affected module: <span className="font-medium">{ticket.affectedModule}</span></p>
        )}

        <div className="card mb-6">
          <h2 className="text-sm font-medium text-gray-500 mb-2">Description</h2>
          <p className="text-gray-900 whitespace-pre-wrap">{ticket.description}</p>
        </div>

        {ticket.attachments.length > 0 && (
          <div className="card mb-6">
            <h2 className="text-sm font-medium text-gray-500 mb-3">Attachments ({ticket.attachments.length})</h2>
            <div className="space-y-2">
              {ticket.attachments.map(a => (
                <a key={a.id} href={a.fileUrl} target="_blank" rel="noreferrer" className="flex items-center gap-3 p-3 rounded-lg border border-gray-200 hover:bg-gray-50 transition-colors">
                  <Paperclip size={16} className="text-gray-400" />
                  <div className="flex-1 min-w-0">
                    <p className="text-sm font-medium text-gray-900 truncate">{a.fileName}</p>
                    <p className="text-xs text-gray-500">{formatFileSize(a.fileSize)}</p>
                  </div>
                </a>
              ))}
            </div>
          </div>
        )}

        {/* Activity Timeline */}
        {history && history.length > 0 && (
          <div className="card">
            <h2 className="text-sm font-medium text-gray-500 mb-4">Activity Timeline</h2>
            <div className="space-y-3">
              {history.map((h: any) => (
                <div key={h.id} className="flex gap-3 text-sm">
                  <div className="w-2 h-2 rounded-full bg-primary-500 mt-1.5 flex-shrink-0" />
                  <div>
                    <p className="text-gray-900">
                      <span className="font-medium">{h.changedBy}</span> changed status from{' '}
                      <span className={getStatusClass(h.oldStatus)}>{getStatusLabel(h.oldStatus)}</span> to{' '}
                      <span className={getStatusClass(h.newStatus)}>{getStatusLabel(h.newStatus)}</span>
                    </p>
                    {h.remarks && <p className="text-gray-500 text-xs mt-0.5">{h.remarks}</p>}
                    <p className="text-xs text-gray-400">{formatRelativeTime(h.changedAt)}</p>
                  </div>
                </div>
              ))}
            </div>
          </div>
        )}
      </div>

      {/* Right Panel - Chat */}
      <div className="w-96 border-l border-gray-200 bg-white flex flex-col flex-shrink-0">
        <div className="border-b border-gray-200">
          <div className="flex">
            <button
              onClick={() => setChatTab('public')}
              className={`flex-1 py-3 text-sm font-medium transition-colors ${chatTab === 'public' ? 'text-primary-600 border-b-2 border-primary-600' : 'text-gray-500 hover:text-gray-700'}`}
            >
              Public Chat
            </button>
            {user?.role !== UserRole.EndUser && (
              <button
                onClick={() => setChatTab('internal')}
                className={`flex-1 py-3 text-sm font-medium transition-colors ${chatTab === 'internal' ? 'text-primary-600 border-b-2 border-primary-600' : 'text-gray-500 hover:text-gray-700'}`}
              >
                Internal
              </button>
            )}
          </div>
        </div>

        <div className="flex-1 overflow-y-auto p-4 space-y-3">
          {currentMessages.length === 0 && (
            <p className="text-center text-gray-400 text-sm py-8">No messages yet. Start the conversation!</p>
          )}
          {currentMessages.map(msg => {
            const isMe = msg.senderUserId === user?.id
            return (
              <div key={msg.id} className={`flex ${isMe ? 'justify-end' : 'justify-start'}`}>
                <div className={`max-w-[80%] ${isMe ? 'items-end' : 'items-start'} flex flex-col`}>
                  {!isMe && <p className="text-xs text-gray-500 mb-1">{msg.senderName}</p>}
                  <div className={`px-3 py-2 rounded-2xl text-sm ${isMe ? 'bg-primary-600 text-white rounded-br-none' : 'bg-gray-100 text-gray-900 rounded-bl-none'}`}>
                    {msg.messageText}
                  </div>
                  <p className="text-xs text-gray-400 mt-1">{formatRelativeTime(msg.sentAt)}</p>
                </div>
              </div>
            )
          })}
          {isTyping && (
            <div className="text-xs text-gray-500 italic">{typingUser} is typing...</div>
          )}
          <div ref={messagesEndRef} />
        </div>

        {ticket.status !== TicketStatus.Closed && ticket.status !== TicketStatus.Cancelled && (
          <div className="border-t border-gray-200 p-3">
            <form onSubmit={handleChatSubmit(sendMessage)} className="flex gap-2">
              <input
                className="flex-1 input-field text-sm py-2"
                placeholder={chatTab === 'internal' ? 'Internal note...' : 'Type a message...'}
                {...chatRegister('message', { required: true })}
                onKeyDown={() => signalRService.invoke('Typing', id!, chatTab).catch(() => {})}
              />
              <button type="submit" className="btn-primary px-3 py-2" disabled={chatSubmitting}>
                <Send size={16} />
              </button>
            </form>
          </div>
        )}
      </div>

      {/* Modals */}
      {showAssignModal && (
        <Modal title="Assign Ticket" onClose={() => setShowAssignModal(false)}>
          <div className="space-y-4">
            <div>
              <label className="label">Assign to Agent</label>
              <select className="input-field" value={assignUserId} onChange={e => setAssignUserId(e.target.value)}>
                <option value="">Select agent</option>
                {agents?.map(a => <option key={a.id} value={a.id}>{a.fullName}</option>)}
              </select>
            </div>
            <div>
              <label className="label">Assign to Team</label>
              <select className="input-field" value={assignTeamId} onChange={e => setAssignTeamId(e.target.value)}>
                <option value="">Select team</option>
                {teams?.map(t => <option key={t.id} value={t.id}>{t.name}</option>)}
              </select>
            </div>
            <button className="btn-primary w-full" onClick={handleAssign}>Assign</button>
          </div>
        </Modal>
      )}

      {showStatusModal && (
        <Modal title="Change Status" onClose={() => setShowStatusModal(false)}>
          <div className="space-y-4">
            <div>
              <label className="label">New Status</label>
              <select className="input-field" value={newStatus} onChange={e => setNewStatus(e.target.value)}>
                <option value="">Select status</option>
                {[
                  { value: '2', label: 'Open' }, { value: '4', label: 'In Progress' },
                  { value: '5', label: 'Pending User' }, { value: '6', label: 'Pending Vendor' },
                  { value: '10', label: 'Cancelled' }
                ].map(s => <option key={s.value} value={s.value}>{s.label}</option>)}
              </select>
            </div>
            <button className="btn-primary w-full" onClick={handleStatusChange} disabled={!newStatus}>Update Status</button>
          </div>
        </Modal>
      )}

      {showResolveModal && (
        <Modal title="Resolve Ticket" onClose={() => setShowResolveModal(false)}>
          <form onSubmit={handleResolveSubmit(handleResolve)} className="space-y-4">
            <div>
              <label className="label">Root Cause *</label>
              <textarea className="input-field h-20 resize-none" {...resolveRegister('rootCause', { required: true })} />
            </div>
            <div>
              <label className="label">Resolution Summary *</label>
              <textarea className="input-field h-24 resize-none" {...resolveRegister('resolutionSummary', { required: true })} />
            </div>
            <div>
              <label className="label">Fix Type</label>
              <input className="input-field" placeholder="e.g., Configuration change, Bug fix" {...resolveRegister('fixType')} />
            </div>
            <button type="submit" className="btn-primary w-full" disabled={resolveSubmitting}>
              {resolveSubmitting ? 'Resolving...' : 'Mark as Resolved'}
            </button>
          </form>
        </Modal>
      )}

      {showReopenModal && (
        <Modal title="Reopen Ticket" onClose={() => setShowReopenModal(false)}>
          <div className="space-y-4">
            <div>
              <label className="label">Reason for reopening *</label>
              <textarea className="input-field h-24 resize-none" value={reopenReason} onChange={e => setReopenReason(e.target.value)} />
            </div>
            <button className="btn-danger w-full" onClick={handleReopen} disabled={!reopenReason.trim()}>Reopen Ticket</button>
          </div>
        </Modal>
      )}
    </div>
  )
}

function Modal({ title, children, onClose }: { title: string; children: React.ReactNode; onClose: () => void }) {
  return (
    <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50 p-4">
      <div className="bg-white rounded-xl shadow-xl w-full max-w-md">
        <div className="flex items-center justify-between p-5 border-b border-gray-200">
          <h3 className="font-semibold text-gray-900">{title}</h3>
          <button onClick={onClose} className="text-gray-400 hover:text-gray-600">✕</button>
        </div>
        <div className="p-5">{children}</div>
      </div>
    </div>
  )
}
