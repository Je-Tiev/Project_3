# Paperless Meeting (Project_3)

Đây là dự án hệ thống Web họp trực tuyến (Online Meeting) theo mô hình phòng họp không giấy tờ (Paperless Meeting). 

## 🛠 Công nghệ sử dụng (Tech Stack)

- **Frontend:** React.js (Vite), Tailwind CSS
- **Backend:** .NET Core (C#, ASP.NET) 
- **Cơ sở dữ liệu:** PostgreSQL
- **Giao tiếp thời gian thực:** SignalR (WebSockets)
- **Triển khai & Vận hành:** Docker, Docker Compose, Nginx

## 🚀 Hướng dẫn khởi chạy

Dự án đã được cấu hình sẵn với Docker Compose để bạn có thể khởi chạy dễ dàng.

### Yêu cầu hệ thống:
- Đã cài đặt [Docker](https://www.docker.com/) và [Docker Compose](https://docs.docker.com/compose/install/).

### Các bước chạy dự án bằng Docker:

1. **Clone repository:**
   ```bash
   git clone <repo_url>
   cd Project_3
   ```

2. **Chạy Docker Compose:**
   Dự án sử dụng biến môi trường cho mật khẩu DB, do đó hãy đảm bảo cung cấp `DB_PASSWORD`.
   ```bash
   export DB_PASSWORD=your_secure_password
   docker-compose up --build
   ```

3. **Truy cập ứng dụng:**
   - **Frontend:** [http://localhost:3000](http://localhost:3000)
   - **Backend API:** [http://localhost:5000/api](http://localhost:5000/api)

## 📁 Cấu trúc thư mục chính

- `FE/`: Chứa mã nguồn Frontend (React/Vite).
- `BE/`: Chứa mã nguồn Backend (ASP.NET Core).
- `docker-compose.yml`: Cấu hình chạy các services (Frontend, Backend, Postgres).

## 📝 Kế hoạch phát triển
Xem kế hoạch phát triển các tính năng trong file `PLAN.md` (nếu có chia sẻ) hoặc các issue trên repository này.