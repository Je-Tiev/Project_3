# Hệ Thống Quản Trị Phòng Họp Không Giấy Tờ (Paperless Meeting System)

## 1. Tổng Quan Dự Án (Overview)

Paperless Meeting System là giải pháp số hóa toàn diện quy trình tổ chức, điều hành và quản trị cuộc họp cho các cơ quan, tổ chức và doanh nghiệp. Mục tiêu cốt lõi của hệ thống là loại bỏ hoàn toàn tài liệu in ấn truyền thống trên giấy, nâng cao tính bảo mật thông tin, tối ưu hóa chi phí vận hành và tự động hóa các quy trình nghiệp vụ trước, trong và sau phiên họp.

Hệ thống tích hợp đa chức năng trên một nền tảng duy nhất: từ lập lịch họp, quản lý danh mục phòng họp vật lý và thiết bị, phân quyền tài liệu số theo cấp độ bảo mật, trình chiếu và đồng bộ slide trực tuyến, biểu quyết số (bao gồm bỏ phiếu kín bảo vệ danh tính cử tri), ghi chú cá nhân tự động lưu, ký xác nhận biên bản điện tử, cho đến ghi âm phiên họp và tự động tổng hợp biên bản cuộc họp bằng AI.

## 2. Tính Năng Chính (Key Features)

### Quản trị lịch họp và phòng họp vật lý
- Lập lịch cuộc họp theo phòng ban, thiết lập chủ trì, thư ký và danh sách đại biểu tham gia.
- Quản lý danh mục phòng họp vật lý, sức chứa, vị trí và trang thiết bị đi kèm.
- Tự động kiểm tra và cảnh báo trùng lặp thời gian sử dụng phòng họp vật lý giữa các cuộc họp.

### Số hóa và bảo mật tài liệu cuộc họp
- Tải lên và quản lý tài liệu đính kèm với cơ chế phân quyền truy cập theo cấp độ bảo mật (Công khai, Nội bộ, Mật).
- Trình xem tài liệu nhúng trong ứng dụng (In-App Document Viewer) hỗ trợ định dạng PDF trực tiếp trên trình duyệt mà không cần tải về máy cục bộ.
- Hỗ trợ truyền phát tài liệu tối ưu bằng HTTP Range Processing (HTTP 206 Partial Content), tiết kiệm bộ nhớ đệm máy chủ khi phục vụ tệp tin dung lượng lớn.
- Đồng bộ slide trình chiếu (Slide Sync): Khi chủ tọa hoặc người thuyết trình chuyển trang hoặc thay đổi tỉ lệ thu phóng, màn hình của toàn bộ đại biểu tự động cập nhật đồng bộ theo thời gian thực qua SignalR.

### Tương tác thời gian thực trong phiên họp
- Kênh truyền thông âm thanh và hình ảnh trực tuyến dựa trên WebRTC kết hợp máy chủ TURN/STUN chuyên dụng (Coturn) giúp vượt qua tường lửa doanh nghiệp và mạng di động 4G/5G.
- Text chat nhóm tích hợp trong phòng họp với cơ chế lưu trữ bền vững vào cơ sở dữ liệu PostgreSQL, tự động đồng bộ lại lịch sử tin nhắn khi người dùng tải lại trang.
- Quyền kiểm soát phiên họp dành cho Chủ tọa (Host Controls): Tắt micro toàn bộ đại biểu (Mute All), tắt micro từng cá nhân, khóa phòng họp (Lock Room), và mời đại biểu rời khỏi phiên họp.

### Biểu quyết số và bỏ phiếu kín (Digital Voting & Secret Ballot)
- Khởi tạo các câu hỏi biểu quyết nhanh trong phiên họp với thiết lập biểu quyết công khai hoặc bỏ phiếu kín.
- Cơ chế bỏ phiếu kín (Secret Ballot) tự động che giấu danh tính cử tri trên API và giao diện, chỉ hiển thị tổng số lượng phiếu đồng ý hoặc không đồng ý.
- Chống gian lận bỏ phiếu trùng lặp (Double Voting) bằng ràng buộc khóa duy nhất cấp độ cơ sở dữ liệu kết hợp giao dịch an toàn.
- Cập nhật biểu đồ tỉ lệ biểu quyết theo thời gian thực tới toàn bộ người tham gia qua SignalR.

