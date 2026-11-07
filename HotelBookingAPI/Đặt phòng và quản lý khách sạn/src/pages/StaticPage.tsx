import { HeartHandshake, Leaf, ShieldCheck, Sparkles } from "lucide-react";
import { Link } from "react-router";

export function ExperiencePage() {
  return (
    <div>
      <section className="relative flex min-h-[520px] items-end overflow-hidden bg-navy">
        <img
          src="https://images.unsplash.com/photo-1565503261152-b42ab6c1a925?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&q=85&w=1800"
          alt="Bờ biển nhiệt đới"
          className="absolute inset-0 size-full object-cover opacity-65"
        />
        <div className="absolute inset-0 bg-gradient-to-t from-navy via-navy/20 to-transparent" />
        <div className="relative mx-auto w-full max-w-7xl px-4 pb-16 text-white lg:px-8">
          <p className="eyebrow text-gold">Tinh thần Aurora</p>
          <h1 className="max-w-3xl font-display text-5xl">Nơi sự thanh lịch gặp gỡ thiên nhiên</h1>
        </div>
      </section>
      <section className="mx-auto max-w-6xl px-4 py-24 lg:px-8">
        <div className="mx-auto max-w-2xl text-center">
          <p className="eyebrow">Trải nghiệm đáng nhớ</p>
          <h2 className="section-title">Chăm chút từ những điều nhỏ nhất</h2>
          <p className="mt-5 leading-7 text-slate-500">
            Aurora Bay tạo nên một nhịp nghỉ chậm rãi, riêng tư và đầy cảm hứng bên bờ biển Việt
            Nam.
          </p>
        </div>
        <div className="mt-14 grid gap-5 md:grid-cols-4">
          {[
            [Sparkles, "Không gian tinh tế"],
            [HeartHandshake, "Phục vụ tận tâm"],
            [Leaf, "Gần gũi thiên nhiên"],
            [ShieldCheck, "An toàn riêng tư"],
          ].map(([Icon, label]) => (
            <div
              key={label as string}
              className="rounded-2xl border border-slate-200 bg-white p-6 text-center"
            >
              <Icon className="mx-auto text-gold-dark" />
              <p className="mt-4 font-semibold text-navy">{label as string}</p>
            </div>
          ))}
        </div>
      </section>
    </div>
  );
}

export function NotFoundPage() {
  return (
    <div className="grid min-h-[65vh] place-items-center px-4 text-center">
      <div>
        <p className="font-display text-8xl text-gold">404</p>
        <h1 className="mt-3 font-display text-3xl font-semibold text-navy">
          Trang này không tồn tại
        </h1>
        <Link to="/" className="mt-6 inline-block font-semibold text-gold-dark">
          Về trang chủ →
        </Link>
      </div>
    </div>
  );
}
