# INFRASTRUCTURE, DOCKER & NGINX STANDARDS

> **Phạm vi áp dụng:** `docker-compose.yml`, `**/Dockerfile`, `**/nginx.conf`, file cấu hình mạng.
> **Chi tiết lỗi hạ tầng cần tránh:** [01_SYSTEM_AUDIT_REPORT.md](file:///c:/Users/coc14/OneDrive/Documents/GitHub/Project_3/docs/audit_and_plan/01_SYSTEM_AUDIT_REPORT.md#1-p0-infra-01-docker-compose-sai-đường-dẫn-build-frontend-làm-sập-triển-khai)

---

## 1. TIÊU CHUẨN NGINX REVERSE PROXY (`FE/nginx.conf`)
- **Định tuyến WebSocket SignalR Bắt Buộc:**
  ```nginx
  location /meetinghub {
      proxy_pass http://backend:8080/meetinghub;
      proxy_http_version 1.1;
      proxy_set_header Upgrade $http_upgrade;
      proxy_set_header Connection "upgrade";
      proxy_set_header Host $host;
      proxy_read_timeout 86400s;
  }
  ```
- **Dung lượng Tải lên Tối đa:** Bắt buộc khai báo `client_max_body_size 500M;` trong `server block` để hỗ trợ upload tài liệu lớn và file âm thanh cuộc họp.
- **Bảo Mật Headers:** Bắt buộc có `server_tokens off;`, `add_header X-Frame-Options "SAMEORIGIN" always;`, `add_header X-Content-Type-Options "nosniff" always;`.

---

## 2. TIÊU CHUẨN DOCKER COMPOSE (`docker-compose.yml`)
- **Build Context Chính Xác:** Trỏ đúng `build: ./FE` cho frontend (cấm dùng `./FE/FE`).
- **Bảo Toàn Dữ Liệu Tệp Upload (Persistent Volume):** Bắt buộc mount named volume `backend_uploads:/app/Uploads` cho service `backend` để chống mất trắng tài liệu khi restart container.
- **Restart Policy & Healthcheck:** Mọi service (`db`, `backend`, `frontend`) đều phải có `restart: unless-stopped`. Service `backend` phải đợi DB sẵn sàng (`condition: service_healthy`).
- **Mạng Nội Bộ:** Giao tiếp giữa frontend/backend/db phải qua Docker bridge network nội bộ, không phụ thuộc vào `localhost` hay IP máy host.

---

## 3. TIÊU CHUẨN DOCKERFILE & BUILD CONTEXT
- **Quyền Hạn Tối Thiểu (Non-Root User):** Runtime container của Backend ASP.NET Core 8 bắt buộc khai báo `USER app` (UID 1654), không chạy dưới quyền root.
- **Tối Ưu Layer Caching:** Copy file `.csproj` và `package.json` để restore dependencies trước khi copy toàn bộ mã nguồn.
- **Bộ Lọc `.dockerignore` Đầy Đủ:** Bắt buộc có `.dockerignore` tại root, `BE/Paperless_Meeting/` và `FE/` loại trừ triệt để: `bin/`, `obj/`, `node_modules/`, `dist/`, `.git/`, `Uploads/`, và các file `*.env*`.

---

## 4. MỞ RỘNG HẠ TẦNG THEO LỘ TRÌNH (INFRA ROADMAP)
- **Phase 3:** Bổ sung container `coturn` (STUN/TURN cổng 3478 UDP/TCP) và `redis:7-alpine` (Pub/Sub SignalR backplane).
- **Phase 4:** Bổ sung container `minio` (S3 Object Storage) thay thế đĩa cứng cục bộ.
