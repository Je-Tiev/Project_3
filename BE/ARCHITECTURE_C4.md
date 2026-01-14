# Paperless Meeting - Backend Architecture (C4 Model)

## Tổng Quan

Tài liệu này mô tả kiến trúc backend của hệ thống **Paperless Meeting** sử dụng mô hình C4.

---

## 1. System Context (Ngữ Cảnh Hệ Thống)

### 1.1 Mô Tả Chung

```
┌─────────────────────────────────────────────────────────────┐
│                      Paperless Meeting System               │
│                                                             │
│  Hệ thống quản lý cuộc họp không giấy tờ, cho phép:        │
│  - Quản lý người tham dự cuộc họp                          │
│  - Chia sẻ và quản lý tài liệu                             │
│  - Ghi âm và ghi chú cuộc họp                              │
│  - Bỏ phiếu và khảo sát trực tuyến                         │
│  - Real-time collaboration với WebSocket (SignalR)         │
└─────────────────────────────────────────────────────────────┘
```

### 1.2 Người Dùng và Hệ Thống Bên Ngoài

| Người Dùng/Hệ Thống | Mô Tả | Tương Tác |
|---|---|---|
| **Meeting Organizer** | Người tạo và quản lý cuộc họp | Tạo họp, quản lý tài liệu, thêm người tham dự |
| **Meeting Participant** | Người tham gia cuộc họp | Xem tài liệu, bỏ phiếu, viết ghi chú, nghe ghi âm |
| **Admin** | Quản trị viên hệ thống | Quản lý người dùng, tạo cuộc họp tự động |
| **Frontend Web App** | Ứng dụng React/Vite | Gửi request HTTP, kết nối MeetingHub (SignalR) |
| **PostgreSQL Database** | CSDL chính | Lưu trữ dữ liệu người dùng, cuộc họp, tài liệu |
| **File Storage** | Lưu trữ tệp (Local/Cloud) | Lưu tài liệu, ghi âm |

---

## 2. Container Diagram (Kiến Trúc Container)

### 2.1 Mô Tả Các Container Chính

```
┌─────────────────────────────────────────────────────────────────────┐
│                                                                     │
│              Frontend (React/Vite)          │  External Systems    │
│                                             │                      │
│  ┌──────────────────────────┐              │  ┌─────────────────┐ │
│  │   React Web Application  │              │  │  File Storage   │ │
│  │  - Pages & Components    │              │  │  (Documents,    │ │
│  │  - API Client            │──────HTTP───→│→ │   Audio Files)  │ │
│  │  - WebSocket Client      │   REST API   │  │                 │ │
│  │                          │←─ JSON/Data  │  └─────────────────┘ │
│  └──────────────────────────┘              │                      │
│           │                                 │  ┌─────────────────┐ │
│           │ WebSocket                       │  │  PostgreSQL     │ │
│           │ (SignalR)                       │  │  Database       │ │
│           │                                 │  │                 │ │
│           ↓                                 │  └─────────────────┘ │
│  ┌───────────────────────────────────────────────────────────────┐ │
│  │         .NET Core Backend API (ASP.NET Core 8)              │ │
│  │                                                              │ │
│  │  ┌──────────────────────────────────────────────────────┐  │ │
│  │  │ Controllers Layer                                    │  │ │
│  │  │ - AuthController                                    │  │ │
│  │  │ - MeetingsController                               │  │ │
│  │  │ - DocumentsController                              │  │ │
│  │  │ - ParticipantController                            │  │ │
│  │  └──────────────────────────────────────────────────────┘  │ │
│  │                         ↓                                   │ │
│  │  ┌──────────────────────────────────────────────────────┐  │ │
│  │  │ Services Layer                                       │  │ │
│  │  │ - AuthService                                       │  │ │
│  │  │ - MeetingService                                    │  │ │
│  │  │ - DocumentService                                  │  │ │
│  │  │ - ParticipantService                               │  │ │
│  │  │ - PollService                                       │  │ │
│  │  └──────────────────────────────────────────────────────┘  │ │
│  │                         ↓                                   │ │
│  │  ┌──────────────────────────────────────────────────────┐  │ │
│  │  │ Repository Pattern (Data Access)                    │  │ │
│  │  │ - AuthRepository                                    │  │ │
│  │  │ - MeetingRepository                                 │  │ │
│  │  │ - DocumentRepository                                │  │ │
│  │  │ - ParticipantRepository                             │  │ │
│  │  │ - PollRepository                                    │  │ │
│  │  └──────────────────────────────────────────────────────┘  │ │
│  │                         ↓                                   │ │
│  │  ┌──────────────────────────────────────────────────────┐  │ │
│  │  │ Entity Framework Core ORM                           │  │ │
│  │  │ - PaperlessMeetingDbContext                         │  │ │
│  │  │ - Data Models & Relationships                       │  │ │
│  │  └──────────────────────────────────────────────────────┘  │ │
│  │                         ↓                                   │ │
│  │  ┌──────────────────────────────────────────────────────┐  │ │
│  │  │ SignalR Hub                                          │  │ │
│  │  │ - MeetingHub (Real-time updates)                    │  │ │
│  │  └──────────────────────────────────────────────────────┘  │ │
│  └───────────────────────────────────────────────────────────────┘ │
│                                                                     │
└─────────────────────────────────────────────────────────────────────┘
```

