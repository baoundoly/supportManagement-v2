import { TicketStatus, TicketPriority, UserRole } from '../types'

export const getStatusLabel = (status: TicketStatus): string => {
  const labels: Record<TicketStatus, string> = {
    [TicketStatus.New]: 'New',
    [TicketStatus.Open]: 'Open',
    [TicketStatus.Assigned]: 'Assigned',
    [TicketStatus.InProgress]: 'In Progress',
    [TicketStatus.PendingUser]: 'Pending User',
    [TicketStatus.PendingVendor]: 'Pending Vendor',
    [TicketStatus.Resolved]: 'Resolved',
    [TicketStatus.Closed]: 'Closed',
    [TicketStatus.Reopened]: 'Reopened',
    [TicketStatus.Cancelled]: 'Cancelled',
  }
  return labels[status] ?? 'Unknown'
}

export const getStatusClass = (status: TicketStatus): string => {
  const classes: Record<TicketStatus, string> = {
    [TicketStatus.New]: 'badge-new',
    [TicketStatus.Open]: 'badge-open',
    [TicketStatus.Assigned]: 'badge-assigned',
    [TicketStatus.InProgress]: 'badge-inprogress',
    [TicketStatus.PendingUser]: 'badge-assigned',
    [TicketStatus.PendingVendor]: 'badge-assigned',
    [TicketStatus.Resolved]: 'badge-resolved',
    [TicketStatus.Closed]: 'badge-closed',
    [TicketStatus.Reopened]: 'badge-reopened',
    [TicketStatus.Cancelled]: 'badge-cancelled',
  }
  return classes[status] ?? 'badge-new'
}

export const getPriorityLabel = (priority: TicketPriority): string => {
  return TicketPriority[priority] ?? 'Unknown'
}

export const getPriorityClass = (priority: TicketPriority): string => {
  const classes: Record<TicketPriority, string> = {
    [TicketPriority.Low]: 'badge-low',
    [TicketPriority.Medium]: 'badge-medium',
    [TicketPriority.High]: 'badge-high',
    [TicketPriority.Critical]: 'badge-critical',
  }
  return classes[priority] ?? 'badge-low'
}

export const getRoleLabel = (role: UserRole): string => {
  const labels: Record<UserRole, string> = {
    [UserRole.EndUser]: 'End User',
    [UserRole.SupportAgent]: 'Support Agent',
    [UserRole.TeamLead]: 'Team Lead',
    [UserRole.Admin]: 'Admin',
  }
  return labels[role] ?? 'Unknown'
}

export const formatDate = (date?: string): string => {
  if (!date) return '-'
  return new Date(date).toLocaleDateString('en-US', {
    year: 'numeric', month: 'short', day: 'numeric',
    hour: '2-digit', minute: '2-digit'
  })
}

export const formatRelativeTime = (date: string): string => {
  const diff = Date.now() - new Date(date).getTime()
  const minutes = Math.floor(diff / 60000)
  if (minutes < 1) return 'just now'
  if (minutes < 60) return `${minutes}m ago`
  const hours = Math.floor(minutes / 60)
  if (hours < 24) return `${hours}h ago`
  const days = Math.floor(hours / 24)
  return `${days}d ago`
}

export const formatFileSize = (bytes: number): string => {
  if (bytes < 1024) return `${bytes} B`
  if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`
  return `${(bytes / 1024 / 1024).toFixed(1)} MB`
}
