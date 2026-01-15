# Paperless Meeting - Backend Architecture (C4 Model)

## Tổng Quan

Tài liệu này mô tả kiến trúc backend của hệ thống **Paperless Meeting** sử dụng mô hình C4.

---

## 1. Ngữ Cảnh Hệ Thống

### 1.1 Mô Tả Chung

Hệ thống quản lý cuộc họp không giấy tờ, cho phép:
- Quản lý người tham dự cuộc họp
- Chia sẻ và quản lý tài liệu
- Ghi âm và ghi chú cuộc họp
- Bỏ phiếu và khảo sát trực tuyến
- Real-time collaboration với WebSocket (SignalR)

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

## 2. Kiến Trúc Container

### 2.1 Mô Tả Các Container Chính

1. Controllers
AuthController: xử lý đăng nhập, xác thực và phân quyền người dùng
MeetingsController: quản lý cuộc họp 
DocumentsController: quản lý tài liệu cuộc họp
ParticipantController: quản lý người tham gia cuộc họp

2. Services
AuthService: xử lý xác thực, đăng nhập
MeetingService: xử lý nghiệp vụ liên quan đến cuộc họp
DocumentService: xử lý nghiệp vụ tài liệu
ParticipantService: xử lý nghiệp vụ người tham gia
PollService: xử lý biểu quyết và thống kê kết quả

3. Models
Chứa các thực thể dữ liệu (Entity), ánh xạ với các bảng trong cơ sở dữ liệu PostgreSQL.

4. Hubs
MeetingHub: gửi và nhận các sự kiện realtime (trạng thái cuộc họp, biểu quyết, thông báo)

5. DTOs
Chứa các Data Transfer Object dùng để trao đổi dữ liệu giữa Backend và Frontend.

6. Program.cs
File khởi động ứng dụng, cấu hình:
Dependency Injection
Middleware pipeline
Routing
SignalR
Entity Framework Core

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

### 3.2 Sơ đồ tương tác giữa các thành phần

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
│   Repositories      │ ← Service interfaces
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
├── Repositories/         # Service interfaces (contracts)
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

## 5. API Endpoints Overview

### 5.1 Authentication Endpoints
```
POST   /api/auth/register          - Đăng ký người dùng mới
POST   /api/auth/login             - Đăng nhập
POST   /api/auth/refresh-token     - Làm mới token
POST   /api/auth/logout            - Đăng xuất
```

### 5.2 Meeting Endpoints
```
GET    /api/meetings               - Lấy danh sách cuộc họp
GET    /api/meetings/{id}          - Lấy chi tiết cuộc họp
POST   /api/meetings               - Tạo cuộc họp mới
PUT    /api/meetings/{id}          - Cập nhật cuộc họp
DELETE /api/meetings/{id}          - Xóa cuộc họp
```

### 5.3 Participant Endpoints
```
GET    /api/meetings/{id}/participants      - Lấy danh sách người tham dự
POST   /api/meetings/{id}/participants      - Thêm người tham dự
DELETE /api/meetings/{id}/participants/{uid} - Loại bỏ người tham dự
```

### 5.4 Document Endpoints
```
GET    /api/meetings/{id}/documents         - Lấy danh sách tài liệu
POST   /api/meetings/{id}/documents         - Upload tài liệu
DELETE /api/documents/{id}                  - Xóa tài liệu
GET    /api/documents/{id}/download         - Download tài liệu
```

### 5.5 Poll Endpoints
```
GET    /api/meetings/{id}/polls             - Lấy danh sách bỏ phiếu
POST   /api/meetings/{id}/polls             - Tạo bỏ phiếu
POST   /api/polls/{id}/vote                 - Bỏ phiếu
GET    /api/polls/{id}/results              - Lấy kết quả bỏ phiếu
```

### 5.6 SignalR Endpoints
```
WebSocket: /hubs/meeting               - Kết nối real-time
- Events: DocumentUploaded
          ParticipantJoined
          ParticipantLeft
          PollUpdated
          NoteCreated
```

---

## 6. Technology Stack

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

## Tài Liệu Tham Khảo

- [C4 Model - structurizr.com](https://c4model.com/)
- [ASP.NET Core Documentation](https://docs.microsoft.com/en-us/aspnet/core/)
- [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/)
- [SignalR Documentation](https://docs.microsoft.com/en-us/aspnet/core/signalr/)
- [JWT Authentication](https://tools.ietf.org/html/rfc7519)

---

**Last Updated**: January 4, 2026
**Version**: 1.0
