import { BedDouble, Building2, Check, Search, UsersRound } from "lucide-react";
import { FormEvent, useEffect, useState } from "react";
import { Link, useSearchParams } from "react-router";
import { Button, EmptyState, ErrorState, LoadingState, Pagination } from "../components/ui";
import { useApiData } from "../hooks/useApiData";
import { api, normalizePaged, queryString } from "../lib/api";
import { formatVND, nightsBetween, toDateInput } from "../lib/format";
import type { PriceQuote, Room, RoomType } from "../types/api";

const FALLBACK =
  "https://images.unsplash.com/photo-1718942901281-1d2984221696?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&q=80&w=900";

export function RoomsPage() {
  const [params, setParams] = useSearchParams();
  const [validation, setValidation] = useState("");
  const checkIn = params.get("checkIn") || "";
  const checkOut = params.get("checkOut") || "";
  const roomTypeId = params.get("roomTypeId") || "";
  const page = Number(params.get("page") || 1);
  const nights = checkIn && checkOut ? nightsBetween(checkIn, checkOut) : 0;
  const { data: types } = useApiData(
    () => api<RoomType[]>("/api/RoomTypes").then((v) => normalizePaged<RoomType>(v).items),
    [],
  );
  const endpoint =
    checkIn && checkOut
      ? `/api/Bookings/available-rooms${queryString({ checkIn, checkOut, roomTypeId })}`
      : `/api/Rooms${queryString({ roomTypeId, pageNumber: page, pageSize: 12 })}`;
  const {
    data: roomsResult,
    loading,
    error,
    reload,
  } = useApiData(
    () => api<unknown>(endpoint).then((v) => normalizePaged<Room>(v)),
    [endpoint],
  );
  const data = roomsResult?.items;
  const [quotes, setQuotes] = useState<Record<number, PriceQuote>>({});
  const [quoteError, setQuoteError] = useState("");

  useEffect(() => {
    let cancelled = false;
    setQuotes({});
    setQuoteError("");
    if (!data || !checkIn || !checkOut || nights < 1) return;
    Promise.all(
      data.map(async (room) => {
        try {
          const quote = await api<PriceQuote>(
            `/api/PricingRules/quote${queryString({ roomId: room.id, checkIn, checkOut })}`,
          );
          return { entry: [room.id, quote] as const, error: undefined };
        } catch (reason) {
          return {
            entry: undefined,
            error: reason instanceof Error ? reason.message : "Không thể lấy báo giá phòng.",
          };
        }
      }),
    ).then((results) => {
      if (cancelled) return;
      setQuotes(
        Object.fromEntries(
          results.flatMap((result) => (result.entry ? [result.entry] : [])),
        ),
      );
      setQuoteError(results.find((result) => result.error)?.error || "");
    });
    return () => {
      cancelled = true;
    };
  }, [data, checkIn, checkOut, nights]);

  const submit = (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    const form = new FormData(event.currentTarget);
    const nextIn = String(form.get("checkIn"));
    const nextOut = String(form.get("checkOut"));
    const count = nightsBetween(nextIn, nextOut);
    if (count <= 0) return setValidation("Ngày trả phòng phải sau ngày nhận phòng.");
    if (count > 90) return setValidation("Mỗi lần đặt phòng không quá 90 đêm.");
    setValidation("");
    setParams({
      checkIn: nextIn,
      checkOut: nextOut,
      roomTypeId: String(form.get("roomTypeId") || ""),
    });
  };

  return (
    <div>
      <section className="bg-navy py-16 text-white">
        <div className="mx-auto max-w-7xl px-4 lg:px-8">
          <p className="eyebrow text-gold">Không gian của bạn</p>
          <h1 className="font-display text-4xl sm:text-5xl">Chọn căn phòng hoàn hảo</h1>
          <p className="mt-4 text-white/60">
            Giá và tình trạng phòng được cập nhật trực tiếp theo ngày lưu trú.
          </p>
        </div>
      </section>
      <div className="mx-auto max-w-7xl px-4 py-10 lg:px-8">
        <form
          onSubmit={submit}
          className="grid gap-4 rounded-2xl border border-slate-200 bg-white p-5 shadow-sm md:grid-cols-[1fr_1fr_1fr_auto]"
        >
          <label className="field-label">
            Nhận phòng
            <input
              name="checkIn"
              required
              type="date"
              min={toDateInput()}
              defaultValue={checkIn}
              className="input"
            />
          </label>
          <label className="field-label">
            Trả phòng
            <input
              name="checkOut"
              required
              type="date"
              min={checkIn || toDateInput()}
              defaultValue={checkOut}
              className="input"
            />
          </label>
          <label className="field-label">
            Loại phòng
            <select name="roomTypeId" defaultValue={roomTypeId} className="input">
              <option value="">Tất cả</option>
              {types?.map((type) => (
                <option key={type.id} value={type.id}>
                  {type.name}
                </option>
              ))}
            </select>
          </label>
          <Button className="self-end">
            <Search size={17} />
            Kiểm tra phòng
          </Button>
          {validation && <p className="text-sm text-red-600 md:col-span-4">{validation}</p>}
        </form>
        <div className="mb-6 mt-10 flex flex-wrap items-end justify-between gap-4">
          <div>
            <p className="text-sm font-semibold text-gold-dark">
              {roomsResult?.totalCount || 0} phòng phù hợp
            </p>
            <h2 className="font-display text-3xl font-semibold text-navy">
              {nights > 0 ? `${nights} đêm lưu trú` : "Tất cả phòng nghỉ"}
            </h2>
          </div>
          {checkIn && (
            <p className="text-sm text-slate-500">
              {checkIn} → {checkOut}
            </p>
          )}
          {quoteError && (
            <p role="status" className="mb-5 rounded-xl bg-amber-50 p-3 text-sm text-amber-800">
              Một số báo giá chưa tải được: {quoteError} Giá tạm thời hiển thị theo giá cơ bản.
            </p>
          )}
        </div>
        {loading ? (
          <LoadingState label="Đang kiểm tra phòng trống và mức giá…" />
        ) : error ? (
          <ErrorState message={error} retry={reload} />
        ) : !data?.length ? (
          <EmptyState
            title="Chưa tìm thấy phòng phù hợp"
            description="Hãy thử thay đổi ngày lưu trú hoặc chọn một loại phòng khác."
          />
        ) : (
          <div className="grid gap-6">
            {data.map((room) => {
              const quote = quotes[room.id];
              const total = quote?.totalAmount ?? quote?.total;
              return (
                <article
                  key={room.id}
                  className="grid overflow-hidden rounded-2xl border border-slate-200 bg-white shadow-sm transition hover:shadow-lg md:grid-cols-[300px_1fr_auto]"
                >
                  <img
                    src={room.imageUrl || room.images?.[0] || FALLBACK}
                    alt={`Phòng ${room.roomNumber}`}
                    className="h-60 size-full object-cover md:h-full"
                  />
                  <div className="p-6">
                    <p className="text-xs font-bold uppercase tracking-wider text-gold-dark">
                      {room.roomType?.name || "Phòng nghỉ"}
                    </p>
                    <h3 className="mt-1 font-display text-2xl font-semibold text-navy">
                      Phòng {room.roomNumber}
                    </h3>
                    <div className="mt-4 flex flex-wrap gap-4 text-sm text-slate-500">
                      <span className="flex items-center gap-1.5">
                        <UsersRound size={16} />
                        {room.capacity || room.roomType?.capacity || "—"} khách
                      </span>
                      <span className="flex items-center gap-1.5">
                        <Building2 size={16} />
                        Tầng {room.floor || "—"}
                      </span>
                    </div>
                    <p className="mt-4 line-clamp-2 text-sm leading-6 text-slate-500">
                      {room.description ||
                        room.roomType?.description ||
                        "Không gian nghỉ dưỡng tiện nghi, riêng tư và thanh lịch."}
                    </p>
                    <div className="mt-4 flex flex-wrap gap-2">
                      {room.amenities?.slice(0, 4).map((item) => (
                        <span
                          key={item.id}
                          className="flex items-center gap-1 rounded-full bg-cream px-2.5 py-1 text-xs text-slate-600"
                        >
                          <Check size={12} />
                          {item.name}
                        </span>
                      ))}
                    </div>
                  </div>
                  <div className="flex min-w-52 flex-col justify-between border-t border-slate-100 bg-slate-50/60 p-6 md:border-l md:border-t-0">
                    <div className="text-right">
                      <p className="text-xs text-slate-400">
                        {total ? `Tổng ${nights} đêm` : "Từ"}
                      </p>
                      <p className="mt-1 text-xl font-bold text-navy">
                        {formatVND(
                          total || room.price || room.basePrice || room.roomType?.basePrice,
                        )}
                      </p>
                      {quote?.nights?.length ? (
                        <p className="mt-1 text-xs text-slate-400">Giá từng đêm theo báo giá API</p>
                      ) : (
                        <p className="mt-1 text-xs text-slate-400">/ đêm</p>
                      )}
                    </div>
                    <Link to={`/phong/${room.id}${params.toString() ? `?${params}` : ""}`}>
                      <Button className="mt-5 w-full">
                        <BedDouble size={17} />
                        Xem phòng
                      </Button>
                    </Link>
                  </div>
                </article>
              );
            })}
          </div>
        )}
        {roomsResult && roomsResult.totalPages > 1 && (
          <div className="mt-8">
            <Pagination
              page={roomsResult.pageNumber}
              totalPages={roomsResult.totalPages}
              onChange={(nextPage) =>
                setParams((current) => {
                  const next = new URLSearchParams(current);
                  next.set("page", String(nextPage));
                  return next;
                })
              }
            />
          </div>
        )}
      </div>
    </div>
  );
}
