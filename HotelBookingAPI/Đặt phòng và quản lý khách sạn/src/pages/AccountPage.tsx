import { ArrowRight, BookOpen, CreditCard, KeyRound, LogOut, Plus, Star } from "lucide-react";
import { FormEvent, useState } from "react";
import { Link, NavLink, Outlet, useNavigate, useParams } from "react-router";
import { useAuth } from "../auth/AuthProvider";
import {
  Badge,
  Button,
  EmptyState,
  ErrorState,
  Field,
  LoadingState,
  Modal,
  Pagination,
  useToast,
} from "../components/ui";
import { useApiData } from "../hooks/useApiData";
import { api, normalizePaged, queryString } from "../lib/api";
import { formatDate, formatVND } from "../lib/format";
import type { Booking } from "../types/api";
import { ChangePasswordForm } from "./AuthPages";

export function AccountLayout() {
  const { user, logout } = useAuth();
  const navigate = useNavigate();
  return (
    <div className="mx-auto grid max-w-7xl gap-8 px-4 py-10 lg:grid-cols-[260px_1fr] lg:px-8">
      <aside className="h-fit rounded-2xl bg-navy p-5 text-white lg:sticky lg:top-28">
        <p className="text-xs text-white/50">Tài khoản của</p>
        <p className="mt-1 font-semibold">{user?.fullName || user?.username}</p>
        <nav className="mt-6 grid gap-1">
          <NavLink
            to="/tai-khoan"
            end
            className={({ isActive }) => `account-link ${isActive ? "account-link-active" : ""}`}
          >
            <BookOpen size={18} />
            Booking của tôi
          </NavLink>
          <NavLink
            to="/tai-khoan/doi-mat-khau"
            className={({ isActive }) => `account-link ${isActive ? "account-link-active" : ""}`}
          >
            <KeyRound size={18} />
            Đổi mật khẩu
          </NavLink>
          <button
            onClick={async () => {
              await logout();
              navigate("/");
            }}
            className="account-link"
          >
            <LogOut size={18} />
            Đăng xuất
          </button>
        </nav>
      </aside>
      <div>
        <Outlet />
      </div>
    </div>
  );
}

export function BookingsListPage() {
  const [page, setPage] = useState(1);
  const { user } = useAuth();
  const result = useApiData(
    () =>
      api<Booking[]>(`/api/Bookings${queryString({ pageNumber: page, pageSize: 8 })}`).then((v) =>
        normalizePaged<Booking>(v),
      ),
    [page],
  );
  return (
    <section>
      <div className="mb-8 flex flex-col gap-5 rounded-3xl bg-navy p-6 text-white sm:flex-row sm:items-center sm:justify-between sm:p-8">
        <div>
          <p className="text-sm font-medium text-gold">KHÔNG GIAN CỦA BẠN</p>
          <h1 className="mt-2 font-display text-3xl font-semibold">
            Xin chào, {user?.fullName || user?.username || "bạn"}!
          </h1>
          <p className="mt-2 max-w-xl text-sm leading-6 text-white/70">
            Theo dõi kỳ nghỉ, thanh toán và các dịch vụ đã chọn tại đây.
          </p>
        </div>
        <Link to="/phong" className="shrink-0">
          <Button variant="secondary">
            <Plus size={17} />
            Khám phá phòng
          </Button>
        </Link>
      </div>
      <p className="eyebrow">Kỳ nghỉ của bạn</p>
      <h2 className="section-title">Booking của tôi</h2>
      {result.loading ? (
        <LoadingState />
      ) : result.error ? (
        <div className="mt-7">
          <ErrorState message={result.error} retry={result.reload} />
        </div>
      ) : !result.data?.items.length ? (
        <div className="mt-7">
          <EmptyState
            title="Bạn chưa có booking nào"
            description="Khám phá các phòng nghỉ và bắt đầu lên kế hoạch cho chuyến đi đầu tiên."
          />
        </div>
      ) : (
        <div className="mt-7 grid gap-4">
          {result.data.items.map((booking) => (
            <Link
              to={`/tai-khoan/booking/${booking.id}`}
              key={booking.id}
              className="grid gap-4 rounded-2xl border border-slate-200 bg-white p-5 transition hover:border-gold hover:shadow-md sm:grid-cols-[1fr_auto]"
            >
              <div>
                <div className="flex flex-wrap items-center gap-2">
                  <h2 className="font-display text-xl font-semibold text-navy">
                    {booking.room
                      ? `Phòng ${booking.room.roomNumber}`
                      : `Booking #${booking.bookingCode || booking.id}`}
                  </h2>
                  <Badge status={booking.status} />
                  <Badge status={booking.paymentStatus} />
                </div>
                <p className="mt-2 text-sm text-slate-500">
                  {formatDate(booking.checkIn)} → {formatDate(booking.checkOut)}
                </p>
              </div>
              <div className="flex items-center justify-between gap-4 sm:justify-end">
                <b className="text-navy">{formatVND(booking.totalAmount)}</b>
                <ArrowRight size={18} className="text-gold-dark" />
              </div>
            </Link>
          ))}
          <Pagination
            page={result.data.pageNumber}
            totalPages={result.data.totalPages}
            onChange={setPage}
          />
        </div>
      )}
    </section>
  );
}

