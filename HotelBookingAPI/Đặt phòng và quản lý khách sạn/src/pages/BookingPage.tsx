import { CalendarDays, Check, ChevronRight, Minus, Plus, ShieldCheck } from "lucide-react";
import { FormEvent, useEffect, useState } from "react";
import { Link, useNavigate, useParams, useSearchParams } from "react-router";
import { Button, ErrorState, Field, LoadingState, useToast } from "../components/ui";
import { useApiData } from "../hooks/useApiData";
import { api, normalizePaged, queryString } from "../lib/api";
import { formatVND, nightsBetween, toDateInput } from "../lib/format";
import type { ExtraService, PriceQuote, Room } from "../types/api";

export function BookingPage() {
  const { id } = useParams();
  const [search] = useSearchParams();
  const navigate = useNavigate();
  const notify = useToast();
  const [checkIn, setCheckIn] = useState(search.get("checkIn") || "");
  const [checkOut, setCheckOut] = useState(search.get("checkOut") || "");
  const [couponCode, setCouponCode] = useState("");
  const [quantities, setQuantities] = useState<Record<number, number>>({});
  const [quote, setQuote] = useState<PriceQuote | null>(null);
  const [quoteError, setQuoteError] = useState("");
  const [busy, setBusy] = useState(false);
  const room = useApiData(() => api<Room>(`/api/Rooms/${id}`), [id]);
  const services = useApiData(
    () =>
      api<ExtraService[]>("/api/ExtraServices?activeOnly=true").then(
        (v) => normalizePaged<ExtraService>(v).items,
      ),
    [],
  );

  useEffect(() => {
    let cancelled = false;
    if (!checkIn || !checkOut || nightsBetween(checkIn, checkOut) <= 0) {
      setQuote(null);
      setQuoteError("");
      return;
    }
    setQuote(null);
    setQuoteError("");
    api<PriceQuote>(`/api/PricingRules/quote${queryString({ roomId: id, checkIn, checkOut })}`)
      .then((value) => {
        if (!cancelled) setQuote(value);
      })
      .catch((reason) => {
        if (!cancelled) {
          setQuoteError(reason instanceof Error ? reason.message : "Không thể tải báo giá.");
        }
      });
    return () => {
      cancelled = true;
    };
  }, [id, checkIn, checkOut]);
  if (room.loading) return <LoadingState />;
  if (room.error || !room.data)
    return (
      <div className="mx-auto max-w-3xl p-8">
        <ErrorState message={room.error || "Không tìm thấy phòng"} />
      </div>
    );
  const nights = checkIn && checkOut ? nightsBetween(checkIn, checkOut) : 0;
  const serviceTotal =
    services.data?.reduce((sum, item) => sum + item.price * (quantities[item.id] || 0), 0) || 0;
  const roomTotal = quote?.totalAmount ?? quote?.total ?? quote?.subtotal ?? 0;
  const submit = async (event: FormEvent) => {
    event.preventDefault();
    if (nights <= 0 || nights > 90)
      return notify("Vui lòng chọn khoảng ngày hợp lệ, tối đa 90 đêm.", "error");
    setBusy(true);
    try {
      const booking = await api<{ id: number }>("/api/Bookings", {
        method: "POST",
        body: JSON.stringify({
          roomId: Number(id),
          checkInDate: checkIn,
          checkOutDate: checkOut,
          couponCode: couponCode || undefined,
          services: Object.entries(quantities)
            .filter(([, quantity]) => quantity > 0)
            .map(([serviceId, quantity]) => ({ extraServiceId: Number(serviceId), quantity })),
        }),
      });
      notify("Đặt phòng đã được ghi nhận và đang chờ xác nhận.");
      navigate(`/tai-khoan/booking/${booking.id}`);
    } catch (reason) {
      notify(reason instanceof Error ? reason.message : "Không thể tạo booking", "error");
    } finally {
      setBusy(false);
    }
  };
  return (
    <div className="mx-auto max-w-6xl px-4 py-10 lg:px-8">
      <div className="mb-8 flex items-center gap-2 text-sm text-slate-400">
        <Link to={`/phong/${id}?${search}`} className="hover:text-navy">
          Chi tiết phòng
        </Link>
        <ChevronRight size={14} />
        <span className="font-semibold text-navy">Xác nhận đặt phòng</span>
      </div>
      <form onSubmit={submit} className="grid items-start gap-8 lg:grid-cols-[1fr_380px]">
        <div className="grid gap-6">
          <section className="card">
            <div className="card-heading">
              <span>01</span>
              <div>
                <h2>Ngày lưu trú</h2>
                <p>Chọn khoảng thời gian từ 1 đến 90 đêm.</p>
              </div>
            </div>
            <div className="mt-6 grid gap-4 sm:grid-cols-2">
              <Field
                label="Ngày nhận phòng"
                type="date"
                min={toDateInput()}
                required
                value={checkIn}
                onChange={(e) => setCheckIn(e.target.value)}
              />
              <Field
                label="Ngày trả phòng"
                type="date"
                min={checkIn || toDateInput()}
                required
                value={checkOut}
                onChange={(e) => setCheckOut(e.target.value)}
              />
            </div>
          </section>
          <section className="card">
            <div className="card-heading">
              <span>02</span>
              <div>
                <h2>Dịch vụ bổ sung</h2>
                <p>Cá nhân hóa trải nghiệm của bạn.</p>
              </div>
            </div>
            <div className="mt-6 grid gap-3">
              {services.loading ? (
                <LoadingState />
              ) : services.error ? (
                <ErrorState message={services.error} retry={services.reload} />
              ) : services.data?.length ? (
                services.data.map((service) => (
                  <div
                    key={service.id}
                    className="flex items-center justify-between rounded-xl border border-slate-200 p-4"
                  >
                    <div>
                      <p className="font-semibold text-navy">{service.name}</p>
                      <p className="mt-1 text-xs text-slate-500">
                        {service.description || formatVND(service.price)}
                      </p>
                      <p className="mt-2 text-sm font-bold text-gold-dark">
                        {formatVND(service.price)}
                      </p>
                    </div>
                    <div className="flex items-center rounded-xl border border-slate-200">
                      <button
                        type="button"
                        className="p-2.5 hover:bg-slate-50"
                        aria-label={`Giảm ${service.name}`}
                        onClick={() =>
                          setQuantities((q) => ({
                            ...q,
                            [service.id]: Math.max(0, (q[service.id] || 0) - 1),
                          }))
                        }
                      >
                        <Minus size={16} />
                      </button>
                      <span className="min-w-8 text-center text-sm font-bold">
                        {quantities[service.id] || 0}
                      </span>
                      <button
                        type="button"
                        className="p-2.5 hover:bg-slate-50"
                        aria-label={`Tăng ${service.name}`}
                        onClick={() =>
                          setQuantities((q) => ({ ...q, [service.id]: (q[service.id] || 0) + 1 }))
                        }
                      >
                        <Plus size={16} />
                      </button>
                    </div>
                  </div>
                ))
              ) : (
                <p className="text-sm text-slate-500">
                  Hiện chưa có dịch vụ bổ sung đang hoạt động.
                </p>
              )}
            </div>
          </section>
          <section className="card">
            <div className="card-heading">
              <span>03</span>
              <div>
                <h2>Mã ưu đãi</h2>
                <p>Mã sẽ được gửi cùng booking để API kiểm tra.</p>
              </div>
            </div>
            <Field
              label="Mã coupon (không bắt buộc)"
              value={couponCode}
              onChange={(e) => setCouponCode(e.target.value.toUpperCase())}
              placeholder="AURORA..."
              className="mt-6"
            />
          </section>
        </div>
        <aside className="rounded-2xl bg-navy p-6 text-white shadow-xl lg:sticky lg:top-28">
          <p className="text-xs font-bold uppercase tracking-[.2em] text-gold">Tóm tắt kỳ nghỉ</p>
          <h2 className="mt-2 font-display text-2xl">Phòng {room.data.roomNumber}</h2>
          <p className="text-sm text-white/55">{room.data.roomType?.name}</p>
          <div className="my-6 border-y border-white/10 py-5 text-sm">
            <div className="flex justify-between">
              <span className="flex items-center gap-2 text-white/60">
                <CalendarDays size={15} />
                Số đêm
              </span>
              <b>{nights > 0 ? nights : "—"}</b>
            </div>
            {quote?.nights?.map((night) => (
              <div key={night.date} className="mt-3 flex justify-between text-xs">
                <span className="text-white/50">{night.date}</span>
                <span>{formatVND(night.price)}</span>
              </div>
            ))}
          </div>
          <div className="grid gap-3 text-sm">
            <div className="flex justify-between">
              <span className="text-white/60">Tiền phòng</span>
              <span>{roomTotal ? formatVND(roomTotal) : "Theo báo giá"}</span>
            </div>
            {quoteError && (
              <p role="status" className="text-xs leading-5 text-amber-200">
                Báo giá chưa tải được: {quoteError}. Tổng tiền chính thức sẽ do API tính lại khi
                đặt phòng.
              </p>
            )}
            <div className="flex justify-between">
              <span className="text-white/60">Dịch vụ</span>
              <span>{formatVND(serviceTotal)}</span>
            </div>
            <div className="mt-2 flex justify-between border-t border-white/10 pt-5 text-lg">
              <b>Tạm tính</b>
              <b className="text-gold">{roomTotal ? formatVND(roomTotal + serviceTotal) : "—"}</b>
            </div>
          </div>
          <Button
            disabled={busy || nights <= 0}
            className="mt-6 w-full bg-gold text-navy hover:bg-gold-light"
          >
            {busy ? "Đang gửi…" : "Xác nhận đặt phòng"}
            <Check size={17} />
          </Button>
          <p className="mt-4 flex gap-2 text-xs leading-5 text-white/45">
            <ShieldCheck size={15} className="shrink-0" />
            Booking mới có trạng thái Đang chờ. Thanh toán được xử lý qua yêu cầu đối soát, không
            phải thanh toán online.
          </p>
        </aside>
      </form>
    </div>
  );
}
