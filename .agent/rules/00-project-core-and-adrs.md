# PROJECT CORE & ADRs DIRECTIVE (ALWAYS ON)

> **Phạm vi áp dụng:** Toàn bộ Agent, Subagents và mọi phiên làm việc trên `Project_3`.
> **Tài liệu gốc tham chiếu:** [02_GAP_ANALYSIS_AND_DECISIONS.md](file:///c:/Users/coc14/OneDrive/Documents/GitHub/Project_3/docs/audit_and_plan/02_GAP_ANALYSIS_AND_DECISIONS.md)

---

## 1. ĐỊNH VỊ LÕI SẢN PHẨM (CORE POSITIONING)
- **Hệ thống Quản trị Phòng họp Không Giấy Tờ (Paperless Governance)** là trọng tâm cốt lõi duy nhất của dự án.
- Không phát triển dự án thành bản sao Zoom/Teams. Tính năng truyền thông thời gian thực (WebRTC / SignalR) chỉ phục vụ kết nối phụ trợ cho đại biểu tham gia từ xa và đồng bộ tài liệu/biểu quyết.
- Ưu tiên cao nhất là hoàn thiện chu trình: Trước họp (Lập lịch, gửi tài liệu mật) -> Trong họp (Xem tài liệu vector, biểu quyết kín, ghi chép cá nhân) -> Sau họp (Ký duyệt kết luận, ban hành biên bản).

---

## 2. BỐN QUYẾT ĐỊNH KIẾN TRÚC ĐÃ PHÊ DUYỆT (ACCEPTED ADRs)
1. **ADR-01 (Định vị lõi):** Tập trung toàn lực vào Paperless Governance; hoàn thiện 5 entity mồ côi (`Department`, `Note`, `Conclusion`, `MeetingAudio`, `MeetingLog`) và chuyển đổi Admin UI sang dữ liệu động từ DB.
2. **ADR-02 (WebRTC Media):** Ngắn hạn (Phase 0-3): Giữ P2P Mesh (≤ 4 người) + bổ sung máy chủ Coturn (STUN/TURN) để cứu mạng 4G/NAT; Dài hạn (Phase 4+): Mở rộng bằng LiveKit SFU khi cần phòng họp lớn.
3. **ADR-03 (Lưu trữ & Trình chiếu):** Mount Docker Volume cho `/app/Uploads` (Phase 0) -> chuyển sang MinIO S3 Presigned URL (Phase 4). Trình chiếu tài liệu bằng `react-pdf`/PDF.js in-app + đồng bộ số trang qua SignalR JSON payload (< 1KB).
4. **ADR-04 (Chuẩn hóa BE & Kiểm thử):** Tái cấu trúc `Repositories/` thành `Services/Interfaces/`. Giữ Service Layer + EF Core DbContext. Khôi phục EF Core Migrations chuẩn (dùng `Migrate()`, cấm `EnsureCreated()`). Bắt buộc kiểm thử tự động với `Paperless_Meeting.Tests` (xUnit + Moq + FluentAssertions).

---

## 3. BẢN ĐỒ CHỈ MỤC TÀI LIỆU (POINTER INDEX)
Khi cần tra cứu chi tiết kỹ thuật, Agent phải đọc trực tiếp các tài liệu nguồn sau:
- **Chi tiết lỗ hổng P0-P3 & Sơ đồ kiến trúc thực tế:** [01_SYSTEM_AUDIT_REPORT.md](file:///c:/Users/coc14/OneDrive/Documents/GitHub/Project_3/docs/audit_and_plan/01_SYSTEM_AUDIT_REPORT.md)
- **Ma trận 27 tính năng & Toàn văn ADRs:** [02_GAP_ANALYSIS_AND_DECISIONS.md](file:///c:/Users/coc14/OneDrive/Documents/GitHub/Project_3/docs/audit_and_plan/02_GAP_ANALYSIS_AND_DECISIONS.md)
- **Lộ trình Phase 0 -> Phase 4 & Tiêu chí nghiệm thu:** [03_REVISED_ROADMAP.md](file:///c:/Users/coc14/OneDrive/Documents/GitHub/Project_3/docs/audit_and_plan/03_REVISED_ROADMAP.md)
- **Kịch bản 7 PRs Phase 0 & Verification scripts:** [04_EXECUTION_PLAYBOOK.md](file:///c:/Users/coc14/OneDrive/Documents/GitHub/Project_3/docs/audit_and_plan/04_EXECUTION_PLAYBOOK.md)
- **Mô hình kiến trúc mục tiêu C4:** [ARCHITECTURE_C4.md](file:///c:/Users/coc14/OneDrive/Documents/GitHub/Project_3/BE/ARCHITECTURE_C4.md)

---

## 4. KỶ LUẬT THỰC THI CHUNG (GOLDEN RULES)
- **Code is Truth:** Luôn kiểm tra code thực tế bằng `read_file` trước khi phán đoán, không dựa dẫm vào tài liệu cũ.
- **Strict Scope Boundary:** Chỉ chỉnh sửa đúng các file trong phạm vi PR/Task được giao; không tự ý refactor lan man.
- **Zero Mock Data Policy:** Tuyệt đối không mock dữ liệu ở FE cho những tính năng backend đã có API hoạt động.
- **No Test Regression:** Không được xóa hoặc comment out bất kỳ assertion kiểm thử nào để "cho test pass".
