import { Menu, Phone, UserRound, X } from "lucide-react";
import { useState } from "react";
import { Link, NavLink, Outlet } from "react-router";
import { roleHome, useAuth } from "../auth/AuthProvider";
import { Brand } from "../components/Brand";
import { Button } from "../components/ui";

const links = [
  ["/", "Trang chủ"],
  ["/phong", "Phòng nghỉ"],
  ["/gioi-thieu", "Trải nghiệm"],
];

export function PublicLayout() {
  const { user } = useAuth();
  const [open, setOpen] = useState(false);
  return (
    <div className="min-h-screen bg-ivory">
      <div className="bg-navy px-4 py-2 text-center text-xs text-white/75">
        Đặt phòng trực tiếp · Hỗ trợ tận tâm · Trải nghiệm riêng tư
      </div>
      <header className="sticky top-0 z-40 border-b border-slate-200/70 bg-white/95 backdrop-blur">
        <div className="mx-auto flex h-20 max-w-7xl items-center justify-between px-4 lg:px-8">
          <Brand />
          <nav className="hidden items-center gap-8 md:flex">
            {links.map(([to, label]) => (
              <NavLink
                key={to}
                to={to}
                end={to === "/"}
                className={({ isActive }) =>
                  `text-sm font-medium transition hover:text-gold-dark ${isActive ? "text-gold-dark" : "text-slate-600"}`
                }
              >
                {label}
              </NavLink>
            ))}
          </nav>
          <div className="hidden items-center gap-3 md:flex">
            <a
              href="tel:19001234"
              className="mr-2 flex items-center gap-2 text-xs font-semibold text-navy"
            >
              <Phone size={15} /> 1900 1234
            </a>
            {user ? (
              <Link to={roleHome(user.role)}>
                <Button variant="secondary">
                  <UserRound size={17} />
                  {user.fullName || user.username}
                </Button>
              </Link>
            ) : (
              <>
                <Link to="/dang-nhap">
                  <Button variant="ghost">Đăng nhập</Button>
                </Link>
                <Link to="/dang-ky">
                  <Button>Đăng ký</Button>
                </Link>
              </>
            )}
          </div>
          <button
            className="rounded-lg p-2 md:hidden"
            onClick={() => setOpen(!open)}
            aria-label="Mở điều hướng"
          >
            {open ? <X /> : <Menu />}
          </button>
        </div>
        {open && (
          <div className="border-t bg-white p-4 md:hidden">
            {links.map(([to, label]) => (
              <Link
                key={to}
                to={to}
                onClick={() => setOpen(false)}
                className="block rounded-lg px-3 py-3 font-medium text-slate-700 hover:bg-cream"
              >
                {label}
              </Link>
            ))}
            <Link to={user ? roleHome(user.role) : "/dang-nhap"} className="mt-2 block">
              <Button className="w-full">{user ? "Tài khoản" : "Đăng nhập"}</Button>
            </Link>
          </div>
        )}
      </header>
      <main>
        <Outlet />
      </main>
      <footer className="bg-navy text-white">
        <div className="mx-auto grid max-w-7xl gap-10 px-4 py-14 md:grid-cols-4 lg:px-8">
          <div className="md:col-span-2">
            <Brand inverse />
            <p className="mt-5 max-w-md text-sm leading-7 text-white/60">
              Một không gian nghỉ dưỡng thanh lịch, nơi mỗi kỳ nghỉ được chăm chút bằng sự tinh tế
              và lòng hiếu khách Việt Nam.
            </p>
          </div>
          <div>
            <h3 className="mb-4 font-semibold">Khám phá</h3>
            <div className="grid gap-3 text-sm text-white/60">
              <Link to="/phong">Phòng nghỉ</Link>
              <Link to="/gioi-thieu">Tiện nghi</Link>
              <Link to="/tai-khoan">Đặt phòng của tôi</Link>
            </div>
          </div>
          <div>
            <h3 className="mb-4 font-semibold">Liên hệ</h3>
            <div className="grid gap-3 text-sm text-white/60">
              <span>1900 1234</span>
              <span>stay@aurorabay.vn</span>
              <span>Đà Nẵng, Việt Nam</span>
            </div>
          </div>
        </div>
        <div className="border-t border-white/10 px-4 py-5 text-center text-xs text-white/40">
          © 2026 Aurora Bay Hotel & Resort
        </div>
      </footer>
    </div>
  );
}
