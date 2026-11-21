# Kế Hoạch Phát Triển Ứng Dụng Họp Trực Tuyến (Paperless Meeting)

Dưới đây là kế hoạch phát triển các chức năng từ cơ bản đến nâng cao cho dự án web họp trực tuyến, tập trung vào trải nghiệm "không giấy tờ" (Paperless).

---

## 🟢 Giai đoạn 1: Các chức năng cốt lõi (Basic)
Mục tiêu: Đảm bảo hệ thống có thể hoạt động được cho một cuộc họp trực tuyến tiêu chuẩn.

1. **Quản lý Tài khoản & Xác thực (Authentication):**
   - Đăng nhập, đăng ký, đăng xuất (JWT).
   - Quản lý hồ sơ người dùng (Cập nhật avatar, tên, mật khẩu).
2. **Quản lý Phòng Họp (Room Management):**
   - Tạo phòng họp mới, sinh mã phòng ngẫu nhiên (Meeting ID / Link).
   - Tham gia phòng họp bằng Meeting ID hoặc Link.
   - Cài đặt phòng họp (Cho phép ai vào, phòng chờ - Waiting Room).
3. **Giao tiếp Audio & Video cơ bản (WebRTC/SignalR):**
   - Bật/Tắt Microphone.
   - Bật/Tắt Camera.
   - Hiển thị danh sách grid-view các thành viên trong phòng.
4. **Trò chuyện văn bản (Text Chat):**
   - Chat tổng (Public chat) trong phòng họp.
   - Gửi icon, emoji cơ bản.

---

## 🟡 Giai đoạn 2: Các chức năng nâng cao trải nghiệm (Intermediate)
Mục tiêu: Đáp ứng nhu cầu họp làm việc nhóm, học tập chuyên nghiệp, đúng tính chất "Paperless".

1. **Chia sẻ màn hình (Screen Sharing):**
   - Chia sẻ toàn màn hình, chia sẻ một cửa sổ ứng dụng, hoặc một thẻ trình duyệt.
2. **Tài liệu trực tuyến (Paperless Features):**
   - Upload và chia sẻ tài liệu chung trong phòng (PDF, Word, Excel, Slide) để mọi người cùng xem trực tiếp trên nền web mà không cần tải về.
   - Trình chiếu slide được đồng bộ hóa (Host chuyển slide, tất cả người tham gia cùng tự động chuyển theo).
3. **Quản lý Thành viên (Host Controls):**
   - Mute tất cả, yêu cầu bật mic, kick thành viên khỏi phòng.
   - Khóa phòng họp (Lock room) không cho người mới vào.
4. **Tương tác trực tiếp:**
   - Giơ tay phát biểu (Raise hand).
   - Ghi chú cuộc họp chung (Collaborative Meeting Minutes).

---

## 🟠 Giai đoạn 3: Các chức năng chuyên nghiệp (Advanced)
Mục tiêu: Đạt chuẩn các phần mềm họp trực tuyến thương mại (như Zoom, Google Meet).

1. **Ghi hình cuộc họp (Cloud Recording):**
   - Bắt đầu/Dừng ghi hình phiên họp.
   - Xem lại, tải xuống video và tài liệu sau cuộc họp.
2. **Thăm dò ý kiến & Q&A (Polling & Q&A):**
   - Host có thể tạo poll (bầu chọn) trực tiếp trong lúc họp.
   - Mục Q&A riêng biệt để gửi câu hỏi cho diễn giả.
3. **Bảng trắng tương tác (Whiteboard):**
   - Bảng trắng vẽ tay tích hợp, nhiều người có thể cùng vẽ/ghi chú theo thời gian thực.
4. **Background ảo (Virtual Background) & Xóa tiếng ồn (Noise Cancellation):**
   - Làm mờ hậu cảnh, thay nền bằng hình ảnh.
   - Lọc tạp âm bằng công nghệ audio processing.
5. **Breakout Rooms (Chia nhóm nhỏ):**
   - Host có thể chia người tham gia vào các phòng họp nhỏ lẻ để thảo luận, sau đó gom lại phòng chính.

---

## 🔴 Giai đoạn 4: Đột phá với Trí tuệ nhân tạo & Enterprise (AI & Scale)
Mục tiêu: Khác biệt hóa sản phẩm và hướng tới đối tượng doanh nghiệp.

1. **Trợ lý AI trong cuộc họp:**
   - **Live Transcription (Phụ đề trực tiếp):** Tự động chuyển giọng nói thành văn bản (Speech-to-Text).
   - **AI Meeting Summary:** Tự động tóm tắt nội dung cuộc họp, tạo biên bản cuộc họp và gửi email cho người tham gia sau khi kết thúc.
2. **Lên lịch cuộc họp (Calendar Integration):**
   - Tích hợp với Google Calendar, Outlook Calendar.
   - Gửi thư mời tự động nhắc nhở.
3. **Phân quyền doanh nghiệp (Role-based access & SSO):**
   - Đăng nhập thông qua Single Sign-On (SSO, Google, Microsoft).
   - Phân quyền (Admin hệ thống, Admin công ty, User).
4. **Dashboard & Thống kê (Analytics):**
   - Thống kê thời gian họp, mức độ tham gia, báo cáo xuất file.