### 2.2 Container Chi Tiết

| Container | Công Nghệ | Mô Tả |
|---|---|---|
| **Web Server** | ASP.NET Core 8 | API server chính, xử lý HTTP request |
| **WebSocket** | SignalR | Real-time communication cho các cuộc họp |
| **Database** | PostgreSQL | CSDL chính lưu trữ toàn bộ dữ liệu |
| **File Storage** | Local/Cloud | Lưu tài liệu, ghi âm |
| **Authentication** | JWT Token | Xác thực người dùng với JWT Bearer tokens |
| **API Gateway** | Built-in CORS | Quản lý truy cập từ Frontend |

---

## 3. Component Diagram (Mô Hình Thành Phần)

### 3.1 Backend Components

#### **3.1.1 Authentication Component**
```
AuthController (Endpoint)
    ↓
AuthService (Business Logic)
    ↓
AuthRepository (Data Access)
    ↓
Database: User, RefreshToken
```

**Chức năng:**
- Đăng ký người dùng
- Đăng nhập, phát hành JWT token
- Làm mới token
- Quản lý refresh token

---

#### **3.1.2 Meeting Management Component**
```
MeetingsController (Endpoint)
    ↓
MeetingService (Business Logic)
    ↓
MeetingRepository (Data Access)
    ↓
Database: Meeting, Department
```

**Chức năng:**
- Tạo cuộc họp
- Cập nhật thông tin cuộc họp
- Lấy danh sách cuộc họp
- Xóa cuộc họp

---

#### **3.1.3 Participant Management Component**
```
ParticipantController (Endpoint)
    ↓
ParticipantService (Business Logic)
    ↓
ParticipantRepository (Data Access)
    ↓
Database: MeetingParticipant, MeetingLog
```

**Chức năng:**
- Thêm người tham dự vào cuộc họp
- Loại bỏ người tham dự
- Quản lý vai trò trong cuộc họp
- Theo dõi lịch sử người tham dự

---

#### **3.1.4 Document Management Component**
```
DocumentsController (Endpoint)
    ↓
DocumentService (Business Logic)
    ↓
DocumentRepository (Data Access)
    ↓
Database: Document
File Storage: /Uploads/documents/
```

**Chức năng:**
- Upload tài liệu
- Download tài liệu
- Liệt kê tài liệu của cuộc họp
- Xóa tài liệu
- Kiểm soát phiên bản tài liệu

---

#### **3.1.5 Poll & Voting Component**
```
PollController (Endpoint)
    ↓
PollService (Business Logic)
    ↓
PollRepository (Data Access)
    ↓
Database: Poll, UserVote
```

**Chức năng:**
- Tạo bỏ phiếu
- Ghi lại bình chọn người dùng
- Tính kết quả bỏ phiếu
- Quản lý các lựa chọn bỏ phiếu

---

#### **3.1.6 Real-time Communication Component**
```
MeetingHub (SignalR)
    ↓
Signal Broadcast to Connected Clients
    ↓
Events:
- Document uploaded
- Participant joined/left
- Poll results updated
- Notes shared
```

**Chức năng:**
- Thông báo real-time về các sự kiện
- Đồng bộ hóa trạng thái cuộc họp
- Gửi cập nhật tài liệu
- Quảng bá kết quả bỏ phiếu

---

#### **3.1.7 Database Component**
```
PaperlessMeetingDbContext (EF Core)
    ↓
Entity Models:
├── User
├── Meeting
├── MeetingParticipant
├── Document
├── Poll
├── UserVote
├── Note
├── Conclusion
├── Department
├── MeetingAudio
├── MeetingLog
└── RefreshToken
```

---

### 3.2 Component Interaction Diagram

