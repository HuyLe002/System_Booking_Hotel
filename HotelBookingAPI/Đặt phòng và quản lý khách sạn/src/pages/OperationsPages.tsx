import {
  ArrowRight,
  BedDouble,
  BookCheck,
  CalendarCheck,
  CheckCircle2,
  Clock3,
  CreditCard,
  Search,
  Users,
  XCircle,
} from "lucide-react";
import { FormEvent, useState } from "react";
import { Link, useNavigate, useParams, useSearchParams } from "react-router";
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
import { formatDate, formatVND, toDateInput } from "../lib/format";
import type { Booking, PaymentClaim } from "../types/api";

export function OperationsDashboard({ admin = false }: { admin?: boolean }) {
  const today = toDateInput();
  const bookings = useApiData(
    () =>
      api<Booking[]>(`/api/Bookings${queryString({ pageNumber: 1, pageSize: 5 })}`).then((v) =>
        normalizePaged<Booking>(v),
      ),
    [],
  );
  const claims = useApiData(
    () =>
      api<PaymentClaim[]>(
        `/api/PaymentClaims/pending${queryString({ pageNumber: 1, pageSize: 5 })}`,
      ).then((v) =>
        normalizePaged<PaymentClaim>(v),
      ),
    [],
  );
  const checkIns = useApiData(
    () =>
      api<Booking[]>(
        `/api/Bookings${queryString({
          status: "Confirmed",
          from: today,
          to: today,
          pageNumber: 1,
          pageSize: 1,
        })}`,
      ).then((v) => normalizePaged<Booking>(v)),
    [today],
  );
  const inHouse = useApiData(
    () =>
      api<Booking[]>(
        `/api/Bookings${queryString({
          status: "CheckedIn",
          pageNumber: 1,
          pageSize: 1,
        })}`,
      ).then((v) => normalizePaged<Booking>(v)),
    [],
  );
  const prefix = admin ? "/admin" : "/van-hanh";
  const cards = [
    {
      label: "Tổng booking",
      value: bookings.data?.totalCount,
      icon: BookCheck,
      color: "bg-blue-50 text-blue-700",
      loading: bookings.loading,
      error: bookings.error,
    },
    {
      label: "Chờ đối soát",
      value: claims.data?.totalCount,
      icon: CreditCard,
      color: "bg-amber-50 text-amber-700",
      loading: claims.loading,
      error: claims.error,
    },
    {
      label: "Nhận phòng hôm nay",
      icon: CalendarCheck,
      color: "bg-violet-50 text-violet-700",
      loading: checkIns.loading,
      value: checkIns.data?.totalCount,
      error: checkIns.error,
    },
    {
      label: "Đang lưu trú",
      value: inHouse.data?.totalCount,
      icon: BedDouble,
      color: "bg-emerald-50 text-emerald-700",
      loading: inHouse.loading,
      error: inHouse.error,
    },
  ];
  return (
    <div>
      <div className="flex flex-wrap items-end justify-between gap-4">
        <div>
          <p className="eyebrow">{admin ? "Quản trị & phân tích" : "Trung tâm vận hành"}</p>
          <h2 className="section-title">Tổng quan hôm nay</h2>
          <p className="mt-2 text-sm text-slate-500">
            Dữ liệu được cập nhật trực tiếp từ hệ thống.
          </p>
        </div>
        <p className="rounded-full bg-white px-4 py-2 text-xs font-semibold text-slate-500 shadow-sm">
          {new Intl.DateTimeFormat("vi-VN", {
            weekday: "long",
            day: "2-digit",
            month: "long",
          }).format(new Date())}
        </p>
      </div>
      <div className="mt-7 grid gap-4 sm:grid-cols-2 xl:grid-cols-4">
        {cards.map((card) => (
          <div key={card.label} className="rounded-2xl border border-slate-200 bg-white p-5">
            <div className={`grid size-11 place-items-center rounded-xl ${card.color}`}>
              <card.icon size={21} />
            </div>
            <p className="mt-5 text-sm text-slate-500">{card.label}</p>
            <p className="mt-1 text-3xl font-bold text-navy">
              {card.loading
                ? "…"
                : card.error
                  ? "!"
                  : (card.value ?? "—")}
            </p>
            {card.error && <p className="mt-1 text-xs text-red-600">{card.error}</p>}
          </div>
        ))}
      </div>
      <div className="mt-7 grid gap-6 xl:grid-cols-[1.4fr_1fr]">
        <section className="rounded-2xl border border-slate-200 bg-white p-5">
          <div className="flex items-center justify-between">
            <h3 className="card-title">Booking gần đây</h3>
            <Link to={`${prefix}/booking`} className="text-sm font-semibold text-gold-dark">
              Xem tất cả
            </Link>
          </div>
          {bookings.loading ? (
            <LoadingState />
          ) : bookings.error ? (
            <ErrorState message={bookings.error} />
          ) : !bookings.data?.items.length ? (
            <EmptyState title="Chưa có booking" description="Booking mới sẽ xuất hiện tại đây." />
          ) : (
            <div className="mt-4 overflow-x-auto">
              <table>
                <thead>
                  <tr>
                    <th>Mã</th>
                    <th>Khách</th>
                    <th>Thời gian</th>
                    <th>Trạng thái</th>
                  </tr>
                </thead>
                <tbody>
                  {bookings.data.items.map((item) => (
                    <tr key={item.id}>
                      <td>
                        <Link
                          className="font-semibold text-navy hover:text-gold-dark"
                          to={`${prefix}/booking/${item.id}`}
                        >
                          #{item.bookingCode || item.id}
                        </Link>
                      </td>
                      <td>{item.customer?.fullName || "—"}</td>
                      <td>
                        {formatDate(item.checkIn)} – {formatDate(item.checkOut)}
                      </td>
                      <td>
                        <Badge status={item.status} />
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </section>
        <section className="rounded-2xl border border-slate-200 bg-white p-5">
          <div className="flex items-center justify-between">
            <h3 className="card-title">Cần xử lý</h3>
            <Link to={`${prefix}/thanh-toan`} className="text-sm font-semibold text-gold-dark">
              Đến hàng đợi
            </Link>
          </div>
          {claims.loading ? (
            <LoadingState />
          ) : claims.error ? (
            <ErrorState message={claims.error} retry={claims.reload} />
          ) : claims.data?.items.length ? (
            <div className="mt-4 grid gap-3">
              {claims.data.items.map((claim) => (
                <div
                  key={claim.id}
                  className="flex items-center gap-3 rounded-xl bg-amber-50/60 p-3"
                >
                  <span className="grid size-9 place-items-center rounded-full bg-white text-amber-700">
                    <Clock3 size={17} />
                  </span>
                  <div className="min-w-0 flex-1">
                    <p className="truncate text-sm font-semibold text-navy">
                      Booking #{claim.booking?.bookingCode || claim.bookingId}
                    </p>
                    <p className="text-xs text-slate-500">
                      {formatVND(claim.amount)} · {claim.method}
                    </p>
                  </div>
                  <ArrowRight size={17} className="text-slate-400" />
                </div>
              ))}
            </div>
          ) : (
            <EmptyState
              title="Không có yêu cầu chờ"
              description="Hàng đợi thanh toán hiện đã được xử lý."
            />
          )}
        </section>
      </div>
    </div>
  );
}

export function OperationsBookings({ prefix = "/van-hanh" }: { prefix?: string }) {
  const [page, setPage] = useState(1);
  const [filters, setFilters] = useState({
    customerId: "",
    status: "",
    roomId: "",
    from: "",
    to: "",
  });
  const query = queryString({ pageNumber: page, pageSize: 12, ...filters });
  const result = useApiData(
    () => api<Booking[]>(`/api/Bookings${query}`).then((v) => normalizePaged<Booking>(v)),
    [query],
  );
  const submit = (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    const form = new FormData(event.currentTarget);
    const date = String(form.get("date") || "");
    setFilters({
      customerId: String(form.get("customerId") || ""),
      status: String(form.get("status") || ""),
      roomId: String(form.get("roomId") || ""),
      from: date,
      to: date,
    });
    setPage(1);
  };
  return (
    <div>
      <p className="eyebrow">Quản lý lưu trú</p>
      <h2 className="section-title">Danh sách booking</h2>
      <form
        onSubmit={submit}
        className="mt-7 grid gap-3 rounded-2xl border border-slate-200 bg-white p-4 md:grid-cols-[1.5fr_1fr_1fr_1fr_auto]"
      >
        <Field label="Mã khách hàng" name="customerId" type="number" min={1} />
        <label className="field-label">
          Trạng thái
          <select name="status" className="input">
            <option value="">Tất cả</option>
            {["Pending", "Confirmed", "CheckedIn", "CheckedOut", "Cancelled"].map((status) => (
              <option key={status} value={status}>
                {status}
              </option>
            ))}
          </select>
        </label>
        <Field label="Mã phòng" name="roomId" type="number" min={1} />
        <Field label="Ngày nhận phòng" name="date" type="date" />
        <Button className="self-end">
          <Search size={17} />
          Lọc
        </Button>
      </form>
      <section className="mt-5 overflow-hidden rounded-2xl border border-slate-200 bg-white">
        {result.loading ? (
          <LoadingState />
        ) : result.error ? (
          <ErrorState message={result.error} retry={result.reload} />
        ) : !result.data?.items.length ? (
          <EmptyState
            title="Không có booking phù hợp"
            description="Thử thay đổi điều kiện tìm kiếm."
          />
        ) : (
          <>
            <div className="overflow-x-auto">
              <table>
                <thead>
                  <tr>
                    <th>Booking</th>
                    <th>Khách hàng</th>
                    <th>Phòng</th>
                    <th>Lưu trú</th>
                    <th>Thanh toán</th>
                    <th>Trạng thái</th>
                    <th></th>
                  </tr>
                </thead>
                <tbody>
                  {result.data.items.map((booking) => (
                    <tr key={booking.id}>
                      <td className="font-semibold text-navy">
                        #{booking.bookingCode || booking.id}
                      </td>
                      <td>
                        <p className="font-medium text-slate-800">
                          {booking.customer?.fullName || "—"}
                        </p>
                        <p className="text-xs text-slate-400">{booking.customer?.phone}</p>
                      </td>
                      <td>{booking.room?.roomNumber || booking.roomId}</td>
                      <td>
                        {formatDate(booking.checkIn)}
                        <br />
                        <span className="text-xs text-slate-400">
                          đến {formatDate(booking.checkOut)}
                        </span>
                      </td>
                      <td>
                        <Badge status={booking.paymentStatus} />
                      </td>
                      <td>
                        <Badge status={booking.status} />
                      </td>
                      <td>
                        <Link to={`${prefix}/booking/${booking.id}`}>
                          <Button variant="ghost">
                            Chi tiết <ArrowRight size={15} />
                          </Button>
                        </Link>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
            <div className="px-5 pb-5">
              <Pagination
                page={result.data.pageNumber}
                totalPages={result.data.totalPages}
                onChange={setPage}
              />
            </div>
          </>
        )}
      </section>
    </div>
  );
}

export function OperationsBookingDetail() {
  const { id } = useParams();
  const notify = useToast();
  const detail = useApiData(() => api<Booking>(`/api/Bookings/${id}`), [id]);
  const action = async (name: string) => {
    try {
      await api(`/api/Bookings/${id}/${name}`, { method: "PUT" });
      notify("Cập nhật booking thành công.");
      detail.reload();
    } catch (reason) {
      notify(reason instanceof Error ? reason.message : "Thao tác không thành công", "error");
      detail.reload();
    }
  };
  if (detail.loading) return <LoadingState />;
  if (detail.error || !detail.data)
    return <ErrorState message={detail.error || "Không tìm thấy booking"} retry={detail.reload} />;
  const b = detail.data;
  const depositMet = (b.totalPaid || 0) >= (b.totalAmount || 0) * 0.3;
  const paid = (b.balance ?? (b.totalAmount || 0) - (b.totalPaid || 0)) <= 0;
  return (
    <div>
      <Link to="../" className="text-sm font-semibold text-slate-500">
        ← Danh sách booking
      </Link>
      <div className="mt-5 flex flex-wrap items-start justify-between gap-4">
        <div>
          <p className="eyebrow">Booking #{b.bookingCode || b.id}</p>
          <h2 className="section-title">{b.customer?.fullName || "Khách hàng"}</h2>
          <div className="mt-3 flex gap-2">
            <Badge status={b.status} />
            <Badge status={b.paymentStatus} />
          </div>
        </div>
        <div className="flex flex-wrap gap-2">
          {b.status === "Pending" && (
            <Button onClick={() => action("confirm")}>
              <CheckCircle2 size={17} />
              Xác nhận
            </Button>
          )}
          {b.status === "Confirmed" && (
            <Button
              disabled={!depositMet}
              title={!depositMet ? "Cần đạt tối thiểu 30% tiền cọc" : ""}
              onClick={() => action("check-in")}
            >
              <BookCheck size={17} />
              Check-in
            </Button>
          )}
          {b.status === "CheckedIn" && (
            <Button
              disabled={!paid}
              title={!paid ? "Booking cần thanh toán đủ" : ""}
              onClick={() => action("check-out")}
            >
              <BookCheck size={17} />
              Check-out
            </Button>
          )}
          {!["Cancelled", "CheckedOut"].includes(b.status) && (
            <Button variant="danger" onClick={() => action("cancel")}>
              <XCircle size={17} />
              Hủy
            </Button>
          )}
        </div>
      </div>
      {!depositMet && b.status === "Confirmed" && (
        <p className="mt-5 rounded-xl border border-amber-200 bg-amber-50 p-3 text-sm text-amber-800">
          Chưa đủ điều kiện check-in: cần ghi nhận tối thiểu 30% tổng tiền và đúng ngày nhận phòng.
          API sẽ xác thực lần cuối.
        </p>
      )}
      <div className="mt-7 grid gap-5 lg:grid-cols-3">
        <section className="card lg:col-span-2">
          <h3 className="card-title">Thông tin lưu trú</h3>
          <dl className="mt-5 grid gap-5 sm:grid-cols-2">
            <div>
              <dt>Khách hàng</dt>
              <dd>{b.customer?.fullName || "—"}</dd>
            </div>
            <div>
              <dt>Liên hệ</dt>
              <dd>{b.customer?.phone || b.customer?.email || "—"}</dd>
            </div>
            <div>
              <dt>Phòng</dt>
              <dd>
                {b.room?.roomNumber || b.roomId} · {b.room?.roomType?.name}
              </dd>
            </div>
            <div>
              <dt>Thời gian</dt>
              <dd>
                {formatDate(b.checkIn)} – {formatDate(b.checkOut)}
              </dd>
            </div>
          </dl>
          <h4 className="mt-7 font-semibold text-navy">Dịch vụ</h4>
          {b.services?.length ? (
            b.services.map((s, index) => (
              <div
                key={s.id || index}
                className="mt-2 flex justify-between rounded-lg bg-slate-50 p-3 text-sm"
              >
                <span>
                  {s.name} × {s.quantity}
                </span>
                <b>{formatVND((s.price || 0) * (s.quantity || 0))}</b>
              </div>
            ))
          ) : (
            <p className="mt-2 text-sm text-slate-500">Không có dịch vụ bổ sung.</p>
          )}
        </section>
        <section className="card">
          <h3 className="card-title">Tài chính</h3>
          <div className="mt-5 grid gap-4 text-sm">
            <div className="flex justify-between">
              <span>Tổng tiền</span>
              <b>{formatVND(b.totalAmount)}</b>
            </div>
            <div className="flex justify-between">
              <span>Đã trả</span>
              <b className="text-emerald-700">{formatVND(b.totalPaid)}</b>
            </div>
            <div className="flex justify-between border-t pt-4 text-base">
              <span>Còn lại</span>
              <b className="text-gold-dark">{formatVND(b.balance)}</b>
            </div>
            <div className="rounded-xl bg-slate-50 p-3">
              <div className="mb-2 flex justify-between text-xs">
                <span>Tiền cọc</span>
                <span>
                  {Math.min(100, Math.round(((b.totalPaid || 0) / (b.totalAmount || 1)) * 100))}%
                </span>
              </div>
              <div className="h-2 overflow-hidden rounded-full bg-slate-200">
                <div
                  className="h-full bg-gold"
                  style={{
                    width: `${Math.min(100, ((b.totalPaid || 0) / (b.totalAmount || 1)) * 100)}%`,
                  }}
                />
              </div>
            </div>
          </div>
        </section>
      </div>
    </div>
  );
}

export function PaymentClaimsPage() {
  const notify = useToast();
  const [page, setPage] = useState(1);
  const [reject, setReject] = useState<PaymentClaim | null>(null);
  const result = useApiData(
    () =>
      api<PaymentClaim[]>(
        `/api/PaymentClaims/pending${queryString({ pageNumber: page, pageSize: 10 })}`,
      ).then((v) =>
        normalizePaged<PaymentClaim>(v),
      ),
    [page],
  );
  const confirm = async (claim: PaymentClaim) => {
    if (!window.confirm("Chỉ xác nhận sau khi đã kiểm tra tiền thực nhận. Tiếp tục?")) return;
    try {
      await api(`/api/Bookings/${claim.bookingId}/payment-claims/${claim.id}/confirm`, {
        method: "POST",
      });
      notify("Đã xác nhận khoản thanh toán.");
      result.reload();
    } catch (reason) {
      notify(reason instanceof Error ? reason.message : "Không thể xác nhận", "error");
    }
  };
  const submitReject = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    if (!reject) return;
    const reason = new FormData(event.currentTarget).get("reason");
    try {
      await api(`/api/Bookings/${reject.bookingId}/payment-claims/${reject.id}/reject`, {
        method: "POST",
        body: JSON.stringify({ note: reason }),
      });
      notify("Đã từ chối yêu cầu.");
      setReject(null);
      result.reload();
    } catch (error) {
      notify(error instanceof Error ? error.message : "Không thể từ chối", "error");
    }
  };
  return (
    <div>
      <p className="eyebrow">Đối soát thủ công</p>
      <h2 className="section-title">Yêu cầu thanh toán đang chờ</h2>
      <p className="mt-2 max-w-2xl text-sm text-slate-500">
        Kiểm tra tiền thực nhận trước khi xác nhận. Hệ thống không xử lý thanh toán online.
      </p>
      <div className="mt-7 grid gap-4">
        {result.loading ? (
          <LoadingState />
        ) : result.error ? (
          <ErrorState message={result.error} retry={result.reload} />
        ) : !result.data?.items.length ? (
          <EmptyState
            title="Hàng đợi đã trống"
            description="Không còn yêu cầu thanh toán nào cần đối soát."
          />
        ) : (
          result.data.items.map((claim) => (
            <article key={claim.id} className="rounded-2xl border border-slate-200 bg-white p-5">
              <div className="grid gap-5 lg:grid-cols-[1fr_1fr_auto]">
                <div>
                  <p className="text-xs font-semibold text-gold-dark">YÊU CẦU #{claim.id}</p>
                  <h3 className="mt-1 font-display text-xl font-semibold text-navy">
                    {claim.booking?.customer?.fullName ||
                      `Booking #${claim.booking?.bookingCode || claim.bookingId}`}
                  </h3>
                  <p className="mt-2 text-sm text-slate-500">
                    Phòng {claim.booking?.room?.roomNumber || "—"} ·{" "}
                    {formatDate(claim.booking?.checkIn)} – {formatDate(claim.booking?.checkOut)}
                  </p>
                </div>
                <div>
                  <p className="text-xs text-slate-400">Khách báo đã thanh toán</p>
                  <p className="mt-1 text-xl font-bold text-navy">{formatVND(claim.amount)}</p>
                  <p className="mt-1 text-xs text-slate-500">
                    {claim.method}
                  </p>
                  {claim.note && <p className="mt-2 text-sm text-slate-600">“{claim.note}”</p>}
                </div>
                <div className="flex items-center gap-2 lg:flex-col lg:items-stretch">
                  <Button onClick={() => confirm(claim)}>
                    <CheckCircle2 size={17} />
                    Xác nhận
                  </Button>
                  <Button variant="secondary" onClick={() => setReject(claim)}>
                    <XCircle size={17} />
                    Từ chối
                  </Button>
                </div>
              </div>
            </article>
          ))
        )}
      </div>
      {result.data && result.data.totalPages > 1 && (
        <div className="mt-5">
          <Pagination
            page={result.data.pageNumber}
            totalPages={result.data.totalPages}
            onChange={setPage}
          />
        </div>
      )}
      <Modal open={!!reject} title="Từ chối yêu cầu thanh toán" onClose={() => setReject(null)}>
        <form onSubmit={submitReject} className="grid gap-4">
          <p className="text-sm text-slate-600">
            Vui lòng nêu rõ lý do để khách hàng có thể kiểm tra và gửi lại.
          </p>
          <label className="field-label">
            Lý do từ chối
            <textarea name="reason" required minLength={5} className="input min-h-28 py-3" />
          </label>
          <Button variant="danger">Xác nhận từ chối</Button>
        </form>
      </Modal>
    </div>
  );
}

export function CustomersPage() {
  const [search, setSearch] = useState("");
  const [page, setPage] = useState(1);
  const [createOpen, setCreateOpen] = useState(false);
  const notify = useToast();
  const result = useApiData(
    () =>
      api<unknown[]>(
        `/api/Customers${queryString({ keyword: search, pageNumber: page, pageSize: 20 })}`,
      ).then((v) => normalizePaged<Record<string, unknown>>(v)),
    [search, page],
  );
  const createCustomer = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    const form = new FormData(event.currentTarget);
    try {
      await api("/api/Customers", {
        method: "POST",
        body: JSON.stringify({
          fullName: form.get("fullName"),
          email: form.get("email"),
          phone: form.get("phone"),
        }),
      });
      notify("Đã tạo hồ sơ khách hàng.");
      setCreateOpen(false);
      result.reload();
    } catch (reason) {
      notify(reason instanceof Error ? reason.message : "Không thể tạo khách hàng", "error");
    }
  };
  return (
    <div>
      <p className="eyebrow">Hồ sơ khách hàng</p>
      <div className="flex flex-wrap items-end justify-between gap-4">
        <h2 className="section-title">Khách hàng</h2>
        <Button onClick={() => setCreateOpen(true)}>
          <Users size={17} />
          Tạo khách hàng
        </Button>
      </div>
      <form
        className="mt-7 flex max-w-xl gap-2"
        onSubmit={(e) => {
          e.preventDefault();
          setSearch(String(new FormData(e.currentTarget).get("search") || ""));
          setPage(1);
        }}
      >
        <Field label="Tìm theo tên, email hoặc số điện thoại" name="search" className="flex-1" />
        <Button className="self-end">
          <Search size={17} />
          Tìm
        </Button>
      </form>
      <div className="mt-5 overflow-hidden rounded-2xl border border-slate-200 bg-white">
        {result.loading ? (
          <LoadingState />
        ) : result.error ? (
          <ErrorState message={result.error} retry={result.reload} />
        ) : !result.data?.items.length ? (
          <EmptyState
            title="Không tìm thấy khách hàng"
            description="Hồ sơ khách hàng phù hợp sẽ hiển thị tại đây."
          />
        ) : (
          <div className="overflow-x-auto">
            <table>
              <thead>
                <tr>
                  <th>Khách hàng</th>
                  <th>Email</th>
                  <th>Điện thoại</th>
                  <th>Trạng thái</th>
                  <th></th>
                </tr>
              </thead>
              <tbody>
                {result.data.items.map((customer, i) => (
                  <tr key={String(customer.id || i)}>
                    <td className="font-semibold text-navy">
                      {String(customer.fullName || customer.name || "—")}
                    </td>
                    <td>{String(customer.email || "—")}</td>
                    <td>{String(customer.phone || "—")}</td>
                    <td>
                      <Badge status={customer.isDeleted ? "Inactive" : "Active"} />
                    </td>
                    <td>
                      {(typeof customer.id === "number" || typeof customer.id === "string") && (
                        <Link to={`/van-hanh/booking-moi?customerId=${String(customer.id)}`}>
                          <Button variant="ghost">
                            Tạo booking <ArrowRight size={15} />
                          </Button>
                        </Link>
                      )}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>
      {result.data && result.data.totalPages > 1 && (
        <div className="mt-5">
          <Pagination
            page={result.data.pageNumber}
            totalPages={result.data.totalPages}
            onChange={setPage}
          />
        </div>
      )}
      <Modal open={createOpen} title="Tạo hồ sơ khách hàng" onClose={() => setCreateOpen(false)}>
        <form onSubmit={createCustomer} className="grid gap-4">
          <Field name="fullName" label="Họ và tên" required />
          <Field name="email" label="Email" type="email" required />
          <Field name="phone" label="Số điện thoại" type="tel" required />
          <Button>Tạo khách hàng</Button>
        </form>
      </Modal>
    </div>
  );
}

export function StaffCreateBookingPage() {
  const [search] = useSearchParams();
  const navigate = useNavigate();
  const notify = useToast();
  const [busy, setBusy] = useState(false);
  const submit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    const form = new FormData(event.currentTarget);
    setBusy(true);
    try {
      const booking = await api<{ id: number }>("/api/Bookings", {
        method: "POST",
        body: JSON.stringify({
          customerId: Number(form.get("customerId")),
          roomId: Number(form.get("roomId")),
          checkInDate: form.get("checkIn"),
          checkOutDate: form.get("checkOut"),
          couponCode: form.get("couponCode") || undefined,
        }),
      });
      notify("Đã tạo booking hộ khách hàng.");
      navigate(`/van-hanh/booking/${booking.id}`);
    } catch (reason) {
      notify(reason instanceof Error ? reason.message : "Không thể tạo booking", "error");
    } finally {
      setBusy(false);
    }
  };
  return (
    <div>
      <p className="eyebrow">Hỗ trợ tại quầy</p>
      <h2 className="section-title">Tạo booking hộ khách</h2>
      <p className="mt-2 text-sm text-slate-500">
        Tra cứu phòng trống trước khi nhập mã phòng. API sẽ kiểm tra lại toàn bộ điều kiện và mức
        giá khi tạo booking.
      </p>
      <div className="mt-7 grid gap-6 lg:grid-cols-[1fr_320px]">
        <form onSubmit={submit} className="card grid gap-4 sm:grid-cols-2">
          <Field
            name="customerId"
            label="Mã khách hàng"
            type="number"
            required
            defaultValue={search.get("customerId") || ""}
          />
          <Field name="roomId" label="Mã phòng" type="number" required />
          <Field name="checkIn" label="Ngày nhận phòng" type="date" required />
          <Field name="checkOut" label="Ngày trả phòng" type="date" required />
          <Field name="couponCode" label="Mã coupon (nếu có)" className="sm:col-span-2" />
          <Button disabled={busy} className="w-fit sm:col-span-2">
            {busy ? "Đang tạo…" : "Tạo booking"}
          </Button>
        </form>
        <aside className="card h-fit">
          <h3 className="card-title">Kiểm tra phòng trống</h3>
          <p className="mt-2 text-sm leading-6 text-slate-500">
            Mở công cụ tìm phòng công khai trong tab mới, sau đó dùng mã phòng phù hợp để hoàn tất
            booking.
          </p>
          <a href="/phong" target="_blank" rel="noreferrer">
            <Button variant="secondary" className="mt-5 w-full">
              <Search size={17} />
              Tìm phòng trống
            </Button>
          </a>
        </aside>
      </div>
    </div>
  );
}