### Sổ tay ghi chú cá nhân (Personal Notes)
- Cung cấp sổ tay ghi chép riêng tư cho từng đại biểu gắn liền với mã phiên họp.
- Cơ chế tự động lưu nháp trên nền tảng đám mây (Debounced Auto-save 1 giây) giúp bảo toàn dữ liệu ghi chép khi mất kết nối hoặc chuyển đổi thiết bị.

### Biên bản phiên họp và ký xác nhận điện tử
- Thư ký và Chủ tọa dễ dàng soạn thảo và chỉnh sửa dự thảo kết luận phiên họp ngay trong giao diện phòng họp.
- Tích hợp công cụ AI tự động tổng hợp diễn biến, kết quả biểu quyết và danh sách phân công nhiệm vụ thành dự thảo biên bản hành chính chuẩn mực.
- Tích hợp tính năng ký xác nhận điện tử cho phép đại biểu xác thực nội dung kết luận với mốc thời gian rõ ràng.
- Hỗ trợ xuất và in biên bản phiên họp theo mẫu tiêu chuẩn văn bản hành chính.

### Ghi âm phiên họp và nhật ký kiểm toán (Audit Trail)
- Thu âm trực tiếp luồng âm thanh cuộc họp qua trình duyệt bằng HTML5 MediaRecorder API và tải lên lưu trữ tập trung tại máy chủ.
- Nhật ký phiên họp (Meeting Log) tự động lưu vết các hành động quan trọng: thời điểm tham gia, rời phòng, biểu quyết, tải tài liệu và ký biên bản.

### Báo cáo thống kê số liệu thực tế
- Bảng điều khiển tổng quan dành cho Quản trị viên phân tích trực tiếp từ cơ sở dữ liệu PostgreSQL.
- Thống kê số lượng cuộc họp theo từng tháng, tỉ lệ tham gia thực tế của đại biểu, tần suất sử dụng phòng họp vật lý và phân bổ nhân sự theo phòng ban.

## 3. Công Nghệ Sử Dụng (Tech Stack)

### Frontend
- Framework: React 18 (React DOM 18.2.0)
- Công cụ Build: Vite 4
- Định tuyến: React Router DOM v7
- CSS & UI: Tailwind CSS 3, Lucide React (bộ biểu tượng vector)
- Giao tiếp HTTP & API: Axios (tích hợp Interceptor tự động làm mới Access Token qua Refresh Token)
- Kết nối thời gian thực: Microsoft SignalR Client (@microsoft/signalr 10.0.0), Simple-Peer (WebRTC P2P)

### Backend
- Nền tảng: .NET 8 (ASP.NET Core Web API 8.0)
- Ngôn ngữ: C# 12
- ORM & Truy vấn: Entity Framework Core 8.0 (Npgsql.EntityFrameworkCore.PostgreSQL)
- Xác thực & Ủy quyền: JWT (JSON Web Token) Bearer Authentication, Refresh Token Rotation, BCrypt.Net-Next
- Real-time Hub: ASP.NET Core SignalR (tích hợp Microsoft.AspNetCore.SignalR.StackExchangeRedis cho mô hình scale ngang)
- Tài liệu API: Swashbuckle Swagger UI (OpenAPI 3.0)

### Cơ sở dữ liệu & Lưu trữ
- Cơ sở dữ liệu quan hệ: PostgreSQL 15
- Bộ nhớ đệm & Message Broker: Redis 7 (Alpine)
- Lưu trữ tệp tin & Object Storage: Local Persistent Storage, MinIO (tương thích chuẩn AWS S3)

