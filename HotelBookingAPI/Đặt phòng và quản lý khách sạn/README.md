# Aurora Bay Hotel Frontend

Frontend React + TypeScript + Vite cho hệ thống đặt phòng và vận hành khách sạn.

## Cấu hình API

```bash
# Chỉ cần nếu API nằm ở origin khác; mặc định Vite proxy chuyển tiếp /api
VITE_API_BASE_URL=https://localhost:7074
```

Khi chạy local, frontend chuyển tiếp `/api` tới `http://localhost:5233` qua Vite proxy; có thể đổi đích bằng `API_PROXY_TARGET`. Nếu cấu hình `VITE_API_BASE_URL` trỏ thẳng tới backend ở origin khác, hãy khai báo origin frontend trong `Cors:AllowedOrigins` của backend (production dùng biến môi trường `Cors__AllowedOrigins__0`, `Cors__AllowedOrigins__1`, ...).

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
