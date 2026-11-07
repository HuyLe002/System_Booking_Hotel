# Aurora Bay Hotel Frontend

Frontend React + TypeScript + Vite cho hệ thống đặt phòng và vận hành khách sạn.

## Cấu hình API

```bash
# Chỉ cần nếu API nằm ở origin khác; mặc định Vite proxy chuyển tiếp /api
VITE_API_BASE_URL=https://localhost:7074
```

Khi chạy local, frontend chuyển tiếp `/api` tới `http://localhost:5233` qua Vite proxy; có thể đổi đích bằng `API_PROXY_TARGET`. Nếu cấu hình `VITE_API_BASE_URL` trỏ thẳng tới backend ở origin khác, hãy khai báo origin frontend trong `Cors:AllowedOrigins` của backend (production dùng biến môi trường `Cors__AllowedOrigins__0`, `Cors__AllowedOrigins__1`, ...).

## Triển khai GitHub Pages

Workflow `.github/workflows/deploy-frontend.yml` tự build và deploy frontend khi
có thay đổi trên nhánh `main`, hoặc khi chạy thủ công. Nó dùng Node.js 22 và
pnpm 10.34.3 theo `.mise.toml`, đặt base path `/System_Booking_Hotel/` và tạo
`404.html` để GitHub Pages nạp ứng dụng cho các route con được mở trực tiếp.
Lần đầu triển khai, chọn GitHub Actions làm nguồn deploy trong Settings → Pages.

Đặt URL backend thực tế làm repository variable có tên `VITE_API_BASE_URL` trong
GitHub (Settings → Secrets and variables → Actions → Variables). Không cần cung
cấp giá trị này để build/deploy; nếu chưa đặt, ứng dụng vẫn chạy nhưng các chức
năng cần API sẽ không hoạt động. Backend cũng phải cho phép origin Pages thực tế
trong `Cors:AllowedOrigins`. Giá trị này được đóng gói công khai vào frontend;
chỉ dùng URL API công khai, không chứa thông tin xác thực. Không lưu JWT key,
mật khẩu admin hay thông tin bí mật khác trong repository variable hoặc source
code.

## Chạy dự án

```bash
pnpm install
pnpm dev
```

Trong Figma Make, development server đã được khởi động sẵn nên chỉ cần chỉnh biến môi trường và tải lại preview.

## Cấu trúc chính

- `src/lib/api.ts`: API client, refresh token và chuẩn hóa lỗi
- `src/auth`: phiên đăng nhập và phân quyền route
- `src/types`: kiểu dữ liệu API
- `src/components`: design system dùng chung
- `src/layouts`: layout khách hàng và dashboard
- `src/pages`: các màn hình nghiệp vụ

Danh sách API hỗ trợ cả mảng và `PagedResult` qua `normalizePaged`. Khi backend trả lỗi validation, thông báo từ API được ưu tiên hiển thị.