```
Client Request
    ↓
┌─────────────────────┐
│    Controllers      │ ← HTTP Request
└─────────────────────┘
    ↓
┌─────────────────────┐
│    Services         │ ← Business Logic
└─────────────────────┘
    ↓
┌─────────────────────┐
│   Repositories      │ ← Data Access
└─────────────────────┘
    ↓
┌─────────────────────┐
│  DbContext (EF)     │ ← ORM
└─────────────────────┘
    ↓
┌─────────────────────┐
│    PostgreSQL       │ ← Database
└─────────────────────┘

Response Flow (Reverse)
```

---

## 4. Code Level Architecture

### 4.1 Folder Structure

```
BE/Paperless_Meeting/
├── Controllers/          # API Endpoints
│   ├── Auth/
│   │   └── AuthController.cs
│   └── Meeting/
│       ├── DocumentsController.cs
│       ├── MeetingsController.cs
│       ├── ParticipantController.cs
│       └── ...
├── Services/             # Business Logic
│   ├── Auth/
│   ├── Document/
│   ├── Meeting/
│   ├── Participant/
│   └── Poll/
├── Repositories/         # Data Access
│   ├── Auth/
│   ├── Document/
│   ├── Meeting/
│   ├── Participant/
│   └── Poll/
├── Models/               # Entity Classes
│   ├── User.cs
│   ├── Meeting.cs
│   ├── Document.cs
│   ├── Poll.cs
│   ├── UserVote.cs
│   ├── Note.cs
│   ├── Conclusion.cs
│   ├── Department.cs
│   ├── MeetingParticipant.cs
│   ├── MeetingAudio.cs
│   ├── MeetingLog.cs
│   └── RefreshToken.cs
├── Data/                 # Database Context
│   └── PaperlessMeetingDbContext.cs
├── DTOs/                 # Data Transfer Objects
│   ├── Auth/
│   ├── Document/
│   ├── Meeting/
│   ├── Participant/
│   └── Poll/
├── Hubs/                 # SignalR Hubs
│   └── MeetingHub.cs
├── Migrations/           # EF Core Migrations
├── Properties/           # Launch settings
├── sql/                  # Database scripts
├── Uploads/              # File storage
│   └── documents/
├── Program.cs            # Startup configuration
├── Paperless_Meeting.csproj
├── appsettings.json
└── appsettings.Development.json
```

### 4.2 Layer Architecture

```
┌─────────────────────────────────────────┐
│        Presentation Layer               │
│   (Controllers, API Endpoints)          │
└────────────────┬────────────────────────┘
                 ↓
┌─────────────────────────────────────────┐
│        Business Logic Layer             │
│   (Services, Domain Logic)              │
└────────────────┬────────────────────────┘
                 ↓
┌─────────────────────────────────────────┐
│        Data Access Layer                │
│   (Repositories, Entity Framework)      │
└────────────────┬────────────────────────┘
                 ↓
┌─────────────────────────────────────────┐
│        Database Layer                   │
│   (PostgreSQL)                          │
└─────────────────────────────────────────┘
```

---

## 5. Data Model (Entity Relationships)

### 5.1 Entity Diagram

```
┌──────────────┐
│    User      │
├──────────────┤
│ Id (PK)      │
│ Email        │
│ Password     │
│ FullName     │
│ DepartmentId │──┐
│ ...          │  │
└──────────────┘  │
    ▲             │
    │ (1:N)       │
    │             │
    │      ┌──────────────────┐
    │      │   Department     │
    │      ├──────────────────┤
    │      │ Id (PK)          │
    │      │ Name             │
    │      └──────────────────┘
    │
    │ (1:N)
    │
┌───────────────────────────────┐
│    MeetingParticipant         │
├───────────────────────────────┤
│ UserId (FK, PK)               │
│ MeetingId (FK, PK)            │
│ JoinedAt                       │
│ Role                          │
└───────────────┬───────────────┘
                │ (N:1)
                ↓
┌──────────────────────────┐
│      Meeting             │
├──────────────────────────┤
│ Id (PK)                  │
│ Title                    │
│ Description              │
│ StartTime                │
│ EndTime                  │
│ CreatedByUserId (FK)─────┼──→ User
│ Status                   │
│ DepartmentId (FK)────────┼──→ Department
└──────────┬───────────────┘
           │ (1:N)
           ├─→ Document
           ├─→ Note
           ├─→ Poll
           ├─→ MeetingAudio
           ├─→ MeetingLog
           └─→ Conclusion

┌──────────────────────┐
│     Document         │
├──────────────────────┤
│ Id (PK)              │
│ FileName             │
│ FilePath             │
│ MeetingId (FK)───────┼──→ Meeting
│ UploadedBy (FK)──────┼──→ User
│ UploadedAt           │
└──────────────────────┘

┌──────────────────────┐
│       Poll           │
├──────────────────────┤
│ Id (PK)              │
│ Title                │
│ Question             │
│ MeetingId (FK)───────┼──→ Meeting
│ CreatedAt            │
└──────┬───────────────┘
       │ (1:N)
       └─→ UserVote

┌──────────────────────┐
│     UserVote         │
├──────────────────────┤
│ Id (PK)              │
│ PollId (FK)──────────┼──→ Poll
│ UserId (FK)──────────┼──→ User
│ Choice               │
│ VotedAt              │
└──────────────────────┘

┌──────────────────────┐
│       Note           │
├──────────────────────┤
│ Id (PK)              │
│ Content              │
│ MeetingId (FK)───────┼──→ Meeting
│ UserId (FK)──────────┼──→ User
│ CreatedAt            │
└──────────────────────┘

┌──────────────────────────┐
│      RefreshToken        │
├──────────────────────────┤
│ Id (PK)                  │
│ UserId (FK)──────────────┼──→ User
│ Token                    │
│ ExpiryDate               │
│ IsRevoked                │
└──────────────────────────┘
```