export function BookingDetailPage() {
  const { id } = useParams();
  const notify = useToast();
  const [claimOpen, setClaimOpen] = useState(false);
  const [reviewOpen, setReviewOpen] = useState(false);
  const detail = useApiData(() => api<Booking>(`/api/Bookings/${id}`), [id]);
  const mutate = async (path: string, body?: object) => {
    try {
      await api(path, { method: "PUT", body: body ? JSON.stringify(body) : undefined });
      notify("Đã cập nhật booking.");
      detail.reload();
    } catch (reason) {
      notify(reason instanceof Error ? reason.message : "Thao tác thất bại", "error");
      detail.reload();
    }
  };
  const claim = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    const form = new FormData(event.currentTarget);
    try {
      await api(`/api/Bookings/${id}/payment-claims`, {
        method: "POST",
        body: JSON.stringify({
          amount: Number(form.get("amount")),
          method: form.get("method"),
          note: form.get("note"),
        }),
      });
      notify("Đã gửi yêu cầu đối soát thanh toán.");
      setClaimOpen(false);
      detail.reload();
    } catch (reason) {
      notify(reason instanceof Error ? reason.message : "Không thể gửi yêu cầu", "error");
    }
  };
  const review = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    const form = new FormData(event.currentTarget);
    try {
      await api("/api/Reviews", {
        method: "POST",
        body: JSON.stringify({
          bookingId: Number(id),
          rating: Number(form.get("rating")),
          comment: form.get("comment"),
        }),
      });
      notify("Cảm ơn bạn đã chia sẻ trải nghiệm.");
      setReviewOpen(false);
    } catch (reason) {
      notify(reason instanceof Error ? reason.message : "Không thể gửi đánh giá", "error");
    }
  };
  if (detail.loading) return <LoadingState />;
  if (detail.error || !detail.data)
    return <ErrorState message={detail.error || "Không tìm thấy booking"} retry={detail.reload} />;
  const booking = detail.data;
  return (
    <div>
      <Link to="/tai-khoan" className="text-sm font-semibold text-slate-500 hover:text-navy">
        ← Booking của tôi
      </Link>
      <div className="mt-5 flex flex-wrap items-start justify-between gap-4">
        <div>
          <p className="eyebrow">Chi tiết booking</p>
          <h1 className="section-title">#{booking.bookingCode || booking.id}</h1>
          <div className="mt-3 flex gap-2">
            <Badge status={booking.status} />
            <Badge status={booking.paymentStatus} />
          </div>
        </div>
        <div className="flex flex-wrap gap-2">
          {booking.status !== "Cancelled" && booking.status !== "CheckedOut" && (
            <Button variant="secondary" onClick={() => mutate(`/api/Bookings/${id}/cancel`)}>
              Hủy booking
            </Button>
          )}
          {booking.status === "CheckedOut" && (
            <Button variant="secondary" onClick={() => setReviewOpen(true)}>
              <Star size={17} />
              Đánh giá
            </Button>
          )}
          {(booking.balance ?? 0) > 0 && (
            <Button onClick={() => setClaimOpen(true)}>
              <CreditCard size={17} />
              Gửi yêu cầu thanh toán
            </Button>
          )}
        </div>
      </div>
      <div className="mt-8 grid gap-5 md:grid-cols-3">
        <div className="card md:col-span-2">
          <h2 className="card-title">Thông tin lưu trú</h2>
          <dl className="mt-5 grid gap-5 sm:grid-cols-2">
            <div>
              <dt>Phòng</dt>
              <dd>{booking.room?.roomNumber || booking.roomId}</dd>
            </div>
            <div>
              <dt>Loại phòng</dt>
              <dd>{booking.room?.roomType?.name || "—"}</dd>
            </div>
            <div>
              <dt>Nhận phòng</dt>
              <dd>{formatDate(booking.checkIn)}</dd>
            </div>
            <div>
              <dt>Trả phòng</dt>
              <dd>{formatDate(booking.checkOut)}</dd>
            </div>
          </dl>
          <h3 className="mt-7 font-semibold text-navy">Dịch vụ đã chọn</h3>
          {booking.services?.length ? (
            <div className="mt-3 grid gap-2">
              {booking.services.map((service, i) => (
                <div
                  key={service.id || i}
                  className="flex justify-between rounded-lg bg-slate-50 p-3 text-sm"
                >
                  <span>
                    {service.name} × {service.quantity}
                  </span>
                  <b>{formatVND((service.price || 0) * (service.quantity || 0))}</b>
                </div>
              ))}
            </div>
          ) : (
            <p className="mt-2 text-sm text-slate-500">Không có dịch vụ bổ sung.</p>
          )}
        </div>
        <div className="card">
          <h2 className="card-title">Thanh toán</h2>
          <div className="mt-5 grid gap-4 text-sm">
            <div className="flex justify-between">
              <span className="text-slate-500">Tổng tiền</span>
              <b>{formatVND(booking.totalAmount)}</b>
            </div>
            <div className="flex justify-between">
              <span className="text-slate-500">Đã ghi nhận</span>
              <b className="text-emerald-700">{formatVND(booking.totalPaid)}</b>
            </div>
            <div className="flex justify-between border-t pt-4 text-base">
              <span>Còn lại</span>
              <b className="text-gold-dark">
                {formatVND(
                  booking.balance ?? (booking.totalAmount || 0) - (booking.totalPaid || 0),
                )}
              </b>
            </div>
          </div>
          <p className="mt-5 rounded-xl bg-cream p-3 text-xs leading-5 text-slate-600">
            Bạn chỉ gửi yêu cầu để nhân viên đối soát. Đây không phải thanh toán trực tuyến.
          </p>
        </div>
      </div>
      <Modal open={claimOpen} onClose={() => setClaimOpen(false)} title="Gửi yêu cầu thanh toán">
        <form onSubmit={claim} className="grid gap-4">
          <Field
            name="amount"
            label="Số tiền"
            type="number"
            min={1}
            max={booking.balance}
            required
          />
          <label className="field-label">
            Phương thức
            <select name="method" className="input">
              <option value="BankTransfer">Chuyển khoản</option>
              <option value="Cash">Tiền mặt</option>
              <option value="Card">Thẻ tại quầy</option>
            </select>
          </label>
          <label className="field-label">
            Ghi chú
            <textarea name="note" className="input min-h-24 py-3" />
          </label>
          <Button>
            <Plus size={17} />
            Gửi yêu cầu
          </Button>
        </form>
      </Modal>
      <Modal open={reviewOpen} onClose={() => setReviewOpen(false)} title="Đánh giá phòng">
        <form onSubmit={review} className="grid gap-4">
          <label className="field-label">
            Số sao
            <select name="rating" className="input">
              {[5, 4, 3, 2, 1].map((n) => (
                <option value={n} key={n}>
                  {n} sao
                </option>
              ))}
            </select>
          </label>
          <label className="field-label">
            Nhận xét
            <textarea name="comment" required className="input min-h-28 py-3" />
          </label>
          <Button>Gửi đánh giá</Button>
        </form>
      </Modal>
    </div>
  );
}

export function PasswordPage() {
  return (
    <div>
      <p className="eyebrow">Bảo mật tài khoản</p>
      <h1 className="section-title">Đổi mật khẩu</h1>
      <div className="mt-7 card">
        <ChangePasswordForm />
      </div>
    </div>
  );
}