### Hạ tầng & Mạng thời gian thực
- Proxy & Web Server: Nginx (cấu hình reverse proxy, bảo vệ security headers, hỗ trợ nâng cấp giao thức WebSocket cho SignalR)
- Máy chủ STUN/TURN: Coturn Server (chuẩn TURN RFC 5766/6156 hỗ trợ NAT Traversal cho mạng 4G/5G)
- Containerization: Docker, Docker Compose (Multi-stage build)

### Kiểm thử (Testing)
- Framework kiểm thử Backend: xUnit 2.6, Moq 4.20, FluentAssertions 6.12, EF Core InMemory Database

## 4. Yêu Cầu Hệ Thống (Prerequisites)

### Chạy bằng Docker (Khuyến nghị cho môi trường Production / Staging)
- Docker Engine phiên bản 20.10 trở lên.
- Docker Compose v2.0 trở lên (hoặc Docker Desktop phiên bản mới nhất trên Windows / macOS).

### Chạy trực tiếp trên máy cục bộ (Local Development không dùng Docker)
- .NET 8.0 SDK trở lên.
- Node.js phiên bản 18.x hoặc 20.x trở lên kèm công cụ quản lý gói npm.
- PostgreSQL Database Server phiên bản 15 trở lên.
- Redis Server phiên bản 7 trở lên (tùy chọn, cần thiết khi bật chế độ SignalR Redis Backplane).

## 5. Cấu Hình Biến Môi Trường (Environment Variables)

Hệ thống đọc cấu hình thông qua tệp `.env` ở thư mục gốc hoặc thông qua các biến môi trường của hệ điều hành / Docker Compose. Dưới đây là danh mục các biến môi trường cần thiết:

| Tên biến | Áp dụng cho | Mô tả ý nghĩa | Giá trị mẫu / Mặc định an toàn |
| :--- | :--- | :--- | :--- |
| `POSTGRES_USER` | CSDL PostgreSQL / Backend | Tên tài khoản quản trị cơ sở dữ liệu | `postgres` |
| `POSTGRES_PASSWORD` | CSDL PostgreSQL / Backend | Mật khẩu truy cập cơ sở dữ liệu PostgreSQL | `your_secure_db_password` |
| `POSTGRES_DB` | CSDL PostgreSQL / Backend | Tên cơ sở dữ liệu mặc định của ứng dụng | `paperless_meeting` |
| `POSTGRES_PORT` | CSDL PostgreSQL | Cổng nội bộ cơ sở dữ liệu | `5432` |
| `ConnectionStrings__DefaultConnection` | Backend ASP.NET Core | Chuỗi kết nối ADO.NET / Npgsql tới PostgreSQL | `Host=db;Port=5432;Database=paperless_meeting;Username=postgres;Password=your_secure_db_password;SSL Mode=Prefer` |
| `ConnectionStrings__Redis` | Backend ASP.NET Core | Chuỗi kết nối tới Redis cho SignalR Backplane | `redis:6379` |
| `Jwt__Key` | Backend ASP.NET Core | Khóa bí mật dùng để ký xác thực JWT Token (tối thiểu 32 ký tự / 256 bits) | `CHANGE_ME_PRODUCTION_SECRET_KEY_MIN_32_BYTES_LONG` |
| `Jwt__Issuer` | Backend ASP.NET Core | Đơn vị phát hành JWT Token | `PaperlessMeetingBackend` |
| `Jwt__Audience` | Backend ASP.NET Core | Đối tượng tiếp nhận hợp lệ của JWT Token | `PaperlessMeetingFrontend` |
| `Storage__UploadPath` | Backend ASP.NET Core | Thư mục vật lý lưu trữ tệp tin tài liệu và ghi âm | `/app/Uploads` |
| `Webrtc__TurnServer` | Backend / WebRTC | Địa chỉ máy chủ TURN để đại biểu kết nối WebRTC | `turn:127.0.0.1:3478` |
| `Webrtc__TurnUsername` | Backend / WebRTC | Tên đăng nhập xác thực với máy chủ Coturn | `paperless` |
| `Webrtc__TurnPassword` | Backend / WebRTC | Mật khẩu xác thực với máy chủ Coturn | `meeting2026secret` |
| `MINIO_ROOT_USER` | Object Storage MinIO | Tài khoản root quản trị MinIO | `paperless_admin` |
| `MINIO_ROOT_PASSWORD` | Object Storage MinIO | Mật khẩu root quản trị MinIO | `paperless_minio_secret_2026` |
| `VITE_API_URL` | Frontend React / Vite | Đường dẫn gốc cho các API RESTful | `/api` |
| `VITE_HUB_URL` | Frontend React / Vite | Đường dẫn WebSocket cho SignalR Hub | `/meetinghub` |