---

## 6. API Endpoints Overview

### 6.1 Authentication Endpoints
```
POST   /api/auth/register          - Đăng ký người dùng mới
POST   /api/auth/login             - Đăng nhập
POST   /api/auth/refresh-token     - Làm mới token
POST   /api/auth/logout            - Đăng xuất
```

### 6.2 Meeting Endpoints
```
GET    /api/meetings               - Lấy danh sách cuộc họp
GET    /api/meetings/{id}          - Lấy chi tiết cuộc họp
POST   /api/meetings               - Tạo cuộc họp mới
PUT    /api/meetings/{id}          - Cập nhật cuộc họp
DELETE /api/meetings/{id}          - Xóa cuộc họp
```

### 6.3 Participant Endpoints
```
GET    /api/meetings/{id}/participants      - Lấy danh sách người tham dự
POST   /api/meetings/{id}/participants      - Thêm người tham dự
DELETE /api/meetings/{id}/participants/{uid} - Loại bỏ người tham dự
```

### 6.4 Document Endpoints
```
GET    /api/meetings/{id}/documents         - Lấy danh sách tài liệu
POST   /api/meetings/{id}/documents         - Upload tài liệu
DELETE /api/documents/{id}                  - Xóa tài liệu
GET    /api/documents/{id}/download         - Download tài liệu
```

### 6.5 Poll Endpoints
```
GET    /api/meetings/{id}/polls             - Lấy danh sách bỏ phiếu
POST   /api/meetings/{id}/polls             - Tạo bỏ phiếu
POST   /api/polls/{id}/vote                 - Bỏ phiếu
GET    /api/polls/{id}/results              - Lấy kết quả bỏ phiếu
```

### 6.6 SignalR Endpoints
```
WebSocket: /hubs/meeting               - Kết nối real-time
- Events: DocumentUploaded
          ParticipantJoined
          ParticipantLeft
          PollUpdated
          NoteCreated
```

---

## 7. Technology Stack

| Layer | Technology |
|---|---|
| **Backend Runtime** | ASP.NET Core 8.0 (.NET) |
| **API Framework** | ASP.NET Core MVC/Web API |
| **Database** | PostgreSQL |
| **ORM** | Entity Framework Core (EF Core) 8 |
| **Real-time** | SignalR |
| **Authentication** | JWT (JSON Web Token) |
| **CORS** | ASP.NET Core CORS Middleware |
| **Logging** | Microsoft.Extensions.Logging |
| **API Documentation** | Swagger/OpenAPI |
| **File Storage** | Local File System / Cloud (Optional) |

---

## 8. Security Architecture

### 8.1 Authentication Flow

```
Client                              Backend
  │                                   │
  ├─── POST /auth/login ────────────→│
  │     (email, password)             │
  │                                   │
  │← JSON (accessToken, refreshToken)─│
  │                                   │
  ├─── API Request + JWT ────────────→│
  │     (Authorization: Bearer {JWT}) │
  │                                   │
  │← Validate JWT ─────────────────→ │
  │  ✓ Signature                      │
  │  ✓ Expiry                         │
  │  ✓ Claims (Issuer, Audience)      │
  │                                   │
  │← Response (200 OK) ───────────────│
```

### 8.2 Security Features

