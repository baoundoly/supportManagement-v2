export enum UserRole {
  EndUser = 1,
  SupportAgent = 2,
  TeamLead = 3,
  Admin = 4,
}

export enum TicketStatus {
  New = 1,
  Open = 2,
  Assigned = 3,
  InProgress = 4,
  PendingUser = 5,
  PendingVendor = 6,
  Resolved = 7,
  Closed = 8,
  Reopened = 9,
  Cancelled = 10,
}

export enum TicketPriority {
  Low = 1,
  Medium = 2,
  High = 3,
  Critical = 4,
}

export enum ChatRoomType {
  Public = 1,
  Internal = 2,
}

export interface User {
  id: string
  fullName: string
  email: string
  mobile?: string
  employeeId?: string
  designation?: string
  role: UserRole
  departmentId?: string
  departmentName?: string
  teamId?: string
  teamName?: string
  isActive: boolean
  isOnline: boolean
  lastLoginAt?: string
  avatarUrl?: string
  createdAt: string
}

export interface Ticket {
  id: string
  ticketNo: string
  subject: string
  description: string
  categoryId: string
  categoryName: string
  subcategoryId?: string
  subcategoryName?: string
  priority: TicketPriority
  status: TicketStatus
  createdById: string
  createdByName: string
  assignedTeamId?: string
  assignedTeamName?: string
  assignedUserId?: string
  assignedUserName?: string
  affectedModule?: string
  reportedSource?: string
  firstResponseDueAt?: string
  resolutionDueAt?: string
  firstResponseAt?: string
  resolvedAt?: string
  closedAt?: string
  reopenCount: number
  isSlaBreached: boolean
  createdAt: string
  updatedAt: string
  attachments: TicketAttachment[]
}

export interface TicketSummary {
  id: string
  ticketNo: string
  subject: string
  priority: TicketPriority
  status: TicketStatus
  categoryName: string
  assignedUserName?: string
  assignedTeamName?: string
  isSlaBreached: boolean
  resolutionDueAt?: string
  createdAt: string
}

export interface TicketAttachment {
  id: string
  fileName: string
  contentType: string
  fileSize: number
  fileUrl: string
  createdAt: string
}

export interface ChatMessage {
  id: string
  chatRoomId: string
  senderUserId: string
  senderName: string
  senderAvatarUrl?: string
  messageText: string
  attachmentPath?: string
  attachmentName?: string
  isEdited: boolean
  isDeleted: boolean
  sentAt: string
  editedAt?: string
  unreadCount: number
  isReadByCurrentUser: boolean
  attachments: ChatAttachment[]
}

export interface ChatAttachment {
  id: string
  fileName: string
  contentType: string
  fileSize: number
  fileUrl: string
}

export interface Notification {
  id: string
  title: string
  message: string
  actionUrl?: string
  entityType?: string
  entityId?: string
  isRead: boolean
  readAt?: string
  channel: number
  createdAt: string
}

export interface PagedResult<T> {
  items: T[]
  totalCount: number
  page: number
  pageSize: number
  totalPages: number
}

export interface DashboardSummary {
  totalTickets: number
  openTickets: number
  closedTickets: number
  resolvedTickets: number
  overdueTickets: number
  slaBreachedTickets: number
  reopenedTickets: number
  averageFirstResponseMinutes: number
  averageResolutionMinutes: number
  categoryTrends: CategoryTrend[]
  agentPerformance: AgentPerformance[]
}

export interface CategoryTrend {
  categoryName: string
  count: number
}

export interface AgentPerformance {
  agentName: string
  assignedCount: number
  resolvedCount: number
}

export interface SlaPolicy {
  id: string
  name: string
  priority: TicketPriority
  categoryId?: string
  categoryName?: string
  responseTimeMinutes: number
  resolutionTimeMinutes: number
  pauseDuringPendingUser: boolean
  isActive: boolean
}

export interface KnowledgeBaseArticle {
  id: string
  categoryId: string
  categoryName: string
  title: string
  content: string
  tags?: string
  isPublished: boolean
  createdById: string
  createdByName: string
  sourceTicketId?: string
  viewCount: number
  createdAt: string
  updatedAt: string
}

export interface Department {
  id: string
  name: string
  description?: string
  isActive: boolean
  createdAt: string
}

export interface SupportTeam {
  id: string
  name: string
  description?: string
  departmentId?: string
  departmentName?: string
  isActive: boolean
  memberCount: number
}

export interface TicketCategory {
  id: string
  name: string
  description?: string
  subcategories: { id: string; name: string }[]
}
