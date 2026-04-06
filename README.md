# Support Management System

A full-featured enterprise support management system with integrated ticketing, real-time chat, SLA tracking, and reporting.

## Technology Stack

| Layer | Technology |
|-------|-----------|
| Frontend | React 18, TypeScript, Tailwind CSS, Vite |
| Backend | ASP.NET Core Web API (.NET 10), Clean Architecture |
| Real-time | SignalR |
| Database | PostgreSQL (via EF Core 9) |
| Authentication | JWT + Refresh Tokens |
| File Storage | Local filesystem |
| Reporting | Built-in dashboard + Excel (ClosedXML) + PDF (iText7) export |

## Project Structure

```
.
├── src/
│   ├── SupportManagement.API/          # Web API layer (controllers, hubs, middleware)
│   ├── SupportManagement.Application/  # Application layer (DTOs, interfaces, services)
│   ├── SupportManagement.Domain/       # Domain layer (entities, enums, constants)
│   └── SupportManagement.Infrastructure/ # Infrastructure layer (EF Core, auth, storage)
└── frontend/                           # React frontend (Vite + TypeScript + Tailwind CSS)
```

## Getting Started

### Prerequisites

- .NET 10 SDK
- Node.js 18+
- PostgreSQL 15+

### Backend Setup

1. **Configure database connection** in `src/SupportManagement.API/appsettings.json`:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Host=localhost;Port=5432;Database=SupportManagement;Username=postgres;Password=postgres"
     }
   }
   ```

2. **Configure JWT** — update `JwtSettings.SecretKey` in `appsettings.json` (minimum 32 characters).

3. **Run the API**:
   ```bash
   cd src
   dotnet run --project SupportManagement.API
   ```
   The API automatically runs migrations and seeds initial data on first launch.

4. **API runs on**: `http://localhost:5000`

### Frontend Setup

```bash
cd frontend
npm install
npm run dev
```

Frontend runs on: `http://localhost:3000`

### Default Credentials (seeded on first run)

| Role | Email | Password |
|------|-------|----------|
| Admin | admin@support.local | Admin@123 |
| Support Agent | agent@support.local | Agent@123 |
| End User | user@support.local | User@123 |

## Key Features

### Ticket Management
- Auto-generated ticket numbers (TKT-YYYYMM-NNNNN)
- Full status lifecycle: New → Open → Assigned → In Progress → Pending User/Vendor → Resolved → Closed → Reopened
- Priority levels with SLA enforcement
- File attachments (up to 10MB)

### Real-time Chat (SignalR)
- **Public chat**: Visible to ticket creator, assigned agent, supervisors, and admins
- **Internal chat**: Visible only to support team — hidden from end users
- Typing indicators and read receipts

### SLA Management
- Priority-based SLA policies (Critical: 15min response / 4hr resolution)
- Breach detection and dashboard alerts
- SLA pause/resume for Pending User status

### Reporting & Analytics
- Dashboard KPIs and category trends
- Agent performance tracking
- Export to **Excel** and **PDF**

### Knowledge Base
- Searchable articles organized by category
- Agents can create articles from resolved tickets

### Audit Logging
- Full audit trail of all ticket actions and user changes

## API Endpoints

| Resource | Endpoint |
|----------|----------|
| Auth | `POST /api/auth/login`, `POST /api/auth/refresh-token`, `GET /api/auth/me` |
| Users | `GET/POST /api/users`, `GET/PUT /api/users/{id}` |
| Tickets | `GET/POST /api/tickets`, `POST /api/tickets/{id}/assign`, etc. |
| Chat | `GET /api/chat/tickets/{id}/chat/{type}`, `POST /api/chat/tickets/{id}/chat/{type}/message` |
| SLA | `GET/POST /api/sla/policies`, `GET /api/sla/breaches` |
| Reports | `GET /api/reports/tickets-summary`, export endpoints |
| Knowledge Base | `GET/POST /api/kb/articles`, `GET/POST /api/kb/categories` |
| Notifications | `GET /api/notifications`, mark-read endpoints |
| Audit | `GET /api/audit` |

## SignalR Hub (`/hubs/support`)

| Method | Description |
|--------|-------------|
| `JoinTicketRoom(ticketId, roomType)` | Join public/internal chat room |
| `SendPublicMessage(ticketId, message)` | Send public message |
| `SendInternalMessage(ticketId, message)` | Send internal message (non-EndUsers only) |
| `Typing(ticketId, roomType)` | Broadcast typing indicator |
| `MarkMessageRead(messageId)` | Mark message as read |

## Security

- BCrypt password hashing
- JWT access tokens (60 min expiry) with Refresh Token rotation (7 day expiry)
- Role-based authorization on all endpoints
- Internal chat inaccessible to End Users
- Soft-delete for chat messages (auditable)
- File type and size validation
