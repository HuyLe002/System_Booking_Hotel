import {
  BedDouble,
  BookOpen,
  ChevronRight,
  ClipboardCheck,
  CreditCard,
  FileClock,
  Hotel,
  LayoutDashboard,
  LogOut,
  Menu,
  Settings,
  Tag,
  Users,
  X,
} from "lucide-react";
import { useState } from "react";
import { NavLink, Outlet } from "react-router";
import { useAuth } from "../auth/AuthProvider";
import { Brand } from "../components/Brand";

const staffNav = [
  ["/van-hanh", "Tổng quan", LayoutDashboard],
  ["/van-hanh/booking", "Booking", BookOpen],
  ["/van-hanh/khach-hang", "Khách hàng", Users],
  ["/van-hanh/thanh-toan", "Đối soát thanh toán", CreditCard],
];
const adminNav = [
  ["/admin", "Tổng quan", LayoutDashboard],
  ["/admin/booking", "Booking", BookOpen],
  ["/admin/phong", "Phòng & loại phòng", BedDouble],
  ["/admin/danh-muc", "Tiện nghi & dịch vụ", Settings],
  ["/admin/gia", "Mã giảm & quy tắc giá", Tag],
  ["/admin/khach-hang", "Khách hàng", Users],
  ["/admin/nhan-vien", "Nhân viên", ClipboardCheck],
  ["/admin/thanh-toan", "Đối soát thanh toán", CreditCard],
  ["/admin/nhat-ky", "Nhật ký hoạt động", FileClock],
];

export function DashboardLayout({ admin = false }: { admin?: boolean }) {
  const [open, setOpen] = useState(false);
  const { user, logout } = useAuth();
  const nav = admin ? adminNav : staffNav;
  return (
    <div className="min-h-screen bg-slate-50">
      {open && (
        <button
          className="fixed inset-0 z-40 bg-navy/50 lg:hidden"
          aria-label="Đóng menu"
          onClick={() => setOpen(false)}
        />
      )}
      <aside
        className={`fixed inset-y-0 left-0 z-50 flex w-72 flex-col bg-navy text-white transition-transform lg:translate-x-0 ${open ? "translate-x-0" : "-translate-x-full"}`}
      >
        <div className="flex h-24 items-center justify-between px-6">
          <Brand inverse />
          <button className="lg:hidden" onClick={() => setOpen(false)}>
            <X />
          </button>
        </div>
        <div className="mx-5 mb-5 rounded-xl border border-white/10 bg-white/5 p-3">
          <p className="text-xs text-white/50">
            {admin ? "Khu vực quản trị" : "Trung tâm vận hành"}
          </p>
          <p className="mt-1 truncate text-sm font-semibold">{user?.fullName || user?.username}</p>
        </div>
        <nav className="flex-1 space-y-1 overflow-auto px-4">
          {nav.map(([to, label, Icon]) => (
            <NavLink
              key={to as string}
              to={to as string}
              end={to === (admin ? "/admin" : "/van-hanh")}
              onClick={() => setOpen(false)}
              className={({ isActive }) =>
                `flex items-center gap-3 rounded-xl px-3.5 py-3 text-sm transition ${isActive ? "bg-gold text-navy shadow" : "text-white/65 hover:bg-white/10 hover:text-white"}`
              }
            >
              <Icon size={19} />
              <span className="flex-1">{label as string}</span>
              <ChevronRight size={15} />
            </NavLink>
          ))}
        </nav>
        <button
          onClick={logout}
          className="m-4 flex items-center gap-3 rounded-xl border border-white/10 px-4 py-3 text-sm text-white/70 hover:bg-white/10"
        >
          <LogOut size={18} />
          Đăng xuất
        </button>
      </aside>
      <div className="lg:pl-72">
        <header className="sticky top-0 z-30 flex h-20 items-center justify-between border-b border-slate-200 bg-white/95 px-4 backdrop-blur sm:px-8">
          <div className="flex items-center gap-3">
            <button
              className="rounded-lg p-2 hover:bg-slate-100 lg:hidden"
              onClick={() => setOpen(true)}
            >
              <Menu />
            </button>
            <Hotel className="hidden text-gold-dark sm:block" />
            <div>
              <p className="text-xs text-slate-400">Aurora Bay</p>
              <h1 className="font-display text-lg font-semibold text-navy">
                {admin ? "Quản trị khách sạn" : "Vận hành hôm nay"}
              </h1>
            </div>
          </div>
          <div className="size-10 rounded-full bg-navy text-center text-sm font-bold leading-10 text-white">
            {(user?.fullName || user?.username || "NV").slice(0, 2).toUpperCase()}
          </div>
        </header>
        <main className="mx-auto max-w-[1500px] p-4 sm:p-8">
          <Outlet />
        </main>
      </div>
    </div>
  );
}
