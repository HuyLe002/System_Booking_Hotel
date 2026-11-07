export const formatVND = (value?: number | null) =>
  new Intl.NumberFormat("vi-VN", { style: "currency", currency: "VND" }).format(value ?? 0);

export const formatDate = (value?: string) =>
  value
    ? new Intl.DateTimeFormat("vi-VN", {
        day: "2-digit",
        month: "2-digit",
        year: "numeric",
      }).format(new Date(value))
    : "—";

export const toDateInput = (date = new Date()) => {
  const local = new Date(date.getTime() - date.getTimezoneOffset() * 60_000);
  return local.toISOString().slice(0, 10);
};

export const nightsBetween = (checkIn: string, checkOut: string) =>
  Math.round((new Date(checkOut).getTime() - new Date(checkIn).getTime()) / 86_400_000);

export const statusLabels: Record<string, string> = {
  Pending: "Đang chờ",
  Confirmed: "Đã xác nhận",
  CheckedIn: "Đã nhận phòng",
  CheckedOut: "Đã trả phòng",
  Cancelled: "Đã hủy",
  Rejected: "Đã từ chối",
  Unpaid: "Chưa thanh toán",
  PartiallyPaid: "Đã thanh toán một phần",
  Paid: "Đã thanh toán",
  Refunded: "Đã hoàn tiền",
  Retained: "Đã giữ lại",
  Active: "Đang hoạt động",
  Inactive: "Đã tắt",
};