- **JWT Authentication**: Token-based authentication với expiry
- **Refresh Tokens**: Tái tạo access token mà không cần đăng nhập lại
- **CORS Policy**: Whitelist các origin được phép truy cập
- **Password Hashing**: Mã hóa mật khẩu (recommended: bcrypt/ASP.NET Identity)
- **Role-Based Access**: Kiểm soát quyền truy cập dựa trên vai trò
- **HTTPS Only**: API chỉ hoạt động trên kết nối bảo mật

---

## 9. Deployment Architecture

### 9.1 Docker Deployment

```
┌──────────────────────────────────────────────────┐
│         Docker Compose Environment              │
│                                                  │
│  ┌────────────────────────────────────────────┐ │
│  │   Backend Container (ASP.NET Core)         │ │
│  │   - Port: 8080/443                         │ │
│  │   - Image: paperless-meeting:latest        │ │
│  └────────────────────────────────────────────┘ │
│                    │                            │
│                    ↓                            │
│  ┌────────────────────────────────────────────┐ │
│  │   PostgreSQL Container                     │ │
│  │   - Port: 5432                             │ │
│  │   - Volume: Database persistence           │ │
│  └────────────────────────────────────────────┘ │
│                                                  │
└──────────────────────────────────────────────────┘
```

**File**: `docker-compose.yml`

---

## 10. Development Workflow

### 10.1 Local Development Setup

```bash
# 1. Database Migration
dotnet ef migrations add InitialCreate
dotnet ef database update

# 2. Run Backend
dotnet run

# 3. API Documentation
https://localhost:5001/swagger
```

### 10.2 Database Scripts

- `sql/init_schema.sql` - Khởi tạo schema
- `sql/alter_role.sql` - Cấu hình quyền user
- `sql/recreate_refresh_tokens.sql` - Reset refresh tokens

---

## 11. Performance & Scalability Considerations

### 11.1 Optimization Strategies

| Aspect | Strategy |
|---|---|
| **Database Query** | EF Core query optimization, lazy loading, eager loading |
| **Caching** | Implement Redis for frequently accessed data |
| **Real-time** | SignalR connection pooling |
| **File Upload** | Asynchronous file upload, chunking for large files |
| **API Response** | Pagination, DTO mapping, compression |

### 11.2 Monitoring

- Application Logging: Console, File
- Error Tracking: Structured logging
- Performance Metrics: Response times, request counts
- Database Monitoring: Query performance, connection pooling

---

## 12. Key Features & Implementation Notes

### 12.1 Real-time Collaboration

- **WebSocket (SignalR)** cho cập nhật thời gian thực
- **MeetingHub** quản lý các sự kiện: document upload, participant join/leave
- **Broadcast** cập nhật tới tất cả client kết nối

### 12.2 File Management

- Documents lưu trữ tại `/Uploads/documents/`
- Hỗ trợ upload đa tệp với giới hạn 500MB
- Metadata lưu trong Database

### 12.3 Multi-tenancy (Departments)

- Mỗi User thuộc một Department
- Meeting được tổ chức theo Department
- Dữ liệu cô lập theo Department

### 12.4 Meeting Workflow

```
Create → Add Participants → Upload Documents → 
Start Meeting → Share Real-time Updates → 
Conduct Polls → Take Notes → End Meeting → 
Archive Documents & Notes
```

---

## 13. Error Handling & Logging

### 13.1 Error Response Format

```json
{
  "statusCode": 400,
  "message": "Invalid request",
  "errors": {
    "email": ["Email is required"]
  }
}
```

### 13.2 Logging Levels

- **Debug**: Chi tiết nhất, chỉ dùng development
- **Information**: Events quan trọng (login, document upload)
- **Warning**: Điều cần chú ý nhưng không phải lỗi
- **Error**: Lỗi cần xử lý
- **Critical**: Lỗi hệ thống nghiêm trọng

---

## 14. Testing Strategy

### 14.1 Unit Testing

- Service layer logic
- Repository methods
- Business rules validation

### 14.2 Integration Testing

- API endpoints
- Database operations
- Authentication flow

### 14.3 E2E Testing

- Complete user workflows
- Real-time collaboration scenarios

---

## Tài Liệu Tham Khảo

- [C4 Model - structurizr.com](https://c4model.com/)
- [ASP.NET Core Documentation](https://docs.microsoft.com/en-us/aspnet/core/)
- [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/)
- [SignalR Documentation](https://docs.microsoft.com/en-us/aspnet/core/signalr/)
- [JWT Authentication](https://tools.ietf.org/html/rfc7519)

---

**Last Updated**: January 4, 2026
**Version**: 1.0