Chú ý quan trọng về bảo mật:
- Tuyệt đối không commit tệp `.env` chứa khóa bí mật hoặc mật khẩu thật lên hệ thống quản lý mã nguồn Git.
- Biến `Jwt__Key` bắt buộc phải có độ dài từ 32 byte trở lên, nếu không Backend sẽ chủ động ngắt khởi động để ngăn ngừa nguy cơ lộ khóa mã hóa.

## 6. Hướng Dẫn Cài Đặt Và Khởi Chạy (Installation & Running)

### Cách 1: Khởi chạy bằng Docker Compose (Khuyến nghị)

Đây là phương pháp đồng bộ, ổn định nhất giúp khởi động toàn bộ các thành phần: Database PostgreSQL, Cache Redis, Object Storage MinIO, STUN/TURN Coturn, Backend API và Frontend Nginx chỉ bằng một câu lệnh duy nhất.

#### Bước 1: Sao chép mã nguồn về máy
```bash
git clone https://github.com/Je-Tiev/Project_3.git
cd Project_3
```

#### Bước 2: Khởi tạo tệp cấu hình môi trường
Sao chép tệp mẫu `.env.example` thành `.env`:

Trên Linux / macOS:
```bash
cp .env.example .env
```

Trên Windows PowerShell:
```powershell
Copy-Item .env.example .env
```

Chỉnh sửa tệp `.env` vừa tạo để thiết lập các thông số mật khẩu an toàn tùy theo nhu cầu của bạn.

#### Bước 3: Khởi động toàn bộ các dịch vụ
```bash
docker compose up --build -d
```
(Đối với phiên bản Docker cũ, có thể sử dụng lệnh `docker-compose up --build -d`).

#### Bước 4: Kiểm tra trạng thái hoạt động của các Container
```bash
docker compose ps
```

Khi tất cả 6 dịch vụ (`db`, `redis`, `minio`, `coturn`, `backend`, `frontend`) đều hiển thị trạng thái Running hoặc Healthy, hệ thống đã sẵn sàng sử dụng.

#### Lệnh dừng hệ thống khi cần:
```bash
docker compose down
```
(Để xóa toàn bộ dữ liệu volumes khi cần reset môi trường: `docker compose down -v`).

---

### Cách 2: Chạy môi trường phát triển cục bộ (Local Development)

Phương pháp này phù hợp cho lập trình viên cần sửa đổi mã nguồn trực tiếp ở Backend hoặc Frontend.

#### 1. Chuẩn bị cơ sở dữ liệu PostgreSQL và Redis
Đảm bảo PostgreSQL đang chạy ở cổng `5432` và đã tạo cơ sở dữ liệu tên là `paperless_meeting`. Có thể khởi động nhanh CSDL và Redis bằng Docker:
```bash
docker run --name local-postgres -e POSTGRES_PASSWORD=postgres -e POSTGRES_DB=paperless_meeting -p 5432:5432 -d postgres:15
docker run --name local-redis -p 6379:6379 -d redis:7-alpine
```

#### 2. Khởi động Backend (ASP.NET Core Web API)
Di chuyển vào thư mục Backend:
```bash
cd BE/Paperless_Meeting
```

Cập nhật cơ sở dữ liệu bằng EF Core Migrations:
```bash
dotnet ef database update
```

