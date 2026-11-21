# FRONTEND REACT 18 & UX STANDARDS

> **Phạm vi áp dụng:** Toàn bộ thư mục `FE/**`.
> **Chi tiết lỗi UX cần tránh:** [01_SYSTEM_AUDIT_REPORT.md](file:///c:/Users/coc14/OneDrive/Documents/GitHub/Project_3/docs/audit_and_plan/01_SYSTEM_AUDIT_REPORT.md#5-phần-3-chất-lượng-mã-nguồn--uiux)

---

## 1. ĐỒNG BỘ CONTRACT VỚI BACKEND (API & DTO SYNC)
- **Đồng bộ Enum & Status:** Tuyệt đối không tự bịa đặt chuỗi status. Trạng thái cuộc họp phải map chuẩn xác theo chuỗi PascalCase từ BE: `'Scheduled'`, `'Ongoing'`, `'Completed'`, `'Postponed'`.
- **An toàn So sánh Role:** Luôn so sánh role không phân biệt hoa thường: `user?.role?.toLowerCase() === 'admin'`.
- **Đường dẫn Tương đối (Relative URLs):** Không hardcode `http://localhost:5075` hay bất kỳ IP/Port nào trong code. Toàn bộ API gọi qua relative `/api` và Hub gọi qua `/meetinghub` để Nginx điều phối.

---

## 2. QUẢN LÝ STATE & HIỆU NĂNG RENDER (STATE MANAGEMENT)
- **Cấm Timer tại Root Context:** Tuyệt đối không đặt `setInterval` cập nhật thời gian (`currentTime`) trong `AppContext` vì sẽ ép toàn bộ cây component re-render mỗi giây. Đồng hồ thời gian thực phải là component độc lập `<LiveClock />`.
- **Chuẩn Hóa Routing:** Sử dụng `react-router-dom` v6 chuẩn. Xóa bỏ hoàn toàn mã `window.location.hash` và file rác `RouterContext.jsx`. Các trang phân hệ Admin phải dùng Nested Routes (`<Outlet />`) để bảo toàn URL deep-linking và browser history.

---

## 3. KHẢ NĂNG CHỊU LỖI & TRẢI NGHIỆM NGƯỜI DÙNG (RESILIENCE & UX)
- **Bắt buộc Error Boundary:** Bọc component gốc trong `<ErrorBoundary>` để hiển thị giao diện khôi phục thân thiện khi có exception, loại bỏ hoàn toàn lỗi màn hình trắng (White Screen of Death).
- **Bộ 3 Trạng Thái Bắt Buộc:** Mọi component tải dữ liệu từ API đều phải xử lý đủ 3 trạng thái: Loading (Spinner/Skeleton) -> Empty (khi danh sách rỗng) -> Error (thông báo lỗi kèm nút Thử lại). Không lạm dụng `alert()`.
- **Auto-Refresh Token:** Dùng Axios Interceptor với hàng đợi request pending: khi gặp 401, tự động gọi API `/api/Auth/refresh-token` và replay request nền, không đẩy văng người dùng ra trang login khi đang họp.

---

## 4. VÒNG ĐỜI SIGNALR & WEBRTC (REAL-TIME LIFECYCLE)
- **Dọn dẹp Bộ nhớ khi Unmount (Cleanup):** Trong cleanup function của `useEffect` phòng họp, bắt buộc lặp qua toàn bộ `peersRef.current` gọi `peer.close()`, dừng tất cả local media tracks và ngắt kết nối SignalR.
- **Xử lý Reconnect:** Bắt buộc đăng ký sự kiện `connection.onreconnected` để tự động gọi lại `JoinRoom` và tái thiết lập kết nối WebRTC khi mạng chập chờn.

---

## 5. TRÌNH CHIẾU TÀI LIỆU TRONG PHÒNG HỌP (DOCUMENT PREVIEW)
- **In-App PDF Viewer (ADR-03):** Tích hợp thư viện `react-pdf` / PDF.js để hiển thị văn bản vector trực tiếp trong phòng họp, không dùng `window.open(blob)` mở tab mới thô sơ.
- **Đồng bộ Slide Vector:** Đồng bộ số trang và thao tác trỏ laser của Chủ tọa qua SignalR event JSON nhẹ (< 1KB), không dùng Screen Sharing để chiếu tài liệu.
