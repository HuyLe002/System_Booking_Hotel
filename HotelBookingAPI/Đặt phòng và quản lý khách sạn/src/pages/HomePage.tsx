import {
  ArrowRight,
  BedDouble,
  Coffee,
  ConciergeBell,
  MapPin,
  ShieldCheck,
  Sparkles,
  Star,
  Waves,
  Wifi,
} from "lucide-react";
import { FormEvent } from "react";
import { Link, useNavigate } from "react-router";
import { Button } from "../components/ui";
import { useApiData } from "../hooks/useApiData";
import { api, normalizePaged } from "../lib/api";
import { formatVND, toDateInput } from "../lib/format";
import type { Room, RoomType } from "../types/api";

const HERO =
  "https://images.unsplash.com/photo-1787283868772-86d47466cfb6?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&q=85&w=2000";
const ROOM_FALLBACKS = [
  "https://images.unsplash.com/photo-1718942901281-1d2984221696?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&q=80&w=900",
  "https://images.unsplash.com/photo-1551105379-6c50b7647c03?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&q=80&w=900",
  "https://images.unsplash.com/photo-1772006807170-5750a2aa3713?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&q=80&w=900",
];

export function HomePage() {
  const navigate = useNavigate();
  const tomorrow = new Date();
  tomorrow.setDate(tomorrow.getDate() + 1);
  const afterTomorrow = new Date();
  afterTomorrow.setDate(afterTomorrow.getDate() + 2);
  const { data: types } = useApiData(
    () => api<RoomType[]>("/api/RoomTypes").then((v) => normalizePaged<RoomType>(v).items),
    [],
  );
  const { data: rooms } = useApiData(
    () => api<Room[]>("/api/Rooms").then((v) => normalizePaged<Room>(v).items.slice(0, 3)),
    [],
  );

  const search = (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    const values = new FormData(event.currentTarget);
    navigate(
      `/phong?checkIn=${values.get("checkIn")}&checkOut=${values.get("checkOut")}&roomTypeId=${values.get("roomTypeId") || ""}`,
    );
  };

  return (
    <>
      <section className="relative min-h-[710px] overflow-hidden bg-navy">
        <img
          src={HERO}
          alt="Toàn cảnh khu nghỉ dưỡng Aurora Bay"
          className="absolute inset-0 size-full object-cover opacity-70"
        />
        <div className="absolute inset-0 bg-gradient-to-r from-navy/90 via-navy/45 to-transparent" />
        <div className="relative mx-auto flex min-h-[710px] max-w-7xl items-center px-4 pb-28 pt-16 lg:px-8">
          <div className="max-w-3xl text-white">
            <p className="mb-5 flex items-center gap-3 text-xs font-semibold uppercase tracking-[0.3em] text-gold">
              <span className="h-px w-10 bg-gold" />
              Chạm đến kỳ nghỉ trong mơ
            </p>
            <h1 className="font-display text-5xl font-medium leading-[1.08] sm:text-6xl lg:text-7xl">
              Mỗi khoảnh khắc,
              <br />
              <i className="font-normal text-gold">một dấu ấn riêng.</i>
            </h1>
            <p className="mt-6 max-w-xl text-base leading-7 text-white/80 sm:text-lg">
              Tìm về bình yên giữa thiên nhiên, tận hưởng dịch vụ tinh tế và lòng hiếu khách chân
              thành tại Aurora Bay.
            </p>
            <div className="mt-8 flex flex-wrap gap-3">
              <a href="#dat-phong">
                <Button className="px-6">
                  Khám phá phòng nghỉ <ArrowRight size={17} />
                </Button>
              </a>
              <Link to="/gioi-thieu">
                <Button
                  variant="secondary"
                  className="border-white/40 bg-white/10 text-white hover:bg-white/20"
                >
                  Trải nghiệm Aurora
                </Button>
              </Link>
            </div>
          </div>
        </div>
        <div className="absolute bottom-0 right-0 hidden items-center gap-8 bg-white/95 px-9 py-5 text-sm text-navy backdrop-blur lg:flex">
          <span className="flex items-center gap-2">
            <MapPin size={17} className="text-gold-dark" />
            Bên bờ biển Đà Nẵng
          </span>
          <span className="h-7 w-px bg-slate-200" />
          <span className="flex items-center gap-2">
            <Star size={17} className="fill-gold text-gold" />
            Trải nghiệm 5 sao
          </span>
        </div>
      </section>

      <section id="dat-phong" className="relative z-10 mx-auto -mt-20 max-w-6xl px-4">
        <form
          onSubmit={search}
          className="grid gap-4 rounded-2xl bg-white p-5 shadow-[0_20px_70px_rgba(12,35,64,.18)] md:grid-cols-[1fr_1fr_1fr_auto] md:p-7"
        >
          <label className="grid gap-2 text-xs font-bold uppercase tracking-wider text-slate-500">
            Ngày nhận phòng
            <input
              required
              name="checkIn"
              type="date"
              min={toDateInput()}
              defaultValue={toDateInput(tomorrow)}
              className="h-12 rounded-xl border border-slate-200 px-3 text-sm font-medium text-navy outline-none focus:border-gold"
            />
          </label>
          <label className="grid gap-2 text-xs font-bold uppercase tracking-wider text-slate-500">
            Ngày trả phòng
            <input
              required
              name="checkOut"
              type="date"
              min={toDateInput(tomorrow)}
              defaultValue={toDateInput(afterTomorrow)}
              className="h-12 rounded-xl border border-slate-200 px-3 text-sm font-medium text-navy outline-none focus:border-gold"
            />
          </label>
          <label className="grid gap-2 text-xs font-bold uppercase tracking-wider text-slate-500">
            Loại phòng
            <select
              name="roomTypeId"
              className="h-12 rounded-xl border border-slate-200 bg-white px-3 text-sm font-medium text-navy outline-none focus:border-gold"
            >
              <option value="">Tất cả loại phòng</option>
              {types?.map((type) => (
                <option key={type.id} value={type.id}>
                  {type.name}
                </option>
              ))}
            </select>
          </label>
          <Button className="self-end px-7">
            <Sparkles size={17} />
            Tìm phòng
          </Button>
        </form>
      </section>

      <section className="mx-auto max-w-7xl px-4 py-24 lg:px-8">
        <div className="mb-10 flex flex-col justify-between gap-4 sm:flex-row sm:items-end">
          <div>
            <p className="eyebrow">Không gian nghỉ dưỡng</p>
            <h2 className="section-title">Phòng được yêu thích</h2>
            <p className="mt-3 max-w-xl text-slate-500">
              Mỗi căn phòng là một khoảng trời riêng, được chăm chút cho giấc ngủ sâu và những buổi
              sớm an yên.
            </p>
          </div>
          <Link
            to="/phong"
            className="flex items-center gap-2 text-sm font-semibold text-navy hover:text-gold-dark"
          >
            Xem tất cả <ArrowRight size={17} />
          </Link>
        </div>
        {rooms?.length ? (
          <div className="grid gap-6 md:grid-cols-3">
            {rooms.map((room, index) => (
              <Link
                to={`/phong/${room.id}`}
                key={room.id}
                className="group overflow-hidden rounded-2xl bg-white shadow-[0_8px_35px_rgba(12,35,64,.08)] transition hover:-translate-y-1 hover:shadow-xl"
              >
                <div className="relative h-64 overflow-hidden">
                  <img
                    src={room.imageUrl || room.images?.[0] || ROOM_FALLBACKS[index]}
                    alt={`Phòng ${room.roomNumber}`}
                    className="size-full object-cover transition duration-700 group-hover:scale-105"
                  />
                  <span className="absolute left-4 top-4 rounded-full bg-white/90 px-3 py-1 text-xs font-semibold text-navy backdrop-blur">
                    Tầng {room.floor || "—"}
                  </span>
                </div>
                <div className="p-5">
                  <p className="text-xs font-semibold uppercase tracking-wider text-gold-dark">
                    {room.roomType?.name || "Phòng nghỉ"}
                  </p>
                  <div className="mt-2 flex items-start justify-between gap-3">
                    <h3 className="font-display text-2xl font-semibold text-navy">
                      Phòng {room.roomNumber}
                    </h3>
                    <p className="text-right text-sm font-semibold text-navy">
                      {formatVND(room.price || room.basePrice || room.roomType?.basePrice)}
                      <span className="block text-xs font-normal text-slate-400">/ đêm</span>
                    </p>
                  </div>
                </div>
              </Link>
            ))}
          </div>
        ) : (
          <div className="rounded-2xl border border-dashed border-slate-300 bg-white p-10 text-center text-slate-500">
            Phòng nổi bật sẽ hiển thị khi API có dữ liệu.
          </div>
        )}
      </section>

      <section className="bg-white py-24">
        <div className="mx-auto grid max-w-7xl items-center gap-12 px-4 lg:grid-cols-2 lg:px-8">
          <div className="relative">
            <img
              src="https://images.unsplash.com/photo-1551105379-6c50b7647c03?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&q=85&w=1200"
              alt="Hồ bơi hướng biển"
              className="h-[540px] w-full rounded-[2rem] object-cover"
            />
            <div className="absolute -bottom-6 -right-3 rounded-2xl bg-navy p-6 text-white shadow-xl sm:right-8">
              <p className="font-display text-4xl text-gold">24/7</p>
              <p className="mt-1 text-sm text-white/70">Chăm sóc kỳ nghỉ</p>
            </div>
          </div>
          <div>
            <p className="eyebrow">Đặc quyền tại Aurora</p>
            <h2 className="section-title">
              Mọi điều bạn cần,
              <br />
              ngay trong tầm tay.
            </h2>
            <p className="mt-5 leading-7 text-slate-500">
              Từ bữa sáng thảnh thơi đến buổi chiều bên hồ bơi vô cực, đội ngũ Aurora luôn sẵn sàng
              biến mỗi ngày lưu trú thành một kỷ niệm đáng nhớ.
            </p>
            <div className="mt-8 grid gap-5 sm:grid-cols-2">
              {[
                [Waves, "Hồ bơi vô cực"],
                [Coffee, "Bữa sáng tinh tuyển"],
                [Wifi, "Kết nối tốc độ cao"],
                [ConciergeBell, "Dịch vụ tận tâm"],
                [ShieldCheck, "An toàn & riêng tư"],
                [BedDouble, "Phòng nghỉ tiện nghi"],
              ].map(([Icon, label]) => (
                <div
                  key={label as string}
                  className="flex items-center gap-3 text-sm font-semibold text-navy"
                >
                  <span className="grid size-10 place-items-center rounded-full bg-cream text-gold-dark">
                    <Icon size={19} />
                  </span>
                  {label as string}
                </div>
              ))}
            </div>
          </div>
        </div>
      </section>
    </>
  );
}
