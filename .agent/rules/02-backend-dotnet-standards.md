# BACKEND .NET 8 & EF CORE STANDARDS

> **Phạm vi áp dụng:** Toàn bộ thư mục `BE/**`.
> **Kiến trúc chuẩn theo:** [ADR-04](file:///c:/Users/coc14/OneDrive/Documents/GitHub/Project_3/docs/audit_and_plan/02_GAP_ANALYSIS_AND_DECISIONS.md#adr-04-chuẩn-hóa-kiến-trúc-backend--chiến-lược-kiểm-thử-backend-standardization--test-strategy)

---

## 1. PHÂN TẦNG VÀ CẤU TRÚC CODE (ARCHITECTURE LAYERING)
- **Tổ chức thư mục:** `Controllers/` -> `Services/` (implements `Services/Interfaces/`) -> `Data/PaperlessMeetingDbContext`.
- **Loại bỏ Pseudo-Repository:** Đổi tên thư mục `Repositories/` thành `Services/Interfaces/`. Không tạo thêm tầng Repository wrapper thừa thãi; khai thác trực tiếp `DbContext` và `DbSet` trong Service layer.
- **Tách bạch DTOs:** Tuyệt đối không trả về Entity Model ra Controllers. Mọi request/response đều phải dùng DTOs tương ứng trong thư mục `DTOs/`.
- **An toàn Claims:** Luôn dùng `int.TryParse()` khi trích xuất User ID từ Claims; không dùng `int.Parse()` trực tiếp gây lỗi 500 Unhandled Exception.

---

## 2. QUẢN LÝ CƠ SỞ DỮ LIỆU & EF CORE (DATABASE STANDARDS)
- **Chuẩn EF Core Migrations:** Bắt buộc sử dụng `db.Database.Migrate()` trong `Program.cs`. Cấm tuyệt đối sử dụng `db.Database.EnsureCreated()` vì sẽ phá vỡ lịch sử `__EFMigrationsHistory`.
- **Đồng bộ Schema:** Mọi thay đổi về Model C# phải đi kèm migration: `dotnet ef migrations add <MigrationName>`. Không viết script SQL ngoài luồng làm lệch pha schema.
- **Tối ưu hóa Truy vấn:**
  - Sử dụng `.AsNoTracking()` cho tất cả các truy vấn chỉ đọc (Read-only queries).
  - Triệt tiêu N+1 queries: Sử dụng `.Include()` hợp lý hoặc chiếu thẳng sang DTO qua `.Select()`. Sử dụng `AddRangeAsync` cho thao tác thêm nhiều bản ghi.

---

## 3. XỬ LÝ LỖI & LOGGING (ERROR HANDLING & LOGS)
- **Global Exception Middleware:** Mọi ngoại lệ chưa bắt (unhandled) phải đi qua Middleware tập trung trả về chuẩn `ProblemDetails` (RFC 7807), không để lộ stack trace ra môi trường Production.
- **SignalR Error Protection:** Tắt `options.EnableDetailedErrors = false` trên môi trường Production để bảo mật mã nguồn nội bộ.
- **Bảo vệ Dữ liệu Riêng tư:** Không bao giờ ghi log nội dung tin nhắn chat hoặc mật khẩu của người dùng ra console (`MeetingHub.cs`).

---

## 4. TIÊU CHUẨN KIỂM THỬ TỰ ĐỘNG (TESTING STANDARDS)
- **Vị trí Test Project:** `BE/Paperless_Meeting.Tests/` (xUnit + Moq + FluentAssertions + EF Core InMemory/Sqlite).
- **Quy tắc bắt buộc:** Mọi bản vá lỗi bảo mật (P0/P1) và tính năng mới đều phải có Unit/Integration tests đi kèm.
- **Lệnh xác minh:** Lệnh `dotnet test BE/Paperless_Meeting.sln` phải đạt **100% Passed** trước khi bàn giao bất kỳ task backend nào.