Khởi chạy ứng dụng Backend:
```bash
dotnet run
```
Backend sẽ khởi động tại địa chỉ `http://localhost:5000` (hoặc theo cổng được chỉ định trong `launchSettings.json`).

#### 3. Khởi động Frontend (React Vite)
Mở một cửa sổ terminal mới và di chuyển vào thư mục Frontend:
```bash
cd FE
```

Cài đặt các gói thư viện:
```bash
npm install
```

Khởi chạy Development Server:
```bash
npm run dev
```
Trình duyệt sẽ sẵn sàng truy cập tại địa chỉ `http://localhost:5173`.

## 7. Điểm Truy Cập Dịch Vụ (Service Endpoints)

Sau khi hệ thống được khởi chạy thành công bằng Docker Compose hoặc cục bộ, bạn có thể truy cập các dịch vụ qua các địa chỉ sau:

| Tên thành phần dịch vụ | Giao thức / Port | Địa chỉ truy cập | Mô tả chi tiết |
| :--- | :--- | :--- | :--- |
| Giao diện Web người dùng (Frontend) | HTTP / Port 3000 | `http://localhost:3000` | Giao diện web người dùng gồm Trang chủ, Phòng họp trực tuyến và Trang quản trị Admin |
| Backend RESTful API | HTTP / Port 5000 | `http://localhost:5000/api` | Cổng tiếp nhận các yêu cầu API xác thực, quản lý cuộc họp, tài liệu, biểu quyết và nhật ký |
| Tài liệu Swagger API UI | HTTP / Port 5000 | `http://localhost:5000/swagger` | Giao diện tra cứu và thử nghiệm trực tiếp các endpoint API Backend (.NET Swashbuckle) |
| SignalR Meeting Hub | WebSocket / Port 3000 | `ws://localhost:3000/meetinghub` | Kênh truyền tin thời gian thực dành cho WebRTC Signaling, Đồng bộ Slide, Chat và Biểu quyết |
| Bảng quản trị MinIO Console | HTTP / Port 9001 | `http://localhost:9001` | Giao diện Web quản trị lưu trữ bucket tệp tin (User: `paperless_admin`, Pass: `paperless_minio_secret_2026`) |
| MinIO S3 API Endpoint | HTTP / Port 9000 | `http://localhost:9000` | Cổng giao tiếp API tương thích AWS S3 của dịch vụ lưu trữ MinIO |
| Máy chủ Coturn (STUN/TURN) | TCP & UDP / Port 3478 | `turn:localhost:3478` | Cổng trung chuyển gói tin âm thanh và hình ảnh WebRTC (User: `paperless`, Pass: `meeting2026secret`) |
| Cơ sở dữ liệu PostgreSQL | TCP / Port 5432 | `localhost:5432` | Cổng kết nối cơ sở dữ liệu quan hệ PostgreSQL |
| Redis In-Memory Cache | TCP / Port 6379 | `localhost:6379` | Cổng kết nối bộ nhớ đệm phân tán và SignalR Backplane |

## 8. Chạy Kiểm Thử (Testing)

Dự án đã được trang bị đầy đủ bộ kiểm thử tự động (Unit & Integration Tests) cho Backend để đảm bảo an toàn cho logic nghiệp vụ, bảo mật phân quyền và tính toàn vẹn dữ liệu.

### Chạy kiểm thử Backend
Chạy toàn bộ các Test Suite (Auth, IDOR Security, Concurrency Voting, Room Availability, Secret Ballot, Notes, Conclusions, Audios, Reports, AI Summary) thông qua lệnh:
```bash
dotnet test BE/Paperless_Meeting.Tests
```

### Kiểm tra cú pháp và tính toàn vẹn cấu hình Docker Compose
Kiểm tra tính hợp lệ của tệp `docker-compose.yml` trước khi triển khai:
```bash
docker compose config
```

### Kiểm tra mã nguồn Frontend bằng Linter
```bash
cd FE
npm run lint
```
