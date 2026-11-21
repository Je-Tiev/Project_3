# SECURITY GUARDRAILS (ALWAYS ON)

> **Phạm vi áp dụng:** Bắt buộc tuân thủ trên toàn bộ codebase (`BE/`, `FE/`, scripts, configs, docker).
> **Chi tiết lỗ hổng cần phòng tránh:** [01_SYSTEM_AUDIT_REPORT.md](file:///c:/Users/coc14/OneDrive/Documents/GitHub/Project_3/docs/audit_and_plan/01_SYSTEM_AUDIT_REPORT.md#3-danh-mục-điểm-yếu-lỗ-hổng-và-nợ-kỹ-thuật-p0---p3)

---

## 1. QUẢN LÝ BÍ MẬT & THÔNG TIN XÁC THỰC (SECRETS MANAGEMENT)
- **Cấm tuyệt đối hardcode secrets:** Không bao giờ ghi trực tiếp Connection String, JWT Key, Passwords, API Keys vào code C#, JS, `appsettings.json`, hay `docker-compose.yml` khi commit Git.
- **Biến môi trường bắt buộc:** Mọi thông tin nhạy cảm phải nạp qua Environment Variables (`ConnectionStrings__DefaultConnection`, `Jwt__Key`). Luôn duy trì file `.env.example` với dữ liệu mẫu an toàn.
- **Fail-Fast Validation:** Ứng dụng phải dừng ngay khi khởi động (throw exception) nếu JWT Key thiếu hoặc ngắn hơn 256 bits (32 bytes).

---

## 2. PHÂN QUYỀN & CHỐNG LEO THANG ĐẶC QUYỀN (ACCESS CONTROL)
- **Khóa chặt Endpoint Quản trị:** Mọi endpoint tạo, sửa quyền Admin/SuperAdmin bắt buộc phải có `[Authorize(Roles = "SuperAdmin")]` hoặc `[Authorize(Roles = "Admin")]`. Tuyệt đối không mở public endpoint đăng ký Admin.
- **Chuẩn HTTP Status Code:** Trả về `401 Unauthorized` khi chưa đăng nhập; trả về `403 Forbidden` khi đã đăng nhập nhưng không đủ quyền thực hiện hành động.

---

## 3. CHỐNG LỖ HỔNG IDOR (INSECURE DIRECT OBJECT REFERENCES)
- **Nguyên tắc xác thực sở hữu:** Khi nhận `documentId`, `meetingId`, `pollId`, controller không được query trực tiếp và trả về dữ liệu.
- **Bắt buộc kiểm tra tư cách thành viên:** Phải trích xuất `currentUserId` từ JWT Claims và kiểm tra quan hệ `MeetingParticipant` hoặc quyền Host trong cùng `meetingId`. Nếu không thuộc cuộc họp, lập tức trả về `403 Forbidden`.
- **Bảo vệ danh sách cuộc họp:** Endpoint `GetAllMeetings` chỉ dành riêng cho Admin. Người dùng thường chỉ được truy cập endpoint lấy danh sách cuộc họp của chính mình (`GetMyMeetings`).

---

## 4. AN TOÀN LƯU TRỮ VÀ TẢI TỆP (FILE STORAGE SECURITY)
- **Cấm phơi bày Static File:** Tuyệt đối không dùng `UseStaticFiles` map thư mục `Uploads/` ra route public `/files`. Mọi thao tác tải file bắt buộc qua Controller có xác thực và phân quyền.
- **Allowlist Định Dạng:** Chỉ chấp nhận các đuôi file an toàn: `.pdf`, `.docx`, `.xlsx`, `.pptx`, `.png`, `.jpg`. Từ chối mọi file `.exe`, `.sh`, `.bat`, `.html`, `.svg`.
- **Kiểm tra Magic Bytes:** Đọc 4-8 bytes đầu tiên của stream file để xác thực định dạng nhị phân thật, chống giả mạo extension.
- **Chống Path Traversal:** Lưu trữ file bằng GUID ngẫu nhiên (`Guid.NewGuid().ToString() + extension`), không dùng tên file gốc từ client để ghép nối đường dẫn đĩa cứng.

---

## 5. TOÀN VẸN DỮ LIỆU & CHỐNG RACE CONDITION (CONCURRENCY & TRANSACTIONS)
- **Chống Double Voting:** Bắt buộc cấu hình Unique Index trong database: `(PollId, UserId)`. Bắt ngoại lệ `DbUpdateException` để chặn đứng race condition TOCTOU khi cử tri vote đồng thời.
- **Database Transactions:** Mọi thao tác mutation ghi từ 2 bảng trở lên (ví dụ: tạo Meeting + gán Host Participant) bắt buộc phải bọc trong `await using var transaction = await _context.Database.BeginTransactionAsync()` và rollback khi gặp lỗi.
