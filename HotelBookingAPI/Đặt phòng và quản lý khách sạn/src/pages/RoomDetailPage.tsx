import { ArrowLeft, Check, Star, Users } from "lucide-react";
import { Link, useParams, useSearchParams } from "react-router";
import { Button, EmptyState, ErrorState, LoadingState, Pagination } from "../components/ui";
import { useApiData } from "../hooks/useApiData";
import { api, queryString } from "../lib/api";
import { formatDate, formatVND } from "../lib/format";
import type { ReviewPage, Room } from "../types/api";

const FALLBACK =
  "https://images.unsplash.com/photo-1718942901281-1d2984221696?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&q=85&w=1600";

export function RoomDetailPage() {
  const { id } = useParams();
  const [search, setSearch] = useSearchParams();
  const page = Number(search.get("reviewPage") || 1);
  const {
    data: room,
    loading,
    error,
    reload,
  } = useApiData(() => api<Room>(`/api/Rooms/${id}`), [id]);
  const reviews = useApiData(
    () =>
      api<ReviewPage>(
        `/api/Reviews/room/${id}${queryString({ pageNumber: page, pageSize: 6 })}`,
      ),
    [id, page],
  );
  if (loading) return <LoadingState />;
  if (error || !room)
    return (
      <div className="mx-auto max-w-4xl p-8">
        <ErrorState message={error || "Không tìm thấy phòng"} retry={reload} />
      </div>
    );
  const rating = reviews.data?.averageRating || 0;
  return (
    <div className="mx-auto max-w-7xl px-4 py-8 lg:px-8">
      <Link
        to={`/phong?${search}`}
        className="mb-6 flex items-center gap-2 text-sm font-semibold text-slate-500 hover:text-navy"
      >
        <ArrowLeft size={17} />
        Quay lại danh sách phòng
      </Link>
      <div className="grid gap-4 lg:grid-cols-[2fr_1fr]">
        <img
          src={room.imageUrl || room.images?.[0] || FALLBACK}
          alt={`Phòng ${room.roomNumber}`}
          className="h-[480px] size-full rounded-3xl object-cover"
        />
        <div className="grid grid-cols-2 gap-4 lg:grid-cols-1">
          {(room.images?.slice(1, 3) || []).map((image) => (
            <img
              key={image}
              src={image}
              alt="Không gian phòng"
              className="h-full min-h-40 size-full rounded-2xl object-cover"
            />
          ))}
          <div className="grid min-h-40 place-items-center rounded-2xl bg-navy p-6 text-center text-white">
            <div>
              <Star className="mx-auto fill-gold text-gold" />
              <p className="mt-3 font-display text-3xl">{rating ? rating.toFixed(1) : "Mới"}</p>
              <p className="text-xs text-white/60">{reviews.data?.totalReviews || 0} đánh giá</p>
            </div>
          </div>
        </div>
      </div>
      <div className="mt-10 grid gap-12 lg:grid-cols-[1fr_350px]">
        <div>
          <p className="eyebrow">{room.roomType?.name || "Phòng nghỉ"}</p>
          <h1 className="section-title">Phòng {room.roomNumber}</h1>
          <div className="mt-4 flex gap-5 text-sm text-slate-500">
            <span className="flex items-center gap-2">
              <Users size={17} />
              Tối đa {room.capacity || room.roomType?.capacity || "—"} khách
            </span>
            <span>Tầng {room.floor || "—"}</span>
          </div>
          <p className="mt-8 leading-8 text-slate-600">
            {room.description ||
              room.roomType?.description ||
              "Một không gian riêng tư với đầy đủ tiện nghi cho kỳ nghỉ thư thái."}
          </p>
          <h2 className="mt-10 font-display text-2xl font-semibold text-navy">
            Tiện nghi trong phòng
          </h2>
          <div className="mt-5 grid gap-3 sm:grid-cols-2">
            {room.amenities?.map((item) => (
              <div
                key={item.id}
                className="flex items-center gap-3 rounded-xl bg-white p-3 text-sm text-slate-600"
              >
                <span className="grid size-8 place-items-center rounded-full bg-cream text-gold-dark">
                  <Check size={15} />
                </span>
                {item.name}
              </div>
            )) || <p className="text-sm text-slate-500">Thông tin tiện nghi đang được cập nhật.</p>}
          </div>
        </div>
        <aside className="h-fit rounded-2xl border border-slate-200 bg-white p-6 shadow-lg lg:sticky lg:top-28">
          <p className="text-sm text-slate-500">Giá phòng từ</p>
          <p className="mt-1 text-2xl font-bold text-navy">
            {formatVND(room.price || room.basePrice || room.roomType?.basePrice)}{" "}
            <span className="text-sm font-normal text-slate-400">/ đêm</span>
          </p>
          <p className="mt-4 rounded-xl bg-cream p-3 text-xs leading-5 text-slate-600">
            Giá cuối cùng và chi tiết từng đêm sẽ được lấy từ báo giá API theo ngày bạn chọn.
          </p>
          <Link to={`/dat-phong/${room.id}?${search}`}>
            <Button className="mt-5 w-full">Chọn phòng này</Button>
          </Link>
        </aside>
      </div>
      <section className="mt-20 border-t border-slate-200 pt-12">
        <h2 className="font-display text-3xl font-semibold text-navy">
          Đánh giá của khách lưu trú
        </h2>
        {reviews.loading ? (
          <LoadingState />
        ) : reviews.error ? (
          <ErrorState message={reviews.error} retry={reviews.reload} />
        ) : !reviews.data?.items.length ? (
          <div className="mt-6">
            <EmptyState
              title="Chưa có đánh giá"
              description="Hãy là vị khách đầu tiên chia sẻ trải nghiệm về căn phòng này."
            />
          </div>
        ) : (
          <div className="mt-7 grid gap-4 md:grid-cols-2">
            {reviews.data.items.map((review) => (
              <article key={review.id} className="rounded-2xl border border-slate-200 bg-white p-5">
                <div className="flex items-center justify-between">
                  <p className="font-semibold text-navy">
                    {review.customerName || "Khách lưu trú"}
                  </p>
                  <span className="flex items-center gap-1 text-sm font-bold text-gold-dark">
                    <Star size={15} className="fill-gold" />
                    {review.rating}/5
                  </span>
                </div>
                <p className="mt-3 text-sm leading-6 text-slate-600">
                  {review.comment || "Đã đánh giá phòng."}
                </p>
                <p className="mt-3 text-xs text-slate-400">{formatDate(review.createdAt)}</p>
              </article>
            ))}
          </div>
        )}
        {reviews.data && reviews.data.totalPages > 1 && (
          <div className="mt-6">
            <Pagination
              page={reviews.data.pageNumber}
              totalPages={reviews.data.totalPages}
              onChange={(nextPage) =>
                setSearch((current) => {
                  const next = new URLSearchParams(current);
                  next.set("reviewPage", String(nextPage));
                  return next;
                })
              }
            />
          </div>
        )}
      </section>
    </div>
  );
}
